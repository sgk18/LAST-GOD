// BuildLabEnvironment2D.cs
// ACT 1 — ORIGIN / LABORATORY — Hand-Crafted 2D Platformer Environment Builder
// ===========================================================================
// Procedurally constructs the complete modular 2D laboratory environment
// for The Last God in Act1_Lab_2D.unity.
// Adheres strictly to 32 PPU, 384x216 base resolution, multi-plane parallax,
// 2D lighting, and modular tile/prop libraries.
// ===========================================================================

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using LastGod.Environment2D;

public static class BuildLabEnvironment2D
{
    const string SCENE_PATH = "Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D.unity";
    const float PPU = 32f;

    // Master Palette Colors
    static readonly Color ColDarkBase     = new Color(8f/255f, 11f/255f, 15f/255f, 1f);     // #080B0F
    static readonly Color ColDeepCharcoal = new Color(17f/255f, 22f/255f, 28f/255f, 1f);    // #11161C
    static readonly Color ColDarkBlueGrey = new Color(27f/255f, 36f/255f, 48f/255f, 1f);    // #1B2430
    static readonly Color ColIndGrey      = new Color(48f/255f, 56f/255f, 65f/255f, 1f);    // #303841
    static readonly Color ColLightMetal   = new Color(74f/255f, 83f/255f, 92f/255f, 1f);    // #4A535C
    static readonly Color ColDarkCyan     = new Color(36f/255f, 91f/255f, 112f/255f, 1f);   // #245B70
    static readonly Color ColPrimaryCyan  = new Color(111f/255f, 227f/255f, 255f/255f, 1f); // #6FE3FF
    static readonly Color ColBrightCyan   = new Color(207f/255f, 244f/255f, 255f/255f, 1f); // #CFF4FF
    static readonly Color ColWarnOrange   = new Color(196f/255f, 80f/255f, 46f/255f, 1f);   // #C4502E
    static readonly Color ColIndYellow    = new Color(179f/255f, 154f/255f, 69f/255f, 1f);  // #B39A45

    [MenuItem("The Last God/Build 2D Lab Environment", false, 10)]
    public static void BuildScene()
    {
        Debug.Log(">>> [BuildLabEnvironment2D] Starting 2D Laboratory Construction pass...");

        // Ensure directory
        string dir = Path.GetDirectoryName(SCENE_PATH);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. Root Container
        var envRoot = new GameObject("=== ACT 1 LABORATORY 2D ===");

        // 2. Camera Setup (Orthographic, Size 3.375 for 384x216 native platformer framing)
        var camGo = new GameObject("Main_Camera_2D");
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(0f, 2.5f, -10f);
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 3.375f; // (216 / 32) / 2 = 3.375
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = ColDarkBase;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 50f;
        camGo.AddComponent<AudioListener>();

        // Universal Additional Camera Data
        var camData = camGo.AddComponent<UniversalAdditionalCameraData>();
        camData.renderPostProcessing = true;

        // 3. Create Multi-Plane Depth Layers
        var layerFarBG = CreateLayerContainer("Layer_0_Background_Far", envRoot, 10f, 0.15f);
        var layerBG    = CreateLayerContainer("Layer_1_Background",     envRoot, 5f,  0.40f);
        var layerMG    = CreateLayerContainer("Layer_2_Midground",      envRoot, 2f,  0.75f);
        var layerGP    = CreateLayerContainer("Layer_3_Gameplay",       envRoot, 0f,  1.00f); // Static
        var layerFG    = CreateLayerContainer("Layer_5_Foreground",     envRoot, -2f, 1.25f);
        var layerVFX   = CreateLayerContainer("Layer_6_Atmospheric_VFX",envRoot, -1f, 1.10f);

        // 4. Create Grid & Tilemaps in Gameplay
        var gridGo = new GameObject("Grid_Gameplay");
        gridGo.transform.SetParent(layerGP.transform, false);
        var grid = gridGo.AddComponent<Grid>();
        grid.cellSize = new Vector3(1f, 1f, 0f);

        // Tilemap: Floor
        var tmFloorGo = CreateTilemapGO("Tilemap_Floor", gridGo, "Gameplay", 0);
        var tmFloor = tmFloorGo.GetComponent<Tilemap>();
        var tmFloorCollider = tmFloorGo.AddComponent<TilemapCollider2D>();
        var rbFloor = tmFloorGo.AddComponent<Rigidbody2D>();
        rbFloor.bodyType = RigidbodyType2D.Static;
        var compFloor = tmFloorGo.AddComponent<CompositeCollider2D>();
        compFloor.geometryType = CompositeCollider2D.GeometryType.Polygons;
        tmFloorCollider.compositeOperation = Collider2D.CompositeOperation.Merge;

        // Tilemap: Platforms
        var tmPlatGo = CreateTilemapGO("Tilemap_Platforms", gridGo, "Gameplay", 5);
        var tmPlat = tmPlatGo.GetComponent<Tilemap>();
        var tmPlatCollider = tmPlatGo.AddComponent<TilemapCollider2D>();
        var rbPlat = tmPlatGo.AddComponent<Rigidbody2D>();
        rbPlat.bodyType = RigidbodyType2D.Static;
        var compPlat = tmPlatGo.AddComponent<CompositeCollider2D>();
        compPlat.geometryType = CompositeCollider2D.GeometryType.Polygons;
        tmPlatCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
        var effector = tmPlatGo.AddComponent<PlatformEffector2D>();
        effector.useOneWay = true;
        compPlat.usedByEffector = true;

        // Tilemap: Background Walls
        var gridBgGo = new GameObject("Grid_Background");
        gridBgGo.transform.SetParent(layerBG.transform, false);
        var gridBg = gridBgGo.AddComponent<Grid>();
        gridBg.cellSize = new Vector3(1f, 1f, 0f);
        var tmWallGo = CreateTilemapGO("Tilemap_Background_Walls", gridBgGo, "Background", 0);
        var tmWall = tmWallGo.GetComponent<Tilemap>();

        // 5. Populate Tiles
        PopulateTilemaps(tmFloor, tmPlat, tmWall);

        // 6. Spawn Structural Props, Machinery, & Hero Stasis Chamber
        SpawnEnvironmentalProps(layerGP, layerMG, layerBG, layerFG);

        // 7. Spawn Temporary Player Silhouette Proxy
        SpawnPlayerProxy(layerGP);

        // 8. 2D Lighting Rig
        SpawnLightingRig(envRoot);

        // 9. Atmospheric VFX (Steam, Sparks, Cyan Energy, Dust)
        SpawnAtmosphericVFX(layerVFX);

        // Save scene and assets
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        AssetDatabase.Refresh();

        Debug.Log($">>> [BuildLabEnvironment2D] Scene successfully built and saved to {SCENE_PATH}!");
    }

