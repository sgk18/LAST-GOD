import json

code = """using UnityEngine;

public class InspectColliders
{
    public static string Execute()
    {
        var guard = GameObject.Find("CyberGuard_Catwalk");
        if (guard == null) return "Guard not found";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Guard Colliders:");
        foreach (var c in guard.GetComponents<Collider2D>())
        {
            sb.AppendLine($"- Type: {c.GetType().Name}, isTrigger: {c.isTrigger}, bounds: {c.bounds}");
        }

        var aeron = GameObject.Find("Aeron_Protagonist");
        if (aeron != null)
        {
            sb.AppendLine("Aeron Colliders:");
            foreach (var c in aeron.GetComponents<Collider2D>())
            {
                sb.AppendLine($"- Type: {c.GetType().Name}, isTrigger: {c.isTrigger}, bounds: {c.bounds}");
            }
        }

        return sb.ToString();
    }
}"""

payload = {
    "className": "InspectColliders",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/inspect_colls.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("inspect_colls.json written")
