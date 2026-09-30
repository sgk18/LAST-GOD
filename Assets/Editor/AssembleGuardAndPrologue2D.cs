// AssembleGuardAndPrologue2D.cs
// Automated assembly of Guard character, weapon systems, and prologue scene integration.
// ======================================================================================

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using LastGod.Core;
using LastGod.Player;
using LastGod.Player.Guard;
using LastGod.Weapons;
using LastGod.Environment;
using LastGod.UI;

public static class AssembleGuardAndPrologue2D
{
    const string SCENE_PATH = "Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D.unity";
    const string PREFAB_DIR = "Assets/Prefabs/Characters/Guard";
    const string PREFAB_PATH = "Assets/Prefabs/Characters/Guard/PF_Guard_Player.prefab";

    [MenuItem("The Last God/Assemble Guard and Playable Prologue", false, 12)]
    public static void AssembleAll()
    {
        Debug.Log(">>> [AssembleGuardAndPrologue2D] Starting Guard production and prologue assembly...");

        Directory.CreateDirectory(PREFAB_DIR);

        // 1. Open Active Scene
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        // 2. Load Assets
        Sprite idleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Characters/Guard/HeroBenchmark/GUARD_HERO_IDLE.png");
        RuntimeAnimatorController animCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Characters/Guard/Animations/Guard_Animator.controller");

        AudioClip pistolFireSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/guard_pistol_fire.wav");
        AudioClip pistolDrySfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/guard_pistol_dry.wav");
        AudioClip reloadStartSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/guard_reload_start.wav");
        AudioClip reloadFinishSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/guard_reload_finish.wav");
        AudioClip flashlightSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/guard_flashlight_click.wav");
        AudioClip radioSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/guard_radio_chime.wav");
        AudioClip footstepSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/guard_footstep.wav");
        AudioClip sparkSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/conduit_spark.wav");
        AudioClip dischargeSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/conduit_discharge.wav");

        // 3. Assemble Guard GameObject
        var existingGuards = GameObject.FindObjectsByType<GuardController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var g in existingGuards)
        {
            Object.DestroyImmediate(g.gameObject);
        }
        var strayGuards = GameObject.Find("PF_Guard_Player");
        if (strayGuards != null)
        {
            Object.DestroyImmediate(strayGuards);
        }

        GameObject guardGO = new GameObject("PF_Guard_Player");
        guardGO.tag = "Player";
        guardGO.transform.position = new Vector3(-3.20f, -2.35f, 0f);

        // Visuals
        SpriteRenderer sr = guardGO.AddComponent<SpriteRenderer>();
        sr.sprite = idleSprite;
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 10;

        Animator anim = guardGO.AddComponent<Animator>();
        anim.runtimeAnimatorController = animCtrl;

        // Physics
        Rigidbody2D rb = guardGO.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
        rb.mass = 1.0f;
        rb.gravityScale = 2.5f;

