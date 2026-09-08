import json

code = """using UnityEngine;
using UnityEditor;

public class CheckEditorPause
{
    public static string Execute()
    {
        bool isPaused = EditorApplication.isPaused;
        bool isPlaying = EditorApplication.isPlaying;
        bool runInBg = Application.runInBackground;

        if (isPaused)
        {
            EditorApplication.isPaused = false;
        }
        Application.runInBackground = true;

        return $"isPlaying: {isPlaying}, isPaused: {isPaused} (set to false now), runInBackground: {runInBg} (set to true), Time: {Time.time}";
    }
}"""

payload = {
    "className": "CheckEditorPause",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/check_pause.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("check_pause.json written")
