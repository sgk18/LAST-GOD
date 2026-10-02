// BuildGuardWalkPipeline.cs
// Canonical 2D Guard Front and Back Walk Cycle Production & Unity Integration Pipeline
// ====================================================================================

using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;

public static class BuildGuardWalkPipeline
{
    const string FRONT_SHEET_PATH = "Assets/Art/Characters/Guard/Sprites/Guard_Walk_Front_8F.png";
    const string BACK_SHEET_PATH = "Assets/Art/Characters/Guard/Sprites/Guard_Walk_Back_8F.png";
    const string FRONT_CLIP_PATH = "Assets/Art/Characters/Guard/Animations/Guard_Walk_Front.anim";
    const string BACK_CLIP_PATH = "Assets/Art/Characters/Guard/Animations/Guard_Walk_Back.anim";
    const string ANIM_CTRL_PATH = "Assets/Art/Characters/Guard/Animations/Guard.controller";
    const string SCENE_PATH = "Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D.unity";
    const string FRONT_SCREENSHOT_PATH = "Assets/Art/Characters/Guard/Documentation/GUARD_WALK_FRONT_IN_ENGINE.png";
    const string BACK_SCREENSHOT_PATH = "Assets/Art/Characters/Guard/Documentation/GUARD_WALK_BACK_IN_ENGINE.png";

    [MenuItem("The Last God/Execute Guard Walk Pipeline", false, 3)]
    public static void ExecuteWalkPipeline()
    {
        Debug.Log(">>> [BuildGuardWalkPipeline] Starting Guard Walk Pipeline...");

        // 1. Configure Sprites & Texture Import Settings
        ConfigureWalkSpriteAssets();

        // 2. Build Animation Clips
        AnimationClip frontClip = BuildWalkAnimationClip(FRONT_CLIP_PATH, "Guard_Walk_Front", "Front");
        AnimationClip backClip = BuildWalkAnimationClip(BACK_CLIP_PATH, "Guard_Walk_Back", "Back");

        // 3. Update Animator Controller
        UpdateAnimatorController(frontClip, backClip);

        // 4. Capture In-Engine Screenshots
        CaptureWalkInEngineScreenshots();

        Debug.Log(">>> [BuildGuardWalkPipeline] Guard Walk Pipeline finished successfully!");
    }

