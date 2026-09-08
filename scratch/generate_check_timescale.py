import json

code = """using UnityEngine;

public class CheckTimeScaleAndManager
{
    public static string Execute()
    {
        var seq = Object.FindFirstObjectByType<LastGod.Core.Act1Scene1SequenceManager>();
        string seqInfo = seq != null ? "SequenceManager found" : "No SequenceManager";

        return $"Time.timeScale: {Time.timeScale}, Time.time: {Time.time}, {seqInfo}";
    }
}"""

payload = {
    "className": "CheckTimeScaleAndManager",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/check_timescale.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("check_timescale.json written")
