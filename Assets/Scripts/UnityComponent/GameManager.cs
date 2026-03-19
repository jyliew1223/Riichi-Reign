using System.Collections.Generic;
using System.Text;
using RiichiReign.MahjongEngine;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    public class GameManager : MonoBehaviour
    {
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

        public Player AddPlayer(string playerID)
        {
            if (_playerList.Count >= _playerCount)
                throw new System.Exception("Player Count Exceed!");

            Debug.Log($"[{GetType().Name}] Adding Player with ID {playerID}");

            Player player = new(playerID);
            _playerList.Add(player);

            return player;
        }

        public void StartGame()
        {
            GameEngine game = new(
                _playerList,
                _playerCount,
                HandleOnGameStart,
                HandleOnPlayerDataChanged,
                HandleOnPlayerHandChanged,
                Debug.Log
            );
            game.StartGame();
        }

        public void HandleOnGameStart(List<Player> playerList)
        {
            Debug.Log($"[{GetType().Name}] Resolving OnGameStart...");

            ServerManager.Instance.InitializeClientGame(playerList);
        }

        public void HandleOnPlayerDataChanged(Player player)
        {
            throw new System.NotImplementedException();
        }

        public void HandleOnPlayerHandChanged(Player player)
        {
            StringBuilder sb = new();

            sb.AppendLine($"[{GetType().Name}] Resolving OnPlayerHandChanged...");
            sb.AppendLine($"Player ID: {player.PlayerID}");
            sb.AppendLine($"New hand: {player.Hand}");

            Debug.Log(sb.ToString());

            ServerManager.Instance.SyncPlayerHand(player);
        }
    }
}
