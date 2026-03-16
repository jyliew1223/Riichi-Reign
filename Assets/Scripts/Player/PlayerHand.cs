using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json;
using RiichiReign.GameComponent;
using RiichiReign.UnityComponent;
using UnityEngine;

namespace RiichiReign.GamePlayer
{
    public class InvalidHandException : Exception { }

    public class PlayerHand
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
        public PlayerHand(Tile tempTile, List<Tile> tileInHand, List<IMeldType> melds)
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
