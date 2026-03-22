using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace RiichiReign.MahjongEngine
{
    [System.Serializable]
    public enum MahjongAction
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

    [System.Serializable]
    public class GameAction
    {
        [JsonProperty("action")]
        public MahjongAction Action { get; private set; }

        [JsonProperty("relatedTile")]
        public List<Tile> RelatedTile { get; private set; }

        public GameAction(MahjongAction action, List<Tile> relatedTile = null)
        {
            Action = action;
            RelatedTile = relatedTile;
        }

        public GameAction(MahjongAction action, Tile tile)
        {
            Action = action;
            RelatedTile = new() { tile };
        }

        [JsonConstructor]
        private GameAction(List<Tile> relatedTile, MahjongAction action)
        {
            Action = action;
            RelatedTile = relatedTile;
        }
    }
}
