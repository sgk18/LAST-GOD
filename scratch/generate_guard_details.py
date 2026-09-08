import json

code = """using UnityEngine;

public class InspectGuardDetails
{
    public static string Execute()
    {
        var guard = GameObject.Find("CyberGuard_Catwalk");
        if (guard == null) return "Guard not found";

        var sr = guard.GetComponent<SpriteRenderer>();
        var col = guard.GetComponent<CapsuleCollider2D>();
        var firePoint = guard.transform.Find("FirePoint");

        string s = "SR Bounds: " + (sr != null ? sr.bounds.ToString() : "null") +
                   " | Col Bounds: " + (col != null ? col.bounds.ToString() : "null") +
                   " | FirePoint World: " + (firePoint != null ? firePoint.position.ToString() : "null");
        return s;
    }
}"""

payload = {
    "className": "InspectGuardDetails",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/inspect_guard_details.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("inspect_guard_details.json written")
