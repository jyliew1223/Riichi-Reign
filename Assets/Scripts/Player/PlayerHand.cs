using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Newtonsoft.Json;
using RiichiReign.GameComponent;
using Unity.Netcode;
using UnityEngine.XR;

namespace RiichiReign.Player
{
    public enum PlayerAction
    {
        None,
        Skip,
        Draw,
        Discard,
        Riichi,
        ClosedKan,
        AddedKan,
        Tsumo,
        Kyushukyuhai,
    }

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

        #region Methods

        #region Tile Management
        public void AddTile(Tile tile) => TilesInHand.Add(tile);

        public void RemoveTile(Tile tile)
        {
            if (!TilesInHand.Remove(tile))
            {
                throw new System.ArgumentException(
                    "The specified tile is not in the player's hand."
                );
            }
        }

        public void DrawTile(Pool pool) => TempTile = pool.DrawTile();

        #endregion

        #region Meld Management

        public void AddMeld(IMeldType meld) => Melds.Add(meld);

        #endregion

        #region Validation

        public bool IsValidHand() => TotalTilesCount() == 13;

        #endregion

        #region Querying

        public int TotalTilesCount() => TilesInHand.Count + Melds.Count * 3;

        public bool CheckTempTile(out Tile tile)
        {
            tile = null;

            if (TempTile == null)
                return false;

            tile = TempTile;
            return true;
        }

        #endregion

        #region PlayerAction Checker

        public PlayerAction CheckKyushukyuhai()
        {
            // Kyushukyuhai
            int terminalCount = 0;

            foreach (Tile tile in TilesInHand)
            {
                if (tile.IsTerminal())
                {
                    terminalCount++;
                }
            }

            return terminalCount >= 9 ? PlayerAction.Kyushukyuhai : PlayerAction.None;
        }

        #endregion

        #endregion

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
