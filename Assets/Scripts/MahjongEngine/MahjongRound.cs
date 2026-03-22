using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.PlasticSCM.Editor.WebApi;

namespace RiichiReign.MahjongEngine
{
    internal class MahjongRound
    {
        Wall wall;
        int _playerCount;
        Dictionary<int, Player> _windValuePlayerPair = new();
        event Action<List<GameAction>, string> _onWaitingPlayerReaction;
        event Action<Player> _onPlayerHandChanged;
        event Action<string> _log;

        public MahjongRound(
            Action<List<GameAction>, string> _onWaitingPlayerReaction,
            Action<Player> onPlayerHandChangedResolver,
            Action<string> logger
        )
        {
            _onPlayerHandChanged = onPlayerHandChangedResolver;
            _log = logger;
        }

        public void InitRound(List<Player> playerList, int playerCount)
        {
            foreach (var player in playerList)
            {
                if (_windValuePlayerPair.ContainsKey(player.WindValue))
                    throw new Exception($"[{GetType().Name}] Duplicated wind value found!");

                _windValuePlayerPair[player.WindValue] = player;
            }
            _playerCount = playerCount;

            wall = new();
            wall.Initialize();
            wall.Shuffle();
        }

        public async Task StartRound()
        {
            // Deal Initial Hand
            for (int i = 0; i < 13; i++)
            {
                for (int j = 1; j <= _playerCount; j++)
                {
                    Player player = _windValuePlayerPair[j];

                    _log?.Invoke($"[{GetType().Name}] {player} drawing initial tile from wall");
                    player.AddTile(wall);
                    _onPlayerHandChanged?.Invoke(player);
                    await Task.Delay(100);
                }
            }
        }

        enum TurnPhase
        {
            DrawPhase,
            EvaluationPhase,
            EndPhase,
        }

        public async Task TurnRoutine()
        {
            int maxTurn = 10;
            int currentWind = 1;

            while (true)
            {
                if (maxTurn < 0)
                {
                    _log?.Invoke($"[{GetType().Name}] Break Turn loop due to max turn limit");
                    break;
                }

                Player currentPlayer = _windValuePlayerPair[currentWind];
                int nextWind = ((currentWind + 1) % _playerCount) + 1;

                StringBuilder sb = new();

                sb.AppendLine($"[{GetType().Name}] TurnRoutineLoop:");
                sb.AppendLine($"Current Player: {currentPlayer}");
                sb.AppendLine($"Current Wind: {currentWind}");

                _log?.Invoke(sb.ToString());

                await MinorTurnRoutine(currentPlayer, (value) => nextWind = value);

                currentWind = nextWind;
                maxTurn--;
            }
        }

        public async Task MinorTurnRoutine(Player currentPlayer, Action<int> nextWindOverwrite)
        {
            int maxTurn = 100;
            TurnPhase currentPhase = TurnPhase.DrawPhase;

            while (currentPhase != TurnPhase.EndPhase)
            {
                if (maxTurn < 0)
                {
                    _log?.Invoke($"[{GetType().Name}] Break Turn loop due to max turn limit");
                    break;
                }

                await Task.Delay((int)(DebugTimeControl.Instance.m_turnTimeLoop * 1000));

                switch (currentPhase)
                {
                    case TurnPhase.DrawPhase:

                        DrawPhase(currentPlayer);
                        break;

                    case TurnPhase.EvaluationPhase:

                        List<GameAction> availableActions = EvaluateHand(currentPlayer);
                        _onWaitingPlayerReaction?.Invoke(availableActions, currentPlayer.PlayerID);

                        break;

                    case TurnPhase.EndPhase:

                        break;

                    default:
                        throw new Exception(
                            $"[{GetType().Name}] Unknown TurnPhase: {currentPhase}"
                        );
                }

                currentPhase++;
                maxTurn--;
            }
        }

        public void DrawPhase(Player player)
        {
            player.DrawTile(wall);
            _onPlayerHandChanged?.Invoke(player);
        }

        public List<GameAction> EvaluateHand(Player player)
        {
            PlayerHand hand = player.Hand;
            return HandEvaluator.EvaluateHand(hand);
        }
    }
}
