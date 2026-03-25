using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace RiichiReign.MahjongEngine
{
    public static class HandEvaluator
    {
        public static List<GameAction> EvaluateHand(PlayerHand hand)
        {
            bool hadRiichi = false;
            List<GameAction> availableActions = new();

            if (hadRiichi)
            {
                GameAction tempTileAction = new(MahjongAction.Discard, hand.TempTile);
                availableActions.Add(tempTileAction);

                return availableActions;
            }

            // Changed this for condition that don't accpet discard tile
            if (true)
            {
                AddDiscardTiles(hand.TilesInHand, ref availableActions);
            }

            List<Tile> fullHand = new(hand.TilesInHand) { hand.TempTile };

            CheckKan(fullHand, ref availableActions);

            return availableActions;
        }

        static void AddDiscardTiles(List<Tile> tilesInHand, ref List<GameAction> availableActions)
        {
            foreach (var tile in tilesInHand)
            {
                GameAction action = new(MahjongAction.Discard, tile);
                availableActions.Add(action);
            }
        }

        static void CheckKan(List<Tile> fullHand, ref List<GameAction> availableActions)
        {
            var grouped = fullHand.GroupBy(t => t);

            foreach (var group in grouped)
            {
                if (group.Count() >= 4)
                {
                    GameAction action = new(MahjongAction.ClosedKan, group.ToList());
                    availableActions.Add(action);
                }
            }
        }
    }
}
