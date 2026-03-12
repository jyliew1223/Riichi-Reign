using System.Collections.Generic;
using RiichiReign.GameComponent;

namespace RiichiReign.Player
{
    public enum GameAction
    {
        None,
        Skip,
        Discard,
        Pon,
        ClosedKan,
        AddedKan,
        Riichi,
        Tsumo,
        Kyushukyuhai,
    }

    public class PlayerAction
    {
        public GameAction Action { get; private set; }
        public List<Tile> RelatedTile { get; private set; }

        public PlayerAction(GameAction action, List<Tile> relatedTile = null)
        {
            Action = action;
            RelatedTile = relatedTile;
        }
    }
}
