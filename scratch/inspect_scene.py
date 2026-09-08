import json

code = """using UnityEngine;
using UnityEngine.Tilemaps;

public class CheckSceneTilemaps
{
    public static string Execute()
    {
        var grid = GameObject.Find("Grid_CatwalkFloor");
        if (grid == null) return "Grid_CatwalkFloor NOT found";
        
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Found Grid_CatwalkFloor:");
        foreach (Transform child in grid.transform)
        {
            var tm = child.GetComponent<Tilemap>();
            var col = child.GetComponent<TilemapCollider2D>();
            var eff = child.GetComponent<PlatformEffector2D>();
            int count = tm != null ? tm.GetUsedTilesCount() : 0;
            sb.AppendLine("- " + child.name + ": Tilemap=" + (tm != null) + ", Tiles=" + count + ", Collider=" + (col != null) + ", Effector=" + (eff != null) + ", Layer=" + LayerMask.LayerToName(child.gameObject.layer) + " (" + child.gameObject.layer + ")");
        }

        var aeron = GameObject.Find("Aeron_Protagonist");
        if (aeron != null)
        {
            sb.AppendLine("Aeron Pos: " + aeron.transform.position.ToString());
        }

        var guard = GameObject.Find("CyberGuard_Catwalk");
        if (guard != null)
        {
            sb.AppendLine("Guard Pos: " + guard.transform.position.ToString());
            var ai = guard.GetComponent<LastGod.Enemies.EnemyAI>();
            sb.AppendLine("Guard AI: " + (ai != null));
        }

        return sb.ToString();
    }
}"""

payload = {
    "className": "CheckSceneTilemaps",
    "methodName": "Execute",
    "isMethodBody": False,
    "csharpCode": code
}

with open("scratch/check_scene_tilemaps.json", "w", encoding="utf-8") as f:
    json.dump(payload, f, indent=2)
print("Created check_scene_tilemaps.json")
