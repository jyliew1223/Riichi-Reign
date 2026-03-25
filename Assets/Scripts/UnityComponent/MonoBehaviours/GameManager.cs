using System.Collections.Generic;
using RiichiReign.MahjongEngine;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        int _playerCount = 3;

        public static GameManager Instance;

        GameEventHub eventHub;
        MahjongGame game;
        List<Player> _playerList = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void OnDestroy()
        {
            // make sure unsubcribe all subscription
            game = null;

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
            eventHub = new();
            ServerManager.Instance.AttachGameEventHub(eventHub);
            game = new(eventHub);

            game.InitGame(_playerList, _playerCount);
            game.StartGame();
        }
    }
}
