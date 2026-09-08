import json

code = """using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LastGod.Enemies;

public class WireGuardAndSavePrefab
{
    public static string Execute()
    {
        var guard = GameObject.Find("CyberGuard_Catwalk");
        if (guard == null) return "CyberGuard_Catwalk not found";

        var ai = guard.GetComponent<EnemyAI>();
        if (ai == null) ai = guard.AddComponent<EnemyAI>();

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

        var so = new SerializedObject(ai);
        var pBullet = so.FindProperty("bulletPrefab");
        if (pBullet != null) pBullet.objectReferenceValue = bulletPrefab;
        var pVfx = so.FindProperty("muzzleFlashPrefab");
        if (pVfx != null) pVfx.objectReferenceValue = muzzleVfx;
        var pSFX = so.FindProperty("gunshotSFX");
        if (pSFX != null) pSFX.objectReferenceValue = gunshotClip;
        var pFirePoint = so.FindProperty("firePoint");
        if (pFirePoint != null) pFirePoint.objectReferenceValue = firePoint;
        var pAnim = so.FindProperty("animator");
        if (pAnim != null) pAnim.objectReferenceValue = anim;
        var pSR = so.FindProperty("spriteRenderer");
        if (pSR != null) pSR.objectReferenceValue = sr;

        so.ApplyModifiedProperties();

        // Ensure Rigidbody2D is simulated and rotation frozen
        var rb = guard.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // Save as prefab asset
        System.IO.Directory.CreateDirectory("Assets/Prefabs/Enemies");
        PrefabUtility.SaveAsPrefabAssetAndConnect(guard, "Assets/Prefabs/Enemies/CyberGuard.prefab", InteractionMode.AutomatedAction);

        // Also wire Aeron's fireballPrefab in scene if present
        var aeron = GameObject.Find("Aeron_Protagonist");
        if (aeron != null)
        {
            var pc = aeron.GetComponent<LastGod.Player.PlayerController>();
            if (pc != null)
            {
                var fbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/Aeron_Fireball_FX.prefab");
                var soPc = new SerializedObject(pc);
                var pFb = soPc.FindProperty("fireballPrefab");
                if (pFb != null) pFb.objectReferenceValue = fbPrefab;
                soPc.ApplyModifiedProperties();
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        return "SUCCESS: Guard wired, CyberGuard.prefab saved, Aeron fireball prefab wired, and scene saved!";
    }
}"""

payload = {
    "className": "WireGuardAndSavePrefab",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/wire_guard_and_save_prefab.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("wire_guard_and_save_prefab.json written")
