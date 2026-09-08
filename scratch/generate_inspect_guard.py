import json

code = """using UnityEngine;

public class InspectGuard
{
    public static string Execute()
    {
        var guard = GameObject.Find("CyberGuard_Catwalk");
        if (guard == null) return "Guard not found";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Guard Position: " + guard.transform.position.ToString());
        sb.AppendLine("Guard Scale: " + guard.transform.localScale.ToString());
        foreach (Transform t in guard.transform)
        {
            sb.AppendLine("- Child: " + t.name + ", LocalPos: " + t.localPosition.ToString());
        }
        var ai = guard.GetComponent<LastGod.Enemies.EnemyAI>();
        if (ai != null)
        {
            sb.AppendLine("AI State: " + ai.State.ToString());
        }
        return sb.ToString();
    }
}"""

payload = {
    "className": "InspectGuard",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/inspect_guard.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("inspect_guard.json written")
