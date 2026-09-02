using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LastGod.Editor
{
    /// <summary>
    /// Auto-configures the URP 2D render pipeline on project load if it isn't already set.
    /// Run manually via: Tools > LAST-GOD > Setup URP 2D Pipeline
    /// </summary>
    [InitializeOnLoad]
    public static class URPSetup
    {
        static URPSetup()
        {
            EditorApplication.delayCall += EnsureURPActive;
        }

        [MenuItem("Tools/LAST-GOD/Setup URP 2D Pipeline")]
        public static void EnsureURPActive()
        {
            if (GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset)
            {
                Debug.Log("[URPSetup] URP 2D is already active.");
                return;
            }

            // ── 1. Try to find an existing URP pipeline asset in the project ──
            string[] urpGuids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
            UniversalRenderPipelineAsset pipelineAsset = null;

            foreach (string guid in urpGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
                if (pipelineAsset != null)
                {
                    Debug.Log($"[URPSetup] Found existing URP asset at: {path}");
                    break;
                }
            }

            // ── 2. None found — create a fresh URP 2D pipeline ──
            if (pipelineAsset == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                    AssetDatabase.CreateFolder("Assets", "Settings");

                // Create 2D Renderer Data
                var renderer2D = ScriptableObject.CreateInstance<Renderer2DData>();
                AssetDatabase.CreateAsset(renderer2D, "Assets/Settings/Renderer2D.asset");

                // Create the URP pipeline asset wired to the 2D renderer
                pipelineAsset = UniversalRenderPipelineAsset.Create(renderer2D);
                AssetDatabase.CreateAsset(pipelineAsset, "Assets/Settings/UniversalRP.asset");

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[URPSetup] Created URP 2D pipeline at Assets/Settings/UniversalRP.asset");
            }

            // ── 3. Assign to GraphicsSettings (and all Quality levels) ──
            GraphicsSettings.defaultRenderPipeline = pipelineAsset;

            int qualityLevelCount = QualitySettings.names.Length;
            for (int i = 0; i < qualityLevelCount; i++)
            {
                var qualityPipeline = QualitySettings.GetRenderPipelineAssetAt(i);
                if (qualityPipeline == null)
                {
                    // Only override quality levels that don't have their own pipeline set
                    // (they will fall through to GraphicsSettings.defaultRenderPipeline)
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[URPSetup] ✅ URP 2D pipeline assigned! Restart Play Mode to see full 2D lighting.");
        }
    }
}
