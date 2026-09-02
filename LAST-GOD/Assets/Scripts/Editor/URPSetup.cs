using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LastGod.Editor
{
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

            if (pipelineAsset == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                    AssetDatabase.CreateFolder("Assets", "Settings");

                var renderer2D = ScriptableObject.CreateInstance<Renderer2DData>();
                AssetDatabase.CreateAsset(renderer2D, "Assets/Settings/Renderer2D.asset");

                pipelineAsset = UniversalRenderPipelineAsset.Create(renderer2D);
                AssetDatabase.CreateAsset(pipelineAsset, "Assets/Settings/UniversalRP.asset");

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[URPSetup] Created URP 2D pipeline at Assets/Settings/UniversalRP.asset");
            }

            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            AssetDatabase.SaveAssets();
            Debug.Log("[URPSetup] ✅ URP 2D pipeline assigned! Restart Play Mode to see full 2D lighting.");
        }
    }
}
