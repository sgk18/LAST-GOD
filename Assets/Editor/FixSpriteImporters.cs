using System.IO;
using UnityEditor;
using UnityEngine;

namespace LastGod.Editor
{
    public static class FixSpriteImporters
    {
        [MenuItem("The Last God/Fix All Sprite Importers")]
        public static void FixAll()
        {
            Debug.Log("[FixSpriteImporters] Starting sprite importer fix...");
            string[] searchDirs = new string[]
            {
                "Assets/Art/Act1/scenes",
                "Assets/Art/Act1/sprites",
                "Assets/Art/Act1/environment",
                "Assets/Art/Act1/props",
                "Assets/Art/Act1/ui"
            };

            int fixedCount = 0;
            int loadedCount = 0;

            foreach (var dir in searchDirs)
            {
                if (!Directory.Exists(dir)) continue;
                var files = Directory.GetFiles(dir, "*.png", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    string assetPath = file.Replace('\\', '/');
                    var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    if (importer != null)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.spritePixelsPerUnit = 16f;
                        importer.filterMode = FilterMode.Point;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.alphaIsTransparency = true;
                        importer.SaveAndReimport();
                        fixedCount++;
                    }
                }
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            foreach (var dir in searchDirs)
            {
                if (!Directory.Exists(dir)) continue;
                var files = Directory.GetFiles(dir, "*.png", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    string assetPath = file.Replace('\\', '/');
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    if (sprite != null)
                    {
                        loadedCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"[FixSpriteImporters] FAILED to load Sprite at: {assetPath}");
                    }
                }
            }

            Debug.Log($"[FixSpriteImporters] Completed! Fixed: {fixedCount}, Successfully Loaded Sprites: {loadedCount}");
        }
    }
}

