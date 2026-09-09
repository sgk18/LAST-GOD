using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

namespace LastGod.Editor
{
    /// <summary>
    /// One-shot editor tool to build the Act1_Lab_Canvas scene.
    /// Run via: Tools → Last God → Build Lab Canvas Scene
    ///
    /// Creates:
    ///   - Pixel Perfect Camera (240×160, 16 PPU)
    ///   - 3 parallax background layers (Far, Mid, Foreground silhouette)
    ///   - 4 ambient prop GameObjects (Monitor, Flask, Alarm, Steam)
    ///   - ParallaxBackground + AmbientLightFlicker components wired up
    /// </summary>
    public static class LabCanvasSceneBuilder
    {
        private const string SCENE_PATH = "Assets/Scenes/Act1_Lab_Canvas.unity";
        private const string BG_FAR_PATH  = "Assets/Art/Backgrounds/Lab_BG_Far.jpg";
        private const string BG_MID_PATH  = "Assets/Art/Backgrounds/Lab_BG_Mid.jpg";

        [MenuItem("Tools/Last God/Build Lab Canvas Scene")]
        public static void BuildLabCanvasScene()
        {
            // ── Create or clear the scene ────────────────────────────────
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Camera ───────────────────────────────────────────────────
            var cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            var cam = cameraGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.06f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.nearClipPlane = -100f;
            cam.farClipPlane  =  100f;
            cameraGO.transform.position = new Vector3(0, 0, -10);

            // Pixel Perfect Camera
            var ppc = cameraGO.AddComponent<UnityEngine.U2D.PixelPerfectCamera>();
            ppc.refResolutionX = 240;
            ppc.refResolutionY = 160;
            ppc.assetsPPU = 16;
            ppc.cropFrameX = false;
            ppc.cropFrameY = false;
            ppc.stretchFill = false;

            // ── Parallax Root ────────────────────────────────────────────
            var parallaxRoot = new GameObject("LabBackground_Parallax");
            var parallaxScript = parallaxRoot.AddComponent<LastGod.Environment.ParallaxBackground>();

            // ── Layer 1: Far Background ──────────────────────────────────
            var bgFar = CreateBackgroundLayer(
                "Layer1_Far",
                BG_FAR_PATH,
                new Vector3(0, 0, 10),
                0.1f,
                parallaxRoot.transform
            );

            // ── Layer 2: Mid Background ──────────────────────────────────
            var bgMid = CreateBackgroundLayer(
                "Layer2_Mid",
                BG_MID_PATH,
                new Vector3(0, 0, 5),
                0.3f,
                parallaxRoot.transform
            );

            // ── Layer 3: Foreground Silhouette (dark overlay) ────────────
            var bgFg = CreateColorLayer(
                "Layer3_FG_Silhouette",
                new Color(0.02f, 0.02f, 0.05f, 0.7f),
                new Vector3(0, 0, -1),
                parallaxRoot.transform
            );

            // ── Animated Props Root ──────────────────────────────────────
            var propsRoot = new GameObject("LabProps_Ambient");

            // Monitor props (left and right consoles)
            CreateFlickerProp("Monitor_Left",   new Vector3(-3.5f, -0.8f, 0), 
                              LastGod.Environment.AmbientLightFlicker.FlickerMode.MonitorGlow, propsRoot.transform);
            CreateFlickerProp("Monitor_Right",  new Vector3( 3.5f, -0.8f, 0), 
                              LastGod.Environment.AmbientLightFlicker.FlickerMode.MonitorGlow, propsRoot.transform);

            // Alarm lights (on columns)
            CreateFlickerProp("Alarm_Left",     new Vector3(-2.0f,  0.5f, 0), 
                              LastGod.Environment.AmbientLightFlicker.FlickerMode.AlarmBlink,  propsRoot.transform);
            CreateFlickerProp("Alarm_Right",    new Vector3( 2.0f,  0.5f, 0), 
                              LastGod.Environment.AmbientLightFlicker.FlickerMode.AlarmBlink,  propsRoot.transform);

            // Steam vent
            CreateFlickerProp("Steam_Center",   new Vector3( 0.5f, -1.2f, 0), 
                              LastGod.Environment.AmbientLightFlicker.FlickerMode.SteamPulse,  propsRoot.transform);

            // ── Save Scene ───────────────────────────────────────────────
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AssetDatabase.Refresh();

            Debug.Log("[LabCanvas] ✅ Act1_Lab_Canvas.unity built and saved to: " + SCENE_PATH);

            // Only show dialog & open in interactive mode (not -batchmode)
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Lab Canvas Built",
                    "Act1_Lab_Canvas.unity created at:\n" + SCENE_PATH +
                    "\n\nOpen it in the Scene window to review.",
                    "OK"
                );
                EditorSceneManager.OpenScene(SCENE_PATH);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static GameObject CreateBackgroundLayer(
            string name, string spritePath, Vector3 position,
            float parallaxSpeed, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = Mathf.RoundToInt(-position.z);

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePath);
            if (tex != null)
            {
                var sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    16f
                );
                sr.sprite = sprite;
            }
            else
            {
                Debug.LogWarning("[LabCanvas] Texture not found: " + spritePath + " — assign manually.");
            }

            return go;
        }

        private static GameObject CreateColorLayer(
            string name, Color color, Vector3 position, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = new Vector3(30, 10, 1);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = color;
            sr.sortingOrder = 10;

            // Use a 1×1 white sprite as the color rectangle
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

            return go;
        }

        private static GameObject CreateFlickerProp(
            string name,
            Vector3 position,
            LastGod.Environment.AmbientLightFlicker.FlickerMode mode,
            Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;

            // 4×4 pixel quad as placeholder sprite
            var tex = new Texture2D(4, 4);
            Color col = mode == LastGod.Environment.AmbientLightFlicker.FlickerMode.AlarmBlink
                ? new Color(1f, 0.4f, 0f)
                : mode == LastGod.Environment.AmbientLightFlicker.FlickerMode.SteamPulse
                    ? new Color(0.54f, 0.67f, 0.72f)
                    : new Color(0f, 1f, 0.26f);

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    tex.SetPixel(x, y, col);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
            sr.color = col;

            go.AddComponent<LastGod.Environment.AmbientLightFlicker>();

            return go;
        }
    }
}
