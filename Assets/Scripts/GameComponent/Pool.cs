using System;
using System.Collections.Generic;
using System.Linq;

namespace RiichiReign.GameComponent
{
    public class Pool
    {
        Stack<Tile> _tiles;

        #region Constructors

        public Pool()
        {
            _tiles = new Stack<Tile>();
        }

        #endregion

        #region Public Methods

        public void InitializePool()
        {
            // Clear the pool before initializing
            _tiles.Clear();

            // Initialize the pool with the standard 136 _tiles
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j <= 9; j++)
                {
                    _tiles.Push(new Tile(TileType.Manzu, j, i == 0 && j == 5)); // Red Dora for 5
                    _tiles.Push(new Tile(TileType.Pinzu, j, i == 0 && j == 5)); // Red Dora for 5
                    _tiles.Push(new Tile(TileType.Souzu, j, i == 0 && j == 5)); // Red Dora for 5
                }
                _tiles.Push(new Tile(TileType.Wind, 1)); // Ton
                _tiles.Push(new Tile(TileType.Wind, 2)); // Nan
                _tiles.Push(new Tile(TileType.Wind, 3)); // Shaa
                _tiles.Push(new Tile(TileType.Wind, 4)); // Pei
                _tiles.Push(new Tile(TileType.Dragon, 1)); // Haku
                _tiles.Push(new Tile(TileType.Dragon, 2)); // Hatsu
                _tiles.Push(new Tile(TileType.Dragon, 3)); // Chun
            }
        }

        public void Shuffle()
        {
            var rnd = new Random();
            _tiles = new Stack<Tile>(_tiles.OrderBy(x => rnd.Next()));
        }

        public Tile DrawTile()
        {
            if (_tiles.Count > 0)
            {
                return _tiles.Pop();
            }
            else
            {
                throw new InvalidOperationException("The pool is empty.");
            }
        }

        public int RemainingTiles()
        {
            return _tiles.Count;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            string output = "";
            output += "Remaining Tiles in Pool:\n";

            foreach (var tile in _tiles)
            {
                output += tile.ToString() + "\n";
            }

            return output.ToString();
        }

        #endregion
    }
}
