using System;
using System.Collections.Generic;
using System.Linq;
using RiichiReign.MahjongEngine;
using RiichiReign.UnityUIToolKitComponent;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;
        public Dictionary<string, PlayerBehaviour> PlayerIDBehaviorPair { get; private set; } =
            new();

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

        public void RegisterPlayer(PlayerBehaviour player)
        {
            Debug.Log($"[{GetType().Name}] Sending ID to server...");
            ServerManager.Instance.RegisterPlayerServerRpc(player.PlayerID);
        }

        public void FinalizePlayerObject()
        {
            List<PlayerBehaviour> playerBehaviourList = FindObjectsByType<PlayerBehaviour>(
                    FindObjectsSortMode.None
                )
                .ToList();

            foreach (var playerBehaviour in playerBehaviourList)
            {
                PlayerIDBehaviorPair[playerBehaviour.PlayerID] = playerBehaviour;
            }
        }

        public void SetPlayerData(string playerID, int points, int windValue)
        {
            PlayerBehaviour playerBehaviour = PlayerIDBehaviorPair[playerID];

            if (playerBehaviour == null)
            {
                throw new Exception($"[{GetType().Name}] Player{playerID} doesn't exists!");
            }
            else
            {
                Debug.Log($"[{GetType().Name}] Syncing Player Data: {playerID}");

                playerBehaviour.UpdatePlayerData(points, windValue);
            }
        }

        public void InitializeLPlayerUI()
        {
            PlayerUIController.Instance.InitializeGameView();
        }

        public void SyncPlayerHand(PlayerHand newHand)
        {
            PlayerBehaviour.LocalPlayerInstance.UpdatePlayerHand(newHand);
        }

        public void SyncPlayerHand(OpponentHand opponentHand)
        {
            PlayerBehaviour playerBehaviour = PlayerIDBehaviorPair[opponentHand.PlayerID];

            playerBehaviour.UpdatePlayerHand(opponentHand);
        }
    }
}
