using RiichiReign.GameComponent;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;

namespace RiichiReign.UI
{
    public class TileElement : VisualElement
    {
        public Tile BoundTile { get; private set; }

        public TileElement()
        {
            AddToClassList("tile");
        }

        public void Bind(Tile tile)
        {
            BoundTile = tile;

            VisualElement tileBackground = new();
            VisualElement tileFace = new();

            tileBackground.AddToClassList("tile-background");
            tileFace.AddToClassList("tile-face");

            tileBackground.style.backgroundImage = new StyleBackground(
                TileTextureManager.Instance.GetTileBackground(tile)
            );
            tileFace.style.backgroundImage = new StyleBackground(
                TileTextureManager.Instance.GetTileFront(tile)
            );

            if (tile.Type == TileType.Invisible)
            {
                tileFace.style.backgroundSize = new BackgroundSize(
                    new Length(100, LengthUnit.Percent),
                    new Length(100, LengthUnit.Percent)
                );
            }

            Add(tileBackground);
            Add(tileFace);
        }
    }
}
