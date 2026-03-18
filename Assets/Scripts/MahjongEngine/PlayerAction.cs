using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace RiichiReign.MahjongEngine
{
    [System.Serializable]
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

    [System.Serializable]
    public class PlayerAction
    {
        [JsonProperty("action")]
        public GameAction Action { get; private set; }

        [JsonProperty("relatedTile")]
        public List<Tile> RelatedTile { get; private set; }

        public PlayerAction(GameAction action, List<Tile> relatedTile = null)
        {
            Action = action;
            RelatedTile = relatedTile;
        }

        public PlayerAction(GameAction action, Tile tile)
        {
            Action = action;
            RelatedTile = new() { tile };
        }

        [JsonConstructor]
        private PlayerAction(List<Tile> relatedTile, GameAction action)
        {
            Action = action;
            RelatedTile = relatedTile;
        }

        public void SetDiscardingTile(Tile tile)
        {
            if (Action != GameAction.Discard)
                UnityEngine.Debug.Log(
                    $"[{GetType().Name}] Cannot set discarding tile for other Game Actions"
                );

            RelatedTile = new() { tile };
        }

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.AppendLine($"Player Action: {Action}");
            sb.AppendLine($"Related Tiles: {RelatedTile}");

            return sb.ToString();
        }
    }
}
