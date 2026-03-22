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

            // Changed this for condition that don't accpet discard tile
            if (true)
            {
                AddDiscardTiles(hand, ref availableActions, hadRiichi);
            }

            if (hadRiichi)
                return availableActions;

            List<Tile> tiles = new(hand.TilesInHand) { hand.TempTile };

            CheckKan(tiles, ref availableActions);

            return availableActions;
        }

        static void AddDiscardTiles(
            PlayerHand hand,
            ref List<GameAction> availableAction,
            bool hadRiichi = false
        )
        {
            GameAction tempTileAction = new(MahjongAction.Discard, hand.TempTile);
            availableAction.Add(tempTileAction);

            if (hadRiichi)
                return;

            foreach (var tile in hand.TilesInHand)
            {
                GameAction action = new(MahjongAction.Discard, tile);
                availableAction.Add(action);
            }
        }

        static void CheckKan(List<Tile> tiles, ref List<GameAction> availableActions)
        {
            var grouped = tiles.GroupBy(t => t);

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