        CapsuleCollider2D col = guardGO.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.50f, 1.70f);
        col.offset = new Vector2(0f, 0.85f);
        col.direction = CapsuleDirection2D.Vertical;

        // Health
        Health hp = guardGO.AddComponent<Health>();
        // Using serialized object to assign health values
        SerializedObject soHp = new SerializedObject(hp);
        soHp.FindProperty("maxHP").intValue = 30;
        soHp.ApplyModifiedProperties();

        // AudioSource
        AudioSource aud = guardGO.AddComponent<AudioSource>();
        aud.playOnAwake = false;

        // Ground Check Point
        GameObject groundCheck = new GameObject("GroundCheckPoint");
        groundCheck.transform.SetParent(guardGO.transform, false);
        groundCheck.transform.localPosition = new Vector3(0f, 0.05f, 0f);

        // Flashlight Rig
        GameObject flashGO = new GameObject("Flashlight_Rig");
        flashGO.transform.SetParent(guardGO.transform, false);
        flashGO.transform.localPosition = new Vector3(0.15f, 0.95f, 0f);

        Light2D flashLight = flashGO.AddComponent<Light2D>();
        flashLight.lightType = Light2D.LightType.Point;
        flashLight.pointLightInnerAngle = 25f;
        flashLight.pointLightOuterAngle = 45f;
        flashLight.pointLightInnerRadius = 0.2f;
        flashLight.pointLightOuterRadius = 7.0f;
        flashLight.intensity = 1.25f;
        flashLight.color = new Color(0.91f, 0.96f, 0.98f);
        flashLight.overlapOperation = Light2D.OverlapOperation.Additive;

        FlashlightController2D flashCtrl = flashGO.AddComponent<FlashlightController2D>();
        SerializedObject soFlash = new SerializedObject(flashCtrl);
        soFlash.FindProperty("spotLight").objectReferenceValue = flashLight;
        soFlash.FindProperty("audioSource").objectReferenceValue = aud;
        soFlash.FindProperty("toggleClickSfx").objectReferenceValue = flashlightSfx;
        soFlash.ApplyModifiedProperties();

        // Weapon Rig
        GameObject weaponGO = new GameObject("SecurityPistol_Rig");
        weaponGO.transform.SetParent(guardGO.transform, false);
        weaponGO.transform.localPosition = new Vector3(0.25f, 0.85f, 0f);

        GameObject muzzleGO = new GameObject("MuzzlePoint");
        muzzleGO.transform.SetParent(weaponGO.transform, false);
        muzzleGO.transform.localPosition = new Vector3(0.35f, 0f, 0f);

        SecurityPistol pistol = weaponGO.AddComponent<SecurityPistol>();
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
        soWepCtrl.FindProperty("aimPivot").objectReferenceValue = weaponGO.transform;
        soWepCtrl.ApplyModifiedProperties();

        // Guard Controller
        GuardController guardCtrl = guardGO.AddComponent<GuardController>();
        SerializedObject soGuard = new SerializedObject(guardCtrl);
        soGuard.FindProperty("groundCheckPoint").objectReferenceValue = groundCheck.transform;
        soGuard.FindProperty("animator").objectReferenceValue = anim;
        soGuard.FindProperty("flashlight").objectReferenceValue = flashCtrl;
        soGuard.FindProperty("weaponController").objectReferenceValue = wepCtrl;
        soGuard.FindProperty("health").objectReferenceValue = hp;
        soGuard.FindProperty("audioSource").objectReferenceValue = aud;
        soGuard.FindProperty("footstepSfx").objectReferenceValue = footstepSfx;
        soGuard.ApplyModifiedProperties();

        // Save as Prefab
        PrefabUtility.SaveAsPrefabAsset(guardGO, PREFAB_PATH);
        Debug.Log($"PASS: Saved Guard Player Prefab to {PREFAB_PATH}");

        // 4. Clean up Player_Silhouette_Proxy in Scene
        GameObject proxy = GameObject.Find("Player_Silhouette_Proxy");
        if (proxy != null)
        {
            Object.DestroyImmediate(proxy);
            Debug.Log("PASS: Replaced Player_Silhouette_Proxy with active PF_Guard_Player in scene.");
        }

        // 5. Create Damaged Electrical Conduit Hazard
        GameObject conduitGO = GameObject.Find("Damaged_Conduit_Hazard");
        if (conduitGO == null)
        {
            conduitGO = new GameObject("Damaged_Conduit_Hazard");
            conduitGO.transform.position = new Vector3(0.50f, -0.70f, 0f); // On the midground catwalk

            Sprite conduitSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Characters/Guard/VFX/VFX_Damaged_Conduit_Sheet.png");
            SpriteRenderer csr = conduitGO.AddComponent<SpriteRenderer>();
            csr.sprite = conduitSprite;
            csr.sortingLayerName = "Gameplay";
            csr.sortingOrder = 5;

            BoxCollider2D ccol = conduitGO.AddComponent<BoxCollider2D>();
            ccol.size = new Vector2(0.8f, 0.8f);
            ccol.isTrigger = false;

            AudioSource caudio = conduitGO.AddComponent<AudioSource>();
            caudio.playOnAwake = false;

            Light2D clight = conduitGO.AddComponent<Light2D>();
            clight.lightType = Light2D.LightType.Point;
            clight.pointLightOuterRadius = 3.5f;
            clight.color = new Color(0.43f, 0.89f, 1.0f);
            clight.intensity = 1.8f;

            DamagedConduitHazard hazard = conduitGO.AddComponent<DamagedConduitHazard>();
            SerializedObject soHaz = new SerializedObject(hazard);
            soHaz.FindProperty("hazardLight").objectReferenceValue = clight;
            soHaz.FindProperty("audioSource").objectReferenceValue = caudio;
            soHaz.FindProperty("sparkingSfx").objectReferenceValue = sparkSfx;
            soHaz.FindProperty("dischargeSfx").objectReferenceValue = dischargeSfx;
            soHaz.ApplyModifiedProperties();

            Debug.Log("PASS: Configured Damaged Electrical Conduit Hazard at (0.50, -0.70, 0)");
        }

        // 6. Systems Rig (Radio, Comms UI, Ammo HUD, Prologue Director)
        GameObject systemsGO = GameObject.Find("[PROLOGUE_SYSTEMS]");
        if (systemsGO == null)
        {
            systemsGO = new GameObject("[PROLOGUE_SYSTEMS]");
            systemsGO.AddComponent<AudioSource>();
            RadioSystem radio = systemsGO.AddComponent<RadioSystem>();
            systemsGO.AddComponent<RadioDialogueUI>();
            systemsGO.AddComponent<AmmoHUD>();

            PrologueSequenceDirector director = systemsGO.AddComponent<PrologueSequenceDirector>();
            SerializedObject soDir = new SerializedObject(director);
            soDir.FindProperty("radioSystem").objectReferenceValue = radio;
            soDir.FindProperty("guardTransform").objectReferenceValue = guardGO.transform;
            soDir.FindProperty("radioSfx").objectReferenceValue = radioSfx;
            if (conduitGO != null)
            {
                soDir.FindProperty("conduitHazard").objectReferenceValue = conduitGO.GetComponent<DamagedConduitHazard>();
            }
            soDir.ApplyModifiedProperties();

            Debug.Log("PASS: Configured [PROLOGUE_SYSTEMS] narrative director and radio UI.");
        }

        // 7. Update Camera Follow
        Camera cam = Camera.main;
        if (cam != null)
        {
            // Attach CameraFollow if present or configure position
            var follow = cam.GetComponent<LastGod.Player.CameraFollow>();
            if (follow != null)
            {
                SerializedObject soCam = new SerializedObject(follow);
                soCam.FindProperty("target").objectReferenceValue = guardGO.transform;
                soCam.ApplyModifiedProperties();
                Debug.Log("PASS: Updated CameraFollow target to Guard.");
            }
        }

        // 8. Save Scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("PASS: Act1_Lab_2D.unity successfully updated and saved with Guard Production Assets!");
    }
}
