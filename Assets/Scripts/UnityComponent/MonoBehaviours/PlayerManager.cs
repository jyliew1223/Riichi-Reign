using System;
using System.Collections.Generic;
using System.Linq;
using RiichiReign.DataPackets;
using RiichiReign.UnityUIToolKitComponent;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    public class PlayerManager : MonoBehaviour
    {
        public event Action<string> OnSyncedPlayerHand;

        public static PlayerManager Instance;
        public Dictionary<string, PlayerBehaviour> PlayerIDBehaviorPair { get; private set; } =
            new();

        List<IDisposable> _subscriptions = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            var onGameInitSub = new EventSubscription<List<PlayerDataPacket>>(
                HandleOnGameInit,
                h => ServerManager.Instance.OnGameInit -= h
            );
            ServerManager.Instance.OnGameInit += onGameInitSub.Handler;
            _subscriptions.Add(onGameInitSub);

            var onSyncPlayerHandSub = new EventSubscription<HandPacket>(
                SyncPlayerHand,
                h => ServerManager.Instance.OnSyncPlayerHand -= h
            );
            ServerManager.Instance.OnSyncPlayerHand += onSyncPlayerHandSub.Handler;
            _subscriptions.Add(onSyncPlayerHandSub);
        }

        void OnDisable()
        {
            UnsubscribeAndClearAll();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        void UnsubscribeAndClearAll()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }

            _subscriptions.Clear();
        }

        public void RegisterPlayer(PlayerBehaviour player)
        {
            Debug.Log($"[{GetType().Name}] Sending ID to server...");
            ServerManager.Instance.RegisterPlayerServerRpc(player.PlayerID);
        }

        void HandleOnGameInit(List<PlayerDataPacket> packets)
        {
            FinalizePlayerObject();

            foreach (var packet in packets)
            {
                PlayerIDBehaviorPair[packet.PlayerID].UpdatePlayerData(packet);
            }

            InitializePlayerUI();
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

        public void InitializePlayerUI()
        {
            PlayerUIController.Instance.InitializeGameView();
        }

        public void SyncPlayerHand(HandPacket packet)
        {
            if (packet is PlayerHandPacket playerHand)
            {
                PlayerBehaviour.LocalPlayerInstance.UpdatePlayerHand(playerHand);
                OnSyncedPlayerHand?.Invoke(PlayerBehaviour.LocalPlayerInstance.PlayerID);
            }
            else if (packet is OpponentHandPacket oppHand)
            {
                PlayerBehaviour playerBehaviour = PlayerIDBehaviorPair[oppHand.PlayerID];
                playerBehaviour.UpdatePlayerHand(oppHand);
                OnSyncedPlayerHand?.Invoke(oppHand.PlayerID);
            }
        }
    }
}
