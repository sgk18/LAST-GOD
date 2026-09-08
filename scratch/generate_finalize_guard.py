import json

code = """using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LastGod.Enemies;
using LastGod.Core;

public class FinalizeGuardSetup
{
    public static string Execute()
    {
        var guard = GameObject.Find("CyberGuard_Catwalk");
        if (guard == null) return "CyberGuard_Catwalk not found";

        // 1. Tag
        guard.tag = "Enemy";

        // 2. Scale / Facing
        guard.transform.localScale = new Vector3(-1f, 1f, 1f);

        // 3. Components
        var ai = guard.GetComponent<EnemyAI>();
        if (ai == null) ai = guard.AddComponent<EnemyAI>();

        var health = guard.GetComponent<Health>();
        if (health == null) health = guard.AddComponent<Health>();
        var soHealth = new SerializedObject(health);
        soHealth.FindProperty("maxHP").intValue = 6;
        soHealth.ApplyModifiedProperties();

        var bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectiles/Guard_Laser_Bullet.prefab");
        var muzzleVfx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/VFX_MuzzleFlash_Cyber.prefab");
        var gunshotClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/gunshot.wav");
        var animController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Animations/Guard/Guard.controller");
        var idleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Guard/Guard_Idle_0.png");

        var sr = guard.GetComponent<SpriteRenderer>();
        if (sr != null && idleSprite != null)
        {
            sr.sprite = idleSprite;
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 9;
        }

        var anim = guard.GetComponent<Animator>();
        if (anim != null && animController != null)
        {
            anim.runtimeAnimatorController = animController;
            anim.enabled = true;
        }

        Transform firePoint = guard.transform.Find("FirePoint");
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(guard.transform, false);
            fp.transform.localPosition = new Vector3(1.0f, 2.2f, 0f);
            firePoint = fp.transform;
        }

        var soAi = new SerializedObject(ai);
        var pBullet = soAi.FindProperty("bulletPrefab");
        if (pBullet != null) pBullet.objectReferenceValue = bulletPrefab;
        var pVfx = soAi.FindProperty("muzzleFlashPrefab");
        if (pVfx != null) pVfx.objectReferenceValue = muzzleVfx;
        var pSFX = soAi.FindProperty("gunshotSFX");
        if (pSFX != null) pSFX.objectReferenceValue = gunshotClip;
        var pFirePoint = soAi.FindProperty("firePoint");
        if (pFirePoint != null) pFirePoint.objectReferenceValue = firePoint;
        var pAnim = soAi.FindProperty("animator");
        if (pAnim != null) pAnim.objectReferenceValue = anim;
        var pSR = soAi.FindProperty("spriteRenderer");
        if (pSR != null) pSR.objectReferenceValue = sr;
        var pMaxAtk = soAi.FindProperty("maxAttackRange");
        if (pMaxAtk != null) pMaxAtk.floatValue = 9.0f;
        var pMinAtk = soAi.FindProperty("minAttackRange");
        if (pMinAtk != null) pMinAtk.floatValue = 3.5f;
        var pGroundLayer = soAi.FindProperty("groundLayer");
        if (pGroundLayer != null) pGroundLayer.intValue = LayerMask.GetMask("Ground") | (1 << 8);

        soAi.ApplyModifiedProperties();

        var rb = guard.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // Save as prefab asset
        PrefabUtility.SaveAsPrefabAssetAndConnect(guard, "Assets/Prefabs/Enemies/CyberGuard.prefab", InteractionMode.AutomatedAction);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        return "SUCCESS: Finalized Guard setup, Enemy tag set, prefab saved, and scene saved!";
    }
}"""

payload = {
    "className": "FinalizeGuardSetup",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/finalize_guard_setup.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("finalize_guard_setup.json written")
