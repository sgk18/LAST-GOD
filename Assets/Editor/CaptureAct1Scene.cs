using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LastGod.Editor
{
    public static class CaptureAct1Scene
    {
        [MenuItem("The Last God/Capture Act 1 Scene")]
        public static void Capture()
        {
            string scenePath = "Assets/Scenes/Act1_Lab_Origin.unity";
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("No MainCamera found in scene!");
                return;
            }

            int w = 960;
            int h = 540;

            // 1. Capture Chamber Focus view (cam at x = -4.5f)
            cam.transform.position = new Vector3(-4.5f, 0f, -10f);
            RenderAndSave(cam, w, h, "Assets/Scenes/Act1_Lab_Origin_Capture.png");

            // 2. Capture Full Lab Overview (cam at x = 0f)
            cam.transform.position = new Vector3(0f, 0f, -10f);
            RenderAndSave(cam, w, h, "Assets/Scenes/Act1_Lab_Origin_Overview.png");

            // Reset cam position back to default starting position
            cam.transform.position = new Vector3(-4.5f, 0f, -10f);

            AssetDatabase.Refresh();
            Debug.Log("[CaptureAct1Scene] Both Chamber Focus and Lab Overview screenshots captured successfully!");
        }

        private static void RenderAndSave(Camera cam, int w, int h, string outPath)
        {
            RenderTexture rt = new RenderTexture(w, h, 24);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(w, h, TextureFormat.RGB24, false);

            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            screenShot.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            byte[] bytes = screenShot.EncodeToPNG();
            Object.DestroyImmediate(screenShot);

            File.WriteAllBytes(outPath, bytes);
            Debug.Log($"[CaptureAct1Scene] Saved: {outPath}");
        }
    }
}
