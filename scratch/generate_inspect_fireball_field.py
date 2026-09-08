import json

code = """using UnityEngine;
using System.Reflection;

public class InspectFireballField
{
    public static string Execute()
    {
        var aeron = GameObject.Find("Aeron_Protagonist");
        if (aeron == null) return "Aeron not found";

        Component pc = null;
        foreach (var c in aeron.GetComponents<Component>())
        {
            if (c.GetType().Name == "PlayerController") { pc = c; break; }
        }
        if (pc == null) return "PlayerController not found";

        var fPrefab = pc.GetType().GetField("fireballPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
        var fSpawn = pc.GetType().GetField("fireballSpawnPoint", BindingFlags.NonPublic | BindingFlags.Instance);
        var fPower = pc.GetType().GetField("hasFireballPower", BindingFlags.NonPublic | BindingFlags.Instance);

        var valPrefab = fPrefab != null ? fPrefab.GetValue(pc) : null;
        var valSpawn = fSpawn != null ? fSpawn.GetValue(pc) : null;
        var valPower = fPower != null ? fPower.GetValue(pc) : null;

        return $"fireballPrefab: {valPrefab} | fireballSpawnPoint: {valSpawn} | hasFireballPower: {valPower}";
    }
}"""

payload = {
    "className": "InspectFireballField",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/inspect_fireball_field.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("inspect_fireball_field.json written")
