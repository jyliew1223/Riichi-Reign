using RiichiReign.GameAsset;
using RiichiReign.MahjongEngine;
using UnityEngine;

namespace RiichiReign.UI
{
    internal class TileTextureManager : MonoBehaviour
    {
        [SerializeField]
        TileTextureAsset TileTextureAsset;

        public static TileTextureManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            TileTextureAsset.Initialize();
        }

        public Texture2D GetTileBackground()
        {
            Texture2D texture = TileTextureAsset.GetBackground();
            return texture;
        }

        public Texture2D GetTileFront(Tile tile)
        {
            string key = tile.GetTextureKey();
            Texture2D texture = TileTextureAsset.GetFront(key);
            return texture;
        }

        public Texture2D GetFlipped()
        {
            Texture2D texture = TileTextureAsset.GetFront("Back");
            return texture;
        }
    }
}
