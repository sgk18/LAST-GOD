import json

code = """using UnityEngine;

public class TestFireballCombat
{
    public static string Execute()
    {
        var aeron = GameObject.Find("Aeron_Protagonist");
        if (aeron == null) return "Aeron not found";

        aeron.SendMessage("CastFireball", SendMessageOptions.DontRequireReceiver);
        return "Fireball cast SendMessage triggered!";
    }
}"""

payload = {
    "className": "TestFireballCombat",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/test_fireball.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("test_fireball.json written")
