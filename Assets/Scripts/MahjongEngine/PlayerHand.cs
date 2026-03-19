using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace RiichiReign.MahjongEngine
{
    [Serializable]
    public abstract class Hand { }

    [Serializable]
    public class OpponentHand : Hand
    {
        [JsonProperty("playerID")]
        public string PlayerID;

        [JsonProperty("tileCount")]
        public int TileCount;

        [JsonProperty("hasTempTile")]
        public bool HasTempTile;

        public OpponentHand(string playerID, PlayerHand hand)
        {
            PlayerID = playerID;
            TileCount = hand.TilesInHand.Count;
            HasTempTile = hand.TempTile != null;
        }

        [JsonConstructor]
        private OpponentHand(string playerID, int tileCount, bool hasTempTile)
        {
            PlayerID = playerID;
            TileCount = tileCount;
            HasTempTile = hasTempTile;
        }
    }

    [Serializable]
    public class PlayerHand : Hand
    {
        [JsonProperty("tempTile")]
        public Tile TempTile { get; private set; }

        [JsonProperty("tileInHand")]
        public List<Tile> TilesInHand { get; private set; }

        [JsonProperty("melds")]
        public List<IMeldType> Melds { get; private set; }

        #region Constructors

        public PlayerHand()
        {
            TilesInHand = new List<Tile>();
            Melds = new List<IMeldType>();
            TempTile = null;
        }

        [JsonConstructor]
        private PlayerHand(Tile tempTile, List<Tile> tileInHand, List<IMeldType> melds)
        {
            TempTile = tempTile;
            TilesInHand = tileInHand ?? new List<Tile>();
            Melds = melds ?? new List<IMeldType>();
        }

        #endregion

        public void AddTile(Tile tile)
        {
            TilesInHand.Add(tile);
        }

        public void DrawTile(Tile tile)
        {
            TempTile = tile;
        }

        #region Overrides

        public override string ToString()
        {
            StringBuilder handRepresentation = new StringBuilder();
            handRepresentation.AppendLine("Tiles in Hand:");

            handRepresentation.AppendLine("Total Tiiles: " + (TilesInHand.Count + Melds.Count * 3));

            foreach (var tile in TilesInHand)
            {
                handRepresentation.AppendLine(tile.ToString());
            }
            handRepresentation.AppendLine("Melds:");
            foreach (var meld in Melds)
            {
                handRepresentation.AppendLine(meld.ToString());
            }

            return handRepresentation.ToString();
        }

        #endregion
    }
}
