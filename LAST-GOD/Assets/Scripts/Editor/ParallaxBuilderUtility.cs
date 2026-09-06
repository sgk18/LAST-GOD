#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using LastGod.Environment;

namespace LastGod.EditorTools
{
    /// <summary>
    /// Editor utility script to easily populate and configure 2D Parallax Layers and Ambient Animated Props in Unity scenes.
    /// </summary>
    public static class ParallaxBuilderUtility
    {
        [MenuItem("Tools/LAST GOD/Setup Parallax & Ambient Background System")]
        public static void CreateParallaxHierarchy()
        {
            // Find or create root Parallax Controller Object
            GameObject rootObj = GameObject.Find("Parallax_System");
            if (rootObj == null)
            {
                rootObj = new GameObject("Parallax_System");
                Undo.RegisterCreatedObjectUndo(rootObj, "Create Parallax System");
            }

            ParallaxBackgroundController controller = rootObj.GetComponent<ParallaxBackgroundController>();
            if (controller == null)
            {
                controller = Undo.AddComponent<ParallaxBackgroundController>(rootObj);
            }

            // Create Layer 0: Far Background (Sky, distant mountains) - Parallax 0.05
            CreateOrUpdateLayer(rootObj.transform, "Layer_00_SkyFarBG", 0.05f, 0.02f, sortingOrder: -100);

            // Create Layer 1: Distant City / Village Silhouette - Parallax 0.2
            GameObject layer1 = CreateOrUpdateLayer(rootObj.transform, "Layer_01_DistantSilhouettes", 0.2f, 0.1f, sortingOrder: -80);

            // Create Layer 2: Midground Props & Buildings - Parallax 0.5
            GameObject layer2 = CreateOrUpdateLayer(rootObj.transform, "Layer_02_MidgroundProps", 0.5f, 0.2f, sortingOrder: -50);

            // Create Layer 3: Foreground Atmospheric Fog & Overhanging Vegetation - Parallax 1.15
            GameObject layer3 = CreateOrUpdateLayer(rootObj.transform, "Layer_03_ForegroundDetails", 1.15f, 0.3f, sortingOrder: 100);

            // Add sample ambient prop children to Layer 2 & 3 for demonstration
            AddSampleAmbientProps(layer2.transform, layer3.transform);

            Selection.activeGameObject = rootObj;
            Debug.Log("[LAST GOD] Parallax & Ambient Background Prop hierarchy constructed successfully!");
        }

        private static GameObject CreateOrUpdateLayer(Transform parent, string name, float factorX, float factorY, int sortingOrder)
        {
            Transform existing = parent.Find(name);
            GameObject layerObj;

            if (existing == null)
            {
                layerObj = new GameObject(name);
                layerObj.transform.SetParent(parent, false);
                Undo.RegisterCreatedObjectUndo(layerObj, "Create Parallax Layer " + name);
            }
            else
            {
                layerObj = existing.gameObject;
            }

            ParallaxLayer layerComp = layerObj.GetComponent<ParallaxLayer>();
            if (layerComp == null)
            {
                layerComp = Undo.AddComponent<ParallaxLayer>(layerObj);
            }

            layerComp.ParallaxFactorX = factorX;
            layerComp.ParallaxFactorY = factorY;

            return layerObj;
        }

        private static void AddSampleAmbientProps(Transform midgroundLayer, Transform foregroundLayer)
        {
            // Sample Midground Swaying Banner / Sign
            if (midgroundLayer.Find("Ambient_SwayingBanner") == null)
            {
                GameObject banner = new GameObject("Ambient_SwayingBanner");
                banner.transform.SetParent(midgroundLayer, false);
                banner.transform.localPosition = new Vector3(-2f, 1f, 0f);

                SpriteRenderer sr = banner.AddComponent<SpriteRenderer>();
                sr.sortingOrder = -49;

                SwayingProp sway = banner.AddComponent<SwayingProp>();
                Undo.RegisterCreatedObjectUndo(banner, "Create Ambient Swaying Banner");
            }

            // Sample Midground Flickering Torch / Light
            if (midgroundLayer.Find("Ambient_FlickeringLight") == null)
            {
                GameObject light = new GameObject("Ambient_FlickeringLight");
                light.transform.SetParent(midgroundLayer, false);
                light.transform.localPosition = new Vector3(3f, 0.5f, 0f);

                SpriteRenderer sr = light.AddComponent<SpriteRenderer>();
                sr.sortingOrder = -48;

                LightFlicker2D flicker = light.AddComponent<LightFlicker2D>();
                Undo.RegisterCreatedObjectUndo(light, "Create Ambient Flickering Light");
            }

            // Sample Foreground Floating Fog / Spore
            if (foregroundLayer.Find("Ambient_FloatingFog") == null)
            {
                GameObject fog = new GameObject("Ambient_FloatingFog");
                fog.transform.SetParent(foregroundLayer, false);
                fog.transform.localPosition = new Vector3(0f, -1f, 0f);

                SpriteRenderer sr = fog.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 101;

                FloatingProp floatProp = fog.AddComponent<FloatingProp>();
                Undo.RegisterCreatedObjectUndo(fog, "Create Ambient Floating Fog");
            }
        }
    }
}
#endif
