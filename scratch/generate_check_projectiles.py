import json

code = """using UnityEngine;
using LastGod.Combat;

public class CheckFireballSpawn
{
    public static string Execute()
    {
        var projectiles = Object.FindObjectsByType<FireballProjectile>(FindObjectsSortMode.None);
        var bullets = Object.FindObjectsByType<Bullet>(FindObjectsSortMode.None);

        string s = $"Fireballs count: {projectiles.Length}, Bullets count: {bullets.Length}";
        foreach (var p in projectiles)
        {
            s += $" | FB Pos: {p.transform.position}";
        }
        foreach (var b in bullets)
        {
            s += $" | Bullet Pos: {b.transform.position}";
        }
        return s;
    }
}"""

payload = {
    "className": "CheckFireballSpawn",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/check_projectiles.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("check_projectiles.json written")
