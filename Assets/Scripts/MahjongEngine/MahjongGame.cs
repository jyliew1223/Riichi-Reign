using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiichiReign.MahjongEngine
{
    public class MahjongGame
    {
        GameEventHub _eventHub;
        List<Player> _playerList = new();
        int _playerCount;

        event Action<List<Player>> _onGameInit;
        event Action<string> _log;

        public MahjongGame(GameEventHub eventHub)
        {
            _eventHub = eventHub;
            _onGameInit = _eventHub.RaiseGameInit;
            _log = _eventHub.RaiseLog;
        }

        public void InitGame(List<Player> playerList, int playerCount)
        {
            if (playerList.Count < _playerCount)
                throw new Exception(
                    $"Not enough players to init a game, current player count: {_playerList.Count}"
                );

            _playerList = playerList;
            _playerCount = playerCount;

            _log?.Invoke($"[{GetType().Name}] Initializing Player...");

            int windValue = 1;
            foreach (var player in _playerList)
            {
                player.SetWindValue(windValue++);
                player.SetPoint(25000);
            }

            _onGameInit?.Invoke(_playerList);
        }

        public async void StartGame()
        {
            _log?.Invoke($"Starting Game with player count: {_playerList.Count}");
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

                MahjongRound round = new(_eventHub);
                round.InitRound(_playerList, _playerCount);
                await round.StartRound();
                await round.TurnRoutine();
            }
        }
    }
}
