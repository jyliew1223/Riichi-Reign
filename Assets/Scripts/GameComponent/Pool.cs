using System;
using System.Collections.Generic;
using System.Linq;
using RiichiReign.GameComponent;

namespace RiichiReign.GameComponent
{
    public class Pool
    {
        Stack<Tile> tiles;

        #region Constructors

        public Pool()
        {
            tiles = new Stack<Tile>();
        }

        #endregion

        #region Public Methods

        public void InitializePool()
        {
            // Clear the pool before initializing
            tiles.Clear();

            // Initialize the pool with the standard 136 tiles
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j <= 9; j++)
                {
                    tiles.Push(new Tile(TileType.Manzu, j, i == 0 && j == 5)); // Red Dora for 5
                    tiles.Push(new Tile(TileType.Pinzu, j, i == 0 && j == 5)); // Red Dora for 5
                    tiles.Push(new Tile(TileType.Souzu, j, i == 0 && j == 5)); // Red Dora for 5
                }
                tiles.Push(new Tile(TileType.Wind, 1)); // Ton
                tiles.Push(new Tile(TileType.Wind, 2)); // Nan
                tiles.Push(new Tile(TileType.Wind, 3)); // Shaa
                tiles.Push(new Tile(TileType.Wind, 4)); // Pei
                tiles.Push(new Tile(TileType.Dragon, 1)); // Haku
                tiles.Push(new Tile(TileType.Dragon, 2)); // Hatsu
                tiles.Push(new Tile(TileType.Dragon, 3)); // Chun
            }
        }

        public void Shuffle()
        {
            var rnd = new Random();
            tiles = new Stack<Tile>(tiles.OrderBy(x => rnd.Next()));
        }

        public Tile DrawTile()
        {
            if (tiles.Count > 0)
            {
                return tiles.Pop();
            }
            else
            {
                throw new InvalidOperationException("The pool is empty.");
            }
        }

        public int RemainingTiles()
        {
            return tiles.Count;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            string output = "";
            output += "Remaining Tiles in Pool:\n";

            foreach (var tile in tiles)
            {
                output += tile.ToString() + "\n";
            }

            return output.ToString();
        }

        #endregion
    }
}
