using UnityEngine;

namespace LastGod.Core
{
    /// <summary>
    /// Ensures that all three parallax background layers (FarBackground, Midground, Foreground) 
    /// exist in the scene and are properly configured with sprites and ParallaxLayer components.
    /// Executes automatically at scene load / runtime.
    /// </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-100)]
    public class ParallaxAutoSetup : MonoBehaviour
    {
        [Header("Layer Sprites")]
        [SerializeField] private Sprite farBackgroundSprite;
        [SerializeField] private Sprite midgroundSprite;
        [SerializeField] private Sprite foregroundSprite;

        private void Awake()
        {
            EnsureLayers();
        }

        private void OnValidate()
        {
            EnsureLayers();
        }

        /// <summary>
        /// Finds or creates the 3 parallax layers and applies correct sorting layers and parallax factors.
        /// </summary>
        public void EnsureLayers()
        {
            // Load sprites from Resources / Assets if unassigned
            if (farBackgroundSprite == null)
            {
                farBackgroundSprite = Resources.Load<Sprite>("Backgrounds/Layer1_FarBackground") 
                    ?? LoadSpriteFromAssetPath("Assets/Art/Backgrounds/Layer1_FarBackground.png");
            }
            if (midgroundSprite == null)
            {
                midgroundSprite = Resources.Load<Sprite>("Backgrounds/Layer2_Midground") 
                    ?? LoadSpriteFromAssetPath("Assets/Art/Backgrounds/Layer2_Midground.png");
            }
            if (foregroundSprite == null)
            {
                foregroundSprite = Resources.Load<Sprite>("Backgrounds/Layer3_Foreground") 
                    ?? LoadSpriteFromAssetPath("Assets/Art/Backgrounds/Layer3_Foreground.png");
            }

            // 1. Far Background (Factor: 0.2, Sorting Layer: FarBackground)
            SetupLayer("FarBackground_Parallax", farBackgroundSprite, "FarBackground", 0, 0.2f, new Vector3(0, 0, 5));

            // 2. Midground (Factor: 1.0, Sorting Layer: Midground)
            SetupLayer("Midground_Parallax", midgroundSprite, "Midground", 0, 1.0f, new Vector3(0, 0, 0));

            // 3. Foreground (Factor: 1.4, Sorting Layer: Foreground)
            SetupLayer("Foreground_Parallax", foregroundSprite, "Foreground", 10, 1.4f, new Vector3(0, 0, -5));
        }

        private void SetupLayer(string layerName, Sprite sprite, string sortingLayerName, int sortingOrder, float parallaxFactor, Vector3 localPos)
        {
            Transform existing = transform.Find(layerName);
            GameObject layerObj;

            if (existing == null)
            {
                // Check root scene
                GameObject rootFind = GameObject.Find("/" + layerName);
                if (rootFind != null)
                {
                    layerObj = rootFind;
                    layerObj.transform.SetParent(transform, true);
                }
                else
                {
                    layerObj = new GameObject(layerName);
                    layerObj.transform.SetParent(transform, false);
                    layerObj.transform.localPosition = localPos;
                }
            }
            else
            {
                layerObj = existing.gameObject;
            }

            // Configure SpriteRenderer
            if (!layerObj.TryGetComponent<SpriteRenderer>(out var sr))
            {
                sr = layerObj.AddComponent<SpriteRenderer>();
            }

            if (sprite != null && sr.sprite == null)
            {
                sr.sprite = sprite;
            }

            if (!string.IsNullOrEmpty(sortingLayerName))
            {
                sr.sortingLayerName = sortingLayerName;
            }
            sr.sortingOrder = sortingOrder;

            // Configure ParallaxLayer component
            if (!layerObj.TryGetComponent<ParallaxLayer>(out var parallaxComp))
            {
                parallaxComp = layerObj.AddComponent<ParallaxLayer>();
            }

            parallaxComp.ParallaxFactor = parallaxFactor;
        }

        private Sprite LoadSpriteFromAssetPath(string assetPath)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
#else
            return null;
#endif
        }
    }
}
