using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RiichiReign.DataPackets;
using RiichiReign.MahjongEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    public class ServerManager : NetworkBehaviour
    {
        public static ServerManager Instance;

        public event Action<List<PlayerDataPacket>> OnGameInit;
        public event Action<HandPacket> OnSyncPlayerHand;
        public event Action<List<GameAction>> OnRequestPlayerAction;

        GameEventHub _eventHub;
        Dictionary<string, ulong> _playerIDClientIDPair = new();
        Dictionary<ulong, string> _clientIDPlayerIDPair = new();
        List<IDisposable> _subscriptions = new();

        void Start()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public override void OnNetworkSpawn() { }

        public override void OnNetworkDespawn()
        {
            UnsubscribeAndClearAll();
        }

        void UnsubscribeAndClearAll()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }

            _subscriptions.Clear();
        }

        public void AttachGameEventHub(GameEventHub eventHub)
        {
            _eventHub = eventHub;

            var initGameSub = new EventSubscription<List<Player>>(
                HandleOnGameInit,
                h => _eventHub.OnGameInit -= h
            );
            _eventHub.OnGameInit += initGameSub.Handler;
            _subscriptions.Add(initGameSub);

            var syncPlayerHandSub = new EventSubscription<Player>(
                HandleOnSyncPlayerHand,
                h => _eventHub.OnPlayerHandChanged -= h
            );
            _eventHub.OnPlayerHandChanged += syncPlayerHandSub.Handler;
            _subscriptions.Add(syncPlayerHandSub);

            var requestPlayerActionSub = new EventSubscription<List<GameAction>, string>(
                HandleOnRequestPlayerAction,
                h => _eventHub.OnRequestPlayerAction -= h
            );
            _eventHub.OnRequestPlayerAction += requestPlayerActionSub.Handler;
            _subscriptions.Add(requestPlayerActionSub);
        }

        [Rpc(SendTo.Server)]
        public void RegisterPlayerServerRpc(string playerID, RpcParams rpcParams = default)
        {
            if (!IsServer)
                throw new Exception(
                    $"[{GetType().Name}][Client] This intance is not authorize to call this method: RegisterPlayerServerRpc!"
                );

            ulong clientID = rpcParams.Receive.SenderClientId;

            Player newPlayer = GameManager.Instance.AddPlayer(playerID);
            PlayerDataPacket packet = new(newPlayer);
            string json = JsonConvert.SerializeObject(packet);

            StringBuilder sb = new();
            sb.AppendLine(
                $"[{GetType().Name}][Server] Player created. Sending back to client and register on server."
            );
            sb.AppendLine();
            sb.AppendLine($"JSON: {json}");

            _playerIDClientIDPair.Add(newPlayer.PlayerID, clientID);
            _clientIDPlayerIDPair.Add(clientID, newPlayer.PlayerID);

            Debug.Log(sb.ToString());

            var targetParams = new RpcParams
            {
                Send = new RpcSendParams { Target = RpcTarget.Single(clientID, RpcTargetUse.Temp) },
            };

            ConfirmRegistrationRpc(json, targetParams);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void ConfirmRegistrationRpc(string json, RpcParams rpcParams = default)
        {
            PlayerDataPacket packet = JsonConvert.DeserializeObject<PlayerDataPacket>(json);

            if (PlayerBehaviour.LocalPlayerInstance.PlayerID != packet.PlayerID)
                throw new Exception($"[{GetType().Name}][Client] targetPlayerID mismatch!");

            if (packet != null)
            {
                StringBuilder sb = new();
                sb.AppendLine($"[{GetType().Name}][Client] Successfully received Player object!");
                sb.AppendLine();
                sb.AppendLine($"Player: {packet}");

                Debug.Log(sb.ToString());
            }
            else
            {
                throw new Exception($"[{GetType().Name}][Client] Received JSON but is null!");
            }
        }

        public void HandleOnGameInit(List<Player> playerList)
        {
            Debug.Log($"[{GetType().Name}][Server] Start Initializing Game View for clients...");

            List<PlayerDataPacket> packets = new();
            foreach (var player in playerList)
            {
                packets.Add(new(player));
            }

            string json = JsonConvert.SerializeObject(packets);
            InitializeClientGameRpc(json);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void InitializeClientGameRpc(string json)
        {
            StringBuilder sb = new();

            sb.AppendLine(
                $"[{GetType().Name}][Client] Received Player List for game initialization..."
            );
            sb.AppendLine($"JSON: {json.Prettify()}");

            Debug.Log(sb.ToString());

            List<PlayerDataPacket> packets = JsonConvert.DeserializeObject<List<PlayerDataPacket>>(
                json
            );

            OnGameInit?.Invoke(packets);
        }

        public void HandleOnSyncPlayerHand(Player player)
        {
            if (!IsServer)
                throw new Exception(
                    $"[{GetType().Name}][Client] This intance is not authorize to call this method: SyncPlayerHand!"
                );

            ulong clientID = _playerIDClientIDPair[player.PlayerID];

            PlayerHandPacket playerHandPacket = new(player.Hand);
            OpponentHandPacket opponentHandPacket = new(player.PlayerID, player.Hand);

            string myHandJson = JsonConvert.SerializeObject(playerHandPacket);
            string opponentHandJson = JsonConvert.SerializeObject(opponentHandPacket);

            var sendToOwner = RpcTarget.Single(clientID, RpcTargetUse.Temp);
            var sendToOthers = RpcTarget.Not(clientID, RpcTargetUse.Temp);

            SyncRealHandClientRpc(
                myHandJson,
                new RpcParams { Send = new RpcSendParams { Target = sendToOwner } }
            );
            SyncInvisibleHandClientRpc(
                opponentHandJson,
                new RpcParams { Send = new RpcSendParams { Target = sendToOthers } }
            );
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void SyncRealHandClientRpc(string json, RpcParams rpcParams = default)
        {
            Debug.Log($"[{GetType().Name}][Client] Syncing hand data.");
            PlayerHandPacket myHand = JsonConvert.DeserializeObject<PlayerHandPacket>(json);

            OnSyncPlayerHand?.Invoke(myHand);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void SyncInvisibleHandClientRpc(string json, RpcParams rpcParams = default)
        {
            OpponentHandPacket oppHand = JsonConvert.DeserializeObject<OpponentHandPacket>(json);

            Debug.Log(
                $"[{GetType().Name}][Client] Received hand data for Player {oppHand.PlayerID}."
            );

            OnSyncPlayerHand?.Invoke(oppHand);
        }

        public void HandleOnRequestPlayerAction(
            List<GameAction> availableActions,
            string targetPlayerID
        )
        {
            ulong clientID = _playerIDClientIDPair[targetPlayerID];

            var sendTo = RpcTarget.Single(clientID, RpcTargetUse.Temp);

            string json = JsonConvert.SerializeObject(availableActions);

            RequestPlayerActionRpc(
                targetPlayerID,
                json,
                new RpcParams { Send = new RpcSendParams { Target = sendTo } }
            );
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void RequestPlayerActionRpc(
            string targetPlayerID,
            string json,
            RpcParams rpcParams = default
        )
        {
            if (PlayerBehaviour.LocalPlayerInstance.PlayerID != targetPlayerID)
                throw new Exception($"[{GetType().Name}][Client] targetPlayerID mismatch!");

            List<GameAction> availableActions = JsonConvert.DeserializeObject<List<GameAction>>(
                json
            );

            OnRequestPlayerAction?.Invoke(availableActions);
        }

        public void ReturnPlayerAction(GameAction playerAction)
        {
            string json = JsonConvert.SerializeObject(playerAction);
            ReturnPlayerActionRpc(json);
        }

        [Rpc(SendTo.Server)]
        private void ReturnPlayerActionRpc(string json, RpcParams rpcParams = default)
        {
            ulong clientID = rpcParams.Receive.SenderClientId;
            string playerID = _clientIDPlayerIDPair[clientID];

            GameAction playerAction = JsonConvert.DeserializeObject<GameAction>(json);

            _eventHub.RaisePlayerResponse(playerID, playerAction);
        }
    }
}
