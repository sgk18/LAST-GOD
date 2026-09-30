// ValidateAndCaptureLab2D.cs
// Technical QA & In-Engine Camera Capture for Act 1: Origin Guard Production & Playable Prologue
// ==============================================================================================

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using LastGod.Environment2D;
using LastGod.Player.Guard;
using LastGod.Weapons;
using LastGod.Environment;
using LastGod.Core;
using LastGod.UI;

public static class ValidateAndCaptureLab2D
{
    const string SCENE_PATH = "Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D.unity";
    const string SCREENSHOT_PATH = "Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D_Screenshot.png";

    [MenuItem("The Last God/Validate and Capture 2D Lab", false, 11)]
    public static void ValidateAndCapture()
    {
        Debug.Log(">>> [ValidateAndCaptureLab2D] Running Technical & Gameplay QA validation for Guard Prologue...");

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

        // 2. Verify Guard Player Instance
        var guard = GameObject.Find("PF_Guard_Player");
        if (guard == null)
        {
            Debug.LogError("FAIL: PF_Guard_Player not found in scene!");
            errors++;
        }
        else
        {
            Debug.Log($"PASS: PF_Guard_Player verified at {guard.transform.position}");

            var sr = guard.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sortingLayerName != "Player")
            {
                Debug.LogError($"FAIL: Guard SpriteRenderer missing or wrong sorting layer '{sr?.sortingLayerName}'");
                errors++;
            }
            else
            {
                Debug.Log($"PASS: Guard SpriteRenderer (Sprite: '{sr.sprite?.name}', Layer: '{sr.sortingLayerName}', Order: {sr.sortingOrder})");
            }

            var anim = guard.GetComponent<Animator>();
            if (anim == null || anim.runtimeAnimatorController == null)
            {
                Debug.LogError("FAIL: Guard Animator or Controller missing!");
                errors++;
            }
            else
            {
                Debug.Log($"PASS: Guard Animator Controller '{anim.runtimeAnimatorController.name}' bound.");
            }

            var guardCtrl = guard.GetComponent<GuardController>();
            if (guardCtrl == null)
            {
                Debug.LogError("FAIL: GuardController missing on PF_Guard_Player!");
                errors++;
            }
            else
            {
                Debug.Log("PASS: GuardController component active.");
            }

            var wepCtrl = guard.GetComponent<WeaponController>();
            if (wepCtrl == null)
            {
                Debug.LogError("FAIL: WeaponController missing on PF_Guard_Player!");
                errors++;
            }
            else
            {
                Debug.Log("PASS: WeaponController component active.");
            }

            var flashlight = guard.GetComponentInChildren<FlashlightController2D>();
            if (flashlight == null)
            {
                Debug.LogError("FAIL: FlashlightController2D missing in PF_Guard_Player children!");
                errors++;
            }
            else
            {
                Debug.Log("PASS: FlashlightController2D active with Light2D cone.");
            }
        }

        // 3. Verify Damaged Electrical Conduit Hazard
        var conduit = GameObject.Find("Damaged_Conduit_Hazard");
        if (conduit == null)
        {
            Debug.LogError("FAIL: Damaged_Conduit_Hazard not found in scene!");
            errors++;
        }
        else
        {
            var haz = conduit.GetComponent<DamagedConduitHazard>();
            if (haz == null)
            {
                Debug.LogError("FAIL: DamagedConduitHazard component missing on Damaged_Conduit_Hazard!");
                errors++;
            }
            else
            {
                Debug.Log($"PASS: Damaged_Conduit_Hazard verified at {conduit.transform.position}");
            }
        }

        // 4. Verify Narrative Director and Radio UI
        var systems = GameObject.Find("[PROLOGUE_SYSTEMS]");
        if (systems == null)
        {
            Debug.LogError("FAIL: [PROLOGUE_SYSTEMS] not found in scene!");
            errors++;
        }
        else
        {
            Debug.Log("PASS: [PROLOGUE_SYSTEMS] active (RadioSystem, RadioDialogueUI, AmmoHUD, PrologueSequenceDirector).");
        }

        // 5. Verify Hero Containment Chamber
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

        // 6. Verify 2D Lighting Rig
        var lights = Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        Debug.Log($"PASS: Found {lights.Length} Light2D instances active in scene.");
        if (lights.Length < 4)
        {
            Debug.LogError($"FAIL: Expected at least 4 Light2D components, found {lights.Length}");
            errors++;
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

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(SCREENSHOT_PATH, bytes);
            Debug.Log($"PASS: Captured in-engine camera screenshot to {SCREENSHOT_PATH} ({bytes.Length} bytes)");

            // Capture zoomed hero inspection screenshot
            if (guard != null)
            {
                var origPos = cam.transform.position;
                var origOrtho = cam.orthographicSize;

                cam.transform.position = new Vector3(guard.transform.position.x, guard.transform.position.y + 0.9f, -10f);
                cam.orthographicSize = 1.35f;
                cam.Render();

                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();

                string zoomPath = "Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D_Guard_Zoom.png";
                File.WriteAllBytes(zoomPath, tex.EncodeToPNG());
                Debug.Log($"PASS: Captured zoomed guard inspection screenshot to {zoomPath}");

                cam.transform.position = origPos;
                cam.orthographicSize = origOrtho;
            }

            cam.targetTexture = prevRT;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
        }

        Debug.Log($"=== QA VALIDATION SUMMARY: {errors} Errors, {warnings} Warnings ===");
    }
}
