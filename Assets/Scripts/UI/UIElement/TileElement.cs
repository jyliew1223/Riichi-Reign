using System.Diagnostics;
using RiichiReign.GameComponent;
using UnityEngine.UIElements;

namespace RiichiReign.UI
{
    public class TileElement : VisualElement
    {
        public Tile BoundedTile { get; private set; }
        public static System.Action<TileElement> OnTileClicked;
        public static TileElement selectedTile;

        public TileElement()
        {
            AddToClassList("tile");
        }

        public void Bind(Tile tile)
        {
            BoundedTile = tile;

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

            if (BoundedTile.Type == TileType.Invisible)
                return;

            RegisterCallback<ClickEvent>(evt =>
            {
                HandleOnClicked();
            });
            RegisterCallback<PointerEnterEvent>(evt =>
            {
                HandleOnPointerEnter();
            });
            RegisterCallback<PointerLeaveEvent>(evt =>
            {
                HandleOnPointerLeave();
            });
        }

        private void HandleOnClicked()
        {
            if (ClassListContains("tile--up"))
            {
                OnTileClicked?.Invoke(this);
                return;
            }

            if (selectedTile != this)
            {
                selectedTile?.RemoveFromClassList("tile--up");

                selectedTile = this;
                AddToClassList("tile--up");
            }
        }

        private void HandleOnPointerEnter()
        {
            if (!ClassListContains("tile--up"))
                AddToClassList("tile--up");
        }

        private void HandleOnPointerLeave()
        {
            if (ClassListContains("tile--up"))
                RemoveFromClassList("tile--up");
        }
    }
}
