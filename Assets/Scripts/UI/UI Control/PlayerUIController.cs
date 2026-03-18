using System.Collections.Generic;
using RiichiReign.MahjongEngine;
using RiichiReign.UI;
using RiichiReign.UnityComponent;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.UIElements;

namespace RiichiReign.UnityUIToolKitComponent
{
    struct PlayerContainerElement
    {
        public VisualElement HandContainer { get; private set; }

        public VisualElement TempTileContainer { get; private set; }

        public PlayerContainerElement(VisualElement handContainer, VisualElement tempTileContainer)
        {
            HandContainer = handContainer;
            TempTileContainer = tempTileContainer;
            Clear();
        }

        public readonly void Clear()
        {
            HandContainer.Clear();
            TempTileContainer.Clear();
        }
    }

    [RequireComponent(typeof(UIDocument))]
    public class PlayerUIController : NetworkBehaviour
    {
        List<PlayerContainerElement> _containerList = new();
        Dictionary<string, PlayerContainerElement> _playerIDContainerPair = new();

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                gameObject.SetActive(false);
                return;
            }

            InitializeUIContainers();
            Invoke(nameof(Register), 1f);
        }

        private void Register()
        {
            PlayerManager.Instance.RegisterPlayer(this);
        }

        private void InitializeUIContainers()
        {
            var root =
                GetComponent<UIDocument>().rootVisualElement
                ?? throw new System.Exception(
                    $"[{GetType().Name}] Failed to get root visual element"
                );
            var table =
                root.Q<VisualElement>("Table")
                ?? throw new System.Exception($"[{GetType().Name}] Failed to get Table element");

            for (int i = 1; i <= 4; i++)
            {
                VisualElement playerContainer = table.Q<VisualElement>($"PlayerContainer{i}");
                if (playerContainer == null)
                {
                    Debug.LogWarning($"[{GetType().Name}] PlayerContainer{i} not found in UI");
                    continue;
                }

                VisualElement handContainer = playerContainer.Q<VisualElement>("HandContainer");
                VisualElement tempTileContainer = playerContainer.Q<VisualElement>(
                    "TempTileContainer"
                );

                if (handContainer == null || tempTileContainer == null)
                {
                    Debug.LogWarning(
                        $"[{GetType().Name}] Containers not found in PlayerContainer{i}"
                    );
                    continue;
                }

                PlayerContainerElement containerElement = new(handContainer, tempTileContainer);
                containerElement.Clear();
                _containerList.Add(containerElement);
            }
        }

        public void InitializeGameView()
        {
            Debug.Log($"[{GetType().Name}] Initializing Game...");

            int localWindValue = PlayerManager.Instance.LocalPLayerData.WindValue;

            foreach (var pair in PlayerManager.Instance.StoredPlayerIDDataPair)
            {
                int localIndex = (pair.Value.WindValue - localWindValue + 4) % 4;
                _playerIDContainerPair[pair.Key] = _containerList[localIndex];
            }
        }

        public void UpdateLocalPlayerHand(PlayerHand hand)
        {
            Debug.Log($"[{GetType().Name}] Updating Local Player Hand...");

            string localPlayerID = PlayerManager.Instance.LocalPlayerID;

            if (hand.TempTile != null)
            {
                TileElement element = new(hand.TempTile);
                _playerIDContainerPair[localPlayerID].TempTileContainer.Add(element);
            }

            foreach (var tile in hand.TilesInHand)
            {
                TileElement element = new(tile);
                _playerIDContainerPair[localPlayerID].HandContainer.Add(element);
            }
        }

        public void UpdateOpponentPlayerHand(OpponentHand opponentHand)
        {
            string playerID = opponentHand.PlayerID;

            if (opponentHand.HasTempTile)
            {
                TileElement element = new();
                _playerIDContainerPair[playerID].TempTileContainer.Add(element);
            }

            for (int i = 0; i < opponentHand.TileCount; i++)
            {
                TileElement element = new();
                _playerIDContainerPair[playerID].HandContainer.Add(element);
            }
        }
    }
}
