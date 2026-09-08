import json

code = """using UnityEngine;
using LastGod.Enemies;

public class InspectEnemyAIFields
{
    public static string Execute()
    {
        var guard = GameObject.Find("CyberGuard_Catwalk");
        if (guard == null) return "Guard not found";

        var ai = guard.GetComponent<EnemyAI>();
        if (ai == null) return "EnemyAI component not found";

        var so = new UnityEditor.SerializedObject(ai);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("EnemyAI serialized properties:");
        var iter = so.GetIterator();
        bool enterChildren = true;
        while (iter.NextVisible(enterChildren))
        {
            enterChildren = false;
            sb.AppendLine("- " + iter.name + " (" + iter.propertyType + "): " + iter.displayName);
        }
        return sb.ToString();
    }
}"""

payload = {
    "className": "InspectEnemyAIFields",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/inspect_ai_fields.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("inspect_ai_fields.json written")
