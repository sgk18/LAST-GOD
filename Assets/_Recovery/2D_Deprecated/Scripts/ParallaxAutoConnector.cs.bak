using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LastGod.Core;

namespace LastGod.EditorTools
{
    /// <summary>
    /// Automatic Editor hook that connects and saves parallax layers on scene open or domain reload.
    /// </summary>
    [InitializeOnLoad]
    public static class ParallaxAutoConnector
    {
        static ParallaxAutoConnector()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.delayCall += () => CheckAndConnectActiveScene();
        }

        private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            CheckAndConnectActiveScene();
        }

        public static void CheckAndConnectActiveScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.isLoaded) return;
            if (activeScene.name == "Act1_Scene1" || activeScene.name == "Act1_Origin") return; // Dedicated Lab interior backgrounds

            Sprite farSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Layer1_FarBackground.png");
            Sprite midSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Layer2_Midground.png");
            Sprite foreSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Layer3_Foreground.png");

            if (farSprite == null || midSprite == null || foreSprite == null)
            {
                AssetDatabase.Refresh();
                farSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Layer1_FarBackground.png");
                midSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Layer2_Midground.png");
                foreSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Layer3_Foreground.png");
            }

            GameObject parent = GameObject.Find("Environment_Laboratory") ?? GameObject.Find("Background") ?? GameObject.Find("=== ACT 1 ORIGIN DIRECTOR ===");

            bool changed = false;

            // Ensure ParallaxRuntimeManager component on parent
            if (parent != null)
            {
                if (!parent.TryGetComponent<ParallaxRuntimeManager>(out var prm))
                {
                    prm = parent.AddComponent<ParallaxRuntimeManager>();
                    changed = true;
                }
            }

            // Create/Connect layers directly
            changed |= EnsureLayer("FarBackground_Parallax", farSprite, "FarBackground", 0, 0.2f, new Vector3(0, 0, 5), parent);
            changed |= EnsureLayer("Midground_Parallax", midSprite, "Midground", 0, 1.0f, new Vector3(0, 0, 0), parent);
            changed |= EnsureLayer("Foreground_Parallax", foreSprite, "Foreground", 10, 1.4f, new Vector3(0, 0, -5), parent);

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                Debug.Log($"[ParallaxAutoConnector] Connected and saved Parallax Background layers in '{activeScene.name}'.");
            }
        }

        private static bool EnsureLayer(string name, Sprite sprite, string sortingLayer, int sortingOrder, float factor, Vector3 localPos, GameObject parent)
        {
            bool modified = false;
            GameObject layerObj = GameObject.Find(name);
            if (layerObj == null && parent != null)
            {
                Transform tr = parent.transform.Find(name);
                if (tr != null) layerObj = tr.gameObject;
            }

            if (layerObj == null)
            {
                layerObj = new GameObject(name);
                if (parent != null) layerObj.transform.SetParent(parent.transform, false);
                layerObj.transform.localPosition = localPos;
                modified = true;
            }

            if (!layerObj.TryGetComponent<SpriteRenderer>(out var sr))
            {
                sr = layerObj.AddComponent<SpriteRenderer>();
                modified = true;
            }

            if (sprite != null && sr.sprite != sprite)
            {
                sr.sprite = sprite;
                modified = true;
            }

            if (!string.IsNullOrEmpty(sortingLayer) && sr.sortingLayerName != sortingLayer)
            {
                sr.sortingLayerName = sortingLayer;
                modified = true;
            }

            if (sr.sortingOrder != sortingOrder)
            {
                sr.sortingOrder = sortingOrder;
                modified = true;
            }

            if (!layerObj.TryGetComponent<ParallaxLayer>(out var pl))
            {
                pl = layerObj.AddComponent<ParallaxLayer>();
                modified = true;
            }

            if (pl.ParallaxFactor != factor)
            {
                pl.ParallaxFactor = factor;
                modified = true;
            }

            return modified;
        }
    }
}
