using System;
using System.Collections.Generic;

namespace RiichiReign.MahjongEngine
{
    public class GameEventHub
    {
        #region MahjongEngine

        public event Action<List<Player>> OnGameInit;
        public event Action<Player> OnPlayerDataChanged;
        public event Action<List<GameAction>, string> OnRequestPlayerAction;
        public event Action<Player> OnPlayerHandChanged;
        public event Action<string> OnLog;

        public void RaiseGameInit(List<Player> players) => OnGameInit?.Invoke(players);

        public void RaisePlayerDataChanged(Player player) => OnPlayerDataChanged?.Invoke(player);

        public void RaiseRequestPlayerReaction(List<GameAction> actions, string playerId) =>
            OnRequestPlayerAction?.Invoke(actions, playerId);

        public void RaisePlayerHandChanged(Player player) => OnPlayerHandChanged?.Invoke(player);

        public void RaiseLog(string message) => OnLog?.Invoke(message);

        #endregion

        #region UnityEngine

        public event Action<string, GameAction> OnPlayerResponse;

        public void RaisePlayerResponse(string playerID, GameAction playerAction)
        {
            OnPlayerResponse?.Invoke(playerID, playerAction);
        }

        #endregion
    }
}
