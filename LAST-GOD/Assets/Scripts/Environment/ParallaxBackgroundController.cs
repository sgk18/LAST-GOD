using System.Collections.Generic;
using UnityEngine;

namespace LastGod.Environment
{
    /// <summary>
    /// Manages and registers all Parallax Layers in the scene.
    /// Coordinates smooth background movement relative to camera movement.
    /// </summary>
    public class ParallaxBackgroundController : MonoBehaviour
    {
        [Header("Target Tracking")]
        [Tooltip("Camera transform to track. Defaults to Camera.main if null.")]
        [SerializeField] private Transform targetCamera;

        [Header("Parallax Layers")]
        [Tooltip("List of managed parallax layers. Automatically populated on awake if empty.")]
        [SerializeField] private List<ParallaxLayer> layers = new List<ParallaxLayer>();

        private void Awake()
        {
            if (targetCamera == null && Camera.main != null)
            {
                targetCamera = Camera.main.transform;
            }

            if (layers == null || layers.Count == 0)
            {
                layers.AddRange(GetComponentsInChildren<ParallaxLayer>());
            }
        }

        /// <summary>
        /// Dynamically add a new parallax layer at runtime.
        /// </summary>
        public void RegisterLayer(ParallaxLayer layer)
        {
            if (layer != null && !layers.Contains(layer))
            {
                layers.Add(layer);
            }
        }

        /// <summary>
        /// Remove a parallax layer at runtime.
        /// </summary>
        public void UnregisterLayer(ParallaxLayer layer)
        {
            if (layer != null && layers.Contains(layer))
            {
                layers.Remove(layer);
            }
        }
    }
}
