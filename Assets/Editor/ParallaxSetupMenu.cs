using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LastGod.Core;

namespace LastGod.EditorTools
{
    /// <summary>
    /// Editor utility script that builds and configures the 3-layer parallax background setup 
    /// directly inside Unity Editor active scene.
    /// </summary>
    [InitializeOnLoad]
    public static class ParallaxSetupMenu
    {
        static ParallaxSetupMenu()
        {
            EditorApplication.delayCall += () =>
            {
                SetupParallaxInActiveScene(silent: true);
            };
        }

        [MenuItem("LastGod/Setup Act 1 Parallax", false, 1)]
        public static void SetupParallaxManual()
        {
            SetupParallaxInActiveScene(silent: false);
        }

        public static void SetupParallaxInActiveScene(bool silent)
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.isLoaded) return;
            if (activeScene.name == "Act1_Scene1" || activeScene.name == "Act1_Origin") return; // Dedicated Lab interior backgrounds

            Sprite farSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Layer1_FarBackground.png");
            Sprite midSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Layer2_Midground.png");
            Sprite foreSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Layer3_Foreground.png");

            if (farSprite == null || midSprite == null || foreSprite == null)
            {
                if (!silent)
                {
                    Debug.LogWarning("[ParallaxSetup] Layer sprite assets not found in Assets/Art/Backgrounds/. Refreshing AssetDatabase.");
                }
                AssetDatabase.Refresh();
                farSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Layer1_FarBackground.png");
                midSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Layer2_Midground.png");
                foreSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Layer3_Foreground.png");
            }

            // Find parent environment container if present (e.g. Environment_Laboratory or Background)
            GameObject parentContainer = GameObject.Find("Environment_Laboratory") ?? GameObject.Find("Background");

            // 1. Far Background (Factor: 0.2, Sorting Layer: FarBackground)
            CreateOrUpdateLayer("FarBackground_Parallax", farSprite, "FarBackground", 0, 0.2f, new Vector3(0, 0, 5), parentContainer);

            // 2. Midground (Factor: 1.0, Sorting Layer: Midground)
            CreateOrUpdateLayer("Midground_Parallax", midSprite, "Midground", 0, 1.0f, new Vector3(0, 0, 0), parentContainer);

            // 3. Foreground (Factor: 1.4, Sorting Layer: Foreground)
            CreateOrUpdateLayer("Foreground_Parallax", foreSprite, "Foreground", 10, 1.4f, new Vector3(0, 0, -5), parentContainer);

            EditorSceneManager.MarkSceneDirty(activeScene);
            if (!silent)
            {
                Debug.Log($"[ParallaxSetup] Successfully configured Parallax Background layers in scene '{activeScene.name}'.");
            }
        }

        private static GameObject CreateOrUpdateLayer(string name, Sprite sprite, string sortingLayer, int sortingOrder, float parallaxFactor, Vector3 localPos, GameObject parent)
        {
            GameObject layerObj = GameObject.Find(name);
            if (layerObj == null && parent != null)
            {
                Transform childTr = parent.transform.Find(name);
                if (childTr != null) layerObj = childTr.gameObject;
            }

            if (layerObj == null)
            {
                layerObj = new GameObject(name);
                if (parent != null)
                {
                    layerObj.transform.SetParent(parent.transform, false);
                }
                layerObj.transform.localPosition = localPos;
            }
            else if (parent != null && layerObj.transform.parent != parent.transform)
            {
                layerObj.transform.SetParent(parent.transform, false);
                layerObj.transform.localPosition = localPos;
            }

            if (!layerObj.TryGetComponent<SpriteRenderer>(out var sr))
            {
                sr = layerObj.AddComponent<SpriteRenderer>();
            }

            if (sprite != null)
            {
                sr.sprite = sprite;
            }

            if (!string.IsNullOrEmpty(sortingLayer))
            {
                sr.sortingLayerName = sortingLayer;
            }
            sr.sortingOrder = sortingOrder;

            if (!layerObj.TryGetComponent<ParallaxLayer>(out var pl))
            {
                pl = layerObj.AddComponent<ParallaxLayer>();
            }

            pl.ParallaxFactor = parallaxFactor;

            return layerObj;
        }
    }
}
