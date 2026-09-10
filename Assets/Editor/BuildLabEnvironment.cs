// BuildLabEnvironment.cs
// ACT 1 — ORIGIN / LABORATORY — Complete Environment Production Pass
// ============================================================
// This editor script procedurally constructs the full modular 3D laboratory
// environment for The Last God within the 2.5D_Lab_Scene.
//
// Preserves: Characters group, Lighting group, Main_Camera, DontDestroyOnLoad
// Replaces:  Lab_Environment with complete modular architecture
//
// Usage: The Last God → Build Lab Environment
// ============================================================

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;

/// <summary>
/// Procedural builder for the ACT 1 containment laboratory environment.
/// Builds geometry entirely via Unity primitives and procedural meshes —
/// no external FBX dependency. Designed for the 27-degree 2.5D constrained camera.
/// </summary>
public static class BuildLabEnvironment
{
    // ─── Color Palette ─────────────────────────────────────────────────────
    static readonly Color ColBase0    = new Color(0.039f, 0.047f, 0.063f); // #0A0C10
    static readonly Color ColBase1    = new Color(0.106f, 0.141f, 0.188f); // #1B2430
    static readonly Color ColBase2    = new Color(0.145f, 0.176f, 0.212f); // #252D36
    static readonly Color ColBase3    = new Color(0.227f, 0.247f, 0.278f); // #3A3F47
    static readonly Color ColCyan     = new Color(0.435f, 0.890f, 1.000f); // #6FE3FF
    static readonly Color ColCyanFill = new Color(0.812f, 0.957f, 1.000f); // #CFF4FF
    static readonly Color ColWarning  = new Color(0.769f, 0.314f, 0.180f); // #C4502E
    static readonly Color ColEmissive = new Color(0.435f * 1.5f, 0.890f * 1.5f, 1.0f * 1.5f);

    // ─── Scale Constants ────────────────────────────────────────────────────
    const float FLOOR_Y   = 0f;      // Main floor top surface
    const float WALL_H    = 8f;      // Ceiling height
    const float ROOM_W    = 16f;     // Room half-width = ±8m
    const float ROOM_D    = 12f;     // Room depth (camera-facing)
    const float PLAT_Y    = 2.5f;    // Raised platform height
    const float UPPER_Y   = 4.5f;    // Upper catwalk height

    // Material GUIDs cached at build time
    static Material s_MatMetalDark;
    static Material s_MatMetalWorn;
    static Material s_MatFloor;
    static Material s_MatGrating;
    static Material s_MatGlass;
    static Material s_MatCyanEmission;
    static Material s_MatConsole;
    static Material s_MatCable;
    static Material s_MatConcrete;
    static Material s_MatWarning;

    // ─── Entry Points ───────────────────────────────────────────────────────

    [MenuItem("The Last God/Build 3D Lab Scene from Blender FBX", false, 1)]
    public static void BuildBlenderScene()
    {
        string scenePath = "Assets/Scenes/2.5D_Lab_Scene.unity";
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (scene.path != scenePath)
        {
            if (File.Exists(scenePath))
            {
                scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            }
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Build 3D Lab Scene from Blender FBX");

        EnsureDirectories();
        CreateAllMaterials();

        // 1. Destroy old Lab_Environment and Lab_Environment_3D
        var oldEnv = GameObject.Find("Lab_Environment");
        if (oldEnv != null) Undo.DestroyObjectImmediate(oldEnv);
        var old3D = GameObject.Find("Lab_Environment_3D");
        if (old3D != null) Undo.DestroyObjectImmediate(old3D);

        // 2. Load and Instantiate Blender Environment FBX
        string envFbxPath = "Assets/Environment/Lab/Lab_Environment_Production.fbx";
        var envPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(envFbxPath);
        if (envPrefab != null)
        {
            var envInstance = (GameObject)PrefabUtility.InstantiatePrefab(envPrefab);
            envInstance.name = "Lab_Environment_3D";
            envInstance.transform.position = Vector3.zero;
            envInstance.transform.rotation = Quaternion.identity;
            Undo.RegisterCreatedObjectUndo(envInstance, "Instantiate Lab_Environment_3D");

            // Setup Colliders and Ground Layers on Catwalk
            var deck = envInstance.transform.Find("Catwalk_Main_Deck");
            if (deck != null)
            {
                var boxCol = deck.gameObject.GetComponent<BoxCollider>();
                if (boxCol == null) boxCol = deck.gameObject.AddComponent<BoxCollider>();
            }
            SetLayer(envInstance, 0);
            if (deck != null) SetLayer(deck.gameObject, 8); // Ground layer
        }
        else
        {
            Debug.LogError($"[LabBuilder] Could not find FBX at {envFbxPath}. Reverting to procedural fallback.");
            Build();
            return;
        }

        // 3. Load and Instantiate Aeron 3D Character
        string aeronFbxPath = "Assets/Characters/Aeron/Aeron_Mesh.fbx";
        var aeronPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(aeronFbxPath);
        if (aeronPrefab != null)
        {
            var oldAeron3D = GameObject.Find("Aeron_3D_Model");
            if (oldAeron3D != null) Undo.DestroyObjectImmediate(oldAeron3D);

            var aeronInstance = (GameObject)PrefabUtility.InstantiatePrefab(aeronPrefab);
            aeronInstance.name = "Aeron_3D_Model";
            aeronInstance.transform.position = new Vector3(-1.5f, 0f, 0f);
            aeronInstance.transform.rotation = Quaternion.Euler(0, 180f, 0);
            Undo.RegisterCreatedObjectUndo(aeronInstance, "Instantiate Aeron_3D_Model");

            var charGrp = GameObject.Find("Characters");
            if (charGrp != null)
            {
                aeronInstance.transform.SetParent(charGrp.transform, true);
            }
            SetLayer(aeronInstance, 9); // Player layer
        }

        // 4. Reposition other characters
        RepositionCharacters();

        // 5. Save Scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        AssetDatabase.Refresh();

        Debug.Log("[LabBuilder] ✅ 3D Blender Lab Environment & Aeron Model successfully built in Unity!");
    }

    [MenuItem("The Last God/Build Lab Environment", false, 10)]
    public static void Build()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (!scene.name.Contains("2.5D_Lab_Scene") && !scene.name.Contains("Lab"))
        {
            Debug.LogWarning($"[LabBuilder] Active scene is '{scene.name}', building anyway.");
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Build Lab Environment");

        // 1. Materials
        EnsureDirectories();
        CreateAllMaterials();

        // 2. Destroy old Lab_Environment only
        var existing = GameObject.Find("Lab_Environment");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
            Debug.Log("[LabBuilder] Destroyed old Lab_Environment.");
        }

        // 3. Build root hierarchy
        var root = CreateEmpty("Lab_Environment", null, Vector3.zero);
        var grpArch       = CreateEmpty("Architecture",    root, Vector3.zero);
        var grpPlatforms  = CreateEmpty("Platforms",       root, Vector3.zero);
        var grpContain    = CreateEmpty("Containment",     root, Vector3.zero);
        var grpDoors      = CreateEmpty("Doors",           root, Vector3.zero);
        var grpMachinery  = CreateEmpty("Machinery",       root, Vector3.zero);
        var grpProps      = CreateEmpty("Props",           root, Vector3.zero);
        var grpPipes      = CreateEmpty("PipesAndCables",  root, Vector3.zero);
        var grpLighting   = CreateEmpty("Lighting",        root, Vector3.zero);
        var grpCollision  = CreateEmpty("Collision",       root, Vector3.zero);

        // 4. Build each zone
        BuildArchitecture(grpArch);
        BuildPlatforms(grpPlatforms);
        BuildContainment(grpContain);
        BuildDoors(grpDoors);
        BuildMachinery(grpMachinery);
        BuildProps(grpProps);
        BuildPipesAndCables(grpPipes);
        BuildAdditionalLighting(grpLighting);
        BuildCollision(grpCollision);

        // 5. Reposition characters to match floor
        RepositionCharacters();

        // 6. Save
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        AssetDatabase.Refresh();

        Debug.Log("[LabBuilder] ✅ Lab environment built successfully. Check Game view at FOV 27° for composition.");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SECTION 1 — MATERIALS
    // ═══════════════════════════════════════════════════════════════════════

    static void EnsureDirectories()
    {
        var dirs = new[]
        {
            "Assets/Materials/Environment",
            "Assets/Prefabs/Environment/Lab",
            "Assets/Environment/Lab"
        };
        foreach (var d in dirs)
        {
            if (!AssetDatabase.IsValidFolder(d))
            {
                var parts = d.Split('/');
                string acc = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = acc + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(acc, parts[i]);
                    acc = next;
                }
            }
        }
    }

