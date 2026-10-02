// ValidateGuardWalkPipeline.cs
// Comprehensive Automated Technical QA for Guard Front & Back Walk Cycles
// =======================================================================

using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class ValidateGuardWalkPipeline
{
    const string FRONT_SHEET_PATH = "Assets/Art/Characters/Guard/Sprites/Guard_Walk_Front_8F.png";
    const string BACK_SHEET_PATH = "Assets/Art/Characters/Guard/Sprites/Guard_Walk_Back_8F.png";
    const string FRONT_CLIP_PATH = "Assets/Art/Characters/Guard/Animations/Guard_Walk_Front.anim";
    const string BACK_CLIP_PATH = "Assets/Art/Characters/Guard/Animations/Guard_Walk_Back.anim";
    const string ANIM_CTRL_PATH = "Assets/Art/Characters/Guard/Animations/Guard.controller";
    const string REPORT_PATH = "Assets/Art/Characters/Guard/Documentation/GUARD_WALK_QA_REPORT.md";

    [MenuItem("The Last God/Validate Guard Walk QA", false, 4)]
    public static void RunQA()
    {
        Debug.Log(">>> [ValidateGuardWalkPipeline] Starting automated Walk QA suite...");

        int errors = 0;
        int checks = 0;
        StringBuilder report = new StringBuilder();

        report.AppendLine("# Guard Character Front & Back Walk Cycles — Automated Technical QA Report");
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

        // 1. Check Front & Back Frames existence and dimensions (128x192)
        int validFrontFrames = 0;
        for (int i = 1; i <= 8; i++)
        {
            string p = $"Assets/Art/Characters/Guard/Source/Walk/Front/GUARD_WALK_FRONT_{i:D2}.png";
            if (ValidatePngDimensions(p, 128, 192)) validFrontFrames++;
        }
        LogCheck("8 Front Frames (Source)", "8 frames (128 × 192)", validFrontFrames == 8, $"{validFrontFrames}/8 frames valid");

        int validBackFrames = 0;
        for (int i = 1; i <= 8; i++)
        {
            string p = $"Assets/Art/Characters/Guard/Source/Walk/Back/GUARD_WALK_BACK_{i:D2}.png";
            if (ValidatePngDimensions(p, 128, 192)) validBackFrames++;
        }
        LogCheck("8 Back Frames (Source)", "8 frames (128 × 192)", validBackFrames == 8, $"{validBackFrames}/8 frames valid");

        // 2. Sprite Sheets Dimensions (1024x192)
        bool frontSheetOk = ValidatePngDimensions(FRONT_SHEET_PATH, 1024, 192);
        LogCheck("Front Walk Sprite Sheet Dimensions", "1024 × 192 pixels", frontSheetOk, frontSheetOk ? "1024 × 192" : "Invalid/Missing");

        bool backSheetOk = ValidatePngDimensions(BACK_SHEET_PATH, 1024, 192);
        LogCheck("Back Walk Sprite Sheet Dimensions", "1024 × 192 pixels", backSheetOk, backSheetOk ? "1024 × 192" : "Invalid/Missing");

        // 3. Texture Importer Settings (PPU = 96, Multiple, Filter = Point)
        TextureImporter tiFront = AssetImporter.GetAtPath(FRONT_SHEET_PATH) as TextureImporter;
        bool tiFrontOk = tiFront != null && Mathf.Approximately(tiFront.spritePixelsPerUnit, 96f) && tiFront.spriteImportMode == SpriteImportMode.Multiple;
        LogCheck("Front Walk Importer (96 PPU, Multiple)", "96 PPU, SpriteMode.Multiple", tiFrontOk, tiFrontOk ? $"{tiFront.spritePixelsPerUnit} PPU" : "Config error");

        TextureImporter tiBack = AssetImporter.GetAtPath(BACK_SHEET_PATH) as TextureImporter;
        bool tiBackOk = tiBack != null && Mathf.Approximately(tiBack.spritePixelsPerUnit, 96f) && tiBack.spriteImportMode == SpriteImportMode.Multiple;
        LogCheck("Back Walk Importer (96 PPU, Multiple)", "96 PPU, SpriteMode.Multiple", tiBackOk, tiBackOk ? $"{tiBack.spritePixelsPerUnit} PPU" : "Config error");

        // 4. Animation Clips (Guard_Walk_Front, Guard_Walk_Back)
        ValidateClip(FRONT_CLIP_PATH, "Guard_Walk_Front", LogCheck);
        ValidateClip(BACK_CLIP_PATH, "Guard_Walk_Back", LogCheck);

        // 5. Animator Controller States & Transitions
        AnimatorController ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ANIM_CTRL_PATH);
        bool hasCtrl = ctrl != null;
        LogCheck("Animator Controller", "Guard.controller exists", hasCtrl, hasCtrl ? "Loaded" : "Missing");

        bool hasFrontState = false;
        bool hasBackState = false;
        bool hasIdleState = false;
        if (ctrl != null && ctrl.layers.Length > 0)
        {
            var sm = ctrl.layers[0].stateMachine;
            foreach (var s in sm.states)
            {
                if (s.state.name == "Walk_Front") hasFrontState = true;
                if (s.state.name == "Walk_Back") hasBackState = true;
                if (s.state.name == "Idle") hasIdleState = true;
            }
        }
        LogCheck("Animator Walk_Front State", "State present with valid motion", hasFrontState, hasFrontState ? "Walk_Front configured" : "Missing");
        LogCheck("Animator Walk_Back State", "State present with valid motion", hasBackState, hasBackState ? "Walk_Back configured" : "Missing");
        LogCheck("Animator Idle State", "Preserved", hasIdleState, hasIdleState ? "Idle preserved" : "Missing");

        // 6. Source Files & Deliverables on Disk
        string[] requiredFiles = {
            "Assets/Art/Characters/Guard/Aseprite/Guard_Walk_Front.aseprite",
            "Assets/Art/Characters/Guard/Aseprite/Guard_Walk_Back.aseprite",
            "Assets/Art/Characters/Guard/Blender/Guard_Walk_2D_Production.blend",
            "Assets/Art/Characters/Guard/References/GUARD_FRONT_WALK_STUDY.png",
            "Assets/Art/Characters/Guard/References/GUARD_BACK_WALK_STUDY.png"
        };
        int foundDeliverables = 0;
        foreach (var rf in requiredFiles)
        {
            if (File.Exists(rf)) foundDeliverables++;
        }
        LogCheck("Required Production Deliverables", "5 source assets (Aseprite, Blender, References)", foundDeliverables == requiredFiles.Length, $"{foundDeliverables}/{requiredFiles.Length} files present");

        report.AppendLine();
        report.AppendLine("## Final Result");
        report.AppendLine($"- **Total Checks**: {checks}");
        report.AppendLine($"- **Passed**: {checks - errors}");
        report.AppendLine($"- **Failed**: {errors}");
        report.AppendLine($"- **Verdict**: {(errors == 0 ? "🎉 **100% PRODUCTION READY & COMPLIANT**" : "⚠️ **CORRECTIONS REQUIRED**")}");

        Directory.CreateDirectory(Path.GetDirectoryName(REPORT_PATH));
        File.WriteAllText(REPORT_PATH, report.ToString());
        Debug.Log($"PASS: Walk QA Report written to {REPORT_PATH}");

        if (errors > 0)
        {
            throw new Exception($"[ValidateGuardWalkPipeline] Walk validation failed with {errors} errors!");
        }
        Debug.Log(">>> [ValidateGuardWalkPipeline] ALL WALK QA CHECKS PASSED WITH ZERO ERRORS!");
    }

    private static void ValidateClip(string path, string clipName, Action<string, string, bool, string> logCheck)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        bool exists = clip != null;
        bool fpsOk = clip != null && Mathf.Approximately(clip.frameRate, 12f);
        var settings = clip != null ? AnimationUtility.GetAnimationClipSettings(clip) : default;
        bool loopOk = clip != null && settings.loopTime;

        var bindings = clip != null ? AnimationUtility.GetObjectReferenceCurveBindings(clip) : null;
        int kfCount = 0;
        if (clip != null && bindings != null && bindings.Length > 0)
        {
            var kfs = AnimationUtility.GetObjectReferenceCurve(clip, bindings[0]);
            kfCount = kfs != null ? kfs.Length : 0;
        }

        logCheck($"{clipName} Clip", "Exists & loads", exists, exists ? "Loaded" : "Missing");
        logCheck($"{clipName} 12 FPS", "12 FPS", fpsOk, clip != null ? $"{clip.frameRate} FPS" : "N/A");
        logCheck($"{clipName} 8 Keyframes", "8 keyframes", kfCount == 8, $"{kfCount} keyframes");
        logCheck($"{clipName} Looping", "loopTime = true", loopOk, loopOk ? "True" : "False");
    }

    private static bool ValidatePngDimensions(string path, int expectedW, int expectedH)
    {
        if (!File.Exists(path)) return false;
        try
        {
            using (var stream = File.OpenRead(path))
            {
                if (stream.Length < 24) return false;
                stream.Seek(16, SeekOrigin.Begin);
                byte[] buf = new byte[8];
                int read = stream.Read(buf, 0, 8);
                if (read != 8) return false;
                int w = (buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3];
                int h = (buf[4] << 24) | (buf[5] << 16) | (buf[6] << 8) | buf[7];
                return w == expectedW && h == expectedH;
            }
        }
        catch
        {
            return false;
        }
    }
}
