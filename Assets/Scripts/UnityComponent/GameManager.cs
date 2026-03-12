using RiichiReign.Player;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    internal class GameManager : MonoBehaviour
    {
        [SerializeField]
        bool _startGame = false;

        public static GameManager Instance { get; private set; }
        public bool HasGameEnded { get; private set; } = false;

        #region Unity Logics

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }

            Instance = this;
        }

        void Update()
        {
            if (_startGame)
            {
                _startGame = false;
                StartGameLoop();
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Game Loop Logic

        #region Start Game Logics

        void StartGameLoop()
        {
            // Initialize players
            InitPlayers();
            PlayerInstance winner = TurnManager.Instance.StartTurn();
        }

        void InitPlayers()
        {
            if (PlayerManager.Server.IsReady)
                PlayerManager.Server.InitializePlayer();
            else
                Debug.LogError(
                    $"[{GetType().Name}] Getting PlayerInstance while it's not Ready",
                    this
                );
        }

        #endregion

        #endregion
    }
}
