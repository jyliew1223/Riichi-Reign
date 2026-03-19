using System.Collections.Generic;
using System.Linq;
using RiichiReign.MahjongEngine;
using RiichiReign.UI;
using RiichiReign.UnityComponent;
using UnityEngine;
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
    public class PlayerUIController : MonoBehaviour
    {
        public static PlayerUIController Instance { get; private set; }
        List<PlayerContainerElement> _containerList = new();
        Dictionary<string, PlayerContainerElement> _playerIDContainerPair = new();

        void Start()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            InitializeUIContainers();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
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

            int localWindValue = PlayerBehaviour.LocalPlayerInstance.WindValue;

            List<PlayerBehaviour> playerBehaviours = GameObject
                .FindObjectsByType<PlayerBehaviour>(FindObjectsSortMode.None)
                .ToList();

            foreach (var playerBehaviour in playerBehaviours)
            {
                int localIndex = (playerBehaviour.WindValue - localWindValue + 4) % 4;
                _playerIDContainerPair[playerBehaviour.PlayerID] = _containerList[localIndex];
            }
        }

        public void UpdatePlayerHand(string playerID)
        {
            PlayerBehaviour playerBehaviour = PlayerManager.Instance.PlayerIDBehaviorPair[playerID];
            PlayerContainerElement playerContainer = _playerIDContainerPair[playerID];

            Hand playerHand = playerBehaviour.PlayerHand;

            if (PlayerBehaviour.LocalPlayerInstance.PlayerID == playerID)
            {
                Debug.Log($"[{GetType().Name}] Updating Local Player Hand...");

                if (playerHand is not PlayerHand localPlayerHand)
                {
                    throw new System.Exception(
                        $"[{GetType().Name}] Assigning Non-PlayerHand data to local player!"
                    );
                }
                else
                {
                    if (localPlayerHand.TempTile != null)
                    {
                        TileElement element = new(localPlayerHand.TempTile);
                        _playerIDContainerPair[playerID].TempTileContainer.Add(element);
                    }

                    foreach (var tile in localPlayerHand.TilesInHand)
                    {
                        TileElement element = new(tile);
                        _playerIDContainerPair[playerID].HandContainer.Add(element);
                    }
                }
            }
            else
            {
                Debug.Log($"[{GetType().Name}] Updating Player {playerID} Hand...");

                if (playerHand is not OpponentHand opponentPlayerHand)
                {
                    throw new System.Exception(
                        $"[{GetType().Name}] Assigning PlayerHand data to Non-local player!"
                    );
                }
                else
                {
                    if (opponentPlayerHand.HasTempTile)
                    {
                        TileElement element = new();
                        _playerIDContainerPair[playerID].TempTileContainer.Add(element);
                    }

                    for (int i = 0; i < opponentPlayerHand.TileCount; i++)
                    {
                        TileElement element = new();
                        _playerIDContainerPair[playerID].HandContainer.Add(element);
                    }
                }
            }
        }
    }
}
