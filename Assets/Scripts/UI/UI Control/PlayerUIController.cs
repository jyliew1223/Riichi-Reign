using System.Collections;
using System.Collections.Generic;
using RiichiReign.GameComponent;
using RiichiReign.Player;
using RiichiReign.UI;
using RiichiReign.UnityComponent;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace RiichiReign.UnityUIToolKitComponent
{
    struct PlayerContainerElement
    {
        /// <summary>Container for displaying the player's hand tiles.</summary>
        public VisualElement HandContainer { get; private set; }

        /// <summary>Container for displaying the player's temporarily drawn tile.</summary>
        public VisualElement TempTileContainer { get; private set; }

        /// <summary>
        /// Initializes a new PlayerContainerElement with the specified hand and temp tile containers.
        /// </summary>
        /// <param name="handContainer">The visual element that displays tiles in the player's hand.</param>
        /// <param name="tempTileContainer">The visual element that displays the temporarily drawn tile.</param>
        public PlayerContainerElement(VisualElement handContainer, VisualElement tempTileContainer)
        {
            HandContainer = handContainer;
            TempTileContainer = tempTileContainer;
            Clear();
        }

        /// <summary>
        /// Clears all child elements from both the hand and temp tile containers.
        /// </summary>
        public readonly void Clear()
        {
            HandContainer.Clear();
            TempTileContainer.Clear();
        }
    }

    /// <summary>
    /// Manages the game UI for displaying player hands and game state in a networked multiplayer environment.
    /// This controller is responsible for synchronizing the local player's UI with the game state,
    /// handling tile displays, and managing player-specific UI interactions.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class PlayerUIController : NetworkBehaviour
    {
        public ulong LocalNetworkObjectID => this.NetworkObjectId;

        int _localWindValue;
        PlayerInstance _localPlayer;
        Dictionary<PlayerInstance, PlayerContainerElement> _playerContainerPairs = new();
        PlayerAction _selectedAction;

        #region Netcode Logics

        public override void OnNetworkSpawn()
        {
            // Check authority
            if (!IsOwner)
            {
                gameObject.SetActive(false);
                return;
            }

            // Clear placeholder tiles from the UXML at the start
            InitializeUIContainers();

            // Initialize the local player instance and send it to the server
            Invoke(nameof(RegisterPlayer), 1f);

            TileElement.OnTileClicked += HandleOnTileClick;
        }

        public override void OnNetworkDespawn()
        {
            TileElement.OnTileClicked -= HandleOnTileClick;
        }

        #endregion

        #region Methods

        #region Public Methods

        public void AssignPlayer(PlayerInstance player, int playerWindValue)
        {
            if (!IsOwner)
            {
                gameObject.SetActive(false);
                return;
            }

            _localPlayer = player;
            _localWindValue = playerWindValue;
            Debug.Log($"[{GetType().Name}] _localPlayer assinged: {_localPlayer}");
        }

        public void AssignPlayerUIs(List<PlayerInstance> playerList)
        {
            if (!IsOwner)
                return;

            if (_localPlayer == null)
            {
                Debug.LogWarning($"[{GetType().Name}] _localPlayer is null", this);
                return;
            }

            var root = GetComponent<UIDocument>().rootVisualElement;

            if (root == null)
                Debug.Log($"[{GetType().Name}] root is null");

            var table = root.Q<VisualElement>("Table");

            for (int i = 0; i < playerList.Count; i++)
            {
                // Normalize the player index relative to the local player
                // This places the local player at index 0 for UI layout purposes
                int normalizedIndex = ((i - _localWindValue + 4 + 1) % 4) + 1;

                // Query the UI hierarchy for containers corresponding to this normalized position
                VisualElement playerContainer = table.Q<VisualElement>(
                    $"PlayerContainer{normalizedIndex}"
                );

                VisualElement handContainer = playerContainer.Q<VisualElement>("HandContainer");
                VisualElement tempTileContainer = playerContainer.Q<VisualElement>(
                    "TempTileContainer"
                );
                PlayerContainerElement containerElement = new(handContainer, tempTileContainer);

                if (
                    containerElement.HandContainer == null
                    || containerElement.TempTileContainer == null
                )
                {
                    Debug.Log("Container element is null");
                }

                // Map the player to their UI containers
                _playerContainerPairs.Add(playerList[i], containerElement);
            }
        }

        public void UpdatePlayerHandUI(List<PlayerInstance> playerList)
        {
            if (_localPlayer == null)
            {
                Debug.LogWarning($"[{GetType().Name}] _localPlayer is null", this);
                return;
            }

            foreach (var player in playerList)
            {
                // Verify the player has a UI container assigned
                if (!_playerContainerPairs.ContainsKey(player))
                {
                    Debug.LogWarning(
                        $"[{GetType().Name}] Player {player} doesnt stored locally",
                        this
                    );
                    continue; // Skip this player instead of returning
                }

                // Clear previous tile displays
                _playerContainerPairs[player].Clear();

                // Display tiles from the player's hand
                foreach (Tile tile in player.Hand.TilesInHand)
                {
                    TileElement tileElement = new();
                    // Show actual tile data only for the local player; show hidden tiles for others
                    tileElement.Bind((player == _localPlayer ? tile : new Tile()));
                    _playerContainerPairs[player].HandContainer.Add(tileElement);
                }

                // Display the temporarily drawn tile if one exists
                if (player.Hand.CheckTempTile(out Tile tempTile))
                {
                    TileElement tileElement = new();
                    // Show actual tile data only for the local player; show hidden tile for others
                    tileElement.Bind((player == _localPlayer ? tempTile : new Tile()));
                    _playerContainerPairs[player].TempTileContainer.Add(tileElement);
                }
            }
        }

        #endregion

        #region  Private Methods

        private void InitializeUIContainers()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            if (root == null)
            {
                Debug.LogError($"[{GetType().Name}] Failed to get root visual element");
                return;
            }

            var table = root.Q<VisualElement>("Table");
            if (table == null)
            {
                Debug.LogError($"[{GetType().Name}] Failed to get Table element");
                return;
            }

            // Initialize all player containers (assuming 4 players: 0-3)
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

                // Create a placeholder entry to track containers for clearing later
                PlayerContainerElement containerElement = new(handContainer, tempTileContainer);
                containerElement.Clear();
            }
        }

        private void RegisterPlayer()
        {
            PlayerManager.Local.RegisterPlayerUI(this);
            ReactionManager.Local.RegisterPlayerUI(this);

            Debug.Log(
                $"[{GetType().Name}] Sending player network id to Server: {LocalNetworkObjectID}"
            );

            PlayerManager.Local.SendPlayerDataServerRPC(LocalNetworkObjectID);
        }

        #endregion

        #region Player Actions Methods

        private bool _isTileClickable = false;

        public IEnumerator PrompForPlayerInputRoutine(System.Action<PlayerAction> selectedAction)
        {
            _selectedAction = null;

            List<PlayerAction> availableAction = _localPlayer.Hand.CheckAvailableAction();

            _isTileClickable = availableAction.Exists(x => x.Action == GameAction.Discard);

            while (_selectedAction == null && ReactionManager.Local.IsWaitingResponse.Value)
            {
                yield return null;
            }

            selectedAction?.Invoke(_selectedAction);

            _isTileClickable = false;
            _selectedAction = null;
        }

        private
        #endregion

        #region Event Listener

        void HandleOnTileClick(TileElement tileElement)
        {
            if (!_isTileClickable && _selectedAction != null)
                return;

            _selectedAction = new(GameAction.Discard, tileElement.BoundedTile);
        }

        #endregion

        #endregion
    }
}
