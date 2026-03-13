using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RiichiReign.Player;
using RiichiReign.UnityUIToolKitComponent;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    internal class PlayerManager : NetworkBehaviour
    {
        public bool IsReady { get; private set; } = false;

        public List<PlayerInstance> PlayerList { get; private set; } = new();
        public NetworkVariable<FixedString4096Bytes> PlayerListJson = new();

        private static PlayerManager _instance;
        private PlayerUIController _localPlayerUI;

        #region Netcode Logics

        public override void OnNetworkSpawn()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            IsReady = true;
            PlayerListJson.OnValueChanged += HandleOnValueChanged;

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"[{GetType().Name}] Network spawned:");
            stringBuilder.AppendLine("IsServer: " + IsServer);
            stringBuilder.AppendLine("IsLocalPlayer: " + IsLocalPlayer);
            stringBuilder.AppendLine("IsHost: " + IsHost);
            stringBuilder.AppendLine("IsClient: " + IsClient);

            Debug.Log(stringBuilder.ToString());
        }

        public static PlayerManager Server =>
            _instance.IsServer
                ? _instance
                : throw new System.Exception(
                    $"[{_instance.GetType().Name}] Local instance is not server instance"
                );
        public static PlayerManager Local =>
            _instance.IsClient
                ? _instance
                : throw new System.Exception(
                    $"[{_instance.GetType().Name}] Local instance is not client instance"
                );

        public override void OnNetworkDespawn()
        {
            PlayerListJson.OnValueChanged -= HandleOnValueChanged;

            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region  Methods

        public void RegisterPlayerUI(PlayerUIController playerUI)
        {
            _localPlayerUI = playerUI;
            Debug.Log($"[{GetType().Name}] _localPlayerUI assigned", _localPlayerUI.gameObject);
        }

        public void AddPlayer(ulong networkID)
        {
            if (!IsServer)
                throw new System.Exception(
                    $"[{GetType().Name}] Calling server method in client machine"
                );

            PlayerInstance player = new(networkID);
            PlayerList.Add(player);
            Debug.Log($"[(Server) {GetType().Name}] Player Added: {player}");
        }

        public void InitializePlayer()
        {
            if (!IsServer)
                throw new System.Exception(
                    $"[{GetType().Name}] Calling server method in client machine"
                );

            string json = JsonConvert.SerializeObject(PlayerList);
            InitializePlayerClientRPC(json);
            Debug.Log($"[(Server) {GetType().Name}] Initializing players");
        }

        public void SyncPlayerData()
        {
            if (!IsServer)
                throw new System.Exception(
                    $"[{GetType().Name}] Calling server method in client machine"
                );

            string json = JsonConvert.SerializeObject(PlayerList);
            FixedString4096Bytes newValue = new(json);
            if (PlayerListJson.Value != newValue)
            {
                PlayerListJson.Value = newValue;
            }
            Debug.Log(
                $"[{GetType().Name}] Syncing player data via NetworkVariable: " + json.Prettify()
            );
        }

        #endregion

        #region RPCs

        [Rpc(SendTo.ClientsAndHost)]
        private void InitializePlayerClientRPC(string json)
        {
            if (_localPlayerUI == null)
                Debug.LogWarning($"[{GetType().Name}] _localPlayerUI not assgin!");

            List<PlayerInstance> playerList = JsonConvert.DeserializeObject<List<PlayerInstance>>(
                json
            );

            PlayerInstance localPlayer = playerList.Find(x =>
                x.PlayerNetworkID == _localPlayerUI.NetworkObjectId
            );

            _localPlayerUI.AssignPlayer(localPlayer, playerList.IndexOf(localPlayer) + 1);
            _localPlayerUI.AssignPlayerUIs(playerList);
        }

        [Rpc(SendTo.Server)]
        public void SendPlayerDataServerRPC(ulong clientNetworkObjectId)
        {
            AddPlayer(clientNetworkObjectId);
        }

        #endregion

        #region Network value changes callbacks

        private void HandleOnValueChanged(
            FixedString4096Bytes oldValue,
            FixedString4096Bytes newValue
        )
        {
            if (_localPlayerUI == null)
            {
                Debug.LogWarning(
                    $"[{GetType().Name}] _localPlayerUI is null when Handling OnValueChange"
                );
                return;
            }

            if (string.IsNullOrEmpty(newValue.ToString()))
            {
                return;
            }

            try
            {
                List<PlayerInstance> playerList = JsonConvert.DeserializeObject<
                    List<PlayerInstance>
                >(newValue.ToString());

                _localPlayerUI.UpdatePlayerHandUI(playerList);
            }
            catch (JsonException ex)
            {
                Debug.LogError($"Failed to deserialize player list in UI: {ex.Message}");
            }
        }

        #endregion
    }
}
