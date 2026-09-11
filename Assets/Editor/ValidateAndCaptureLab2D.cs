// ValidateAndCaptureLab2D.cs
// Automated Technical QA & In-Engine Camera Capture for Act 1: The Laboratory 2D
// =============================================================================

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
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
            if (Mathf.Abs(cam.orthographicSize - 3.375f) > 0.01f)
            {
                Debug.LogWarning($"WARN: Camera orthographic size is {cam.orthographicSize}, expected 3.375");
                warnings++;
            }
            Debug.Log($"PASS: Main Camera verified (Ortho Size: {cam.orthographicSize}, Pos: {cam.transform.position})");
        }

        // 2. Verify Tilemaps
        var tilemaps = Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        Debug.Log($"PASS: Found {tilemaps.Length} Tilemaps in scene.");
        if (tilemaps.Length < 3)
        {
            Debug.LogError($"FAIL: Expected at least 3 Tilemaps, found {tilemaps.Length}");
            errors++;
        }
        foreach (var tm in tilemaps)
        {
            int tileCount = tm.GetUsedTilesCount();
            Debug.Log($" - Tilemap '{tm.name}': {tileCount} tiles placed, sortingLayer='{tm.GetComponent<TilemapRenderer>().sortingLayerName}'");
            if (tileCount == 0)
            {
                Debug.LogError($"FAIL: Tilemap '{tm.name}' has 0 tiles!");
                errors++;
            }
        }

        // 3. Verify Colliders
        var compColliders = Object.FindObjectsByType<CompositeCollider2D>(FindObjectsSortMode.None);
        Debug.Log($"PASS: Found {compColliders.Length} CompositeCollider2D instances.");
        if (compColliders.Length < 2)
        {
            Debug.LogError($"FAIL: Expected floor and platform CompositeCollider2D, found {compColliders.Length}");
            errors++;
        }

        // 4. Verify Parallax Layers
        var parallaxLayers = Object.FindObjectsByType<ParallaxLayer>(FindObjectsSortMode.None);
        Debug.Log($"PASS: Found {parallaxLayers.Length} ParallaxLayer components configured across depth planes.");
        if (parallaxLayers.Length < 4)
        {
            Debug.LogError($"FAIL: Expected at least 4 ParallaxLayers, found {parallaxLayers.Length}");
            errors++;
        }
        foreach (var pl in parallaxLayers)
        {
            Debug.Log($" - ParallaxLayer '{pl.name}': FactorX={pl.parallaxFactorX}, FactorY={pl.parallaxFactorY}, PixelSnap={pl.pixelSnap}");
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
        var heroPod = GameObject.Find("HERO_CONTAINMENT_MAIN");
        if (heroPod == null)
        {
            Debug.LogError("FAIL: HERO_CONTAINMENT_MAIN not found in scene!");
            errors++;
        }
        else
        {
            Debug.Log($"PASS: HERO_CONTAINMENT_MAIN located at {heroPod.transform.position}");
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