    public static void ConfigureWalkSpriteAssets()
    {
        Debug.Log(">>> Step 1: Configuring Texture Importers for Front and Back Walk sprites...");

        // Configure individual Front Walk frames
        for (int i = 1; i <= 8; i++)
        {
            string framePath = $"Assets/Art/Characters/Guard/Sprites/Walk/Front/GUARD_WALK_FRONT_{i:02d}.png";
            ConfigureSingleSprite(framePath);
        }

        // Configure individual Back Walk frames
        for (int i = 1; i <= 8; i++)
        {
            string framePath = $"Assets/Art/Characters/Guard/Sprites/Walk/Back/GUARD_WALK_BACK_{i:02d}.png";
            ConfigureSingleSprite(framePath);
        }

        // Configure Front Walk 8-frame sheet
        ConfigureMultipleSpriteSheet(FRONT_SHEET_PATH, "Guard_Walk_Front");

        // Configure Back Walk 8-frame sheet
        ConfigureMultipleSpriteSheet(BACK_SHEET_PATH, "Guard_Walk_Back");

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    private static void ConfigureSingleSprite(string path)
    {
        if (!File.Exists(path)) return;
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;

        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 96f;
        TextureImporterSettings tis = new TextureImporterSettings();
        ti.ReadTextureSettings(tis);
        tis.spriteAlignment = (int)SpriteAlignment.Custom;
        tis.spritePivot = new Vector2(0.5f, 0.052f); // Grounded baseline at y=10/192
        ti.SetTextureSettings(tis);
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
    }

    private static void ConfigureMultipleSpriteSheet(string sheetPath, string prefix)
    {
        if (!File.Exists(sheetPath)) return;
        TextureImporter ti = AssetImporter.GetAtPath(sheetPath) as TextureImporter;
        if (ti == null) return;

        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Multiple;
        ti.spritePixelsPerUnit = 96f;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;

        #pragma warning disable CS0618
        SpriteMetaData[] sheetMeta = new SpriteMetaData[8];
        for (int i = 0; i < 8; i++)
        {
            sheetMeta[i] = new SpriteMetaData
            {
                name = $"{prefix}_{i + 1:02d}",
                rect = new Rect(i * 128, 0, 128, 192),
                alignment = (int)SpriteAlignment.Custom,
                pivot = new Vector2(0.5f, 0.052f)
            };
        }
        ti.spritesheet = sheetMeta;
        #pragma warning restore CS0618

        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
        Debug.Log($"PASS: Configured {sheetPath} with 8 multiple sprite slices ({prefix}_01..08).");
    }

    public static AnimationClip BuildWalkAnimationClip(string clipPath, string clipName, string direction)
    {
        Debug.Log($">>> Step 2: Creating {clipName} (12 FPS, Loop)...");
        Directory.CreateDirectory(Path.GetDirectoryName(clipPath));

        Sprite[] sprites = new Sprite[8];
        string sheetPath = direction == "Front" ? FRONT_SHEET_PATH : BACK_SHEET_PATH;

        for (int i = 1; i <= 8; i++)
        {
            string framePath = $"Assets/Art/Characters/Guard/Sprites/Walk/{direction}/GUARD_WALK_{direction.ToUpper()}_{i:02d}.png";
            sprites[i - 1] = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
            if (sprites[i - 1] == null)
            {
                Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
                foreach (var sa in subAssets)
                {
                    if (sa is Sprite s && s.name == $"{clipName}_{i:02d}")
                    {
                        sprites[i - 1] = s;
                        break;
                    }
                }
            }
            if (sprites[i - 1] == null)
            {
                Debug.LogError($"FAIL: Could not load sprite for {direction} walk frame {i}!");
            }
        }

        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new AnimationClip();
            clip.name = clipName;
            AssetDatabase.CreateAsset(clip, clipPath);
        }

        clip.frameRate = 12f;

        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[8];
        for (int i = 0; i < 8; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * (1f / 12f),
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

        AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        clipSettings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
        Debug.Log($"PASS: Animation clip saved at {clipPath} (Length: {clip.length:F3}s, FPS: {clip.frameRate})");
        return clip;
    }

    public static void UpdateAnimatorController(AnimationClip frontClip, AnimationClip backClip)
    {
        Debug.Log(">>> Step 3: Integrating Walk_Front and Walk_Back into Guard.controller...");
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ANIM_CTRL_PATH);
        if (controller == null)
        {
            Debug.LogError($"FAIL: Could not find AnimatorController at {ANIM_CTRL_PATH}");
            return;
        }

        // Ensure parameters exist
        bool hasWalkDir = false;
        foreach (var p in controller.parameters)
        {
            if (p.name == "WalkDirection") hasWalkDir = true;
        }
        if (!hasWalkDir)
        {
            controller.AddParameter("WalkDirection", AnimatorControllerParameterType.Int);
        }

        var rootStateMachine = controller.layers[0].stateMachine;

        // Find or create Walk_Front state
        AnimatorState stateWalkFront = null;
        AnimatorState stateWalkBack = null;
        AnimatorState stateIdle = null;

        foreach (var childState in rootStateMachine.states)
        {
            if (childState.state.name == "Walk_Front") stateWalkFront = childState.state;
            else if (childState.state.name == "Walk_Back") stateWalkBack = childState.state;
            else if (childState.state.name == "Idle") stateIdle = childState.state;
            else if (childState.state.name == "Walk")
            {
                // Update generic Walk motion to front walk
                childState.state.motion = frontClip;
            }
        }

        if (stateWalkFront == null)
        {
            stateWalkFront = rootStateMachine.AddState("Walk_Front");
        }
        stateWalkFront.motion = frontClip;

        if (stateWalkBack == null)
        {
            stateWalkBack = rootStateMachine.AddState("Walk_Back");
        }
        stateWalkBack.motion = backClip;

        if (stateIdle != null)
        {
            // Clean up old transitions between Idle and Walk_Front/Walk_Back if present
            RemoveTransitionsBetween(stateIdle, stateWalkFront);
            RemoveTransitionsBetween(stateWalkFront, stateIdle);
            RemoveTransitionsBetween(stateIdle, stateWalkBack);
            RemoveTransitionsBetween(stateWalkBack, stateIdle);
            RemoveTransitionsBetween(stateWalkFront, stateWalkBack);
            RemoveTransitionsBetween(stateWalkBack, stateWalkFront);

            // Idle -> Walk_Front (Speed > 0.1, WalkDirection == 0)
            var idleToFront = stateIdle.AddTransition(stateWalkFront);
            idleToFront.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToFront.AddCondition(AnimatorConditionMode.Equals, 0, "WalkDirection");
            idleToFront.hasExitTime = false;
            idleToFront.duration = 0.05f;

            // Walk_Front -> Idle (Speed < 0.1)
            var frontToIdle = stateWalkFront.AddTransition(stateIdle);
            frontToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            frontToIdle.hasExitTime = false;
            frontToIdle.duration = 0.05f;

            // Idle -> Walk_Back (Speed > 0.1, WalkDirection == 1)
            var idleToBack = stateIdle.AddTransition(stateWalkBack);
            idleToBack.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToBack.AddCondition(AnimatorConditionMode.Equals, 1, "WalkDirection");
            idleToBack.hasExitTime = false;
            idleToBack.duration = 0.05f;

            // Walk_Back -> Idle (Speed < 0.1)
            var backToIdle = stateWalkBack.AddTransition(stateIdle);
            backToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            backToIdle.hasExitTime = false;
            backToIdle.duration = 0.05f;

            // Walk_Front -> Walk_Back (WalkDirection == 1)
            var frontToBack = stateWalkFront.AddTransition(stateWalkBack);
            frontToBack.AddCondition(AnimatorConditionMode.Equals, 1, "WalkDirection");
            frontToBack.hasExitTime = false;
            frontToBack.duration = 0.05f;

            // Walk_Back -> Walk_Front (WalkDirection == 0)
            var backToFront = stateWalkBack.AddTransition(stateWalkFront);
            backToFront.AddCondition(AnimatorConditionMode.Equals, 0, "WalkDirection");
            backToFront.hasExitTime = false;
            backToFront.duration = 0.05f;
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("PASS: Updated Guard.controller with Walk_Front and Walk_Back states and bi-directional transitions.");
    }

    private static void RemoveTransitionsBetween(AnimatorState from, AnimatorState to)
    {
        var transitions = from.transitions;
        for (int i = transitions.Length - 1; i >= 0; i--)
        {
            if (transitions[i].destinationState == to)
            {
                from.RemoveTransition(transitions[i]);
            }
        }
    }

    public static void CaptureWalkInEngineScreenshots()
    {
        Debug.Log(">>> Step 4: Capturing in-engine camera screenshots for Walk Front & Back...");
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        var guard = GameObject.Find("PF_Guard");
        if (guard == null)
        {
            Debug.LogError("FAIL: PF_Guard not found in scene!");
            return;
        }

        var sr = guard.GetComponent<SpriteRenderer>();
        var cam = Camera.main;
        if (cam == null || sr == null) return;

        Directory.CreateDirectory(Path.GetDirectoryName(FRONT_SCREENSHOT_PATH));

        // 1. Capture Front Walk in Lab
        Sprite frontSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Characters/Guard/Sprites/Walk/Front/GUARD_WALK_FRONT_01.png");
        if (frontSprite != null) sr.sprite = frontSprite;

        CaptureCameraScreenshot(cam, FRONT_SCREENSHOT_PATH);
        Debug.Log($"PASS: Captured Front Walk in-engine screenshot to {FRONT_SCREENSHOT_PATH}");

        // 2. Capture Back Walk in Lab
        Sprite backSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Characters/Guard/Sprites/Walk/Back/GUARD_WALK_BACK_01.png");
        if (backSprite != null) sr.sprite = backSprite;

        CaptureCameraScreenshot(cam, BACK_SCREENSHOT_PATH);
        Debug.Log($"PASS: Captured Back Walk in-engine screenshot to {BACK_SCREENSHOT_PATH}");

        // Restore Idle sprite
        Sprite idleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Characters/Guard/Sprites/GUARD_IDLE_01.png");
        if (idleSprite != null) sr.sprite = idleSprite;
        EditorSceneManager.SaveScene(scene);
    }

    private static void CaptureCameraScreenshot(Camera cam, string outputPath)
    {
        Vector3 origCamPos = cam.transform.position;
        float origOrthoSize = cam.orthographicSize;

        // Position camera centered on Guard (-2.60, 1.25, -10) with ortho size 1.2
        cam.transform.position = new Vector3(-2.60f, 1.25f, -10f);
        cam.orthographicSize = 1.2f;

        RenderTexture rt = new RenderTexture(800, 1000, 24, RenderTextureFormat.ARGB32);
        var prevTarget = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(800, 1000, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 800, 1000), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(outputPath, bytes);

        cam.transform.position = origCamPos;
        cam.orthographicSize = origOrthoSize;
        cam.targetTexture = prevTarget;
        RenderTexture.active = null;

        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
    }
}
