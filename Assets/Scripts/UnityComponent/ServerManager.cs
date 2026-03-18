using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RiichiReign.MahjongEngine;
using RiichiReign.UnityComponent;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace RiichiReiign.UnityComponent
{
    struct PlayerDataPacket
    {
        [JsonProperty("playerID")]
        public string PlayerID;

        [JsonProperty("points")]
        public int Points;

        [JsonProperty("windValue")]
        public int WindValue;

        public PlayerDataPacket(string playerID, int points, int windValue)
        {
            PlayerID = playerID;
            Points = points;
            WindValue = windValue;
        }

        public PlayerDataPacket(Player player)
        {
            PlayerID = player.PlayerID;
            Points = player.Points;
            WindValue = player.WindValue;
        }
    }

    public class ServerManager : NetworkBehaviour
    {
        public static ServerManager Instance;

        Dictionary<string, ulong> _playerIDClientIDPair = new();

        public override void OnNetworkSpawn()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Debug.Log($"[{GetType().Name}] Setting Singleton...");
            Instance = this;
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        [Rpc(SendTo.Server)]
        public void RegisterPlayerServerRpc(RpcParams rpcParams = default)
        {
            if (!IsServer)
                throw new System.Exception(
                    $"[{GetType().Name}][Client] This intance is not authorize to call this method: RegisterPlayerServerRpc!"
                );

            ulong clientID = rpcParams.Receive.SenderClientId;

            Debug.Log(
                $"[{GetType().Name}][Server] Client ID({clientID}) Received. Creating new player."
            );

            // temp random player ID
            string playerID = System.Guid.NewGuid().ToString();

            Player newPlayer = GameManager.Instance.AddPlayer(playerID);
            string json = JsonConvert.SerializeObject(newPlayer);

            StringBuilder sb = new();
            sb.AppendLine(
                $"[{GetType().Name}][Server] Player created. Sending back to client and register on server."
            );
            sb.AppendLine();
            sb.AppendLine($"JSON: {json}");

            _playerIDClientIDPair.Add(newPlayer.PlayerID, clientID);

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
            Player returnedlPlayer = JsonConvert.DeserializeObject<Player>(json);

            if (returnedlPlayer != null)
            {
                StringBuilder sb = new();
                sb.AppendLine($"[{GetType().Name}][Client] Successfully received Player object!");
                sb.AppendLine();
                sb.AppendLine($"Player: {returnedlPlayer}");

                Debug.Log(sb.ToString());

                PlayerManager.Instance.SetLocalPlayer(returnedlPlayer);
            }
            else
            {
                throw new System.Exception(
                    $"[{GetType().Name}][Client] Received JSON but is null!"
                );
            }
        }

        public void InitializeClientGame(List<Player> playerList)
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

            foreach (var packet in packets)
            {
                PlayerManager.Instance.SetPlayerData(
                    packet.PlayerID,
                    packet.Points,
                    packet.WindValue,
                    true
                );
            }

            PlayerManager.Instance.InitializeLPlayerUI();
        }

        public void SyncPlayerHand(Player player)
        {
            if (!IsServer)
                throw new System.Exception(
                    $"[{GetType().Name}][Client] This intance is not authorize to call this method: SyncPlayerHand!"
                );

            ulong clientID = _playerIDClientIDPair[player.PlayerID];

            StringBuilder sb = new();

            sb.AppendLine($"[{GetType().Name}][Server] Start syncing player hand...");
            sb.AppendLine($"Player client ID: {clientID}");

            Debug.Log(sb.ToString());

            OpponentHand opponentHand = new(player.PlayerID, player.Hand);

            string myHandJson = JsonConvert.SerializeObject(player.Hand);
            string opponentHandJson = JsonConvert.SerializeObject(opponentHand);

            var sendToOwner = RpcTarget.Single(clientID, RpcTargetUse.Temp);
            var sendToOthers = RpcTarget.Not(clientID, RpcTargetUse.Temp);

            SyncRealHandClientRpc(
                myHandJson,
                clientID,
                new RpcParams { Send = new RpcSendParams { Target = sendToOwner } }
            );
            SyncInvisibleHandClientRpc(
                opponentHandJson,
                clientID,
                new RpcParams { Send = new RpcSendParams { Target = sendToOthers } }
            );
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void SyncRealHandClientRpc(
            string json,
            ulong clientID,
            RpcParams rpcParams = default
        )
        {
            Debug.Log($"[{GetType().Name}][Client] Received REAL hand data.");
            PlayerHand myHand = JsonConvert.DeserializeObject<PlayerHand>(json);

            PlayerManager.Instance.SyncLocalPlayerHand(myHand);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void SyncInvisibleHandClientRpc(
            string json,
            ulong clientID,
            RpcParams rpcParams = default
        )
        {
            Debug.Log(
                $"[{GetType().Name}][Client] Received INVISIBLE hand data for Player {clientID}."
            );
            OpponentHand oppHand = JsonConvert.DeserializeObject<OpponentHand>(json);

            PlayerManager.Instance.SyncOpponentPlayerHand(oppHand);
        }
    }
}