    static GameObject CreateLayerContainer(string name, GameObject parent, float zDepth, float parallaxFactor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position = new Vector3(0f, 0f, zDepth);

        if (Mathf.Abs(parallaxFactor - 1.0f) > 0.001f)
        {
            var pl = go.AddComponent<ParallaxLayer>();
            pl.parallaxFactorX = parallaxFactor;
            pl.parallaxFactorY = parallaxFactor * 0.2f;
            pl.pixelSnap = true;
            pl.pixelsPerUnit = PPU;
        }

        return go;
    }

    static GameObject CreateTilemapGO(string name, GameObject parent, string sortingLayerName, int orderInLayer)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tm = go.AddComponent<Tilemap>();
        var tmr = go.AddComponent<TilemapRenderer>();
        tmr.sortingLayerName = sortingLayerName;
        tmr.sortingOrder = orderInLayer;
        return go;
    }

    static Tile CreateTileAsset(string spritePath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            Debug.LogWarning($"[BuildLabEnvironment2D] Sprite missing at {spritePath}");
            return null;
        }
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.colliderType = Tile.ColliderType.Grid;
        return tile;
    }

    static void PopulateTilemaps(Tilemap tmFloor, Tilemap tmPlat, Tilemap tmWall)
    {
        var tileFloorStraight = CreateTileAsset("Assets/Art/Tiles/LAB_Tile_Floor_Straight.png");
        var tileFloorGrating  = CreateTileAsset("Assets/Art/Tiles/LAB_Tile_Floor_Grating.png");
        var tileFloorHazard   = CreateTileAsset("Assets/Art/Tiles/LAB_Tile_Floor_Hazard.png");
        var tileFloorDamaged  = CreateTileAsset("Assets/Art/Tiles/LAB_Tile_Floor_Damaged.png");

        var tilePlatSurface   = CreateTileAsset("Assets/Art/Tiles/LAB_Tile_Platform_Surface.png");
        var tilePlatEdgeL     = CreateTileAsset("Assets/Art/Tiles/LAB_Tile_Platform_Edge_L.png");
        var tilePlatEdgeR     = CreateTileAsset("Assets/Art/Tiles/LAB_Tile_Platform_Edge_R.png");
        var tilePlatSupport   = CreateTileAsset("Assets/Art/Tiles/LAB_Tile_Platform_Support.png");

        var tileWallStraight  = CreateTileAsset("Assets/Art/Tiles/LAB_Tile_Wall_Straight.png");
        var tileWallPanel     = CreateTileAsset("Assets/Art/Tiles/LAB_Tile_Wall_Panel.png");
        var tileCeilingPipes  = CreateTileAsset("Assets/Art/Tiles/LAB_Tile_Ceiling_Pipes.png");

        // 1. Ground Floor (Y = 0, X from -12 to +12)
        for (int x = -12; x <= 12; x++)
        {
            Tile tileToUse = tileFloorStraight;
            if (x == -6 || x == 6) tileToUse = tileFloorHazard;
            else if (x == -2 || x == 2) tileToUse = tileFloorGrating;
            else if (x == 4) tileToUse = tileFloorDamaged;

            tmFloor.SetTile(new Vector3Int(x, 0, 0), tileToUse);
            // Foundation row underneath floor
            tmFloor.SetTile(new Vector3Int(x, -1, 0), tileFloorStraight);
        }

        // 2. Left Raised Catwalk (Y = 2, X from -8 to -4)
        tmPlat.SetTile(new Vector3Int(-8, 2, 0), tilePlatEdgeL);
        for (int x = -7; x <= -5; x++)
        {
            tmPlat.SetTile(new Vector3Int(x, 2, 0), tilePlatSurface);
        }
        tmPlat.SetTile(new Vector3Int(-4, 2, 0), tilePlatEdgeR);
        // Vertical support strut under platform
        tmPlat.SetTile(new Vector3Int(-6, 1, 0), tilePlatSupport);

        // 3. Right Raised Catwalk (Y = 2, X from 3 to 8)
        tmPlat.SetTile(new Vector3Int(3, 2, 0), tilePlatEdgeL);
        for (int x = 4; x <= 7; x++)
        {
            tmPlat.SetTile(new Vector3Int(x, 2, 0), tilePlatSurface);
        }
        tmPlat.SetTile(new Vector3Int(8, 2, 0), tilePlatEdgeR);
        tmPlat.SetTile(new Vector3Int(6, 1, 0), tilePlatSupport);

        // 4. Background Wall Grid (Y from 1 to 7, X from -10 to 10)
        for (int x = -10; x <= 10; x++)
        {
            for (int y = 1; y <= 6; y++)
            {
                Tile wallTile = ((x + y) % 3 == 0) ? tileWallPanel : tileWallStraight;
                tmWall.SetTile(new Vector3Int(x, y, 0), wallTile);
            }
            // Ceiling pipe row
            tmWall.SetTile(new Vector3Int(x, 7, 0), tileCeilingPipes);
        }
    }

    static void SpawnEnvironmentalProps(GameObject layerGP, GameObject layerMG, GameObject layerBG, GameObject layerFG)
    {
        // --- HERO STASIS CHAMBER (Aeron's pod at X = 0, Y = 1.0) ---
        SpawnSprite("HERO_CONTAINMENT_MAIN", "Assets/Art/Environment/Containment/LAB_CONTAINMENT_Main.png",
            layerGP, new Vector3(0f, 2.5f, 0f), "Gameplay", 15);

        // --- SECONDARY CONTAINMENT UNITS (Midground) ---
        SpawnSprite("Containment_Small_Left", "Assets/Art/Environment/Containment/LAB_Containment_Small.png",
            layerMG, new Vector3(-3.8f, 2.0f, 0f), "Midground", 5);

        SpawnSprite("Containment_Damaged_Right", "Assets/Art/Environment/Containment/LAB_Containment_Damaged.png",
            layerMG, new Vector3(4.5f, 2.2f, 0f), "Midground", 5);

        // --- STRUCTURAL PILLARS (Background) ---
        SpawnSprite("Pillar_A7", "Assets/Art/Environment/Background/LAB_Pillar_A.png",
            layerBG, new Vector3(6.5f, 4.0f, 0f), "Background", 10);

        SpawnSprite("Pillar_B3", "Assets/Art/Environment/Background/LAB_Pillar_B.png",
            layerBG, new Vector3(-6.5f, 4.0f, 0f), "Background", 10);

        // --- OVERHEAD BEAMS ---
        SpawnSprite("Overhead_Beam_A", "Assets/Art/Environment/Background/LAB_Beam_A.png",
            layerBG, new Vector3(0f, 6.8f, 0f), "Background", 12);

        // --- MACHINERY & CONSOLES (Gameplay) ---
        SpawnSprite("Console_Research_A", "Assets/Art/Environment/Machinery/LAB_Console_A.png",
            layerGP, new Vector3(-2.2f, 1.5f, 0f), "Gameplay", 8);

        SpawnSprite("PowerUnit_Left", "Assets/Art/Environment/Machinery/LAB_PowerUnit_A.png",
            layerGP, new Vector3(-7.5f, 1.75f, 0f), "Gameplay", 8);

        SpawnSprite("ServerRack_Right", "Assets/Art/Environment/Machinery/LAB_ServerRack_A.png",
            layerGP, new Vector3(7.5f, 2.25f, 0f), "Gameplay", 8);

        // --- PROPS & VALVES ---
        SpawnSprite("Pressure_Valve_A", "Assets/Art/Environment/Props/LAB_Valve_A.png",
            layerGP, new Vector3(-1.8f, 3.2f, 0f), "Gameplay", 7);

        SpawnSprite("Pipe_Junction_A", "Assets/Art/Environment/Props/LAB_Pipe_Junction.png",
            layerGP, new Vector3(2.2f, 3.2f, 0f), "Gameplay", 7);

        // Catwalk Railings
        SpawnSprite("Railing_Left", "Assets/Art/Environment/Props/LAB_Railing_A.png",
            layerGP, new Vector3(-6.0f, 3.25f, 0f), "Gameplay", 12);

        SpawnSprite("Railing_Right", "Assets/Art/Environment/Props/LAB_Railing_A.png",
            layerGP, new Vector3(5.5f, 3.25f, 0f), "Gameplay", 12);

        // --- DECALS ---
        SpawnSprite("Decal_A7", "Assets/Art/Environment/Decals/LAB_DECAL_A7.png",
            layerGP, new Vector3(6.5f, 2.8f, 0f), "Gameplay", 11);

        SpawnSprite("Decal_B3", "Assets/Art/Environment/Decals/LAB_DECAL_B3.png",
            layerGP, new Vector3(-6.5f, 2.8f, 0f), "Gameplay", 11);

        SpawnSprite("Decal_Restricted", "Assets/Art/Environment/Decals/LAB_DECAL_Restricted.png",
            layerGP, new Vector3(0f, 4.3f, 0f), "Gameplay", 16);

        // --- FOREGROUND FRAMING PIPES ---
        SpawnSprite("FG_Conduit_Left", "Assets/Art/Environment/Props/LAB_Pipe_B.png",
            layerFG, new Vector3(-9.5f, 3.0f, 0f), "Foreground", 10);
        var fgLeftScale = layerFG.transform.Find("FG_Conduit_Left");
        if (fgLeftScale != null) fgLeftScale.localScale = new Vector3(1.5f, 3f, 1f);

        SpawnSprite("FG_Conduit_Right", "Assets/Art/Environment/Props/LAB_Pipe_B.png",
            layerFG, new Vector3(9.5f, 3.0f, 0f), "Foreground", 10);
        var fgRightScale = layerFG.transform.Find("FG_Conduit_Right");
        if (fgRightScale != null) fgRightScale.localScale = new Vector3(1.5f, 3f, 1f);
    }

    static void SpawnPlayerProxy(GameObject layerGP)
    {
        var go = SpawnSprite("Player_Silhouette_Proxy", "Assets/Art/Environment/Gameplay/Player_Silhouette_Proxy.png",
            layerGP, new Vector3(-1.2f, 2.0f, 0f), "Player", 0);

        // Add capsule collider for scale clearance verification
        if (go != null)
        {
            var col = go.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.6f, 1.8f);
            col.offset = new Vector2(0f, 0f);
        }
    }

    static GameObject SpawnSprite(string name, string spritePath, GameObject parent, Vector3 pos, string sortingLayer, int order)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            Debug.LogWarning($"[BuildLabEnvironment2D] Missing sprite: {spritePath}");
            return null;
        }

        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = sortingLayer;
        sr.sortingOrder = order;
        return go;
    }

    static void SpawnLightingRig(GameObject envRoot)
    {
        var lightRoot = new GameObject("[LIGHTING ROOT]");
        lightRoot.transform.SetParent(envRoot.transform, false);

        // 1. Global Ambient Light (Cold blue-grey #1B2430 at 0.30 intensity)
        var globalGo = new GameObject("Light2D_Global_Cold");
        globalGo.transform.SetParent(lightRoot.transform, false);
        var globalLight = globalGo.AddComponent<Light2D>();
        globalLight.lightType = Light2D.LightType.Global;
        globalLight.color = ColDarkBlueGrey;
        globalLight.intensity = 0.35f;

        // 2. Key Hero Stasis Cyan Point Light (#6FE3FF at 1.8 intensity)
        var stasisLightGo = new GameObject("Light2D_Hero_Stasis");
        stasisLightGo.transform.SetParent(lightRoot.transform, false);
        stasisLightGo.transform.position = new Vector3(0f, 2.5f, -0.5f);
        var stasisLight = stasisLightGo.AddComponent<Light2D>();
        stasisLight.lightType = Light2D.LightType.Point;
        stasisLight.color = ColPrimaryCyan;
        stasisLight.intensity = 1.8f;
        stasisLight.pointLightInnerRadius = 0.5f;
        stasisLight.pointLightOuterRadius = 4.5f;

        // 3. Warning Amber Indicator Point Light (#C4502E)
        var warnLightGo = new GameObject("Light2D_Warning_Amber");
        warnLightGo.transform.SetParent(lightRoot.transform, false);
        warnLightGo.transform.position = new Vector3(4.5f, 2.8f, -0.5f);
        var warnLight = warnLightGo.AddComponent<Light2D>();
        warnLight.lightType = Light2D.LightType.Point;
        warnLight.color = ColWarnOrange;
        warnLight.intensity = 0.9f;
        warnLight.pointLightInnerRadius = 0.2f;
        warnLight.pointLightOuterRadius = 2.5f;

        // 4. Research Terminal Diagnostic Glow (#245B70)
        var termLightGo = new GameObject("Light2D_Terminal_Blue");
        termLightGo.transform.SetParent(lightRoot.transform, false);
        termLightGo.transform.position = new Vector3(-2.2f, 1.8f, -0.5f);
        var termLight = termLightGo.AddComponent<Light2D>();
        termLight.lightType = Light2D.LightType.Point;
        termLight.color = ColDarkCyan;
        termLight.intensity = 0.7f;
        termLight.pointLightInnerRadius = 0.3f;
        termLight.pointLightOuterRadius = 2.0f;
    }

    static void SpawnAtmosphericVFX(GameObject layerVFX)
    {
        // 1. Steam Vent at floor grating (X = -2.0, Y = 1.0)
        var steamGo = new GameObject("FX_Steam_Vent");
        steamGo.transform.SetParent(layerVFX.transform, false);
        steamGo.transform.position = new Vector3(-2.0f, 1.1f, 0f);
        var psSteam = steamGo.AddComponent<ParticleSystem>();
        var mainSteam = psSteam.main;
        mainSteam.startLifetime = 2.0f;
        mainSteam.startSpeed = 0.8f;
        mainSteam.startSize = 0.6f;
        mainSteam.startColor = new Color(74f/255f, 83f/255f, 92f/255f, 0.4f);
        var emissionSteam = psSteam.emission;
        emissionSteam.rateOverTime = 6f;
        var shapeSteam = psSteam.shape;
        shapeSteam.shapeType = ParticleSystemShapeType.Cone;
        shapeSteam.angle = 15f;
        shapeSteam.radius = 0.1f;

        // 2. Cyan Energy Particles rising around Hero Stasis Pod
        var cyanEnergyGo = new GameObject("FX_Cyan_Energy_Motes");
        cyanEnergyGo.transform.SetParent(layerVFX.transform, false);
        cyanEnergyGo.transform.position = new Vector3(0f, 1.5f, 0f);
        var psCyan = cyanEnergyGo.AddComponent<ParticleSystem>();
        var mainCyan = psCyan.main;
        mainCyan.startLifetime = 3.0f;
        mainCyan.startSpeed = 0.3f;
        mainCyan.startSize = 0.25f;
        mainCyan.startColor = new Color(111f/255f, 227f/255f, 255f/255f, 0.8f);
        var emissionCyan = psCyan.emission;
        emissionCyan.rateOverTime = 8f;
        var shapeCyan = psCyan.shape;
        shapeCyan.shapeType = ParticleSystemShapeType.Box;
        shapeCyan.scale = new Vector3(1.5f, 2.0f, 0.1f);

        // 3. Ambient Floating Dust Motes
        var dustGo = new GameObject("FX_Dust_Motes");
        dustGo.transform.SetParent(layerVFX.transform, false);
        dustGo.transform.position = new Vector3(0f, 2.5f, 0f);
        var psDust = dustGo.AddComponent<ParticleSystem>();
        var mainDust = psDust.main;
        mainDust.startLifetime = 5.0f;
        mainDust.startSpeed = 0.1f;
        mainDust.startSize = 0.12f;
        mainDust.startColor = new Color(207f/255f, 244f/255f, 255f/255f, 0.35f);
        var emissionDust = psDust.emission;
        emissionDust.rateOverTime = 12f;
        var shapeDust = psDust.shape;
        shapeDust.shapeType = ParticleSystemShapeType.Box;
        shapeDust.scale = new Vector3(12f, 5f, 0.1f);
    }
}
