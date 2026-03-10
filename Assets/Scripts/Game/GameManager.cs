using System.Collections.Generic;
using RiichiReign.GameComponent;
using RiichiReign.Player;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        bool StartGame = false;

        public static GameManager Instance { get; private set; }
        public bool HasGameEnded { get; private set; } = false;

        Pool pool;
        List<PlayerInstance> players;

        #region Unity Logics

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }

            Instance = this;
        }

        void Start()
        {
            players = new();
        }

        void Update()
        {
            if (StartGame)
            {
                StartGame = false;
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

        void StartGameLoop()
        {
            // Initialize players
            InitPlayers();

            PlayerInstance winner = TurnManager.Instance.StartTurn(players);
        }

        #endregion

        #region Start Game Logics

        void InitPlayers()
        {
            PlayerManager.Instance.StartInitializePlayersRPC();
        }

        #endregion
    }
}
