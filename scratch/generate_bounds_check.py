import json

code = """using UnityEngine;
using UnityEngine.Tilemaps;

public class CheckBounds
{
    public static string Execute()
    {
        var groundTm = GameObject.Find("Grid_CatwalkFloor/Tilemap_CatwalkFloor")?.GetComponent<Tilemap>();
        var oneWayTm = GameObject.Find("Grid_CatwalkFloor/Tilemap_OneWayPlatforms")?.GetComponent<Tilemap>();
        
        string gBounds = groundTm != null ? groundTm.cellBounds.ToString() : "null";
        string oBounds = oneWayTm != null ? oneWayTm.cellBounds.ToString() : "null";
        
        return $"Ground Bounds: {gBounds} | OneWay Bounds: {oBounds}";
    }
}"""

payload = {
    "className": "CheckBounds",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/check_bounds.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("Created check_bounds.json")
