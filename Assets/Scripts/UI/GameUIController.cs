using System.Collections.Generic;
using Newtonsoft.Json;
using RiichiReign.GameComponent;
using RiichiReign.Player;
using RiichiReign.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace RiichiReign.UnityComponent
{
    /// <summary>
    /// Represents a pair of UI containers for a single player's tiles in the game board.
    /// Encapsulates the hand tile display area and the temporary/drawn tile area.
    /// </summary>
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
        public void Clear()
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
    public class GameUIController : NetworkBehaviour
    {
        /// <summary>The local player instance associated with this client.</summary>
        public PlayerInstance LocalPlayer { get; private set; }

        /// <summary>Flag indicating whether tile sorting is currently enabled.</summary>
        private bool isSortingOn = false;

        /// <summary>List of all players in the current game session.</summary>
        private List<PlayerInstance> storedPlayerData;

        /// <summary>Maps each player instance to their corresponding UI container elements for rendering tiles.</summary>
        private Dictionary<PlayerInstance, PlayerContainerElement> _PlayerContainerPairs;

        /// <summary>Reference to the tile sorting toggle UI element.</summary>
        private Toggle _sortTileToggle;

        #region Netcode Logics

        /// <summary>
        /// Called when the networked object spawns on the network.
        /// Initializes the player data, UI containers, and registers event callbacks.
        /// Only processes on the owner client; other clients deactivate this component.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            // Only the owning client should manage this UI controller
            if (!IsOwner)
            {
                gameObject.SetActive(false);
                return;
            }

            // Initialize the local player instance and send it to the server
            LocalPlayer = new();
            SendPlayerDataToServer();

            // Initialize collections for tracking players and their UI containers
            storedPlayerData = new();
            _PlayerContainerPairs = new();

            // Get the root UI element and setup the sorting toggle
            var root = GetComponent<UIDocument>().rootVisualElement;
            _sortTileToggle = root.Q<Toggle>("SortTileToggle");

            // Register callback to track when the player toggles tile sorting
            _sortTileToggle.RegisterValueChangedCallback(evt =>
            {
                isSortingOn = evt.newValue;
            });
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the displayed tile information for all players in the provided list.
        /// Validates that all players in the list exist in the stored player data.
        /// </summary>
        /// <param name="playerList">The list of players whose tile displays should be updated.</param>
        public void UpdatePlayerListData(List<PlayerInstance> playerList)
        {
            if (playerList == null)
            {
                Debug.LogError(
                    $"[{GetType().Name}] UpdatePlayerListData received empty or null player list"
                );

                return;
            }

            foreach (PlayerInstance player in playerList)
            {
                if (!storedPlayerData.Contains(player))
                {
                    Debug.LogError(
                        $"[{GetType().Name}]: Player Not Found in storedPlayerData: " + player,
                        this
                    );
                    return;
                }
            }

            storedPlayerData = playerList;

            UpdatePlayerHandUI();
        }

        /// <summary>
        /// Replaces the entire player list and reinitializes the UI containers for all players.
        /// This method should be called when joining a game or when the player list changes significantly.
        /// </summary>
        /// <param name="playerList">The new list of players in the game session.</param>
        public void UpdatePlayerList(List<PlayerInstance> playerList)
        {
            // Handle null or empty player list
            if (playerList == null)
            {
                Debug.LogWarning(
                    $"[{GetType().Name}] UpdatePlayerList received empty or null player list"
                );
                storedPlayerData = new List<PlayerInstance>();
            }
            else
            {
                storedPlayerData = playerList;
            }

            // Clear existing container mappings to rebuild them
            if (_PlayerContainerPairs.Count != 0)
            {
                Debug.LogWarning(
                    $"[{GetType().Name}] _PlayerContainerPairs is not null, Clearing..."
                );
                _PlayerContainerPairs.Clear();
            }

            // Reinitialize UI containers for all players
            AssignPlayers();

            // Display tiles for all players
            UpdatePlayerHandUI();
        }

        /// <summary>
        /// Assigns UI container elements to each player in the storedPlayerData list.
        /// Normalizes player positions relative to the local player, placing the local player at position 0.
        /// Creates a mapping between player instances and their corresponding UI containers.
        /// </summary>
        public void AssignPlayers()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var table = root.Q<VisualElement>("Table");

            // Find the index of the local player to normalize rotations
            int localPlayerIndex = storedPlayerData.IndexOf(LocalPlayer);

            for (int i = 0; i < storedPlayerData.Count; i++)
            {
                // Normalize the player index relative to the local player
                // This places the local player at index 0 for UI layout purposes
                int normalizedIndex =
                    (i - localPlayerIndex + storedPlayerData.Count) % storedPlayerData.Count;

                // Query the UI hierarchy for containers corresponding to this normalized position
                VisualElement playerContainer = table.Q<VisualElement>(
                    $"PlayerContainer{normalizedIndex}"
                );
                VisualElement handContainer = playerContainer.Q<VisualElement>("HandContainer");
                VisualElement tempTileContainer = playerContainer.Q<VisualElement>(
                    "TempTileContainer"
                );
                PlayerContainerElement containerElement = new(handContainer, tempTileContainer);

                // Debug checks for null references
                if (storedPlayerData[i] == null)
                {
                    Debug.Log("Player is null");
                }

                if (
                    containerElement.HandContainer == null
                    || containerElement.TempTileContainer == null
                )
                {
                    Debug.Log("Container element is null");
                }

                // Map the player to their UI containers
                _PlayerContainerPairs.Add(storedPlayerData[i], containerElement);
            }
        }

        /// <summary>
        /// Updates the visual display of tiles for all players.
        /// Renders the player's hand tiles and any temporarily drawn tile.
        /// For other players, displays hidden tiles (empty Tile objects) to maintain game secrecy.
        /// </summary>
        private void UpdatePlayerHandUI()
        {
            foreach (var player in storedPlayerData)
            {
                Debug.Log(LocalPlayer.ToString() + " is updating hand UI for " + player.ToString());

                // Verify the player has a UI container assigned
                if (!_PlayerContainerPairs.ContainsKey(player))
                {
                    Debug.LogWarning(
                        $"Player {player.playerID} doesnt exist in {LocalPlayer.playerID}"
                    );
                    return;
                }

                // Clear previous tile displays
                _PlayerContainerPairs[player].Clear();

                // Display tiles from the player's hand
                foreach (Tile tile in player.Hand.TilesInHand)
                {
                    TileElement tileElement = new();
                    // Show actual tile data only for the local player; show hidden tiles for others
                    tileElement.Bind((player == LocalPlayer ? tile : new Tile()));
                    _PlayerContainerPairs[player].HandContainer.Add(tileElement);
                }

                // Display the temporarily drawn tile if one exists
                if (player.Hand.CheckTempTile(out Tile tempTile))
                {
                    TileElement tileElement = new();
                    // Show actual tile data only for the local player; show hidden tile for others
                    tileElement.Bind((player == LocalPlayer ? tempTile : new Tile()));
                    _PlayerContainerPairs[player].TempTileContainer.Add(tileElement);
                }
            }
        }

        #endregion

        #region Remote Procedure Calls (RPC)

        /// <summary>
        /// Serializes the local player instance to JSON and sends it to the server.
        /// This is called during initialization to register the player with the game session.
        /// </summary>
        private void SendPlayerDataToServer()
        {
            // Serialize the player instance to JSON format
            string json = JsonConvert.SerializeObject(LocalPlayer);

            // Send the JSON data to the server via RPC
            SendPlayerDataServerRPC(json);
            Debug.Log("Sending data to Server: " + json);
        }

        /// <summary>
        /// Server-side RPC that receives player data from a client and registers the player.
        /// Deserializes the incoming player data and adds it to the PlayerManager.
        /// </summary>
        /// <param name="json">Serialized JSON representation of the player instance.</param>
        [ServerRpc]
        private void SendPlayerDataServerRPC(string json)
        {
            // Deserialize the JSON back into a PlayerInstance object
            PlayerInstance incomingPlayer = JsonConvert.DeserializeObject<PlayerInstance>(json);

            // Validate the deserialized player data
            if (incomingPlayer == null)
                Debug.Log("IsNull");
            else
                // Register the player with the game session manager
                PlayerManager.Instance.AddPlayer(incomingPlayer);
        }

        #endregion
    }
}
