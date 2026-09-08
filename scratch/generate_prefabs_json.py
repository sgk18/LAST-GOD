import json

code = """using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using LastGod.Combat;

public class BuildProjectilePrefabs
{
    public static string Execute()
    {
        Directory.CreateDirectory("Assets/Prefabs/Projectiles");
        Directory.CreateDirectory("Assets/Prefabs/Combat");

        // 1. Guard_Laser_Bullet.prefab
        GameObject go = new GameObject("Guard_Laser_Bullet");
        try
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 14;
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Guard/Guard_Bullet_0.png");

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var bullet = go.AddComponent<Bullet>();
            var impactVfx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/VFX_LaserImpact_Sparks.prefab");
            var fImpact = typeof(Bullet).GetField("impactVfxPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fImpact != null) fImpact.SetValue(bullet, impactVfx);
            var fSpeed = typeof(Bullet).GetField("baseSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fSpeed != null) fSpeed.SetValue(bullet, 13.5f);
            var fDmg = typeof(Bullet).GetField("damage", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fDmg != null) fDmg.SetValue(bullet, 1);

            PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Projectiles/Guard_Laser_Bullet.prefab");
        }
        finally
        {
            Object.DestroyImmediate(go);
        }

        // 2. Aeron_Fireball_FX.prefab
        GameObject fb = new GameObject("Aeron_Fireball_FX");
        try
        {
            var sr = fb.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 15;
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Combat/Aeron_Fireball_FX.png");

            var col = fb.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;

            var rb = fb.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var proj = fb.AddComponent<FireballProjectile>();
            var detVfx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/VFX_FireballDetonation.prefab");
            var fDet = typeof(FireballProjectile).GetField("detonationVfxPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fDet != null) fDet.SetValue(proj, detVfx);
            var fSpd = typeof(FireballProjectile).GetField("speed", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fSpd != null) fSpd.SetValue(proj, 14f);
            var fD = typeof(FireballProjectile).GetField("damage", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fD != null) fD.SetValue(proj, 4);

            PrefabUtility.SaveAsPrefabAsset(fb, "Assets/Prefabs/Combat/Aeron_Fireball_FX.prefab");
        }
        finally
        {
            Object.DestroyImmediate(fb);
        }

        AssetDatabase.Refresh();
        return "SUCCESS: Built Guard_Laser_Bullet.prefab and Aeron_Fireball_FX.prefab";
    }
}"""

payload = {
    "className": "BuildProjectilePrefabs",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/build_projectile_prefabs.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("build_projectile_prefabs.json generated successfully.")
