import json

code = """using UnityEngine;

public class InspectAeronComponents
{
    public static string Execute()
    {
        var aeron = GameObject.Find("Aeron_Protagonist");
        if (aeron == null) return "Aeron not found";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Components on " + aeron.name + ":");
        foreach (var c in aeron.GetComponents<Component>())
        {
            sb.AppendLine("- " + c.GetType().FullName);
        }
        return sb.ToString();
    }
}"""

payload = {
    "className": "InspectAeronComponents",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/inspect_aeron_comps.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("inspect_aeron_comps.json written")
