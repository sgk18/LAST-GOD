using UnityEngine;

namespace LastGod.Core
{
    /// <summary>
    /// Decoupled runtime manager that automatically creates, links, and updates 
    /// the 3-layer parallax background setup at scene start.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public class ParallaxRuntimeManager : MonoBehaviour
    {
        [Header("Layer Sprites")]
        [SerializeField] private Sprite farBackgroundSprite;
        [SerializeField] private Sprite midgroundSprite;
        [SerializeField] private Sprite foregroundSprite;

        [Header("Layer Settings")]
        [SerializeField] private float farParallaxFactor = 0.2f;
        [SerializeField] private float midParallaxFactor = 1.0f;
        [SerializeField] private float foreParallaxFactor = 1.4f;

        private GameObject _farLayerObj;
        private GameObject _midLayerObj;
        private GameObject _foreLayerObj;

        private void Awake()
        {
            InitializeParallaxLayers();
        }

        /// <summary>
        /// Instantiates missing parallax layer objects and attaches ParallaxLayer components.
        /// </summary>
        public void InitializeParallaxLayers()
        {
            if (gameObject.scene.name == "Act1_Scene1" || gameObject.scene.name == "Act1_Origin") return;

            // Load sprites from Resources / Assets if unassigned
            if (farBackgroundSprite == null)
                farBackgroundSprite = Resources.Load<Sprite>("Layer1_FarBackground");
            if (midgroundSprite == null)
                midgroundSprite = Resources.Load<Sprite>("Layer2_Midground");
            if (foregroundSprite == null)
                foregroundSprite = Resources.Load<Sprite>("Layer3_Foreground");

            GameObject parentEnv = GameObject.Find("Environment_Laboratory") ?? GameObject.Find("Background");

            // 1. Layer 1 — Far Background
            _farLayerObj = SetupLayer("FarBackground_Parallax", farBackgroundSprite, "FarBackground", 0, farParallaxFactor, new Vector3(0, 0, 5), parentEnv);

            // 2. Layer 2 — Midground
            _midLayerObj = SetupLayer("Midground_Parallax", midgroundSprite, "Midground", 0, midParallaxFactor, new Vector3(0, 0, 0), parentEnv);

            // 3. Layer 3 — Foreground Silhouette
            _foreLayerObj = SetupLayer("Foreground_Parallax", foregroundSprite, "Foreground", 10, foreParallaxFactor, new Vector3(0, 0, -5), parentEnv);
        }

        private GameObject SetupLayer(string name, Sprite sprite, string sortingLayer, int sortingOrder, float factor, Vector3 localPos, GameObject parent)
        {
            GameObject layerObj = GameObject.Find(name);
            if (layerObj == null && parent != null)
            {
                Transform child = parent.transform.Find(name);
                if (child != null) layerObj = child.gameObject;
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

            if (!layerObj.TryGetComponent<SpriteRenderer>(out var sr))
            {
                sr = layerObj.AddComponent<SpriteRenderer>();
            }

            if (sprite != null && sr.sprite == null)
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

            pl.ParallaxFactor = factor;
            return layerObj;
        }
    }
}