    static void CreateAllMaterials()
    {
        s_MatMetalDark    = MakeMat("MAT_Lab_Metal_Dark",    ColBase1,   0.30f, 0.15f, Color.black, 0f);
        s_MatMetalWorn    = MakeMat("MAT_Lab_Metal_Worn",    ColBase2,   0.20f, 0.10f, Color.black, 0f);
        s_MatFloor        = MakeMat("MAT_Lab_Floor",         ColBase0,   0.10f, 0.20f, Color.black, 0f);
        s_MatGrating      = MakeMat("MAT_Lab_Grating",       ColBase1,   0.40f, 0.12f, Color.black, 0f);
        s_MatGlass        = MakeGlassMat("MAT_Lab_Glass");
        s_MatCyanEmission = MakeMat("MAT_Lab_CyanEmission",  ColBase0,   0.00f, 0.05f, ColEmissive, 1.5f);
        s_MatConsole      = MakeMat("MAT_Lab_Console",       ColBase2,   0.10f, 0.20f, ColCyanFill, 0.3f);
        s_MatCable        = MakeMat("MAT_Lab_Cable",         ColBase0,   0.00f, 0.05f, Color.black, 0f);
        s_MatConcrete     = MakeMat("MAT_Lab_Concrete",      new Color(0.106f, 0.118f, 0.141f), 0f, 0.05f, Color.black, 0f);
        s_MatWarning      = MakeMat("MAT_Lab_Warning",       ColWarning, 0.00f, 0.10f, ColWarning, 0.4f);
    }

