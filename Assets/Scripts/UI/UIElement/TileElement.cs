using RiichiReign.MahjongEngine;
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
            BindFliped();
        }

        public TileElement(Tile tile)
        {
            AddToClassList("tile");
            Bind(tile);
        }

        public void Bind(Tile tile)
        {
            Clear();

            BoundedTile = tile;

            VisualElement tileBackground = new();
            VisualElement tileFace = new();

            tileBackground.AddToClassList("tile-background");
            tileFace.AddToClassList("tile-face");

            tileBackground.style.backgroundImage = new StyleBackground(
                TileTextureManager.Instance.GetTileBackground()
            );
            tileFace.style.backgroundImage = new StyleBackground(
                TileTextureManager.Instance.GetTileFront(tile)
            );

            Add(tileBackground);
            Add(tileFace);

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

        public void BindFliped()
        {
            Clear();

            VisualElement tileBackground = new();
            VisualElement tileFace = new();

            tileBackground.AddToClassList("tile-background");
            tileFace.AddToClassList("tile-face");

            tileBackground.style.backgroundImage = new StyleBackground(
                TileTextureManager.Instance.GetTileBackground()
            );
            tileFace.style.backgroundImage = new StyleBackground(
                TileTextureManager.Instance.GetFlipped()
            );

            tileFace.style.backgroundSize = new BackgroundSize(
                new Length(100, LengthUnit.Percent),
                new Length(100, LengthUnit.Percent)
            );

            Add(tileBackground);
            Add(tileFace);
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
