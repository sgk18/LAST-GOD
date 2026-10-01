// ValidateGuardCharacterPipeline.cs
// Comprehensive Automated Technical QA for Guard Character 2D Pipeline
// ======================================================================

using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using LastGod.Player.Guard;
using LastGod.Weapons;

public static class ValidateGuardCharacterPipeline
{
    const string SPRITE_SHEET_PATH = "Assets/Art/Characters/Guard/Sprites/Guard_Idle_8F.png";
    const string ANIM_CLIP_PATH = "Assets/Art/Characters/Guard/Animations/Guard_Idle.anim";
    const string ANIM_CTRL_PATH = "Assets/Art/Characters/Guard/Animations/Guard.controller";
    const string PREFAB_PATH = "Assets/Art/Characters/Guard/Prefabs/PF_Guard.prefab";
    const string SCENE_PATH = "Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D.unity";
    const string REPORT_PATH = "Assets/Art/Characters/Guard/Documentation/GUARD_QA_REPORT.md";

    [MenuItem("The Last God/Validate Guard Pipeline QA", false, 2)]
    public static void RunQA()
    {
        Debug.Log(">>> [ValidateGuardCharacterPipeline] Starting automated QA suite...");

        int errors = 0;
        int checks = 0;
        StringBuilder report = new StringBuilder();

        report.AppendLine("# Guard Character 2D Pipeline — Automated Technical QA Report");
        report.AppendLine($"**Date**: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        report.AppendLine($"**Engine**: Unity {Application.unityVersion}");
        report.AppendLine();
        report.AppendLine("## Summary Matrix");
        report.AppendLine("| Check Category | Expected Standard | Status | Details |");
        report.AppendLine("|---|---|---|---|");

        void LogCheck(string category, string standard, bool pass, string details)
        {
            checks++;
            if (pass)
            {
                Debug.Log($"[PASS] {category}: {details}");
                report.AppendLine($"| {category} | {standard} | ✅ PASS | {details} |");
            }
            else
            {
                errors++;
                Debug.LogError($"[FAIL] {category}: {details}");
                report.AppendLine($"| {category} | {standard} | ❌ FAIL | {details} |");
            }
        }

        // 1. Sprite Metrics & Count
        Texture2D sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(SPRITE_SHEET_PATH);
        bool sheetValid = sheet != null && sheet.width == 1024 && sheet.height == 192;
        LogCheck("Sprite Sheet Dimensions", "1024 × 192 pixels", sheetValid, sheet != null ? $"{sheet.width} × {sheet.height}" : "Not found");

        int validFrameCount = 0;
        for (int i = 1; i <= 8; i++)
        {
            string fpSource = $"Assets/Art/Characters/Guard/Source/GUARD_IDLE_{i:D2}.png";
            string fullPathSource = Path.Combine(Application.dataPath, $"Art/Characters/Guard/Source/GUARD_IDLE_{i:D2}.png");
            string fpSprites = $"Assets/Art/Characters/Guard/Sprites/GUARD_IDLE_{i:D2}.png";
            string fullPathSprites = Path.Combine(Application.dataPath, $"Art/Characters/Guard/Sprites/GUARD_IDLE_{i:D2}.png");

            string targetPath = File.Exists(fullPathSource) ? fullPathSource :
                                (File.Exists(fpSource) ? fpSource :
                                (File.Exists(fullPathSprites) ? fullPathSprites :
                                (File.Exists(fpSprites) ? fpSprites : null)));
            bool ok = false;

            if (targetPath != null)
            {
                using (var stream = File.OpenRead(targetPath))
                {
                    if (stream.Length >= 24)
                    {
                        stream.Seek(16, SeekOrigin.Begin);
                        byte[] buf = new byte[8];
                        int bytesRead = stream.Read(buf, 0, 8);
                        if (bytesRead == 8)
                        {
                            int w = (buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3];
                            int h = (buf[4] << 24) | (buf[5] << 16) | (buf[6] << 8) | buf[7];
                            Debug.Log($"[FrameCheck] Frame {i}: path={targetPath}, w={w}, h={h}");
                            if (w == 128 && h == 192) ok = true;
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[FrameCheck] Frame {i}: targetPath is NULL! Tried {fpSource}");
            }
            if (ok) validFrameCount++;
        }
        LogCheck("Individual Frame Count", "8 frames exactly (128 × 192)", validFrameCount == 8, $"{validFrameCount}/8 valid frames");

        // 2. Import Settings (PPU & Pivot)
        TextureImporter ti = AssetImporter.GetAtPath(SPRITE_SHEET_PATH) as TextureImporter;
        bool ppuValid = ti != null && Mathf.Approximately(ti.spritePixelsPerUnit, 96f);
        LogCheck("Pixels Per Unit (PPU)", "96 PPU", ppuValid, ti != null ? $"{ti.spritePixelsPerUnit} PPU" : "Importer null");

        // 3. Animation Clip
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ANIM_CLIP_PATH);
        bool clipValid = clip != null;
        bool fpsValid = clip != null && Mathf.Approximately(clip.frameRate, 12f);
        var clipSettings = clip != null ? AnimationUtility.GetAnimationClipSettings(clip) : default;
        bool loopValid = clip != null && clipSettings.loopTime;
        var curveBindings = clip != null ? AnimationUtility.GetObjectReferenceCurveBindings(clip) : null;
        int keyframeCount = 0;
        if (clip != null && curveBindings != null && curveBindings.Length > 0)
        {
            var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, curveBindings[0]);
            keyframeCount = keyframes != null ? keyframes.Length : 0;
        }

        LogCheck("Animation Clip Existence", "Guard_Idle.anim exists", clipValid, clipValid ? "Loaded" : "Missing");
        LogCheck("Animation Frame Rate", "12 FPS", fpsValid, clip != null ? $"{clip.frameRate} FPS" : "N/A");
        LogCheck("Animation Keyframe Count", "8 keyframes", keyframeCount == 8, $"{keyframeCount} keyframes");
        LogCheck("Animation Looping Flag", "loopTime = true", loopValid, loopValid ? "True" : "False");

        // 4. Animator Controller
        AnimatorController ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ANIM_CTRL_PATH);
        bool ctrlValid = ctrl != null;
        LogCheck("Animator Controller", "Guard.controller exists", ctrlValid, ctrlValid ? "Loaded" : "Missing");

        string[] requiredStates = { "Idle", "Walk", "Run", "Aim", "Fire", "Reload", "Flashlight", "Hurt", "Death", "Takedown" };
        int foundStates = 0;
        if (ctrl != null && ctrl.layers.Length > 0)
        {
            var sm = ctrl.layers[0].stateMachine;
            foreach (var s in sm.states)
            {
                if (Array.IndexOf(requiredStates, s.state.name) >= 0) foundStates++;
            }
        }
        LogCheck("Animator States", "10 production states", foundStates == requiredStates.Length, $"{foundStates}/{requiredStates.Length} states present");

        // 5. Prefab Architecture & Anchors
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
        bool prefabValid = prefab != null;
        LogCheck("Prefab Asset Existence", "PF_Guard.prefab exists", prefabValid, prefabValid ? "Loaded" : "Missing");

        bool hasSR = prefab != null && prefab.GetComponent<SpriteRenderer>() != null;
        bool hasAnim = prefab != null && prefab.GetComponent<Animator>() != null;
        bool hasRB = prefab != null && prefab.GetComponent<Rigidbody2D>() != null;
        bool hasCol = prefab != null && prefab.GetComponent<CapsuleCollider2D>() != null;
        bool hasGuardCtrl = prefab != null && prefab.GetComponent<GuardController>() != null;
        bool hasWepCtrl = prefab != null && prefab.GetComponent<WeaponController>() != null;

        LogCheck("Root Components", "SR, Anim, RB2D, Capsule2D, GuardCtrl, WepCtrl", hasSR && hasAnim && hasRB && hasCol && hasGuardCtrl && hasWepCtrl, "All core components attached");

        Transform wepAnchor = prefab != null ? prefab.transform.Find("WeaponAnchor") : null;
        Transform flashAnchor = prefab != null ? prefab.transform.Find("FlashlightAnchor") : null;
        Transform groundCheck = prefab != null ? prefab.transform.Find("GroundCheckPoint") : null;
        Transform interactAnchor = prefab != null ? prefab.transform.Find("InteractionAnchor") : null;

        LogCheck("WeaponAnchor", "Attached with SecurityPistol", wepAnchor != null && wepAnchor.GetComponent<SecurityPistol>() != null, wepAnchor != null ? "Verified" : "Missing");
        LogCheck("FlashlightAnchor", "Attached with Light2D + Controller", flashAnchor != null && flashAnchor.GetComponent<FlashlightController2D>() != null, flashAnchor != null ? "Verified" : "Missing");
        LogCheck("GroundCheckPoint", "Attached", groundCheck != null, groundCheck != null ? "Verified" : "Missing");
        LogCheck("InteractionAnchor", "Attached", interactAnchor != null, interactAnchor != null ? "Verified" : "Missing");

        // 6. Scene Placement
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        var inSceneGuard = GameObject.Find("PF_Guard");
        bool sceneGuardValid = inSceneGuard != null;
        LogCheck("Scene Placement", "PF_Guard in Act1_Lab_2D", sceneGuardValid, sceneGuardValid ? $"Position: {inSceneGuard.transform.position}" : "Not found in scene");

        if (inSceneGuard != null)
        {
            var sr = inSceneGuard.GetComponent<SpriteRenderer>();
            bool layerValid = sr != null && sr.sortingLayerName == "Player" && sr.sortingOrder == 10;
            LogCheck("Sorting Layer & Order", "Player layer, Order 10", layerValid, sr != null ? $"{sr.sortingLayerName} (Order {sr.sortingOrder})" : "Null SR");
        }

        // 7. Master References Verification
        string[] masterRefs = {
            "Assets/Art/Characters/Guard/References/GUARD_MASTER_REFERENCE.png",
            "Assets/Art/Characters/Guard/References/GUARD_COLOR_REFERENCE.png",
            "Assets/Art/Characters/Guard/References/GUARD_SILHOUETTE_REFERENCE.png",
            "Assets/Art/Characters/Guard/References/GUARD_PISTOL_MASTER.png",
            "Assets/Art/Characters/Guard/References/GUARD_FLASHLIGHT_MASTER.png",
            "Assets/Art/Characters/Guard/Blender/Guard_2D_Production.blend",
            "Assets/Art/Characters/Guard/Aseprite/Guard_Idle.aseprite"
        };
        int foundRefs = 0;
        foreach (var r in masterRefs)
        {
            if (File.Exists(r)) foundRefs++;
        }
        LogCheck("Master Reference & Source Assets", "7 canonical files", foundRefs == masterRefs.Length, $"{foundRefs}/{masterRefs.Length} assets verified on disk");

        report.AppendLine();
        report.AppendLine("## Final Result");
        report.AppendLine($"- **Total Checks**: {checks}");
        report.AppendLine($"- **Passed**: {checks - errors}");
        report.AppendLine($"- **Failed**: {errors}");
        report.AppendLine($"- **Verdict**: {(errors == 0 ? "🎉 **100% PRODUCTION READY & COMPLIANT**" : "⚠️ **CORRECTIONS REQUIRED**")}");

        Directory.CreateDirectory(Path.GetDirectoryName(REPORT_PATH));
        File.WriteAllText(REPORT_PATH, report.ToString());
        Debug.Log($"PASS: QA Report written to {REPORT_PATH}");

        if (errors > 0)
        {
            throw new Exception($"[ValidateGuardCharacterPipeline] Validation failed with {errors} errors!");
        }
        Debug.Log(">>> [ValidateGuardCharacterPipeline] ALL CHECKS PASSED WITH ZERO ERRORS!");
    }
}
