// BuildGuardCharacterPipeline.cs
// Canonical 2D Guard Character Production & Unity Integration Pipeline
// ====================================================================

using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;
using LastGod.Core;
using LastGod.Player;
using LastGod.Player.Guard;
using LastGod.Weapons;

public static class BuildGuardCharacterPipeline
{
    const string SPRITE_SHEET_PATH = "Assets/Art/Characters/Guard/Sprites/Guard_Idle_8F.png";
    const string ANIM_CLIP_PATH = "Assets/Art/Characters/Guard/Animations/Guard_Idle.anim";
    const string ANIM_CTRL_PATH = "Assets/Art/Characters/Guard/Animations/Guard.controller";
    const string PREFAB_PATH = "Assets/Art/Characters/Guard/Prefabs/PF_Guard.prefab";
    const string PREFAB_PLAYER_PATH = "Assets/Prefabs/Characters/Guard/PF_Guard_Player.prefab";
    const string SCENE_PATH = "Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D.unity";
    const string SCREENSHOT_FULL_PATH = "Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D_Screenshot.png";
    const string SCREENSHOT_ZOOM_PATH = "Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D_Guard_Zoom.png";
    const string DOC_SCREENSHOT_PATH = "Assets/Art/Characters/Guard/Documentation/GUARD_IN_ENGINE_SCREENSHOT.png";

    [MenuItem("The Last God/Execute Guard Character Pipeline", false, 1)]
    public static void ExecutePipeline()
    {
        Debug.Log(">>> [BuildGuardCharacterPipeline] Starting canonical Guard character pipeline...");

        // 1. Configure Sprites & Texture Import Settings
        ConfigureSpriteAssets();

        // 2. Build 8-Frame Idle Animation Clip
        AnimationClip idleClip = BuildIdleAnimationClip();

        // 3. Build Animator Controller
        RuntimeAnimatorController animController = BuildAnimatorController(idleClip);

        // 4. Assemble Prefab
        GameObject prefabAsset = AssembleGuardPrefab(animController);

        // 5. Integrate into Act 1 Lab Scene
        IntegrateIntoLabScene(prefabAsset);

        // 6. Capture Visual QA Screenshots
        CaptureInEngineScreenshots();

        Debug.Log(">>> [BuildGuardCharacterPipeline] Pipeline execution finished successfully!");
    }

