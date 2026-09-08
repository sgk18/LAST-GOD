import json

code = """using UnityEngine;
using LastGod.Core;

public class CheckGuardHealth
{
    public static string Execute()
    {
        var guard = GameObject.Find("CyberGuard_Catwalk");
        if (guard == null) return "Guard not found";
        var h = guard.GetComponent<Health>();
        if (h == null) return "Health not found";

        return $"Guard HP: {h.CurrentHP} / {h.MaxHP}, IsDead: {h.IsDead}";
    }
}"""

payload = {
    "className": "CheckGuardHealth",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/check_health.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("check_health.json written")
