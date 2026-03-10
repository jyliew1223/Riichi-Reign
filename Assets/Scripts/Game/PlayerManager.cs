using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using RiichiReign.GameComponent;
using RiichiReign.Player;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    internal class PlayerManager : NetworkBehaviour
    {
        public static PlayerManager Instance { get; private set; }

        public List<PlayerInstance> PlayerList { get; private set; }

        List<GameUIController> _gameUIControllerList;

        #region Unity Logics

        void Start()
        {
            if (Instance != null && Instance != this && !IsServer)
            {
                Destroy(gameObject);
            }

            Instance = this;
            PlayerList = new();
            _gameUIControllerList = new();
        }

        #endregion

        #region NetCode Logics

        public override void OnNetworkDespawn()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region methods

        public void AddPlayer(PlayerInstance incomingPlayer)
        {
            PlayerList.Add(incomingPlayer);
            Debug.Log($"[{GetType().Name}] Adding Player: {incomingPlayer}", this);
        }

        #endregion

        #region RPCs

        public void StartSyncPlayerDataRPC()
        {
            string json = JsonConvert.SerializeObject(PlayerList);
            SyncPlayerDataClientRPC(json);
            Debug.Log($"[{GetType().Name}] Start syncing data wiith clien: " + json.Prettify());
        }

        public void StartInitializePlayersRPC()
        {
            string json = JsonConvert.SerializeObject(PlayerList);
            SyncPlayerDataClientRPC(json);
            Debug.Log($"[{GetType().Name}] Start syncing data wiith clien: " + json.Prettify());
        }

        [ClientRpc]
        private void SyncPlayerDataClientRPC(string json)
        {
            List<PlayerInstance> playerList = JsonConvert.DeserializeObject<List<PlayerInstance>>(
                json
            );
            var localUIs = FindObjectsByType<GameUIController>(FindObjectsSortMode.None);

            foreach (var localUI in localUIs)
            {
                if (localUI != null)
                {
                    localUI.UpdatePlayerList(playerList);
                }
            }
        }

        #endregion

        #region Methods

        public void ResetInstance()
        {
            PlayerList.Clear();
        }

        #endregion
    }
}
