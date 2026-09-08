import json

code = """using UnityEngine;
using System.Reflection;

public class DebugCastFireball
{
    public static string Execute()
    {
        var aeron = GameObject.Find("Aeron_Protagonist");
        if (aeron == null) return "Aeron not found";

        MonoBehaviour pc = null;
        foreach (var c in aeron.GetComponents<MonoBehaviour>())
        {
            if (c.GetType().Name == "PlayerController") { pc = c; break; }
        }
        if (pc == null) return "PlayerController not found";

        // Let's call CastFireball directly on MonoBehaviour
        try
        {
            pc.StartCoroutine("PerformCastRoutine");
            return "Started PerformCastRoutine coroutine!";
        }
        catch (System.Exception ex)
        {
            return "Exception: " + ex.ToString();
        }
    }
}"""

payload = {
    "className": "DebugCastFireball",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/debug_cast_fb.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("debug_cast_fb.json written")