    public static void ConfigureSpriteAssets()
    {
        Debug.Log(">>> Step 1: Configuring Texture Importers for Guard sprites...");

        // Configure individual frames
        for (int i = 1; i <= 8; i++)
        {
            string framePath = $"Assets/Art/Characters/Guard/Sprites/GUARD_IDLE_{i:02d}.png";
            if (File.Exists(framePath))
            {
                TextureImporter ti = AssetImporter.GetAtPath(framePath) as TextureImporter;
                if (ti != null)
                {
                    ti.textureType = TextureImporterType.Sprite;
                    ti.spriteImportMode = SpriteImportMode.Single;
                    ti.spritePixelsPerUnit = 96f;
                    TextureImporterSettings tis = new TextureImporterSettings();
                    ti.ReadTextureSettings(tis);
                    tis.spriteAlignment = (int)SpriteAlignment.Custom;
                    tis.spritePivot = new Vector2(0.5f, 0.052f); // Grounded at soles (y=10/192)
                    ti.SetTextureSettings(tis);
                    ti.filterMode = FilterMode.Point;
                    ti.textureCompression = TextureImporterCompression.Uncompressed;
                    ti.mipmapEnabled = false;
                    EditorUtility.SetDirty(ti);
                    ti.SaveAndReimport();
                }
            }
        }

        // Configure 8-frame horizontal sprite sheet
        if (File.Exists(SPRITE_SHEET_PATH))
        {
            TextureImporter ti = AssetImporter.GetAtPath(SPRITE_SHEET_PATH) as TextureImporter;
            if (ti != null)
            {
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
                        name = $"Guard_Idle_{i + 1:02d}",
                        rect = new Rect(i * 128, 0, 128, 192),
                        alignment = (int)SpriteAlignment.Custom,
                        pivot = new Vector2(0.5f, 0.052f)
                    };
                }
                ti.spritesheet = sheetMeta;
                #pragma warning restore CS0618

                EditorUtility.SetDirty(ti);
                ti.SaveAndReimport();
                Debug.Log("PASS: Configured Guard_Idle_8F.png with 8 multiple sprite slices.");
            }
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    public static AnimationClip BuildIdleAnimationClip()
    {
        Debug.Log(">>> Step 2: Creating 8-frame Idle Animation Clip (12 FPS, Loop)...");
        Directory.CreateDirectory(Path.GetDirectoryName(ANIM_CLIP_PATH));

        // Load the 8 sprites
        Sprite[] sprites = new Sprite[8];
        for (int i = 1; i <= 8; i++)
        {
            string framePath = $"Assets/Art/Characters/Guard/Sprites/GUARD_IDLE_{i:02d}.png";
            sprites[i - 1] = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
            if (sprites[i - 1] == null)
            {
                // Fallback to sheet sub-asset
                Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(SPRITE_SHEET_PATH);
                foreach (var sa in subAssets)
                {
                    if (sa is Sprite s && s.name == $"Guard_Idle_{i:02d}")
                    {
                        sprites[i - 1] = s;
                        break;
                    }
                }
            }
            if (sprites[i - 1] == null)
            {
                Debug.LogError($"FAIL: Could not load sprite for frame {i}!");
            }
        }

        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ANIM_CLIP_PATH);
        if (clip == null)
        {
            clip = new AnimationClip();
            clip.name = "Guard_Idle";
            AssetDatabase.CreateAsset(clip, ANIM_CLIP_PATH);
        }

        clip.frameRate = 12f;

        // Configure SpriteRenderer object reference curve
        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        // 8 frames at 12 FPS: each frame duration = 1/12 = 0.08333s
        // Frame times: 0.0, 0.0833, 0.1667, 0.25, 0.3333, 0.4167, 0.50, 0.5833, ending at 0.6667
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