    static Material MakeMat(string name, Color albedo, float metallic, float smoothness,
                             Color emissiveColor, float emissiveIntensity)
    {
        string path = $"Assets/Materials/Environment/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", albedo);
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        if (emissiveIntensity > 0f)
        {
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            mat.SetColor("_EmissionColor", emissiveColor * emissiveIntensity);
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material MakeGlassMat(string name)
    {
        string path = $"Assets/Materials/Environment/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", new Color(0.435f, 0.890f, 1.0f, 0.25f));
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Smoothness", 0.6f);
        mat.SetFloat("_Surface", 1);        // Transparent
        mat.SetFloat("_Blend", 0);          // Alpha blend
        mat.renderQueue = 3000;
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0.435f * 0.3f, 0.890f * 0.3f, 1.0f * 0.3f));
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SECTION 2 — ARCHITECTURE
    // ═══════════════════════════════════════════════════════════════════════

    static void BuildArchitecture(GameObject parent)
    {
        var grpFloors  = CreateEmpty("Floors",         parent, Vector3.zero);
        var grpWalls   = CreateEmpty("Walls",          parent, Vector3.zero);
        var grpCeil    = CreateEmpty("Ceiling",        parent, Vector3.zero);
        var grpPillars = CreateEmpty("Pillars",        parent, Vector3.zero);
        var grpBeams   = CreateEmpty("StructuralBeams",parent, Vector3.zero);

        // ── FLOOR ──────────────────────────────────────────────────────────
        // Main floor slab 16×12m
        var floor = CreateBox("Lab_Floor_Main", grpFloors,
            new Vector3(0, FLOOR_Y - 0.15f, 2f),
            new Vector3(ROOM_W, 0.3f, ROOM_D));
        SetMat(floor, s_MatFloor);
        SetLayer(floor, 8); // Ground layer

        // Floor panel seam lines (4 panels)
        for (int i = -1; i <= 1; i++)
        {
            var seam = CreateBox($"Lab_Floor_Seam_{i+2}", grpFloors,
                new Vector3(i * 4f, FLOOR_Y + 0.005f, 2f),
                new Vector3(0.06f, 0.01f, ROOM_D));
            SetMat(seam, s_MatMetalDark);
        }
        // Z-seams
        for (int i = 0; i <= 2; i++)
        {
            var seam = CreateBox($"Lab_Floor_SeamZ_{i}", grpFloors,
                new Vector3(0, FLOOR_Y + 0.005f, i * 4f - 4f),
                new Vector3(ROOM_W, 0.01f, 0.06f));
            SetMat(seam, s_MatMetalDark);
        }
        // Grating strip down center (drain channel)
        var grate = CreateBox("Lab_Floor_GrateCenter", grpFloors,
            new Vector3(0, FLOOR_Y + 0.01f, 2f),
            new Vector3(0.6f, 0.02f, ROOM_D));
        SetMat(grate, s_MatGrating);

        // ── BACK WALL (Z+) ─────────────────────────────────────────────────
        // Main panels
        for (int i = 0; i < 4; i++)
        {
            float xPos = -6f + i * 4f;
            bool damaged = (i == 1); // Second panel from left is slightly damaged
            var panel = CreateBox($"Lab_Wall_Back_{i}", grpWalls,
                new Vector3(xPos, WALL_H * 0.5f + FLOOR_Y, ROOM_D * 0.5f - 0.15f),
                new Vector3(4f, WALL_H, 0.3f));
            SetMat(panel, damaged ? s_MatMetalWorn : s_MatMetalDark);

            // Panel ribs
            for (int r = 0; r < 3; r++)
            {
                var rib = CreateBox($"Lab_Wall_Back_{i}_Rib_{r}", grpWalls,
                    new Vector3(xPos - 1.5f + r * 1.5f, WALL_H * 0.5f + FLOOR_Y, ROOM_D * 0.5f - 0.02f),
                    new Vector3(0.12f, WALL_H, 0.12f));
                SetMat(rib, s_MatMetalDark);
            }
        }

        // Back wall maintenance section (left side, bottom 2m - slightly different treatment)
        var maintSection = CreateBox("Lab_Wall_Back_MaintPanel", grpWalls,
            new Vector3(-6.5f, 1.2f + FLOOR_Y, ROOM_D * 0.5f - 0.05f),
            new Vector3(1.0f, 2.4f, 0.1f));
        SetMat(maintSection, s_MatMetalWorn);

        // ── LEFT WALL ──────────────────────────────────────────────────────
        for (int i = 0; i < 3; i++)
        {
            float zPos = -2f + i * 4f;
            var panel = CreateBox($"Lab_Wall_Left_{i}", grpWalls,
                new Vector3(-ROOM_W * 0.5f + 0.15f, WALL_H * 0.5f + FLOOR_Y, zPos),
                new Vector3(0.3f, WALL_H, 4f));
            SetMat(panel, s_MatMetalDark);
        }

        // ── RIGHT WALL ─────────────────────────────────────────────────────
        for (int i = 0; i < 3; i++)
        {
            float zPos = -2f + i * 4f;
            var panel = CreateBox($"Lab_Wall_Right_{i}", grpWalls,
                new Vector3(ROOM_W * 0.5f - 0.15f, WALL_H * 0.5f + FLOOR_Y, zPos),
                new Vector3(0.3f, WALL_H, 4f));
            SetMat(panel, s_MatMetalDark);
        }

        // ── FRONT WALL (partial - camera side, partial for depth framing) ──
        // Left foreground wall sliver
        var frontL = CreateBox("Lab_Wall_Front_Left", grpWalls,
            new Vector3(-7f, WALL_H * 0.5f + FLOOR_Y, -ROOM_D * 0.5f + 0.15f),
            new Vector3(2f, WALL_H, 0.3f));
        SetMat(frontL, s_MatMetalDark);
        // Right foreground wall sliver
        var frontR = CreateBox("Lab_Wall_Front_Right", grpWalls,
            new Vector3(7f, WALL_H * 0.5f + FLOOR_Y, -ROOM_D * 0.5f + 0.15f),
            new Vector3(2f, WALL_H, 0.3f));
        SetMat(frontR, s_MatMetalDark);

        // ── CEILING ────────────────────────────────────────────────────────
        var ceil = CreateBox("Lab_Ceiling_Main", grpCeil,
            new Vector3(0, WALL_H + FLOOR_Y + 0.15f, 2f),
            new Vector3(ROOM_W, 0.3f, ROOM_D));
        SetMat(ceil, s_MatConcrete);

        // Ceiling structural beams (X-axis)
        float[] beamZPositions = { -3f, 0f, 3f, 6f };
        for (int i = 0; i < beamZPositions.Length; i++)
        {
            var beam = CreateBox($"Lab_Ceiling_Beam_X_{i}", grpCeil,
                new Vector3(0, WALL_H - 0.4f + FLOOR_Y, beamZPositions[i]),
                new Vector3(ROOM_W, 0.8f, 0.5f));
            SetMat(beam, s_MatMetalDark);
        }
        // Ceiling beams (Z-axis)
        float[] beamXPositions = { -6f, 0f, 6f };
        for (int i = 0; i < beamXPositions.Length; i++)
        {
            var beam = CreateBox($"Lab_Ceiling_Beam_Z_{i}", grpCeil,
                new Vector3(beamXPositions[i], WALL_H - 0.25f + FLOOR_Y, 2f),
                new Vector3(0.5f, 0.5f, ROOM_D));
            SetMat(beam, s_MatMetalDark);
        }
        // Overhead pipe trays (subtle ceiling infrastructure)
        for (int i = 0; i < 3; i++)
        {
            var pipeTray = CreateBox($"Lab_Ceiling_PipeTray_{i}", grpCeil,
                new Vector3(-4f + i * 4f, WALL_H - 0.15f + FLOOR_Y, 2f),
                new Vector3(0.4f, 0.3f, ROOM_D * 0.7f));
            SetMat(pipeTray, s_MatMetalDark);
        }

        // ── PILLARS ────────────────────────────────────────────────────────
        // 4 major pillars (B-3 / A-7 language)
        var pillarPositions = new Vector3[]
        {
            new Vector3(-6f, 0, -1f),   // B-3 — foreground left
            new Vector3( 6f, 0, -1f),   // foreground right
            new Vector3(-6f, 0,  5f),   // A-7 — mid-background left
            new Vector3( 6f, 0,  5f),   // mid-background right
        };
        string[] pillarNames = { "Pillar_B3_Left", "Pillar_Right_Front", "Pillar_A7_Left", "Pillar_Right_Back" };
        for (int i = 0; i < pillarPositions.Length; i++)
        {
            BuildPillar(grpPillars, pillarNames[i], pillarPositions[i]);
        }

        // 2 secondary narrower pillars
        BuildPillarB(grpPillars, "Pillar_B_Left",  new Vector3(-3f, 0,  8f));
        BuildPillarB(grpPillars, "Pillar_B_Right", new Vector3( 3f, 0,  8f));

        // ── STRUCTURAL CROSS-BEAMS (diagonal braces) ──────────────────────
        // Left wall diagonal brace
        BuildDiagonalBrace(grpBeams, "Brace_Left_1",  new Vector3(-7.5f, 3f, 0f), 45f);
        BuildDiagonalBrace(grpBeams, "Brace_Right_1", new Vector3( 7.5f, 3f, 0f), -45f);
    }

    static void BuildPillar(GameObject parent, string name, Vector3 pos)
    {
        // Main shaft
        var shaft = CreateBox(name + "_Shaft", parent,
            new Vector3(pos.x, WALL_H * 0.5f + FLOOR_Y, pos.z),
            new Vector3(0.7f, WALL_H, 0.7f));
        SetMat(shaft, s_MatMetalDark);

        // Ribs at intervals
        for (int r = 0; r < 4; r++)
        {
            var rib = CreateBox(name + $"_Rib_{r}", parent,
                new Vector3(pos.x, 1.0f + r * 1.8f + FLOOR_Y, pos.z),
                new Vector3(1.0f, 0.2f, 1.0f));
            SetMat(rib, s_MatMetalDark);
        }
        // Base
        var pillarBase = CreateBox(name + "_Base", parent,
            new Vector3(pos.x, 0.2f + FLOOR_Y, pos.z),
            new Vector3(1.1f, 0.4f, 1.1f));
        SetMat(pillarBase, s_MatConcrete);
        // Cap
        var cap = CreateBox(name + "_Cap", parent,
            new Vector3(pos.x, WALL_H - 0.15f + FLOOR_Y, pos.z),
            new Vector3(1.0f, 0.3f, 1.0f));
        SetMat(cap, s_MatMetalDark);
        // Cyan accent strip (subtle)
        var accent = CreateBox(name + "_Accent", parent,
            new Vector3(pos.x, 1.2f + FLOOR_Y, pos.z),
            new Vector3(0.72f, 0.06f, 0.72f));
        SetMat(accent, s_MatCyanEmission);
    }

    static void BuildPillarB(GameObject parent, string name, Vector3 pos)
    {
        var shaft = CreateBox(name + "_Shaft", parent,
            new Vector3(pos.x, WALL_H * 0.5f + FLOOR_Y, pos.z),
            new Vector3(0.45f, WALL_H, 0.45f));
        SetMat(shaft, s_MatMetalWorn);
        for (int r = 0; r < 3; r++)
        {
            var rib = CreateBox(name + $"_Rib_{r}", parent,
                new Vector3(pos.x, 1.5f + r * 2.0f + FLOOR_Y, pos.z),
                new Vector3(0.65f, 0.15f, 0.65f));
            SetMat(rib, s_MatMetalWorn);
        }
    }

    static void BuildDiagonalBrace(GameObject parent, string name, Vector3 pos, float angleZ)
    {
        var go = CreateBox(name, parent, pos, new Vector3(0.25f, 4f, 0.25f));
        go.transform.rotation = Quaternion.Euler(0, 0, angleZ);
        SetMat(go, s_MatMetalDark);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SECTION 3 — PLATFORMS / CATWALKS
    // ═══════════════════════════════════════════════════════════════════════

    static void BuildPlatforms(GameObject parent)
    {
        var grpMain  = CreateEmpty("MainPlatform",  parent, Vector3.zero);
        var grpUpper = CreateEmpty("UpperPlatform", parent, Vector3.zero);
        var grpStairs = CreateEmpty("Stairs",       parent, Vector3.zero);
        var grpRails  = CreateEmpty("Railings",     parent, Vector3.zero);

        // ── MAIN RAISED PLATFORM (right side, PLAT_Y = 2.5m) ──────────────
        // Platform deck: 6m wide × 4m deep on right side
        var mainDeck = CreateBox("PlatformDeck_Main", grpMain,
            new Vector3(5f, PLAT_Y - 0.15f + FLOOR_Y, 3f),
            new Vector3(6f, 0.3f, 5f));
        SetMat(mainDeck, s_MatGrating);

        // Platform support legs
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                var leg = CreateBox($"PlatformLeg_{i}_{j}", grpMain,
                    new Vector3(2.5f + i * 5f, PLAT_Y * 0.5f, 1.2f + j * 3.6f),
                    new Vector3(0.2f, PLAT_Y, 0.2f));
                SetMat(leg, s_MatMetalDark);
            }
        }
        // Platform fascia (front face detail)
        var fascia = CreateBox("PlatformDeck_Fascia", grpMain,
            new Vector3(5f, PLAT_Y - 0.3f + FLOOR_Y, 0.55f),
            new Vector3(6f, 0.3f, 0.1f));
        SetMat(fascia, s_MatMetalDark);
        // Hazard stripes on fascia edge
        var hazard = CreateBox("PlatformDeck_Hazard", grpMain,
            new Vector3(5f, PLAT_Y - 0.3f + FLOOR_Y, 0.6f),
            new Vector3(5.8f, 0.12f, 0.05f));
        SetMat(hazard, s_MatWarning);

        // ── LEFT CATWALK (maintenance catwalk behind left pillars) ─────────
        var leftCatwalk = CreateBox("Catwalk_Left", grpMain,
            new Vector3(-5f, PLAT_Y - 0.15f + FLOOR_Y, 4f),
            new Vector3(4f, 0.3f, 6f));
        SetMat(leftCatwalk, s_MatGrating);

        // Left catwalk support
        for (int i = 0; i < 3; i++)
        {
            var leg = CreateBox($"Catwalk_Left_Leg_{i}", grpMain,
                new Vector3(-6.5f, PLAT_Y * 0.5f, 1.5f + i * 2.0f),
                new Vector3(0.15f, PLAT_Y, 0.15f));
            SetMat(leg, s_MatMetalDark);
        }

        // ── UPPER CATWALK (UPPER_Y = 4.5m, back wall maintenance) ─────────
        var upperCatwalk = CreateBox("Catwalk_Upper", grpUpper,
            new Vector3(0, UPPER_Y - 0.15f + FLOOR_Y, 5.5f),
            new Vector3(10f, 0.25f, 2f));
        SetMat(upperCatwalk, s_MatGrating);

        for (int i = 0; i < 3; i++)
        {
            var leg = CreateBox($"UpperCatwalk_Leg_{i}", grpUpper,
                new Vector3(-4f + i * 4f, (UPPER_Y + PLAT_Y) * 0.5f, 5.5f),
                new Vector3(0.15f, UPPER_Y - PLAT_Y, 0.15f));
            SetMat(leg, s_MatMetalDark);
        }

        // ── STAIRS (right side connecting floor to main platform) ──────────
        BuildStaircase(grpStairs, new Vector3(1.8f, FLOOR_Y, 1f));

        // ── RAILINGS ───────────────────────────────────────────────────────
        // Main platform front railing
        BuildRailing(grpRails, "Rail_MainFront", new Vector3(5f, PLAT_Y + FLOOR_Y, 0.55f), 6f, true);
        // Main platform left railing
        BuildRailing(grpRails, "Rail_MainLeft",  new Vector3(2.3f, PLAT_Y + FLOOR_Y, 3f),  5f, false);
        // Left catwalk front railing
        BuildRailing(grpRails, "Rail_LeftFront", new Vector3(-5f, PLAT_Y + FLOOR_Y, 1.0f), 4f, true);
        // Left catwalk back railing
        BuildRailing(grpRails, "Rail_LeftBack",  new Vector3(-5f, PLAT_Y + FLOOR_Y, 7.1f), 4f, true);
        // Upper catwalk front railing
        BuildRailing(grpRails, "Rail_UpperFront",new Vector3(0f, UPPER_Y + FLOOR_Y, 4.4f), 10f, true);
    }

    static void BuildStaircase(GameObject parent, Vector3 origin)
    {
        int stepCount = 5;
        float stepW    = 2.5f;
        float stepH    = PLAT_Y / stepCount;  // height per step
        float stepD    = 0.55f;               // depth per step

        for (int i = 0; i < stepCount; i++)
        {
            var step = CreateBox($"Stair_Step_{i}", parent,
                new Vector3(origin.x, FLOOR_Y + stepH * (i + 0.5f), origin.z + stepD * (i + 0.5f)),
                new Vector3(stepW, stepH, stepD));
            SetMat(step, s_MatMetalDark);
        }
        // Left handrail post
        var handrailL = CreateBox("Stair_Handrail_Left", parent,
            new Vector3(origin.x - stepW * 0.5f + 0.06f, FLOOR_Y + PLAT_Y * 0.5f, origin.z + stepD * stepCount * 0.5f),
            new Vector3(0.08f, PLAT_Y + 0.2f, stepD * stepCount));
        handrailL.transform.rotation = Quaternion.Euler(0f, 0f, -Mathf.Atan2(PLAT_Y, stepD * stepCount) * Mathf.Rad2Deg);
        SetMat(handrailL, s_MatMetalDark);
        // Right handrail post
        var handrailR = CreateBox("Stair_Handrail_Right", parent,
            new Vector3(origin.x + stepW * 0.5f - 0.06f, FLOOR_Y + PLAT_Y * 0.5f, origin.z + stepD * stepCount * 0.5f),
            new Vector3(0.08f, PLAT_Y + 0.2f, stepD * stepCount));
        handrailR.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(PLAT_Y, stepD * stepCount) * Mathf.Rad2Deg);
        SetMat(handrailR, s_MatMetalDark);
        // Stringer (side support under stairs)
        var stringer = CreateBox("Stair_Stringer", parent,
            new Vector3(origin.x, FLOOR_Y + PLAT_Y * 0.5f, origin.z + stepD * stepCount * 0.5f),
            new Vector3(stepW, 0.15f, stepD * stepCount));
        SetMat(stringer, s_MatMetalDark);
    }

    static void BuildRailing(GameObject parent, string name, Vector3 center, float length, bool alongX)
    {
        // Top bar
        var topBar = CreateBox(name + "_TopBar", parent,
            new Vector3(center.x, center.y + 0.95f, center.z),
            alongX ? new Vector3(length, 0.08f, 0.08f) : new Vector3(0.08f, 0.08f, length));
        SetMat(topBar, s_MatMetalDark);
        // Mid bar
        var midBar = CreateBox(name + "_MidBar", parent,
            new Vector3(center.x, center.y + 0.5f, center.z),
            alongX ? new Vector3(length, 0.05f, 0.05f) : new Vector3(0.05f, 0.05f, length));
        SetMat(midBar, s_MatMetalDark);
        // Posts
        int postCount = Mathf.Max(2, Mathf.RoundToInt(length / 1.5f));
        for (int i = 0; i <= postCount; i++)
        {
            float t = (float)i / postCount;
            float offset = Mathf.Lerp(-length * 0.5f, length * 0.5f, t);
            var post = CreateBox(name + $"_Post_{i}", parent,
                new Vector3(
                    alongX ? center.x + offset : center.x,
                    center.y + 0.5f,
                    alongX ? center.z : center.z + offset),
                new Vector3(0.06f, 1.0f, 0.06f));
            SetMat(post, s_MatMetalDark);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SECTION 4 — CONTAINMENT CHAMBERS
    // ═══════════════════════════════════════════════════════════════════════

    static void BuildContainment(GameObject parent)
    {
        // AERON CHAMBER — Primary focal point, center stage
        BuildContainmentChamberA(parent, "Aeron_Chamber",
            new Vector3(0, FLOOR_Y, 3.5f),
            chamberRadius: 1.4f, chamberHeight: 4.2f,
            isActive: true, isPrimary: true);

        // Secondary chambers — background storytelling
        // These are behind / flanking Aeron's chamber
        BuildContainmentChamberB(parent, "Containment_01",
            new Vector3(-4f, FLOOR_Y, 7f),
            chamberRadius: 1.1f, chamberHeight: 3.8f, status: "active");

        BuildContainmentChamberB(parent, "Containment_02",
            new Vector3(4.5f, FLOOR_Y, 7.5f),
            chamberRadius: 1.2f, chamberHeight: 4.0f, status: "inactive");

        BuildContainmentChamberB(parent, "Containment_03",
            new Vector3(-7f, FLOOR_Y, 5f),
            chamberRadius: 0.9f, chamberHeight: 3.2f, status: "malfunction");

        BuildContainmentChamberB(parent, "Containment_04",
            new Vector3(7f, FLOOR_Y, 6f),
            chamberRadius: 1.0f, chamberHeight: 3.5f, status: "inactive");
    }

    static void BuildContainmentChamberA(GameObject parent, string name, Vector3 pos,
        float chamberRadius, float chamberHeight, bool isActive, bool isPrimary)
    {
        var root = CreateEmpty(name, parent, pos);

        // Circular base (octagonal approximation with cubes)
        BuildOctagonalBase(root, "Base", Vector3.zero, chamberRadius * 1.5f, 0.4f);

        // Lower housing ring
        BuildCylinderApprox(root, "LowerHousing", new Vector3(0, 0.4f, 0),
            chamberRadius * 1.1f, 0.8f, 12, s_MatMetalDark);

        // Main glass cylinder
        BuildCylinderApprox(root, "Glass", new Vector3(0, 0.9f, 0),
            chamberRadius, chamberHeight, 16, s_MatGlass);

        // Upper mechanical ring
        BuildCylinderApprox(root, "UpperRing", new Vector3(0, 0.9f + chamberHeight, 0),
            chamberRadius * 1.15f, 0.5f, 12, s_MatMetalDark);

        // Structural frame (4 vertical posts around cylinder)
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            float fx = Mathf.Sin(angle) * (chamberRadius + 0.15f);
            float fz = Mathf.Cos(angle) * (chamberRadius + 0.15f);
            var frame = CreateBox(name + $"_Frame_{i}", root,
                new Vector3(fx, 0.9f + chamberHeight * 0.5f, fz),
                new Vector3(0.15f, chamberHeight, 0.15f));
            SetMat(frame, s_MatMetalDark);
        }

        // Cap / top
        BuildCylinderApprox(root, "Cap", new Vector3(0, 0.9f + chamberHeight + 0.5f, 0),
            chamberRadius * 0.8f, 0.4f, 10, s_MatMetalDark);

        // Conduit pipes (2 pipes coming from base going to floor/wall)
        BuildConduitPipe(root, "Conduit_L",
            new Vector3(-chamberRadius - 0.1f, 0.3f, 0),
            new Vector3(-chamberRadius - 0.25f, 0.3f, 2f), 0.08f);
        BuildConduitPipe(root, "Conduit_R",
            new Vector3(chamberRadius + 0.1f, 0.3f, 0),
            new Vector3(chamberRadius + 0.25f, 0.3f, 2f), 0.08f);

        // Control interface panel (small)
        var panel = CreateBox(name + "_ControlPanel", root,
            new Vector3(chamberRadius + 0.4f, 1.2f, -0.5f),
            new Vector3(0.6f, 0.8f, 0.08f));
        SetMat(panel, s_MatConsole);
        // Panel screen glow
        var screen = CreateBox(name + "_Screen", root,
            new Vector3(chamberRadius + 0.4f, 1.2f, -0.5f),
            new Vector3(0.4f, 0.5f, 0.02f));
        SetMat(screen, s_MatCyanEmission);

        // Status indicator lights
        for (int i = 0; i < 3; i++)
        {
            float angle = (i * 120f + 30f) * Mathf.Deg2Rad;
            var light = CreateBox(name + $"_StatusLight_{i}", root,
                new Vector3(Mathf.Sin(angle) * (chamberRadius * 1.12f),
                           0.55f,
                           Mathf.Cos(angle) * (chamberRadius * 1.12f)),
                new Vector3(0.08f, 0.08f, 0.08f));
            SetMat(light, s_MatCyanEmission);
        }

        // Locking mechanism bolts
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            var bolt = CreateBox(name + $"_Lock_{i}", root,
                new Vector3(Mathf.Sin(angle) * (chamberRadius * 1.08f),
                           0.9f + chamberHeight + 0.22f,
                           Mathf.Cos(angle) * (chamberRadius * 1.08f)),
                new Vector3(0.12f, 0.12f, 0.12f));
            SetMat(bolt, s_MatMetalDark);
        }
    }

    static void BuildContainmentChamberB(GameObject parent, string name, Vector3 pos,
        float chamberRadius, float chamberHeight, string status)
    {
        var root = CreateEmpty(name, parent, pos);

        // Simple base
        BuildOctagonalBase(root, "Base", Vector3.zero, chamberRadius * 1.3f, 0.35f);

        // Housing
        BuildCylinderApprox(root, "Housing", new Vector3(0, 0.35f, 0),
            chamberRadius * 1.05f, 0.6f, 10, s_MatMetalWorn);

        // Glass (dimmer for inactive/malfunction)
        var glassMat = status == "active" ? s_MatGlass : s_MatGrating;
        BuildCylinderApprox(root, "Glass", new Vector3(0, 0.75f, 0),
            chamberRadius, chamberHeight, 12, glassMat);

        // Ring
        BuildCylinderApprox(root, "Ring", new Vector3(0, 0.75f + chamberHeight, 0),
            chamberRadius * 1.1f, 0.4f, 10, s_MatMetalWorn);

        // Frame posts (3 instead of 4 for secondary)
        for (int i = 0; i < 3; i++)
        {
            float angle = i * 120f * Mathf.Deg2Rad;
            var frame = CreateBox(name + $"_Frame_{i}", root,
                new Vector3(Mathf.Sin(angle) * (chamberRadius + 0.12f),
                           0.75f + chamberHeight * 0.5f,
                           Mathf.Cos(angle) * (chamberRadius + 0.12f)),
                new Vector3(0.12f, chamberHeight, 0.12f));
            SetMat(frame, s_MatMetalWorn);
        }

        // Status indicator
        var statusLight = CreateBox(name + "_Status", root,
            new Vector3(chamberRadius + 0.05f, 0.5f, 0),
            new Vector3(0.1f, 0.1f, 0.1f));
        SetMat(statusLight, status == "malfunction" ? s_MatWarning :
                            status == "active"      ? s_MatCyanEmission : s_MatMetalDark);

        // Damage detail for malfunction
        if (status == "malfunction")
        {
            var crack = CreateBox(name + "_DamageMark", root,
                new Vector3(0, 0.9f, chamberRadius + 0.01f),
                new Vector3(0.3f, 0.8f, 0.05f));
            SetMat(crack, s_MatMetalWorn);
        }
    }

    static void BuildOctagonalBase(GameObject parent, string name, Vector3 localPos,
        float radius, float height)
    {
        // Approximate octagon with 8 thin boxes
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            float nx = Mathf.Sin(angle);
            float nz = Mathf.Cos(angle);
            var seg = CreateBox($"{name}_Seg_{i}", parent,
                localPos + new Vector3(nx * radius * 0.65f, height * 0.5f, nz * radius * 0.65f),
                new Vector3(radius * 0.7f, height, radius * 0.35f));
            var go = seg;
            go.transform.localRotation = Quaternion.Euler(0, i * 45f, 0);
            SetMat(seg, s_MatMetalDark);
        }
        // Center fill
        var center = CreateBox($"{name}_Center", parent,
            localPos + new Vector3(0, height * 0.5f, 0),
            new Vector3(radius * 0.9f, height, radius * 0.9f));
        SetMat(center, s_MatConcrete);
    }

    static void BuildCylinderApprox(GameObject parent, string name, Vector3 localPos,
        float radius, float height, int segments, Material mat)
    {
        // Approximate cylinder with N-sided prism of thin boxes
        float segAngle = 360f / segments;
        float segW = 2f * radius * Mathf.Sin(Mathf.PI / segments);
        for (int i = 0; i < segments; i++)
        {
            float angle = i * segAngle * Mathf.Deg2Rad;
            float nx = Mathf.Sin(angle + (segAngle * 0.5f * Mathf.Deg2Rad));
            float nz = Mathf.Cos(angle + (segAngle * 0.5f * Mathf.Deg2Rad));
            var seg = CreateBox($"{name}_Seg_{i}", parent,
                localPos + new Vector3(nx * radius, height * 0.5f, nz * radius),
                new Vector3(segW * 1.05f, height, 0.04f));
            seg.transform.localRotation = Quaternion.Euler(0, -(i * segAngle + segAngle * 0.5f), 0);
            SetMat(seg, mat);
        }
    }

    static void BuildConduitPipe(GameObject parent, string name, Vector3 start, Vector3 end, float radius)
    {
        Vector3 mid = (start + end) * 0.5f;
        float length = Vector3.Distance(start, end);
        Vector3 dir = (end - start).normalized;
        var go = CreateBox(name, parent, mid, new Vector3(radius * 2f, radius * 2f, length));
        go.transform.localRotation = Quaternion.LookRotation(dir);
        SetMat(go, s_MatCable);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SECTION 5 — DOORS
    // ═══════════════════════════════════════════════════════════════════════

    static void BuildDoors(GameObject parent)
    {
        // ── MAIN SECURITY DOOR (back-right of lab) ─────────────────────────
        var secDoor = CreateEmpty("SecurityDoor", parent, new Vector3(5.5f, FLOOR_Y, ROOM_D * 0.5f - 0.2f));

        // Heavy frame
        var frameL = CreateBox("SecDoor_FrameL", secDoor, new Vector3(-1.7f, 2f, 0), new Vector3(0.4f, 4f, 0.4f));
        var frameR = CreateBox("SecDoor_FrameR", secDoor, new Vector3( 1.7f, 2f, 0), new Vector3(0.4f, 4f, 0.4f));
        var frameT = CreateBox("SecDoor_FrameT", secDoor, new Vector3( 0,   4.1f, 0), new Vector3(3.8f, 0.4f, 0.4f));
        SetMat(frameL, s_MatMetalDark); SetMat(frameR, s_MatMetalDark); SetMat(frameT, s_MatMetalDark);

        // Door panels (2 halves)
        var panelL = CreateBox("SecDoor_PanelL", secDoor, new Vector3(-0.75f, 2.0f, 0.05f), new Vector3(1.5f, 4.0f, 0.25f));
        var panelR = CreateBox("SecDoor_PanelR", secDoor, new Vector3( 0.75f, 2.0f, 0.05f), new Vector3(1.5f, 4.0f, 0.25f));
        SetMat(panelL, s_MatMetalDark); SetMat(panelR, s_MatMetalDark);

        // Panel ribs
        for (int i = 0; i < 3; i++)
        {
            float yPos = 0.6f + i * 1.2f;
            var rib = CreateBox($"SecDoor_Rib_{i}", secDoor, new Vector3(0, yPos, 0.2f), new Vector3(3.0f, 0.12f, 0.08f));
            SetMat(rib, s_MatMetalDark);
        }

        // Warning stripes (bottom)
        var warning = CreateBox("SecDoor_Warning", secDoor, new Vector3(0, 0.15f, 0.2f), new Vector3(3.0f, 0.3f, 0.1f));
        SetMat(warning, s_MatWarning);

        // Locking mechanism
        var lockMech = CreateBox("SecDoor_Lock", secDoor, new Vector3(0.05f, 2.0f, 0.3f), new Vector3(0.25f, 0.4f, 0.2f));
        SetMat(lockMech, s_MatMetalDark);

        // Status light
        var statusLight = CreateBox("SecDoor_StatusLight", secDoor,
            new Vector3(1.5f, 3.5f, 0.35f), new Vector3(0.12f, 0.12f, 0.12f));
        SetMat(statusLight, s_MatCyanEmission);

        // ── MAINTENANCE DOOR (left side wall) ──────────────────────────────
        var maintDoor = CreateEmpty("MaintenanceDoor", parent, new Vector3(-ROOM_W * 0.5f + 0.2f, FLOOR_Y, 2f));

        var mFrameTop = CreateBox("MaintDoor_FrameT", maintDoor, new Vector3(0, 3.1f, 0), new Vector3(0.3f, 0.3f, 1.8f));
        var mFrameL   = CreateBox("MaintDoor_FrameL", maintDoor, new Vector3(0, 1.5f, -0.8f), new Vector3(0.3f, 3.0f, 0.3f));
        var mFrameR   = CreateBox("MaintDoor_FrameR", maintDoor, new Vector3(0, 1.5f,  0.8f), new Vector3(0.3f, 3.0f, 0.3f));
        SetMat(mFrameTop, s_MatMetalWorn); SetMat(mFrameL, s_MatMetalWorn); SetMat(mFrameR, s_MatMetalWorn);

        var mPanel = CreateBox("MaintDoor_Panel", maintDoor, new Vector3(0.08f, 1.5f, 0), new Vector3(0.2f, 3.0f, 1.6f));
        SetMat(mPanel, s_MatMetalWorn);

        var mWarning = CreateBox("MaintDoor_Warning", maintDoor, new Vector3(0.12f, 0.2f, 0), new Vector3(0.08f, 0.3f, 1.6f));
        SetMat(mWarning, s_MatWarning);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SECTION 6 — MACHINERY
    // ═══════════════════════════════════════════════════════════════════════

    static void BuildMachinery(GameObject parent)
    {
        var grpServers = CreateEmpty("Servers",    parent, Vector3.zero);
        var grpPower   = CreateEmpty("PowerUnits", parent, Vector3.zero);
        var grpEquip   = CreateEmpty("Equipment",  parent, Vector3.zero);

        // ── SERVER RACKS (right side wall, ground level) ───────────────────
        for (int i = 0; i < 3; i++)
        {
            float zPos = 0.5f + i * 1.6f;
            BuildServerRack(grpServers, $"ServerRack_{i}", new Vector3(7.3f, FLOOR_Y, zPos));
        }

        // ── POWER UNITS (left side background) ────────────────────────────
        for (int i = 0; i < 2; i++)
        {
            BuildPowerUnit(grpPower, $"PowerUnit_{i}", new Vector3(-6.8f, FLOOR_Y, 3.5f + i * 2.5f));
        }

        // ── RESEARCH / CONTROL AREA (left-front of scene) ─────────────────
        BuildConsoleStation(grpEquip, "ControlStation_Left",  new Vector3(-4f, FLOOR_Y, -1.5f));
        BuildConsoleStation(grpEquip, "ControlStation_Right", new Vector3(-2f, FLOOR_Y, -2f));
    }

    static void BuildServerRack(GameObject parent, string name, Vector3 pos)
    {
        var root = CreateEmpty(name, parent, pos);
        // Main chassis
        var chassis = CreateBox("Chassis", root, new Vector3(0, 1.0f, 0), new Vector3(0.5f, 2.0f, 0.7f));
        SetMat(chassis, s_MatMetalDark);
        // Unit slots (visual detail)
        for (int i = 0; i < 8; i++)
        {
            var slot = CreateBox($"Slot_{i}", root,
                new Vector3(0, 0.2f + i * 0.22f, 0.37f),
                new Vector3(0.42f, 0.14f, 0.04f));
            SetMat(slot, i % 3 == 0 ? s_MatCyanEmission : s_MatConsole);
        }
        // Status indicator
        var light = CreateBox("StatusLight", root, new Vector3(0.21f, 1.8f, 0.36f), new Vector3(0.04f, 0.04f, 0.04f));
        SetMat(light, s_MatCyanEmission);
    }

    static void BuildPowerUnit(GameObject parent, string name, Vector3 pos)
    {
        var root = CreateEmpty(name, parent, pos);
        var body = CreateBox("Body", root, new Vector3(0, 0.7f, 0), new Vector3(0.7f, 1.4f, 0.5f));
        SetMat(body, s_MatMetalDark);
        var panel = CreateBox("Panel", root, new Vector3(0.36f, 0.8f, 0), new Vector3(0.05f, 0.6f, 0.4f));
        SetMat(panel, s_MatConsole);
        var meter = CreateBox("Meter", root, new Vector3(0.36f, 1.1f, 0), new Vector3(0.03f, 0.15f, 0.25f));
        SetMat(meter, s_MatCyanEmission);
    }

    static void BuildConsoleStation(GameObject parent, string name, Vector3 pos)
    {
        var root = CreateEmpty(name, parent, pos);
        root.transform.rotation = Quaternion.Euler(0, 15f, 0);

        // Desk/console body
        var desk = CreateBox("Desk", root, new Vector3(0, 0.8f, 0), new Vector3(1.6f, 0.1f, 0.7f));
        SetMat(desk, s_MatMetalDark);

        // Screen stack (angled back)
        var screenAngle = CreateBox("ScreenBack", root, new Vector3(0, 1.35f, 0.1f), new Vector3(1.4f, 0.9f, 0.06f));
        screenAngle.transform.localRotation = Quaternion.Euler(-15f, 0, 0);
        SetMat(screenAngle, s_MatMetalDark);

        // Screen surfaces (mostly dark, subtle glow)
        for (int i = 0; i < 2; i++)
        {
            var screen = CreateBox($"Screen_{i}", root,
                new Vector3(-0.35f + i * 0.72f, 1.36f, 0.14f),
                new Vector3(0.6f, 0.7f, 0.02f));
            screen.transform.localRotation = Quaternion.Euler(-15f, 0, 0);
            SetMat(screen, s_MatConsole);
        }

        // Keyboard surface
        var kb = CreateBox("Keyboard", root, new Vector3(0, 0.86f, -0.1f), new Vector3(1.0f, 0.04f, 0.3f));
        SetMat(kb, s_MatMetalWorn);

        // Support legs
        for (int i = 0; i < 2; i++)
        {
            var leg = CreateBox($"Leg_{i}", root,
                new Vector3(-0.7f + i * 1.4f, 0.4f, 0),
                new Vector3(0.08f, 0.8f, 0.5f));
            SetMat(leg, s_MatMetalDark);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SECTION 7 — PROPS
    // ═══════════════════════════════════════════════════════════════════════

    static void BuildProps(GameObject parent)
    {
        var grpConsoles = CreateEmpty("Consoles",    parent, Vector3.zero);
        var grpCabinets = CreateEmpty("Cabinets",    parent, Vector3.zero);
        var grpCrates   = CreateEmpty("Crates",      parent, Vector3.zero);
        var grpMaint    = CreateEmpty("Maintenance", parent, Vector3.zero);

        // ── EMERGENCY PANELS (wall-mounted on back wall) ───────────────────
        BuildEmergencyPanel(grpMaint, "EmergPanel_L", new Vector3(-3.5f, 2.0f, ROOM_D * 0.5f - 0.2f));
        BuildEmergencyPanel(grpMaint, "EmergPanel_R", new Vector3( 3.5f, 2.0f, ROOM_D * 0.5f - 0.2f));

        // ── ELECTRICAL CABINETS ────────────────────────────────────────────
        BuildElectricalCabinet(grpCabinets, "ElecCab_L1", new Vector3(-7.5f, FLOOR_Y, 0f));
        BuildElectricalCabinet(grpCabinets, "ElecCab_L2", new Vector3(-7.5f, FLOOR_Y, 1.2f));

        // ── INDUSTRIAL CRATES ─────────────────────────────────────────────
        BuildCrate(grpCrates, "Crate_01", new Vector3(7.0f, FLOOR_Y, -1.5f), new Vector3(0.8f, 0.8f, 0.8f));
        BuildCrate(grpCrates, "Crate_02", new Vector3(7.0f, FLOOR_Y, -2.5f), new Vector3(1.0f, 0.6f, 0.8f));
        BuildCrate(grpCrates, "Crate_03", new Vector3(7.8f, FLOOR_Y + 0.8f, -2.0f), new Vector3(0.9f, 0.6f, 0.9f));

        // ── FLOOR VENTS ───────────────────────────────────────────────────
        BuildFloorVent(grpMaint, "Vent_Floor_01", new Vector3(-2.0f, FLOOR_Y + 0.005f, -3f));
        BuildFloorVent(grpMaint, "Vent_Floor_02", new Vector3( 2.0f, FLOOR_Y + 0.005f, -3f));
        BuildFloorVent(grpMaint, "Vent_Floor_03", new Vector3(-2.0f, FLOOR_Y + 0.005f,  7f));

        // ── WARNING LIGHTS (wall-mounted) ─────────────────────────────────
        BuildWarningLight(grpMaint, "WarnLight_L", new Vector3(-7.6f, 4.5f, 2f));
        BuildWarningLight(grpMaint, "WarnLight_R", new Vector3( 7.6f, 4.5f, 2f));
        BuildWarningLight(grpMaint, "WarnLight_Door", new Vector3(5.5f, 4.5f, ROOM_D * 0.5f - 0.1f));

        // ── SMALL MAINTENANCE BOXES (platform) ────────────────────────────
        BuildMaintBox(grpMaint, "MaintBox_01", new Vector3(3.5f, PLAT_Y + 0.15f, 2f));
        BuildMaintBox(grpMaint, "MaintBox_02", new Vector3(7.0f, PLAT_Y + 0.15f, 2.5f));
    }

    static void BuildEmergencyPanel(GameObject parent, string name, Vector3 pos)
    {
        var panel = CreateBox(name, parent, pos, new Vector3(0.6f, 0.4f, 0.1f));
        SetMat(panel, s_MatWarning);
        var detail = CreateBox(name + "_Handle", parent,
            new Vector3(pos.x, pos.y, pos.z + 0.06f), new Vector3(0.08f, 0.25f, 0.04f));
        SetMat(detail, s_MatMetalDark);
    }

    static void BuildElectricalCabinet(GameObject parent, string name, Vector3 pos)
    {
        var root = CreateEmpty(name, parent, pos);
        var body = CreateBox("Body", root, new Vector3(0, 0.85f, 0), new Vector3(0.3f, 1.7f, 0.5f));
        SetMat(body, s_MatMetalDark);
        var door2 = CreateBox("Door", root, new Vector3(0.16f, 0.85f, 0), new Vector3(0.04f, 1.5f, 0.45f));
        SetMat(door2, s_MatMetalWorn);
        var handle = CreateBox("Handle", root, new Vector3(0.2f, 0.85f, 0), new Vector3(0.03f, 0.15f, 0.04f));
        SetMat(handle, s_MatMetalDark);
        var light = CreateBox("Light", root, new Vector3(0.19f, 1.6f, 0), new Vector3(0.03f, 0.06f, 0.03f));
        SetMat(light, s_MatCyanEmission);
    }

    static void BuildCrate(GameObject parent, string name, Vector3 pos, Vector3 size)
    {
        var crate = CreateBox(name, parent,
            new Vector3(pos.x, pos.y + size.y * 0.5f, pos.z), size);
        SetMat(crate, s_MatMetalWorn);
        // Lid seam
        var lid = CreateBox(name + "_Lid", parent,
            new Vector3(pos.x, pos.y + size.y - 0.02f, pos.z),
            new Vector3(size.x, 0.05f, size.z));
        SetMat(lid, s_MatMetalDark);
    }

    static void BuildFloorVent(GameObject parent, string name, Vector3 pos)
    {
        var vent = CreateBox(name, parent, pos, new Vector3(0.6f, 0.04f, 0.4f));
        SetMat(vent, s_MatGrating);
    }

    static void BuildWarningLight(GameObject parent, string name, Vector3 pos)
    {
        var root = CreateEmpty(name, parent, pos);
        // Mount bracket
        var bracket = CreateBox("Bracket", root, Vector3.zero, new Vector3(0.12f, 0.08f, 0.12f));
        SetMat(bracket, s_MatMetalDark);
        // Light dome (sphere approximation)
        var dome = CreateBox("Dome", root, new Vector3(0, 0.12f, 0), new Vector3(0.18f, 0.14f, 0.18f));
        SetMat(dome, s_MatWarning);  // Static - no animation this pass
    }

    static void BuildMaintBox(GameObject parent, string name, Vector3 pos)
    {
        var box = CreateBox(name, parent, pos, new Vector3(0.5f, 0.3f, 0.4f));
        SetMat(box, s_MatMetalWorn);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SECTION 8 — PIPES AND CABLES
    // ═══════════════════════════════════════════════════════════════════════

    static void BuildPipesAndCables(GameObject parent)
    {
        // Overhead pipe runs (Z-direction, ceiling level)
        for (int i = 0; i < 3; i++)
        {
            float xPos = -5f + i * 5f;
            var pipe = CreateBox($"Pipe_Overhead_X_{i}", parent,
                new Vector3(xPos, WALL_H - 0.8f, 2f),
                new Vector3(0.18f, 0.18f, ROOM_D * 0.8f));
            SetMat(pipe, s_MatCable);
        }

        // Wall-mounted cable trays (right wall)
        var cableTrayR = CreateBox("CableTray_Right", parent,
            new Vector3(ROOM_W * 0.5f - 0.4f, 3.5f, 2f),
            new Vector3(0.25f, 0.15f, 10f));
        SetMat(cableTrayR, s_MatMetalDark);

        // Cable bundles dropping from tray to equipment
        for (int i = 0; i < 3; i++)
        {
            var cable = CreateBox($"Cable_Drop_R_{i}", parent,
                new Vector3(ROOM_W * 0.5f - 0.35f, 1.75f, -0.5f + i * 2f),
                new Vector3(0.12f, 3.5f, 0.08f));
            SetMat(cable, s_MatCable);
        }

        // Containment chamber coolant pipes (Z-direction along floor)
        var coolantPipe1 = CreateBox("Pipe_Coolant_01", parent,
            new Vector3(-1.5f, 0.1f, 4.5f), new Vector3(0.14f, 0.14f, 4f));
        SetMat(coolantPipe1, s_MatCable);
        var coolantPipe2 = CreateBox("Pipe_Coolant_02", parent,
            new Vector3( 1.5f, 0.1f, 4.5f), new Vector3(0.14f, 0.14f, 4f));
        SetMat(coolantPipe2, s_MatCable);

        // Elbow joints
        var elbow1 = CreateBox("Pipe_Elbow_01", parent,
            new Vector3(-1.5f, 0.1f, 2.6f), new Vector3(0.2f, 0.2f, 0.2f));
        SetMat(elbow1, s_MatMetalDark);
        var elbow2 = CreateBox("Pipe_Elbow_02", parent,
            new Vector3( 1.5f, 0.1f, 2.6f), new Vector3(0.2f, 0.2f, 0.2f));
        SetMat(elbow2, s_MatMetalDark);

        // Cyan glowing accent pipe (subtle energy conduit)
        var glowPipe = CreateBox("Pipe_EnergyConduit", parent,
            new Vector3(0f, 0.08f, 4.0f), new Vector3(0.08f, 0.08f, 5f));
        SetMat(glowPipe, s_MatCyanEmission);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SECTION 9 — ADDITIONAL LIGHTING
    // ═══════════════════════════════════════════════════════════════════════

    static void BuildAdditionalLighting(GameObject parent)
    {
        var grpContainLights = CreateEmpty("ContainmentLights", parent, Vector3.zero);
        var grpCeilLights    = CreateEmpty("CeilingLights",     parent, Vector3.zero);
        var grpWallLights    = CreateEmpty("WallLights",        parent, Vector3.zero);
        var grpStatus        = CreateEmpty("StatusIndicators",  parent, Vector3.zero);

        // ── AERON CONTAINMENT KEY LIGHT ────────────────────────────────────
        var aeronLight = CreatePointLight("ContainLight_Aeron", grpContainLights,
            new Vector3(0, 5.5f, 3.5f),
            ColCyan, intensity: 1.2f, range: 6f);

        // ── SECONDARY CONTAINMENT LIGHTS ──────────────────────────────────
        CreatePointLight("ContainLight_01", grpContainLights, new Vector3(-4f, 4.5f, 7f),   ColCyan, 0.5f, 5f);
        CreatePointLight("ContainLight_02", grpContainLights, new Vector3( 4.5f, 4.5f, 7.5f), ColCyan, 0.4f, 4f);
        CreatePointLight("ContainLight_03", grpContainLights, new Vector3(-7f, 4f, 5f),     ColCyan, 0.3f, 4f);

        // ── INDUSTRIAL CEILING SPOTS ───────────────────────────────────────
        float[] spotX = { -6f, -2f, 2f, 6f };
        float[] spotZ = { 0f, 2f, 4f, 6f };
        for (int i = 0; i < 4; i++)
        {
            CreateSpotLight($"CeilSpot_{i}", grpCeilLights,
                new Vector3(spotX[i], WALL_H - 0.5f, spotZ[i]),
                ColCyanFill, intensity: 0.8f, range: 8f, angle: 55f);

            // Physical ceiling light fixture geometry
            var fixture = CreateBox($"CeilLight_Fixture_{i}", grpCeilLights,
                new Vector3(spotX[i], WALL_H - 0.35f, spotZ[i]),
                new Vector3(0.4f, 0.2f, 0.6f));
            SetMat(fixture, s_MatMetalDark);
            var bulb = CreateBox($"CeilLight_Bulb_{i}", grpCeilLights,
                new Vector3(spotX[i], WALL_H - 0.5f, spotZ[i]),
                new Vector3(0.25f, 0.08f, 0.5f));
            SetMat(bulb, s_MatCyanEmission);
        }

        // ── CONSOLE AREA AMBIENT ───────────────────────────────────────────
        CreatePointLight("ConsoleLight", grpWallLights,
            new Vector3(-3f, 2.0f, -1.5f), ColCyanFill, 0.4f, 3.5f);

        // ── PLATFORM UNDERLIGHT (subtle cyan under catwalks) ───────────────
        CreatePointLight("PlatformUnderLight", grpWallLights,
            new Vector3(5f, PLAT_Y - 0.5f, 3f), ColCyan, 0.3f, 4f);
    }

    static GameObject CreatePointLight(string name, GameObject parent, Vector3 pos,
        Color color, float intensity, float range)
    {
        var go = CreateEmpty(name, parent, pos);
        var light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.6f;
        return go;
    }

    static GameObject CreateSpotLight(string name, GameObject parent, Vector3 pos,
        Color color, float intensity, float range, float angle)
    {
        var go = CreateEmpty(name, parent, pos);
        go.transform.rotation = Quaternion.Euler(90f, 0, 0);
        var light = go.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.spotAngle = angle;
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.5f;
        return go;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SECTION 10 — COLLISION
    // ═══════════════════════════════════════════════════════════════════════

    static void BuildCollision(GameObject parent)
    {
        // Main floor collision (BoxCollider matching Lab_Floor_Main)
        var floorColl = CreateEmpty("Coll_Floor", parent, new Vector3(0, FLOOR_Y - 0.15f, 2f));
        var floorBox = floorColl.AddComponent<BoxCollider>();
        floorBox.size = new Vector3(ROOM_W, 0.3f, ROOM_D);
        SetLayer(floorColl, 8); // Ground

        // Wall colliders
        var backWallColl = CreateEmpty("Coll_Wall_Back", parent,
            new Vector3(0, WALL_H * 0.5f, ROOM_D * 0.5f));
        backWallColl.AddComponent<BoxCollider>().size = new Vector3(ROOM_W, WALL_H, 0.3f);

        var leftWallColl = CreateEmpty("Coll_Wall_Left", parent,
            new Vector3(-ROOM_W * 0.5f, WALL_H * 0.5f, 2f));
        leftWallColl.AddComponent<BoxCollider>().size = new Vector3(0.3f, WALL_H, ROOM_D);

        var rightWallColl = CreateEmpty("Coll_Wall_Right", parent,
            new Vector3(ROOM_W * 0.5f, WALL_H * 0.5f, 2f));
        rightWallColl.AddComponent<BoxCollider>().size = new Vector3(0.3f, WALL_H, ROOM_D);

        // Platform collision
        var mainPlatColl = CreateEmpty("Coll_Platform_Main", parent,
            new Vector3(5f, PLAT_Y - 0.15f, 3f));
        mainPlatColl.AddComponent<BoxCollider>().size = new Vector3(6f, 0.3f, 5f);
        SetLayer(mainPlatColl, 8);

        var leftCatwalkColl = CreateEmpty("Coll_Catwalk_Left", parent,
            new Vector3(-5f, PLAT_Y - 0.15f, 4f));
        leftCatwalkColl.AddComponent<BoxCollider>().size = new Vector3(4f, 0.3f, 6f);
        SetLayer(leftCatwalkColl, 8);

        var upperCatwalkColl = CreateEmpty("Coll_Catwalk_Upper", parent,
            new Vector3(0f, UPPER_Y - 0.15f, 5.5f));
        upperCatwalkColl.AddComponent<BoxCollider>().size = new Vector3(10f, 0.25f, 2f);
        SetLayer(upperCatwalkColl, 8);

        // Stair collision (simple ramp box)
        var stairColl = CreateEmpty("Coll_Stairs", parent,
            new Vector3(1.8f, PLAT_Y * 0.5f, 1.4f));
        stairColl.AddComponent<BoxCollider>().size = new Vector3(2.5f, PLAT_Y, 1.4f);
        SetLayer(stairColl, 8);

        // Aeron chamber boundary collision (prevents walking through chamber base)
        var chamberColl = CreateEmpty("Coll_AeronChamber", parent, new Vector3(0, FLOOR_Y + 0.3f, 3.5f));
        var capsule = chamberColl.AddComponent<CapsuleCollider>();
        capsule.radius = 1.6f;
        capsule.height = 0.6f;
        capsule.direction = 1; // Y-axis

        // Security door collision
        var doorColl = CreateEmpty("Coll_SecurityDoor", parent,
            new Vector3(5.5f, 2.0f, ROOM_D * 0.5f - 0.1f));
        doorColl.AddComponent<BoxCollider>().size = new Vector3(3.0f, 4.0f, 0.3f);

        // Boundary colliders (prevent characters leaving scene bounds)
        var boundFront = CreateEmpty("Coll_Boundary_Front", parent,
            new Vector3(0, WALL_H * 0.5f, -ROOM_D * 0.5f - 0.5f));
        boundFront.AddComponent<BoxCollider>().size = new Vector3(ROOM_W, WALL_H, 0.5f);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SECTION 11 — CHARACTER REPOSITIONING
    // ═══════════════════════════════════════════════════════════════════════

    static void RepositionCharacters()
    {
        // Aeron — position near containment chamber on main floor
        var aeronGO = GameObject.Find("Aeron_Instance");
        if (aeronGO != null)
        {
            aeronGO.transform.position = new Vector3(-1.5f, FLOOR_Y, 1.0f);
            aeronGO.transform.rotation = Quaternion.Euler(0, 12f, 0); // Slight angle for visual interest
            Debug.Log("[LabBuilder] Repositioned Aeron to containment area.");
        }

        // Guard — security position near right side / door approach
        var guardGO = GameObject.Find("Guard_Instance");
        if (guardGO != null)
        {
            guardGO.transform.position = new Vector3(2.5f, FLOOR_Y, 0.5f);
            guardGO.transform.rotation = Quaternion.Euler(0, -20f, 0); // Facing slightly left
            Debug.Log("[LabBuilder] Repositioned Guard to security position.");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // UTILITY HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    static GameObject CreateEmpty(string name, GameObject parent, Vector3 localPos)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        if (parent != null)
            go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        return go;
    }

    static GameObject CreateBox(string name, GameObject parent, Vector3 worldPos, Vector3 size)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        // Remove default collider — we use explicit collision group
        var col = go.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
        if (parent != null)
            go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = worldPos;
        go.transform.localScale    = size;
        return go;
    }

    static void SetMat(GameObject go, Material mat)
    {
        if (mat == null) return;
        var rend = go.GetComponent<Renderer>();
        if (rend != null) rend.sharedMaterial = mat;
    }

    static void SetLayer(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayer(child.gameObject, layer);
    }
}
