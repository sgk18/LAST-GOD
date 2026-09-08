import json

code = """using UnityEngine;
using System.Reflection;

public class TestFireballReflection
{
    public static string Execute()
    {
        var aeron = GameObject.Find("Aeron_Protagonist");
        if (aeron == null) return "Aeron not found";

        Component pc = null;
        foreach (var c in aeron.GetComponents<Component>())
        {
            if (c.GetType().Name == "PlayerController")
            {
                pc = c;
                break;
            }
        }
        if (pc == null) return "PlayerController not found";

        // Unlock fireball power field
        var fPower = pc.GetType().GetField("hasFireballPower", BindingFlags.NonPublic | BindingFlags.Instance);
        if (fPower != null) fPower.SetValue(pc, true);

        var mCast = pc.GetType().GetMethod("CastFireball", BindingFlags.Public | BindingFlags.Instance);
        if (mCast == null) return "CastFireball method not found";

        mCast.Invoke(pc, null);
        return "Invoked CastFireball successfully via reflection!";
    }
}"""

payload = {
    "className": "TestFireballReflection",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/test_fireball_refl.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("test_fireball_refl.json written")
