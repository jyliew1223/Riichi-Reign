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

namespace RiichiReign.Player
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

        #region PlayerAction Checks

        public List<PlayerAction> CheckAvailableAction(TurnPhase currentTurn = 0)
        {
            if (!IsValidHand())
            {
                throw new InvalidHandException();
            }

            List<PlayerAction> availableActions = new();

            if (currentTurn == TurnPhase.StartTurn)
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

                if (terminalCount >= 9)
                {
                    availableActions.Add(new(GameAction.Kyushukyuhai));
                    availableActions.Add(new(GameAction.Skip));
                }

                // Terminate immediately if user can declare Kyushukyuhai
                return availableActions;
            }

            availableActions.Add(new(GameAction.Discard));

            return availableActions;
        }

        private bool CheckRon(List<Tile> hand)
        {
            if (hand.Count != 14)
            {
                Debug.Log(
                    $"[{GetType().Name}] Cannot Declare Ron without 14 tiles in hand, current hand: \n{hand}"
                );
            }

            // Group by your Equals logic
            var groups = hand.GroupBy(t => t);

            foreach (var group in groups)
            {
                int count = group.Count();
                Tile tileType = group.Key;

                // remove pair for every possible combination and check is it possible to declare Ron
                if (count >= 2)
                {
                    List<Tile> tilesLeft = hand.ToList();
                    tilesLeft.Remove(tileType);
                    tilesLeft.Remove(tileType);
                    return CheckRonRecursive(tilesLeft);
                }
            }

            return false;
        }

        private bool CheckRonRecursive(List<Tile> tilesLeft)
        {
            if (tilesLeft == null || tilesLeft.Count == 0)
                return true;

            return false;
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
