using System.Collections;
using System.Collections.Generic;
using RiichiReign.GameComponent;
using RiichiReign.GamePlayer;
using UnityEditor.Rendering;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    public class GameManager : MonoBehaviour
    {
        private static WaitForSeconds _waitForSeconds_5 = new WaitForSeconds(.5f);

        [SerializeField]
        int _playerCount = 3;

        public static GameManager Instance;

        List<Player> _playerList = new();

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

        public Player AddPlayer(ulong NetworkID)
        {
            if (_playerList.Count >= _playerCount)
                throw new System.Exception("Player Count Exceed!");

            Debug.Log($"[{GetType().Name}] Adding Player with ID {NetworkID}");
            Player player = new(NetworkID);
            _playerList.Add(player);

            return player;
        }

        public void StartGame()
        {
            if (_playerList.Count < 3)
                throw new System.Exception(
                    $"Not enough players to start a game, current player count: {_playerList.Count}"
                );

            Debug.Log($"Starting Game with player count: {_playerList.Count}");

            Debug.Log($"[{GetType().Name}] Initializing Player...");

            int windValue = 1;
            foreach (var player in _playerList)
            {
                player.SetWindValue(windValue++);
                player.SetPoint(25000);
            }

            StartCoroutine(GameRoutine());
        }

        public IEnumerator GameRoutine()
        {
            // Turn Loop to be Implement
            {
                // Reset Player Hand
                foreach (var player in _playerList)
                {
                    player.ResetHand();
                }

                yield return TurnRoutine();
            }
        }

        public IEnumerator TurnRoutine()
        {
            Wall wall = new();
            wall.Initialize();
            wall.Shuffle();

            // Deal Initial Hand
            for (int i = 0; i < 13; i++)
            {
                foreach (var player in _playerList)
                {
                    Debug.Log($"[{GetType().Name}] {player} drawing initial tile from wall");
                    player.DrawTile(wall);
                    yield return _waitForSeconds_5;
                }
            }
        }
    }
}
