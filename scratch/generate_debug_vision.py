import json

code = """using UnityEngine;
using LastGod.Enemies;

public class DebugGuardVision
{
    public static string Execute()
    {
        var guard = GameObject.Find("CyberGuard_Catwalk");
        if (guard == null) return "Guard not found";
        var ai = guard.GetComponent<EnemyAI>();
        if (ai == null) return "EnemyAI not found";

        var aeron = GameObject.Find("Aeron_Protagonist");
        if (aeron == null) return "Aeron not found";

        Vector2 eyePos = (Vector2)guard.transform.position + Vector2.up * 1.5f;
        Vector2 playerPos = (Vector2)aeron.transform.position + Vector2.up * 1.0f;
        Vector2 toPlayer = playerPos - eyePos;
        float dist = toPlayer.magnitude;

        int groundLayer = LayerMask.GetMask("Ground") | (1 << 8);
        RaycastHit2D hit = Physics2D.Raycast(eyePos, toPlayer.normalized, dist, groundLayer);

        string hitInfo = hit.collider != null ? (hit.collider.name + " on " + hit.collider.gameObject.name + " at " + hit.point) : "NONE (CLEAR LOS!)";

        return "AI State: " + ai.State + 
               " | GuardPos: " + guard.transform.position + 
               " | FacingRight: " + (guard.transform.localScale.x > 0) +
               " | Dist: " + dist + 
               " | Hit: " + hitInfo;
    }
}"""

payload = {
    "className": "DebugGuardVision",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/debug_vision.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("debug_vision.json written")
