// BuildLabEnvironment2D.cs
// ACT 1 — ORIGIN / LABORATORY — Hand-Crafted 2D Hero Environment Benchmark
// ========================================================================
// Constructs the approved, hand-crafted hero laboratory environment benchmark
// for The Last God in Act1_Lab_2D.unity.
// Replaces repetitive blockout tilemaps with authentic multi-plane depth layers:
// - Far Background (vast gloomy facility, distant silhouettes)
// - Midground Arches & Bulkheads (massive architectural structure)
// - Hero Containment Focal Chamber (mechanical housing, amniotic subject, cyan energy)
// - Walkable Gameplay Catwalk & Research Consoles (with physics colliders)
// - Foreground Framing Conduits (atmospheric depth framing)
// - 2D URP Lighting Rig (cold blue-grey ambient + localized cyan emission + amber warnings)
// - Atmospheric VFX (rising steam vents, cyan energy motes, floating dust)
// - Live Player Silhouette Proxy
// ========================================================================

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using LastGod.Environment2D;

public static class BuildLabEnvironment2D
{
    const string SCENE_PATH = "Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D.unity";
    const float PPU = 64f;

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
        Debug.Log(">>> [BuildLabEnvironment2D] Constructing Hand-Crafted Hero 2D Laboratory...");

        // Ensure directory
        string dir = Path.GetDirectoryName(SCENE_PATH);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // Create clean scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. Root Container
        var envRoot = new GameObject("=== ACT 1 LABORATORY 2D (HERO BENCHMARK) ===");

        // 2. Camera Setup (Orthographic, Size 6.0 for wide cinematic platformer view)
        var camGo = new GameObject("Main_Camera_2D");
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.0f; // 768 / 64 / 2 = 6.0
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = ColDarkBase;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 50f;
        camGo.AddComponent<AudioListener>();

        // Universal Additional Camera Data
        var camData = camGo.AddComponent<UniversalAdditionalCameraData>();
        camData.renderPostProcessing = true;

        // 3. Multi-Plane Depth Layers
        var layerFarBG = CreateLayerContainer("Layer_0_FarBackground",  envRoot, 6.0f, 0.15f);
        var layerMid   = CreateLayerContainer("Layer_1_MidgroundArches", envRoot, 2.0f, 0.45f);
        var layerGP    = CreateLayerContainer("Layer_2_Gameplay",        envRoot, 0.0f, 1.00f);
        var layerFG    = CreateLayerContainer("Layer_3_Foreground",      envRoot,-2.0f, 1.25f);
        var layerVFX   = CreateLayerContainer("Layer_4_AtmosphericVFX",  envRoot,-0.5f, 1.05f);

        // 4. Populate Depth Layers with Hand-Crafted Hero Sprites
        // A. Far Background (vast gloomy facility chasm)
        SpawnSprite("HERO_LAB_FAR_BACKGROUND", "Assets/Art/HeroLab/HERO_LAB_FAR_BACKGROUND.png",
            layerFarBG, new Vector3(0f, 0f, 0f), "Background_Far", 0);

        // B. Hero Clean Environment Master (Vaulted Arches, Pillars, Catwalk, Machinery, Consoles, Central Stasis)
        SpawnSprite("HERO_LAB_BASE_STRUCTURE", "Assets/Art/HeroLab/HERO_LAB_CLEAN_ENVIRONMENT.png",
            layerGP, new Vector3(0f, 0f, 0f), "Gameplay", 0);

        // C. Hero Containment Chamber (Focal Machine Target & Stasis Trigger)
        var chamberGo = new GameObject("HERO_LAB_CONTAINMENT_CHAMBER");
        chamberGo.transform.SetParent(layerGP.transform, false);
        chamberGo.transform.position = new Vector3(0f, 0.45f, 0f);
        var chamberCol = chamberGo.AddComponent<CircleCollider2D>();
        chamberCol.radius = 1.8f;
        chamberCol.isTrigger = true;

        // D. Gameplay Walkable Catwalk & Consoles (Walkable Platform Colliders)
        var gameplayGo = new GameObject("HERO_LAB_WALKABLE_CATWALK");
        gameplayGo.transform.SetParent(layerGP.transform, false);
        gameplayGo.transform.position = Vector3.zero;

        // Add Solid Box Colliders for walkable catwalk surface
        var colLeft = gameplayGo.AddComponent<BoxCollider2D>();
        colLeft.size = new Vector2(8.5f, 0.5f);
        colLeft.offset = new Vector2(-6.0f, -1.95f);

        var colRight = gameplayGo.AddComponent<BoxCollider2D>();
        colRight.size = new Vector2(8.5f, 0.5f);
        colRight.offset = new Vector2(6.0f, -1.95f);