        // Configure Loop Time
        AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        clipSettings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
        Debug.Log($"PASS: Animation clip saved at {ANIM_CLIP_PATH} (Length: {clip.length:F3}s, FPS: {clip.frameRate})");
        return clip;
    }

    public static RuntimeAnimatorController BuildAnimatorController(AnimationClip idleClip)
    {
        Debug.Log(">>> Step 3: Creating Animator Controller with comprehensive combat/exploration states...");
        Directory.CreateDirectory(Path.GetDirectoryName(ANIM_CTRL_PATH));

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ANIM_CTRL_PATH);

        // Parameters
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsAiming", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Fire", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Reload", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("IsHurt", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Takedown", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("FlashlightOn", AnimatorControllerParameterType.Bool);

        // State Machine
        var rootStateMachine = controller.layers[0].stateMachine;

        // States
        var stateIdle = rootStateMachine.AddState("Idle");
        stateIdle.motion = idleClip;
        rootStateMachine.defaultState = stateIdle;

        var stateWalk = rootStateMachine.AddState("Walk");
        stateWalk.motion = idleClip; // Base motion until dedicated walk cycle is imported

        var stateRun = rootStateMachine.AddState("Run");
        stateRun.motion = idleClip;

        var stateAim = rootStateMachine.AddState("Aim");
        stateAim.motion = idleClip;

        var stateFire = rootStateMachine.AddState("Fire");
        stateFire.motion = idleClip;

        var stateReload = rootStateMachine.AddState("Reload");
        stateReload.motion = idleClip;

        var stateFlashlight = rootStateMachine.AddState("Flashlight");
        stateFlashlight.motion = idleClip;

        var stateHurt = rootStateMachine.AddState("Hurt");
        stateHurt.motion = idleClip;

        var stateDeath = rootStateMachine.AddState("Death");
        stateDeath.motion = idleClip;

        var stateTakedown = rootStateMachine.AddState("Takedown");
        stateTakedown.motion = idleClip;

        // Transitions: Idle <-> Walk
        var idleToWalk = stateIdle.AddTransition(stateWalk);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.05f;

        var walkToIdle = stateWalk.AddTransition(stateIdle);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.05f;

        // Transitions: AnyState -> Hurt, Death, Takedown
        var anyToHurt = rootStateMachine.AddAnyStateTransition(stateHurt);
        anyToHurt.AddCondition(AnimatorConditionMode.If, 0, "IsHurt");
        anyToHurt.hasExitTime = false;
        anyToHurt.duration = 0.02f;

        var hurtToIdle = stateHurt.AddTransition(stateIdle);
        hurtToIdle.hasExitTime = true;
        hurtToIdle.exitTime = 0.8f;
        hurtToIdle.duration = 0.05f;

        var anyToDeath = rootStateMachine.AddAnyStateTransition(stateDeath);
        anyToDeath.AddCondition(AnimatorConditionMode.If, 0, "IsDead");
        anyToDeath.hasExitTime = false;
        anyToDeath.duration = 0.02f;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log($"PASS: AnimatorController saved at {ANIM_CTRL_PATH} with 10 production states.");
        return controller;
    }

    public static GameObject AssembleGuardPrefab(RuntimeAnimatorController animController)
    {
        Debug.Log(">>> Step 4: Assembling canonical PF_Guard Prefab...");
        Directory.CreateDirectory(Path.GetDirectoryName(PREFAB_PATH));
        Directory.CreateDirectory(Path.GetDirectoryName(PREFAB_PLAYER_PATH));

        GameObject guardGO = new GameObject("PF_Guard");
        guardGO.tag = "Player";

        // 1. Sprite Renderer
        SpriteRenderer sr = guardGO.AddComponent<SpriteRenderer>();
        Sprite baseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Characters/Guard/Sprites/GUARD_IDLE_01.png");
        sr.sprite = baseSprite;
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 10;

        // 2. Animator
        Animator anim = guardGO.AddComponent<Animator>();
        anim.runtimeAnimatorController = animController;

        // 3. Rigidbody2D (Dynamic, Continuous, Freeze Rotation)
        Rigidbody2D rb = guardGO.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
        rb.mass = 1.0f;
        rb.gravityScale = 2.5f;

        // 4. CapsuleCollider2D (1.75m height, 0.50m width, grounded base at offset y = 0.88m)
        CapsuleCollider2D col = guardGO.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.50f, 1.70f);
        col.offset = new Vector2(0f, 0.88f);
        col.direction = CapsuleDirection2D.Vertical;

        // 5. Health
        Health hp = guardGO.AddComponent<Health>();
        SerializedObject soHp = new SerializedObject(hp);
        soHp.FindProperty("maxHP").intValue = 30;
        soHp.ApplyModifiedProperties();

        // 6. AudioSource
        AudioSource aud = guardGO.AddComponent<AudioSource>();
        aud.playOnAwake = false;

        // 7. GroundCheckPoint
        GameObject groundCheck = new GameObject("GroundCheckPoint");
        groundCheck.transform.SetParent(guardGO.transform, false);
        groundCheck.transform.localPosition = new Vector3(0f, 0.05f, 0f);

        // 8. FlashlightAnchor & Light2D
        GameObject flashAnchor = new GameObject("FlashlightAnchor");
        flashAnchor.transform.SetParent(guardGO.transform, false);
        flashAnchor.transform.localPosition = new Vector3(0.15f, 0.95f, 0f);

        Light2D flashLight = flashAnchor.AddComponent<Light2D>();
        flashLight.lightType = Light2D.LightType.Point;
        flashLight.pointLightInnerAngle = 25f;
        flashLight.pointLightOuterAngle = 45f;
        flashLight.pointLightInnerRadius = 0.2f;
        flashLight.pointLightOuterRadius = 7.0f;
        flashLight.intensity = 1.35f;
        flashLight.color = new Color(0.91f, 0.96f, 0.98f);
        flashLight.overlapOperation = Light2D.OverlapOperation.Additive;

        FlashlightController2D flashCtrl = flashAnchor.AddComponent<FlashlightController2D>();
        AudioClip flashlightSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/guard_flashlight_click.wav");
        SerializedObject soFlash = new SerializedObject(flashCtrl);
        soFlash.FindProperty("spotLight").objectReferenceValue = flashLight;
        soFlash.FindProperty("audioSource").objectReferenceValue = aud;
        soFlash.FindProperty("toggleClickSfx").objectReferenceValue = flashlightSfx;
        soFlash.ApplyModifiedProperties();

        // 9. WeaponAnchor & SecurityPistol
        GameObject wepAnchor = new GameObject("WeaponAnchor");
        wepAnchor.transform.SetParent(guardGO.transform, false);
        wepAnchor.transform.localPosition = new Vector3(0.20f, 0.85f, 0f);

        GameObject muzzleGO = new GameObject("MuzzlePoint");
        muzzleGO.transform.SetParent(wepAnchor.transform, false);
        muzzleGO.transform.localPosition = new Vector3(0.35f, 0f, 0f);

        SecurityPistol pistol = wepAnchor.AddComponent<SecurityPistol>();
        AudioClip pistolFireSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/guard_pistol_fire.wav");
        AudioClip pistolDrySfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/guard_pistol_dry.wav");
        AudioClip reloadStartSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/guard_reload_start.wav");
        AudioClip reloadFinishSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/guard_reload_finish.wav");

        SerializedObject soPistol = new SerializedObject(pistol);
        soPistol.FindProperty("muzzlePoint").objectReferenceValue = muzzleGO.transform;
        soPistol.FindProperty("audioSource").objectReferenceValue = aud;
        soPistol.FindProperty("fireSfx").objectReferenceValue = pistolFireSfx;
        soPistol.FindProperty("dryFireSfx").objectReferenceValue = pistolDrySfx;
        soPistol.FindProperty("reloadStartSfx").objectReferenceValue = reloadStartSfx;
        soPistol.FindProperty("reloadFinishSfx").objectReferenceValue = reloadFinishSfx;
        soPistol.ApplyModifiedProperties();

        WeaponController wepCtrl = guardGO.AddComponent<WeaponController>();
        SerializedObject soWepCtrl = new SerializedObject(wepCtrl);
        soWepCtrl.FindProperty("defaultWeaponMono").objectReferenceValue = pistol;
        soWepCtrl.FindProperty("aimPivot").objectReferenceValue = wepAnchor.transform;
        soWepCtrl.ApplyModifiedProperties();

        // 10. InteractionAnchor
        GameObject interactAnchor = new GameObject("InteractionAnchor");
        interactAnchor.transform.SetParent(guardGO.transform, false);
        interactAnchor.transform.localPosition = new Vector3(0f, 1.0f, 0f);

        // 11. GuardController
        GuardController guardCtrl = guardGO.AddComponent<GuardController>();
        AudioClip footstepSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/guard_footstep.wav");
        SerializedObject soGuard = new SerializedObject(guardCtrl);
        soGuard.FindProperty("groundCheckPoint").objectReferenceValue = groundCheck.transform;
        soGuard.FindProperty("animator").objectReferenceValue = anim;
        soGuard.FindProperty("flashlight").objectReferenceValue = flashCtrl;
        soGuard.FindProperty("weaponController").objectReferenceValue = wepCtrl;
        soGuard.FindProperty("health").objectReferenceValue = hp;
        soGuard.FindProperty("audioSource").objectReferenceValue = aud;
        soGuard.FindProperty("footstepSfx").objectReferenceValue = footstepSfx;
        soGuard.ApplyModifiedProperties();

        // Save Prefab Assets
        GameObject prefabPrimary = PrefabUtility.SaveAsPrefabAsset(guardGO, PREFAB_PATH);
        GameObject prefabPlayer = PrefabUtility.SaveAsPrefabAsset(guardGO, PREFAB_PLAYER_PATH);

        Object.DestroyImmediate(guardGO);
        Debug.Log($"PASS: Saved PF_Guard prefab to {PREFAB_PATH} and {PREFAB_PLAYER_PATH}");
        return prefabPrimary;
    }

    public static void IntegrateIntoLabScene(GameObject prefabAsset)
    {
        Debug.Log(">>> Step 5: Integrating canonical Guard into Act1_Lab_2D scene...");
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        // Remove old guards/proxies
        var oldGuards = Object.FindObjectsByType<GuardController>(FindObjectsInactive.Include);
        foreach (var g in oldGuards)
        {
            Object.DestroyImmediate(g.gameObject);
        }
        var stray = GameObject.Find("PF_Guard");
        if (stray != null) Object.DestroyImmediate(stray);
        var strayPlayer = GameObject.Find("PF_Guard_Player");
        if (strayPlayer != null) Object.DestroyImmediate(strayPlayer);
        var proxy = GameObject.Find("Player_Silhouette_Proxy");
        if (proxy != null) Object.DestroyImmediate(proxy);

        // Instantiate new Prefab
        GameObject instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
        instance.name = "PF_Guard";
        // Floor level at x = -3.20, y = -2.35m
        instance.transform.position = new Vector3(-3.20f, -2.35f, 0f);

        EditorUtility.SetDirty(instance);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("PASS: Act1_Lab_2D.unity successfully updated with canonical PF_Guard instance.");
    }

    public static void CaptureInEngineScreenshots()
    {
        Debug.Log(">>> Step 6: Capturing verified in-engine camera screenshots...");
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("FAIL: Camera.main not found for screenshot capture!");
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(DOC_SCREENSHOT_PATH));

        // 1. Full Laboratory In-Engine View
        RenderTexture rtFull = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        var prevTarget = cam.targetTexture;
        cam.targetTexture = rtFull;
        cam.Render();

        RenderTexture.active = rtFull;
        Texture2D texFull = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        texFull.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        texFull.Apply();

        byte[] bytesFull = texFull.EncodeToPNG();
        File.WriteAllBytes(SCREENSHOT_FULL_PATH, bytesFull);
        File.WriteAllBytes(DOC_SCREENSHOT_PATH, bytesFull);
        Debug.Log($"PASS: In-Engine Lab Screenshot saved to {SCREENSHOT_FULL_PATH} and {DOC_SCREENSHOT_PATH}");

        // 2. Focused Zoom View on Guard
        Vector3 origCamPos = cam.transform.position;
        float origOrthoSize = cam.orthographicSize;

        // Position camera centered on Guard (-3.20, -1.5, -10) with close ortho size (1.5)
        cam.transform.position = new Vector3(-3.20f, -1.50f, -10f);
        cam.orthographicSize = 1.6f;

        RenderTexture rtZoom = new RenderTexture(800, 1000, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rtZoom;
        cam.Render();

        RenderTexture.active = rtZoom;
        Texture2D texZoom = new Texture2D(800, 1000, TextureFormat.RGB24, false);
        texZoom.ReadPixels(new Rect(0, 0, 800, 1000), 0, 0);
        texZoom.Apply();

        byte[] bytesZoom = texZoom.EncodeToPNG();
        File.WriteAllBytes(SCREENSHOT_ZOOM_PATH, bytesZoom);
        Debug.Log($"PASS: Focused Guard In-Engine Zoom saved to {SCREENSHOT_ZOOM_PATH}");

        // Restore camera
        cam.transform.position = origCamPos;
        cam.orthographicSize = origOrthoSize;
        cam.targetTexture = prevTarget;
        RenderTexture.active = null;

        Object.DestroyImmediate(rtFull);
        Object.DestroyImmediate(texFull);
        Object.DestroyImmediate(rtZoom);
        Object.DestroyImmediate(texZoom);
    }
}
