using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RiichiReign.GameAsset
{
    [CreateAssetMenu(fileName = "TileTextureDatabase",
                     menuName = "Mahjong/Tile Texture Database")]
    public class TileTextureAsset : ScriptableObject
    {
        [System.Serializable]
        public class TileTextureEntry
        {
            public string key;
            public Texture2D background;
            public Texture2D face;
        }

        [SerializeField]
        string backgroundKey = "Back";
        [SerializeField]
        string blankKey = "Blank";
        [SerializeField]
        List<TileTextureEntry> entries = new();

        Dictionary<string, TileTextureEntry> lookup;

#if UNITY_EDITOR
        // This runs in Editor when asset is modified
        void OnValidate()
        {
            entries.Clear(); // Optional: clear previous entries

            string folderPath = "Assets/Art/Tiles/Normal"; // Front textures folder
            string backPath = "Assets/Art/Tiles/Normal/Front.png"; // Back texture

            Texture2D backTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(backPath);

            if (backTexture == null)
            {
                Debug.LogError($"Back texture not found at {backPath}");
                return;
            }

            string[] files = Directory.GetFiles(folderPath, "*.png", SearchOption.TopDirectoryOnly);

            foreach (string filePath in files)
            {
                Texture2D frontTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
                if (frontTexture == null) continue;

                string fileName = Path.GetFileNameWithoutExtension(filePath);

                entries.Add(new TileTextureEntry
                {
                    key = fileName,
                    background = backTexture,
                    face = frontTexture
                });
            }

            // Mark asset dirty so Unity saves it
            EditorUtility.SetDirty(this);
        }
#endif

        public void Initialize()
        {
            lookup = new Dictionary<string, TileTextureEntry>();

            foreach (var entry in entries)
            {
                if (!lookup.ContainsKey(entry.key))
                    lookup.Add(entry.key, entry);
            }
        }

        public Texture2D GetBackground()
        {
            if (lookup.TryGetValue(backgroundKey, out var entry))
                return entry.background;

            Debug.LogError($"No background texture found");
            return null;
        }

        public Texture2D GetFront(string key)
        {
            if (lookup.TryGetValue(key, out var entry))
                return entry.face;

            Debug.LogError($"No face texture found for key: {key}");

            if (lookup.TryGetValue(blankKey, out var blank))
                return blank.face;

            Debug.LogError($"No face texture found for {blankKey}");

            return null;
        }
    }
}