        var colCenter = gameplayGo.AddComponent<BoxCollider2D>();
        colCenter.size = new Vector2(5.5f, 0.5f);
        colCenter.offset = new Vector2(0.0f, -1.95f);

        // E. Foreground Framing Pipes (Heavy dark framing with parallax)
        SpawnSprite("HERO_LAB_FOREGROUND_FRAME", "Assets/Art/HeroLab/LAYER_FOREGROUND_FRAME.png",
            layerFG, new Vector3(0f, 0f, 0f), "Foreground", 10);

        // 5. Spawn Player Silhouette Proxy (Standing on catwalk left of containment chamber)
        SpawnPlayerProxy(layerGP, new Vector3(-3.2f, -1.60f, 0f));

        // 6. 2D Lighting Rig
        SpawnLightingRig(envRoot);

        // 7. Atmospheric VFX (Condensation steam, Cyan motes, Dust motes)
        SpawnAtmosphericVFX(layerVFX);

        // Save scene and assets
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        AssetDatabase.Refresh();

        Debug.Log($">>> [BuildLabEnvironment2D] Hero 2D Laboratory successfully built and saved to {SCENE_PATH}!");
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
            pl.parallaxFactorY = parallaxFactor * 0.15f;
            pl.pixelSnap = true;
            pl.pixelsPerUnit = PPU;
        }

        return go;
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

    static void SpawnPlayerProxy(GameObject layerGP, Vector3 pos)
    {
        var go = SpawnSprite("Player_Silhouette_Proxy", "Assets/Art/HeroLab/PLAYER_PROXY_SILHOUETTE.png",
            layerGP, pos, "Player", 0);

        if (go != null)
        {
            var col = go.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.5f, 1.0f);
            col.offset = new Vector2(0f, 0.5f);
        }
    }

    static void SpawnLightingRig(GameObject envRoot)
    {
        var lightRoot = new GameObject("[LIGHTING RIG]");
        lightRoot.transform.SetParent(envRoot.transform, false);

        // 1. Global Ambient Light (Cold deep blue-grey ambient atmosphere)
        var globalGo = new GameObject("Light2D_Global_Cold");
        globalGo.transform.SetParent(lightRoot.transform, false);
        var globalLight = globalGo.AddComponent<Light2D>();
        globalLight.lightType = Light2D.LightType.Global;
        globalLight.color = new Color(180f/255f, 210f/255f, 240f/255f, 1f);
        globalLight.intensity = 0.95f;

        // 2. Primary Focal Light: Containment Chamber Cyan Energy (#6FE3FF at 3.2 intensity)
        var stasisLightGo = new GameObject("Light2D_Containment_Cyan_Focal");
        stasisLightGo.transform.SetParent(lightRoot.transform, false);
        stasisLightGo.transform.position = new Vector3(0.0f, 0.45f, -0.5f);
        var stasisLight = stasisLightGo.AddComponent<Light2D>();
        stasisLight.lightType = Light2D.LightType.Point;
        stasisLight.color = ColPrimaryCyan;
        stasisLight.intensity = 3.2f;
        stasisLight.pointLightInnerRadius = 1.2f;
        stasisLight.pointLightOuterRadius = 6.0f;

        // 3. Inner Bright Core Light: Glowing amniotic fluid inside glass portal (#CFF4FF)
        var coreLightGo = new GameObject("Light2D_Containment_Core_Bright");
        coreLightGo.transform.SetParent(lightRoot.transform, false);
        coreLightGo.transform.position = new Vector3(0.0f, 0.45f, -0.5f);
        var coreLight = coreLightGo.AddComponent<Light2D>();
        coreLight.lightType = Light2D.LightType.Point;
        coreLight.color = ColBrightCyan;
        coreLight.intensity = 4.0f;
        coreLight.pointLightInnerRadius = 0.6f;
        coreLight.pointLightOuterRadius = 2.8f;

        // 4. Warning Amber Indicator Point Light on Left Archway (#C4502E)
        var warnLightL = new GameObject("Light2D_Warning_Amber_Left");
        warnLightL.transform.SetParent(lightRoot.transform, false);
        warnLightL.transform.position = new Vector3(-7.6f, 0.35f, -0.5f);
        var warnL = warnLightL.AddComponent<Light2D>();
        warnL.lightType = Light2D.LightType.Point;
        warnL.color = new Color(220f/255f, 90f/255f, 50f/255f, 1f);
        warnL.intensity = 2.0f;
        warnL.pointLightInnerRadius = 0.4f;
        warnL.pointLightOuterRadius = 3.5f;

        // 5. Warning Amber Indicator Point Light on Right Archway (#C4502E)
        var warnLightR = new GameObject("Light2D_Warning_Amber_Right");
        warnLightR.transform.SetParent(lightRoot.transform, false);
        warnLightR.transform.position = new Vector3(7.6f, 0.35f, -0.5f);
        var warnR = warnLightR.AddComponent<Light2D>();
        warnR.lightType = Light2D.LightType.Point;
        warnR.color = new Color(220f/255f, 90f/255f, 50f/255f, 1f);
        warnR.intensity = 2.0f;
        warnR.pointLightInnerRadius = 0.4f;
        warnR.pointLightOuterRadius = 3.5f;

        // 6. Research Terminal Monitor Diagnostic Glow (Left: #245B70)
        var termLightL = new GameObject("Light2D_Terminal_Cyan_Left");
        termLightL.transform.SetParent(lightRoot.transform, false);
        termLightL.transform.position = new Vector3(-5.6f, -0.75f, -0.5f);
        var tLightL = termLightL.AddComponent<Light2D>();
        tLightL.lightType = Light2D.LightType.Point;
        tLightL.color = new Color(50f/255f, 180f/255f, 220f/255f, 1f);
        tLightL.intensity = 1.6f;
        tLightL.pointLightInnerRadius = 0.5f;
        tLightL.pointLightOuterRadius = 3.0f;

        // 7. Research Terminal Monitor Diagnostic Glow (Right: #245B70)
        var termLightR = new GameObject("Light2D_Terminal_Cyan_Right");
        termLightR.transform.SetParent(lightRoot.transform, false);
        termLightR.transform.position = new Vector3(5.0f, -0.75f, -0.5f);
        var tLightR = termLightR.AddComponent<Light2D>();
        tLightR.lightType = Light2D.LightType.Point;
        tLightR.color = new Color(50f/255f, 180f/255f, 220f/255f, 1f);
        tLightR.intensity = 1.6f;
        tLightR.pointLightInnerRadius = 0.5f;
        tLightR.pointLightOuterRadius = 3.0f;
    }

    static void SpawnAtmosphericVFX(GameObject layerVFX)
    {
        // 1. Steam Vents rising from lower catwalk drainage
        var steamGo = new GameObject("FX_Condensation_Steam");
        steamGo.transform.SetParent(layerVFX.transform, false);
        steamGo.transform.position = new Vector3(-4.0f, -2.0f, 0f);
        var psSteam = steamGo.AddComponent<ParticleSystem>();
        var mainSteam = psSteam.main;
        mainSteam.startLifetime = 3.0f;
        mainSteam.startSpeed = 0.6f;
        mainSteam.startSize = 0.8f;
        mainSteam.startColor = new Color(74f/255f, 83f/255f, 92f/255f, 0.30f);
        var emissionSteam = psSteam.emission;
        emissionSteam.rateOverTime = 5f;
        var shapeSteam = psSteam.shape;
        shapeSteam.shapeType = ParticleSystemShapeType.Cone;
        shapeSteam.angle = 20f;
        shapeSteam.radius = 0.2f;

        // 2. Cyan Energy Motes rising around Hero Containment Chamber
        var cyanEnergyGo = new GameObject("FX_Cyan_BioEnergy_Motes");
        cyanEnergyGo.transform.SetParent(layerVFX.transform, false);
        cyanEnergyGo.transform.position = new Vector3(0f, -0.5f, 0f);
        var psCyan = cyanEnergyGo.AddComponent<ParticleSystem>();
        var mainCyan = psCyan.main;
        mainCyan.startLifetime = 3.5f;
        mainCyan.startSpeed = 0.25f;
        mainCyan.startSize = 0.18f;
        mainCyan.startColor = new Color(111f/255f, 227f/255f, 255f/255f, 0.75f);
        var emissionCyan = psCyan.emission;
        emissionCyan.rateOverTime = 8f;
        var shapeCyan = psCyan.shape;
        shapeCyan.shapeType = ParticleSystemShapeType.Box;
        shapeCyan.scale = new Vector3(3.0f, 3.5f, 0.1f);

        // 3. Ambient Floating Dust Motes across facility
        var dustGo = new GameObject("FX_Facility_Dust_Motes");
        dustGo.transform.SetParent(layerVFX.transform, false);
        dustGo.transform.position = new Vector3(0f, 0f, 0f);
        var psDust = dustGo.AddComponent<ParticleSystem>();
        var mainDust = psDust.main;
        mainDust.startLifetime = 6.0f;
        mainDust.startSpeed = 0.08f;
        mainDust.startSize = 0.10f;
        mainDust.startColor = new Color(207f/255f, 244f/255f, 255f/255f, 0.25f);
        var emissionDust = psDust.emission;
        emissionDust.rateOverTime = 15f;
        var shapeDust = psDust.shape;
        shapeDust.shapeType = ParticleSystemShapeType.Box;
        shapeDust.scale = new Vector3(20f, 10f, 0.1f);
    }
}
