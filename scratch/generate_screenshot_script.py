import json

code = """using UnityEngine;
using System.IO;

public class CaptureGameScreenshot
{
    public static string Execute()
    {
        int width = 1280;
        int height = 720;
        var cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null) return "No camera found";

        RenderTexture rt = new RenderTexture(width, height, 24);
        var prevRt = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = prevRt;

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);

        byte[] bytes = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);

        string outPath = @"C:\\Users\\Surya VM\\.gemini\\antigravity-ide\\brain\\142d7740-0700-4fab-bd10-b847b06cc0c7\\scratch\\gameplay_platformer_shot.png";
        File.WriteAllBytes(outPath, bytes);

        return "Screenshot saved to " + outPath;
    }
}"""

payload = {
    "className": "CaptureGameScreenshot",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/capture_screenshot.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("capture_screenshot.json written")
