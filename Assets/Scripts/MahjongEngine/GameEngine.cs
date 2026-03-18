using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace RiichiReign.MahjongEngine
{
    public class GameEngine
    {
        List<Player> _playerList = new();
        int _playerCount;

        private readonly Action<List<Player>> _onGameStart;
        private readonly Action<Player> _onPlayerDataChanged;
        private readonly Action<Player> _onPlayerHandChanged;
        private readonly Action<string> _log;

        public GameEngine(
            List<Player> playerList,
            int playerCount,
            Action<List<Player>> onGameStartResolver,
            Action<Player> onPlayerDataChangedResolver,
            Action<Player> onPlayerHandChangedResolver,
            Action<string> logger
        )
        {
            _playerList = playerList;
            _playerCount = playerCount;
            _onGameStart = onGameStartResolver;
            _onPlayerDataChanged = onPlayerDataChangedResolver;
            _onPlayerHandChanged = onPlayerHandChangedResolver;
            _log = logger;
        }

        public async void StartGame()
        {
            if (_playerList.Count < _playerCount)
                throw new Exception(
                    $"Not enough players to start a game, current player count: {_playerList.Count}"
                );

            _log?.Invoke($"Starting Game with player count: {_playerList.Count}");

            _log?.Invoke($"[{GetType().Name}] Initializing Player...");

            int windValue = 1;
            foreach (var player in _playerList)
            {
                player.SetWindValue(windValue++);
                player.SetPoint(25000);
            }

            _onGameStart?.Invoke(_playerList);

            await GameRoutine();
        }

        public async Task GameRoutine()
        {
            // Turn Loop to be Implement
            {
                // Reset Player Hand
                foreach (var player in _playerList)
                {
                    player.ResetHand();
                }

                await TurnRoutine();
            }
        }

        public async Task TurnRoutine()
        {
            Wall wall = new();
            wall.Initialize();
            wall.Shuffle();

            // Deal Initial Hand
            for (int i = 0; i < 13; i++)
            {
                foreach (var player in _playerList)
                {
                    _log?.Invoke($"[{GetType().Name}] {player} drawing initial tile from wall");
                    player.AddTile(wall);
                    await Task.Delay(100);
                }
            }

            foreach (var player in _playerList)
            {
                StringBuilder sb = new();

                sb.AppendLine($"[{GetType().Name}] OnPlayerHandChanged:");
                sb.AppendLine($"Player: {player}");
                sb.AppendLine($"New hand: {player.Hand}");

                _log?.Invoke(sb.ToString());

                _onPlayerHandChanged?.Invoke(player);
            }
        }
    }
}
