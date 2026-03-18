using System.Collections.Generic;
using RiichiReign.MahjongEngine;
using RiichiReign.UnityUIToolKitComponent;
using RiichiReiign.UnityComponent;
using Unity.VisualScripting;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    public class PlayerData
    {
        public int Points;

        public int WindValue;

        public PlayerData(int points, int windValue)
        {
            Points = points;
            WindValue = windValue;
        }
    }

    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;

        public string LocalPlayerID { get; private set; }
        public PlayerData LocalPLayerData => StoredPlayerIDDataPair[LocalPlayerID];

        public Dictionary<string, PlayerData> StoredPlayerIDDataPair = new();
        PlayerUIController _playerUI;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Debug.Log($"[{GetType().Name}] Setting Singleton.....");
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void RegisterPlayer(PlayerUIController playerUI)
        {
            _playerUI = playerUI;
            Debug.Log($"[{GetType().Name}] Sending ID to server...");
            ServerManager.Instance.RegisterPlayerServerRpc();
        }

        public void SetLocalPlayer(Player player)
        {
            Debug.Log($"[{GetType().Name}] Setting LocalPlayer: {player}");
            LocalPlayerID = player.PlayerID;
        }

        public void SetPlayerData(
            string playerID,
            int points,
            int windValue,
            bool isAddingEnabled = true
        )
        {
            if (!StoredPlayerIDDataPair.ContainsKey(playerID) && !isAddingEnabled)
            {
                throw new System.Exception($"[{GetType().Name}] Player{playerID} doesn't exists!");
            }
            else if (!StoredPlayerIDDataPair.ContainsKey(playerID))
            {
                Debug.Log($"[{GetType().Name}] Adding Player Data: {playerID}");
                PlayerData data = new(points, windValue);
                StoredPlayerIDDataPair[playerID] = data;
            }
            else
            {
                Debug.Log($"[{GetType().Name}] Syncing Player Data: {playerID}");
                StoredPlayerIDDataPair[playerID].Points = points;
                StoredPlayerIDDataPair[playerID].WindValue = windValue;
            }
        }

        public void InitializeLPlayerUI()
        {
            _playerUI.InitializeGameView();
        }

        public void SyncLocalPlayerHand(PlayerHand newHand)
        {
            _playerUI.UpdateLocalPlayerHand(newHand);
        }

        public void SyncOpponentPlayerHand(OpponentHand opponentHand)
        {
            if (opponentHand.PlayerID == LocalPlayerID)
            {
                Debug.LogWarning(
                    $"[{GetType().Name}] Receive INVISIBLE hand for LocalPlayer instead!"
                );
            }

            _playerUI.UpdateOpponentPlayerHand(opponentHand);
        }
    }
}
