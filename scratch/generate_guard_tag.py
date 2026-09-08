import json

code = """using UnityEngine;

public class CheckGuardTag
{
    public static string Execute()
    {
        var guard = GameObject.Find("CyberGuard_Catwalk");
        if (guard == null) return "Guard not found";
        return "Guard Tag: " + guard.tag;
    }
}"""

payload = {
    "className": "CheckGuardTag",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/check_guard_tag.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("check_guard_tag.json written")
