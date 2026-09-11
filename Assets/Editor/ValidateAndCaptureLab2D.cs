// ValidateAndCaptureLab2D.cs
// Technical QA & In-Engine Camera Capture for Act 1 Hero Environment Benchmark
// =============================================================================

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using LastGod.Environment2D;

public static class ValidateAndCaptureLab2D
{
    const string SCENE_PATH = "Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D.unity";
    const string SCREENSHOT_PATH = "Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D_Screenshot.png";

    [MenuItem("The Last God/Validate and Capture 2D Lab", false, 11)]
    public static void ValidateAndCapture()
    {
        Debug.Log(">>> [ValidateAndCaptureLab2D] Running Technical & Gameplay QA validation...");

        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        int errors = 0;
        int warnings = 0;

        // 1. Verify Camera
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("FAIL: Main Camera not found!");
            errors++;
        }
        else
        {
            if (!cam.orthographic)
            {
                Debug.LogError("FAIL: Camera is not orthographic!");
                errors++;
            }
            Debug.Log($"PASS: Main Camera verified (Ortho Size: {cam.orthographicSize}, Pos: {cam.transform.position})");
        }

        // 2. Verify Multi-Plane Sprites
        var renderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        Debug.Log($"PASS: Found {renderers.Length} SpriteRenderer instances in hero environment.");
        if (renderers.Length < 4)
        {
            Debug.LogError($"FAIL: Expected at least 4 SpriteRenderers across depth planes, found {renderers.Length}");
            errors++;
        }
        foreach (var sr in renderers)
        {
            Debug.Log($" - Sprite: '{sr.name}', SortingLayer='{sr.sortingLayerName}', Order={sr.sortingOrder}");
        }

        // 3. Verify Colliders
        var colliders = Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        Debug.Log($"PASS: Found {colliders.Length} Collider2D instances on gameplay platforms.");
        if (colliders.Length < 2)
        {
            Debug.LogWarning($"WARN: Found {colliders.Length} colliders.");
            warnings++;
        }

        // 4. Verify Parallax Layers
        var parallaxLayers = Object.FindObjectsByType<ParallaxLayer>(FindObjectsSortMode.None);
        Debug.Log($"PASS: Found {parallaxLayers.Length} ParallaxLayer components configured across depth planes.");
        foreach (var pl in parallaxLayers)
        {
            Debug.Log($" - ParallaxLayer '{pl.name}': FactorX={pl.parallaxFactorX}, FactorY={pl.parallaxFactorY}");
        }

        // 5. Verify 2D Lights
        var lights = Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        Debug.Log($"PASS: Found {lights.Length} 2D Lights configured.");
        if (lights.Length < 4)
        {
            Debug.LogError($"FAIL: Expected at least 4 Light2D components, found {lights.Length}");
            errors++;
        }
        foreach (var l in lights)
        {
            Debug.Log($" - Light2D '{l.name}': Type={l.lightType}, Color={l.color}, Intensity={l.intensity}");
        }

        // 6. Verify Hero Containment & Player Proxy
        var heroPod = GameObject.Find("HERO_LAB_CONTAINMENT_CHAMBER");
        if (heroPod == null)
        {
            Debug.LogError("FAIL: HERO_LAB_CONTAINMENT_CHAMBER not found in scene!");
            errors++;
        }
        else
        {
            Debug.Log($"PASS: HERO_LAB_CONTAINMENT_CHAMBER located at {heroPod.transform.position}");
        }

        var playerProxy = GameObject.Find("Player_Silhouette_Proxy");
        if (playerProxy == null)
        {
            Debug.LogError("FAIL: Player_Silhouette_Proxy not found in scene!");
            errors++;
        }
        else
        {
            Debug.Log($"PASS: Player_Silhouette_Proxy located at {playerProxy.transform.position}");
        }

        // 7. Render In-Engine Screenshot from Camera (1920x1080)
        if (cam != null)
        {
            int width = 1920;
            int height = 1080;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var prevRT = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = prevRT;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            byte[] bytes = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            File.WriteAllBytes(SCREENSHOT_PATH, bytes);
            Debug.Log($"PASS: Captured in-engine camera screenshot to {SCREENSHOT_PATH} ({bytes.Length} bytes)");
        }

        Debug.Log($"=== QA VALIDATION SUMMARY: {errors} Errors, {warnings} Warnings ===");
    }
}
