using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace LastGod.EditorTools
{
    public static class AssembleProductionLabScene
    {
        private const string ScenePath = "Assets/Scenes/2.5D_Lab_Scene.unity";
        private const string ModulesDir = "Assets/Environment/Lab/Modules";
        private const string PrefabsDir = "Assets/Prefabs/Environment/Lab";
        private const string MaterialsDir = "Assets/Materials/Environment";
        private const string TrimSheetPath = "Assets/Environment/Lab/Lab_TrimSheet.png";
        private const string AssembledFbxPath = "Assets/Environment/Lab/Lab_Environment_Production.fbx";

        private static readonly Dictionary<string, string[]> ObjectMaterialSlots = new Dictionary<string, string[]>
        {
            { "Floor_Plate", new[] { "Floor", "MetalDark" } },
            { "Floor_Grate", new[] { "MetalDark", "Floor", "Grating" } },
            { "Aeron_Chamber", new[] { "Concrete", "MetalDark", "MetalWorn", "CyanEmission", "Glass", "Cable" } },
            { "Containment_01", new[] { "Concrete", "MetalDark", "Glass", "MetalWorn", "CyanEmission" } },
            { "Containment_02", new[] { "Concrete", "MetalDark", "Console", "MetalWorn" } },
            { "Containment_03", new[] { "Concrete", "MetalDark", "Console", "MetalWorn", "Warning" } },
            { "Containment_04", new[] { "Concrete", "MetalDark", "Console", "MetalWorn" } },
            { "MainPlatform_Deck", new[] { "Grating", "MetalDark", "Warning", "MetalWorn", "Concrete" } },
            { "Catwalk_Left_Deck", new[] { "Grating", "MetalDark", "Warning", "MetalWorn", "Concrete" } },
            { "Platform_Stairs", new[] { "MetalDark", "Grating" } },
            { "Rail", new[] { "MetalDark", "MetalWorn" } },
            { "SecurityDoor", new[] { "MetalDark", "MetalWorn", "Warning", "CyanEmission" } },
            { "MaintenanceDoor", new[] { "MetalDark", "MetalWorn", "Warning" } },
            { "ServerRack", new[] { "MetalDark", "MetalWorn", "CyanEmission", "Warning" } },
            { "PowerUnit", new[] { "MetalDark", "Grating", "MetalWorn", "CyanEmission" } },
            { "ControlStation", new[] { "MetalDark", "Console", "CyanEmission", "MetalWorn" } },
            { "Crate", new[] { "MetalWorn", "MetalDark" } },
            { "Overhead_Pipes", new[] { "MetalDark", "MetalWorn" } },
            { "Wall_CableTray", new[] { "Grating", "MetalDark", "Cable" } },
            { "Pillar_A7", new[] { "Concrete", "MetalDark", "MetalWorn", "CyanEmission", "Cable" } },
            { "Pillar_B3", new[] { "Concrete", "MetalDark", "MetalWorn", "CyanEmission", "Cable" } },
            { "Pillar_Right", new[] { "Concrete", "MetalDark", "MetalWorn", "CyanEmission", "Cable" } },
            { "Pillar_Slender", new[] { "Concrete", "MetalWorn", "MetalDark" } },
            { "Wall_Back_0", new[] { "MetalDark", "MetalWorn" } },
            { "Wall_Back_1", new[] { "MetalWorn", "MetalDark", "Cable" } },
            { "Wall_Back_2", new[] { "MetalDark", "MetalWorn", "Grating", "Cable" } },
            { "Wall_Back_3", new[] { "MetalDark", "MetalWorn" } },
            { "Wall_Left_0", new[] { "MetalDark", "MetalWorn" } },
            { "Wall_Left_1", new[] { "MetalDark", "MetalWorn", "Grating", "Cable" } },
            { "Wall_Left_2", new[] { "MetalDark", "MetalWorn" } },
            { "Wall_Right_0", new[] { "MetalDark", "MetalWorn" } },
            { "Wall_Right_1", new[] { "MetalDark", "MetalWorn", "Grating", "Cable" } },
            { "Wall_Right_2", new[] { "MetalDark", "MetalWorn", "Grating", "Cable" } },
            { "Lab_Warning_Light", new[] { "MetalDark", "Warning" } },
            { "Lab_Pillar_A", new[] { "Concrete", "MetalDark", "MetalWorn", "CyanEmission", "Cable" } },
            { "Lab_Pillar_B", new[] { "Concrete", "MetalWorn", "MetalDark" } },
            { "Lab_Platform_A", new[] { "Grating", "MetalDark", "Warning", "MetalWorn", "Concrete" } },
            { "Lab_Stairs_A", new[] { "MetalDark", "Grating" } },
            { "Lab_Railing_A", new[] { "MetalDark", "MetalWorn" } },
            { "Lab_Door_Security", new[] { "MetalDark", "MetalWorn", "Warning", "CyanEmission" } },
            { "Lab_Door_Maintenance", new[] { "MetalDark", "MetalWorn", "Warning" } },
            { "Lab_Server_Rack", new[] { "MetalDark", "MetalWorn", "CyanEmission", "Warning" } },
            { "Lab_Power_Unit", new[] { "MetalDark", "Grating", "MetalWorn", "CyanEmission" } },
            { "Lab_Console_A", new[] { "MetalDark", "Console", "CyanEmission", "MetalWorn" } },
            { "Lab_Industrial_Crate", new[] { "MetalWorn", "MetalDark" } },
            { "Lab_Pipe_Network", new[] { "MetalDark", "MetalWorn" } },
            { "Lab_CableTray_A", new[] { "Grating", "MetalDark", "Cable" } },
            { "Lab_Floor_A", new[] { "Floor", "MetalDark" } },
            { "Lab_Floor_Grate", new[] { "MetalDark", "Floor", "Grating" } },
            { "Lab_Wall_A", new[] { "MetalDark", "MetalWorn" } },
            { "Lab_Wall_B", new[] { "MetalDark", "MetalWorn", "Grating", "Cable" } },
            { "Lab_Wall_Damaged", new[] { "MetalWorn", "MetalDark", "Cable" } },
            { "Lab_Containment_A", new[] { "Concrete", "MetalDark", "MetalWorn", "CyanEmission", "Glass", "Cable" } },
            { "Lab_Containment_B", new[] { "Concrete", "MetalDark", "Glass", "MetalWorn", "CyanEmission" } },
        };

        [MenuItem("The Last God/Assemble Production Lab Scene", false, 5)]
        public static void AssembleAndVerify()
        {
            Debug.Log("[AssembleLab] Starting Act 1 Origin Laboratory Production Pass Assembly...");

            EnsureFolder("Assets/Environment/Lab/Modules");
            EnsureFolder("Assets/Prefabs/Environment/Lab");
            EnsureFolder("Assets/Materials/Environment");

            SetupLayers();
            ConfigureTextureImporter(TrimSheetPath);
            var mats = SetupMaterials();
            ConfigureAllFbxImporters();
            CreateModularPrefabs(mats);

            var scene = EditorSceneManager.OpenScene(ScenePath);

            var oldEnv = GameObject.Find("Lab_Environment");
            if (oldEnv != null) Object.DestroyImmediate(oldEnv);
            var old3D = GameObject.Find("Lab_Environment_3D");
            if (old3D != null) Object.DestroyImmediate(old3D);

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer == -1) groundLayer = 8;
            int characterLayer = LayerMask.NameToLayer("Character");
            if (characterLayer == -1) characterLayer = 11;

            GameObject rootEnv = new GameObject("Lab_Environment");
            GameObject grpArch = CreateChild(rootEnv, "Architecture");
            GameObject grpFloors = CreateChild(grpArch, "Floors");
            GameObject grpWalls = CreateChild(grpArch, "Walls");
            GameObject grpCeil = CreateChild(grpArch, "Ceiling");
            GameObject grpPillars = CreateChild(grpArch, "Pillars");
            GameObject grpBeams = CreateChild(grpArch, "StructuralBeams");

            GameObject grpPlat = CreateChild(rootEnv, "Platforms");
            GameObject grpMainPlat = CreateChild(grpPlat, "MainPlatform");
            GameObject grpUpperPlat = CreateChild(grpPlat, "UpperPlatform");
            GameObject grpStairs = CreateChild(grpPlat, "Stairs");
            GameObject grpRails = CreateChild(grpPlat, "Railings");

            GameObject grpContain = CreateChild(rootEnv, "Containment");
            GameObject grpDoors = CreateChild(rootEnv, "Doors");
            GameObject grpMachinery = CreateChild(rootEnv, "Machinery");
            GameObject grpServers = CreateChild(grpMachinery, "Servers");
            GameObject grpPower = CreateChild(grpMachinery, "PowerUnits");
            GameObject grpEquip = CreateChild(grpMachinery, "Equipment");

            GameObject grpProps = CreateChild(rootEnv, "Props");
            GameObject grpConsoles = CreateChild(grpProps, "Consoles");
            GameObject grpCabinets = CreateChild(grpProps, "Cabinets");
            GameObject grpCrates = CreateChild(grpProps, "Crates");
            GameObject grpMaint = CreateChild(grpProps, "Maintenance");

            GameObject grpPipes = CreateChild(rootEnv, "PipesAndCables");
            GameObject grpLighting = CreateChild(rootEnv, "Lighting");
            GameObject grpCollision = CreateChild(rootEnv, "Collision");

            float[] floorXs = { -6.0f, -2.0f, 2.0f, 6.0f };
            float[] floorZs = { -2.0f, 2.0f, 6.0f };
            for (int ix = 0; ix < floorXs.Length; ix++)
            {
                for (int iz = 0; iz < floorZs.Length; iz++)
                {
                    var fl = InstantiateModule("Lab_Floor_A", grpFloors, new Vector3(floorXs[ix], 0f, floorZs[iz]), Quaternion.identity);
                    fl.name = $"Floor_Plate_{ix}_{iz}";
                    AssignMultiMaterial(fl, "Floor_Plate", mats, groundLayer, true);
                }
            }

            for (int i = 0; i < 3; i++)
            {
                var gr = InstantiateModule("Lab_Floor_Grate", grpFloors, new Vector3(0f, 0f, -2.0f + i * 4.0f), Quaternion.identity);
                gr.name = $"Floor_Grate_{i}";
                AssignMultiMaterial(gr, "Floor_Grate", mats, groundLayer, true);
            }

            string[] backWallMods = { "Lab_Wall_A", "Lab_Wall_Damaged", "Lab_Wall_B", "Lab_Wall_A" };
            for (int i = 0; i < 4; i++)
            {
                var w = InstantiateModule(backWallMods[i], grpWalls, new Vector3(-6.0f + i * 4.0f, 0f, 8.0f), Quaternion.identity);
                w.name = $"Wall_Back_{i}";
                AssignMultiMaterial(w, $"Wall_Back_{i}", mats, 0, false);
            }

            for (int i = 0; i < 3; i++)
            {
                var w = InstantiateModule(i == 1 ? "Lab_Wall_B" : "Lab_Wall_A", grpWalls, new Vector3(-8.0f, 0f, -2.0f + i * 4.0f), Quaternion.Euler(0f, 90f, 0f));
                w.name = $"Wall_Left_{i}";
                AssignMultiMaterial(w, $"Wall_Left_{i}", mats, 0, false);
            }

            for (int i = 0; i < 3; i++)
            {
                var w = InstantiateModule(i == 0 ? "Lab_Wall_B" : "Lab_Wall_A", grpWalls, new Vector3(8.0f, 0f, -2.0f + i * 4.0f), Quaternion.Euler(0f, -90f, 0f));
                w.name = $"Wall_Right_{i}";
                AssignMultiMaterial(w, $"Wall_Right_{i}", mats, 0, false);
            }

            var pB3 = InstantiateModule("Lab_Pillar_A", grpPillars, new Vector3(-6.0f, 0f, -1.0f), Quaternion.identity);
            pB3.name = "Pillar_B3_Left";
            AssignMultiMaterial(pB3, "Pillar_B3", mats, 0, false);

            var pRF = InstantiateModule("Lab_Pillar_A", grpPillars, new Vector3(6.0f, 0f, -1.0f), Quaternion.identity);
            pRF.name = "Pillar_Right_Front";
            AssignMultiMaterial(pRF, "Pillar_Right", mats, 0, false);

            var pA7 = InstantiateModule("Lab_Pillar_A", grpPillars, new Vector3(-6.0f, 0f, 5.0f), Quaternion.identity);
            pA7.name = "Pillar_A7_Left";
            AssignMultiMaterial(pA7, "Pillar_A7", mats, 0, false);

            var pRB = InstantiateModule("Lab_Pillar_A", grpPillars, new Vector3(6.0f, 0f, 5.0f), Quaternion.identity);
            pRB.name = "Pillar_Right_Back";
            AssignMultiMaterial(pRB, "Pillar_Right", mats, 0, false);

            var pS1 = InstantiateModule("Lab_Pillar_B", grpPillars, new Vector3(-3.0f, 0f, 7.6f), Quaternion.identity);
            pS1.name = "Pillar_Slender_Left";
            AssignMultiMaterial(pS1, "Pillar_Slender", mats, 0, false);

            var platMain = InstantiateModule("Lab_Platform_A", grpMainPlat, new Vector3(5.5f, 2.5f, 3.5f), Quaternion.identity);
            platMain.name = "MainPlatform_Deck";
            AssignMultiMaterial(platMain, "MainPlatform_Deck", mats, groundLayer, true);

            var catwalkL = InstantiateModule("Lab_Platform_A", grpMainPlat, new Vector3(-5.5f, 2.5f, 4.0f), Quaternion.identity);
            catwalkL.name = "Catwalk_Left_Deck";
            AssignMultiMaterial(catwalkL, "Catwalk_Left_Deck", mats, groundLayer, true);

            var stairs = InstantiateModule("Lab_Stairs_A", grpStairs, new Vector3(3.2f, 0f, 2.2f), Quaternion.identity);
            stairs.name = "Platform_Stairs";
            AssignMultiMaterial(stairs, "Platform_Stairs", mats, groundLayer, true);

            var railMain = InstantiateModule("Lab_Railing_A", grpRails, new Vector3(5.5f, 2.5f, 1.95f), Quaternion.identity);
            railMain.name = "Rail_Main_Front";
            AssignMultiMaterial(railMain, "Rail", mats, 0, false);

            var railLeft = InstantiateModule("Lab_Railing_A", grpRails, new Vector3(-5.5f, 2.5f, 2.45f), Quaternion.identity);
            railLeft.name = "Rail_Left_Front";
            AssignMultiMaterial(railLeft, "Rail", mats, 0, false);

            var aeronChamber = InstantiateModule("Lab_Containment_A", grpContain, new Vector3(-1.8f, 0f, 3.6f), Quaternion.identity);
            aeronChamber.name = "Aeron_Chamber";
            AssignMultiMaterial(aeronChamber, "Aeron_Chamber", mats, 0, false);

            var c1 = InstantiateModule("Lab_Containment_B", grpContain, new Vector3(-4.5f, 0f, 6.5f), Quaternion.identity);
            c1.name = "Containment_01";
            AssignMultiMaterial(c1, "Containment_01", mats, 0, false);

            var c2 = InstantiateModule("Lab_Containment_B", grpContain, new Vector3(4.8f, 0f, 6.8f), Quaternion.identity);
            c2.name = "Containment_02";
            AssignMultiMaterial(c2, "Containment_02", mats, 0, false);

            var c3 = InstantiateModule("Lab_Containment_B", grpContain, new Vector3(-6.8f, 0f, 4.8f), Quaternion.identity);
            c3.name = "Containment_03";
            AssignMultiMaterial(c3, "Containment_03", mats, 0, false);

            var c4 = InstantiateModule("Lab_Containment_B", grpContain, new Vector3(6.8f, 0f, 5.5f), Quaternion.identity);
            c4.name = "Containment_04";
            AssignMultiMaterial(c4, "Containment_04", mats, 0, false);

            var secDoor = InstantiateModule("Lab_Door_Security", grpDoors, new Vector3(5.5f, 0f, 7.8f), Quaternion.identity);
            secDoor.name = "SecurityDoor";
            AssignMultiMaterial(secDoor, "SecurityDoor", mats, 0, false);

            var maintDoor = InstantiateModule("Lab_Door_Maintenance", grpDoors, new Vector3(-7.8f, 0f, 2.0f), Quaternion.Euler(0f, 90f, 0f));
            maintDoor.name = "MaintenanceDoor";
            AssignMultiMaterial(maintDoor, "MaintenanceDoor", mats, 0, false);

            for (int i = 0; i < 3; i++)
            {
                var s = InstantiateModule("Lab_Server_Rack", grpServers, new Vector3(7.4f, 0f, -1.0f + i * 1.5f), Quaternion.Euler(0f, -90f, 0f));
                s.name = $"ServerRack_{i}";
                AssignMultiMaterial(s, "ServerRack", mats, 0, false);
            }

            for (int i = 0; i < 2; i++)
            {
                var p = InstantiateModule("Lab_Power_Unit", grpPower, new Vector3(-7.3f, 0f, 2.5f + i * 2.2f), Quaternion.Euler(0f, 90f, 0f));
                p.name = $"PowerUnit_{i}";
                AssignMultiMaterial(p, "PowerUnit", mats, 0, false);
            }

            var con1 = InstantiateModule("Lab_Console_A", grpEquip, new Vector3(-3.8f, 0f, -1.5f), Quaternion.Euler(0f, 15f, 0f));
            con1.name = "ControlStation_1";
            AssignMultiMaterial(con1, "ControlStation", mats, 0, false);

            var cr1 = InstantiateModule("Lab_Industrial_Crate", grpCrates, new Vector3(6.8f, 0f, -2.2f), Quaternion.identity);
            cr1.name = "Crate_01";
            AssignMultiMaterial(cr1, "Crate", mats, 0, false);

            var cr2 = InstantiateModule("Lab_Industrial_Crate", grpCrates, new Vector3(6.8f, 0.82f, -2.2f), Quaternion.Euler(0f, 12f, 0f));
            cr2.name = "Crate_02";
            AssignMultiMaterial(cr2, "Crate", mats, 0, false);

            var pipes = InstantiateModule("Lab_Pipe_Network", grpPipes, new Vector3(0f, 7.2f, 2.0f), Quaternion.identity);
            pipes.name = "Overhead_Pipes";
            AssignMultiMaterial(pipes, "Overhead_Pipes", mats, 0, false);

            var cTray = InstantiateModule("Lab_CableTray_A", grpPipes, new Vector3(7.2f, 3.8f, 2.0f), Quaternion.Euler(0f, 90f, 0f));
            cTray.name = "Wall_CableTray";
            AssignMultiMaterial(cTray, "Wall_CableTray", mats, 0, false);

            SetupEnvironmentLighting(grpLighting);
            SetupPhysicsColliders(grpCollision, groundLayer);
            SetupCharacters(characterLayer);
            SetupCamera();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[AssembleLab] ✅ Production Lab Environment assembled cleanly with accurate multi-submesh material mappings!");
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static GameObject InstantiateModule(string moduleName, GameObject parent, Vector3 localPos, Quaternion localRot)
        {
            string fbxPath = $"{ModulesDir}/{moduleName}.fbx";
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (asset != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent.transform);
                instance.name = moduleName;
                instance.transform.localPosition = localPos;
                instance.transform.localRotation = localRot;
                return instance;
            }
            Debug.LogError($"[AssembleLab] Could not load FBX at {fbxPath}");
            var fallback = new GameObject(moduleName);
            fallback.transform.SetParent(parent.transform, false);
            fallback.transform.localPosition = localPos;
            fallback.transform.localRotation = localRot;
            return fallback;
        }

        private static void AssignMultiMaterial(GameObject go, string objectKey, Dictionary<string, Material> mats, int layer, bool addMeshCollider)
        {
            go.layer = layer;
            foreach (Transform t in go.transform) t.gameObject.layer = layer;

            string[] keys = null;
            foreach (var kvp in ObjectMaterialSlots)
            {
                if (objectKey.StartsWith(kvp.Key) || go.name.StartsWith(kvp.Key))
                {
                    keys = kvp.Value;
                    break;
                }
            }

            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                int slotCount = r.sharedMaterials.Length;
                var newMats = new Material[slotCount];
                for (int i = 0; i < slotCount; i++)
                {
                    if (keys != null && i < keys.Length && mats.ContainsKey(keys[i]))
                    {
                        newMats[i] = mats[keys[i]];
                    }
                    else if (keys != null && keys.Length > 0 && mats.ContainsKey(keys[0]))
                    {
                        newMats[i] = mats[keys[0]];
                    }
                    else
                    {
                        newMats[i] = mats["MetalDark"];
                    }
                }
                r.sharedMaterials = newMats;
            }

            if (addMeshCollider)
            {
                foreach (var mf in go.GetComponentsInChildren<MeshFilter>(true))
                {
                    var mc = mf.gameObject.GetComponent<MeshCollider>();
                    if (mc == null) mc = mf.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                    mc.convex = false;
                }
            }
        }

        private static void SetupEnvironmentLighting(GameObject parent)
        {
            foreach (Transform c in parent.transform)
            {
                Object.DestroyImmediate(c.gameObject);
            }

            // Set ambient environment lighting for readable shadows
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.18f, 0.24f, 0.32f);
            RenderSettings.ambientIntensity = 1.0f;

            // 1. Key Directional Light (Cold Cyan #6FE3FF, 2.5)
            var keyGo = new GameObject("Key_Light");
            keyGo.transform.SetParent(parent.transform, false);
            keyGo.transform.rotation = Quaternion.Euler(40f, 20f, 0f);
            var keyLight = keyGo.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = new Color(0.435f, 0.890f, 1.0f);
            keyLight.intensity = 2.5f;
            keyLight.shadows = LightShadows.Soft;

            // 2. Fill Directional Light (Deep ambient navy, 0.8)
            var fillGo = new GameObject("Fill_Light");
            fillGo.transform.SetParent(parent.transform, false);
            fillGo.transform.rotation = Quaternion.Euler(55f, 140f, 0f);
            var fillLight = fillGo.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(0.12f, 0.18f, 0.28f);
            fillLight.intensity = 0.8f;
            fillLight.shadows = LightShadows.None;

            // 3. Stasis Chamber Spotlight (#6FE3FF, 3.5) over Aeron and Pod
            var podLightGo = new GameObject("Pod_SpotLight");
            podLightGo.transform.SetParent(parent.transform, false);
            podLightGo.transform.position = new Vector3(-1.4f, 5.5f, 1.2f);
            podLightGo.transform.rotation = Quaternion.Euler(75f, 0f, 0f);
            var podLight = podLightGo.AddComponent<Light>();
            podLight.type = LightType.Spot;
            podLight.color = new Color(0.435f, 0.890f, 1.0f);
            podLight.intensity = 3.5f;
            podLight.range = 10.0f;
            podLight.spotAngle = 65f;
            podLight.shadows = LightShadows.Soft;

            // 4. Stasis Chamber Internal Core Light (illuminates glass from within)
            var podCoreLightGo = new GameObject("Pod_Core_Light");
            podCoreLightGo.transform.SetParent(parent.transform, false);
            podCoreLightGo.transform.position = new Vector3(-1.8f, 2.2f, 3.6f);
            var podCoreLight = podCoreLightGo.AddComponent<Light>();
            podCoreLight.type = LightType.Point;
            podCoreLight.color = new Color(0.435f, 0.890f, 1.0f);
            podCoreLight.intensity = 3.0f;
            podCoreLight.range = 5.0f;

            // 5. Secondary Stasis Ambient Light
            var secLightGo = new GameObject("Secondary_Pods_Light");
            secLightGo.transform.SetParent(parent.transform, false);
            secLightGo.transform.position = new Vector3(0f, 5.5f, 6.5f);
            var secLight = secLightGo.AddComponent<Light>();
            secLight.type = LightType.Point;
            secLight.color = new Color(0.435f, 0.890f, 1.0f);
            secLight.intensity = 1.5f;
            secLight.range = 8.0f;

            // 6. Security Door & Platform Spot (Restricted Orange)
            var doorLightGo = new GameObject("SecurityDoor_SpotLight");
            doorLightGo.transform.SetParent(parent.transform, false);
            doorLightGo.transform.position = new Vector3(5.5f, 5.0f, 7.5f);
            doorLightGo.transform.rotation = Quaternion.Euler(70f, 0f, 0f);
            var doorLight = doorLightGo.AddComponent<Light>();
            doorLight.type = LightType.Spot;
            doorLight.color = new Color(0.769f, 0.314f, 0.180f);
            doorLight.intensity = 2.2f;
            doorLight.range = 8.0f;
            doorLight.spotAngle = 55f;

            // 7. Platform & Stairs Fill Light (Over Guard)
            var platLightGo = new GameObject("Platform_Fill_Light");
            platLightGo.transform.SetParent(parent.transform, false);
            platLightGo.transform.position = new Vector3(1.6f, 4.2f, 0.8f);
            platLightGo.transform.rotation = Quaternion.Euler(75f, 0f, 0f);
            var platLight = platLightGo.AddComponent<Light>();
            platLight.type = LightType.Spot;
            platLight.color = new Color(0.85f, 0.90f, 1.0f);
            platLight.intensity = 3.0f;
            platLight.range = 8.0f;
            platLight.spotAngle = 60f;

            // 8. Console Workstation Light
            var conLightGo = new GameObject("Console_Workstation_Light");
            conLightGo.transform.SetParent(parent.transform, false);
            conLightGo.transform.position = new Vector3(-3.8f, 1.8f, -1.2f);
            var conLight = conLightGo.AddComponent<Light>();
            conLight.type = LightType.Point;
            conLight.color = new Color(0.812f, 0.957f, 1.0f);
            conLight.intensity = 1.5f;
            conLight.range = 4.5f;
        }

        private static void SetupPhysicsColliders(GameObject parent, int groundLayer)
        {
            var floorColGo = new GameObject("Coll_MainFloor");
            floorColGo.transform.SetParent(parent.transform, false);
            floorColGo.transform.position = new Vector3(0f, -0.15f, 2.0f);
            var fb = floorColGo.AddComponent<BoxCollider>();
            fb.size = new Vector3(16.0f, 0.3f, 12.0f);
            floorColGo.layer = groundLayer;

            var platColGo = new GameObject("Coll_Platform_Main");
            platColGo.transform.SetParent(parent.transform, false);
            platColGo.transform.position = new Vector3(5.5f, 2.375f, 3.5f);
            var pb = platColGo.AddComponent<BoxCollider>();
            pb.size = new Vector3(4.0f, 0.25f, 3.0f);
            platColGo.layer = groundLayer;

            var stairColGo = new GameObject("Coll_Stairs_Ramp");
            stairColGo.transform.SetParent(parent.transform, false);
            stairColGo.transform.position = new Vector3(3.2f, 1.25f, 2.2f);
            stairColGo.transform.rotation = Quaternion.Euler(-39.8f, 0f, 0f);
            var sb = stairColGo.AddComponent<BoxCollider>();
            sb.size = new Vector3(2.4f, 0.3f, 3.9f);
            stairColGo.layer = groundLayer;

            var chColGo = new GameObject("Coll_AeronChamber_Plinth");
            chColGo.transform.SetParent(parent.transform, false);
            chColGo.transform.position = new Vector3(-1.8f, 0.4f, 3.6f);
            var cc = chColGo.AddComponent<CapsuleCollider>();
            cc.radius = 1.7f;
            cc.height = 0.8f;
            cc.direction = 1;

            CreateBoundaryBox(parent, "Boundary_Back", new Vector3(0f, 4.0f, 8.2f), new Vector3(17f, 8f, 0.5f));
            CreateBoundaryBox(parent, "Boundary_Left", new Vector3(-8.2f, 4.0f, 2.0f), new Vector3(0.5f, 8f, 13f));
            CreateBoundaryBox(parent, "Boundary_Right", new Vector3(8.2f, 4.0f, 2.0f), new Vector3(0.5f, 8f, 13f));
            CreateBoundaryBox(parent, "Boundary_Front", new Vector3(0f, 4.0f, -4.2f), new Vector3(17f, 8f, 0.5f));
        }

        private static void CreateBoundaryBox(GameObject parent, string name, Vector3 pos, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = pos;
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
        }

        private static void SetupCharacters(int characterLayer)
        {
            var charParent = GameObject.Find("Characters");
            if (charParent == null) return;

            var aeronT = charParent.transform.Find("Aeron_Instance");
            if (aeronT != null)
            {
                aeronT.position = new Vector3(-1.2f, 0.0f, 0.5f);
                aeronT.rotation = Quaternion.Euler(0f, 25f, 0f);
                aeronT.gameObject.layer = characterLayer;

                var anim = aeronT.GetComponent<Animator>();
                if (anim != null) anim.applyRootMotion = false;
            }

            var guardT = charParent.transform.Find("Guard_Instance");
            if (guardT != null)
            {
                guardT.position = new Vector3(1.6f, 0.0f, 0.5f);
                guardT.rotation = Quaternion.Euler(0f, -25f, 0f);
                guardT.gameObject.layer = characterLayer;

                var anim = guardT.GetComponent<Animator>();
                if (anim != null) anim.applyRootMotion = false;
            }
        }

        private static void SetupCamera()
        {
            var camGo = GameObject.Find("Main_Camera");
            if (camGo == null) camGo = Camera.main != null ? Camera.main.gameObject : null;
            if (camGo == null) return;

            camGo.transform.position = new Vector3(0.0f, 2.0f, -8.5f);
            camGo.transform.rotation = Quaternion.Euler(4.5f, 0.0f, 0.0f);

            var cam = camGo.GetComponent<Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.0196f, 0.0235f, 0.0392f, 1.0f);
                cam.fieldOfView = 27.0f;
                cam.nearClipPlane = 0.1f;
                cam.farClipPlane = 100.0f;
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private static void SetupLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProp = tagManager.FindProperty("layers");
            if (layersProp != null)
            {
                if (string.IsNullOrEmpty(layersProp.GetArrayElementAtIndex(8).stringValue))
                    layersProp.GetArrayElementAtIndex(8).stringValue = "Ground";
                if (string.IsNullOrEmpty(layersProp.GetArrayElementAtIndex(11).stringValue))
                    layersProp.GetArrayElementAtIndex(11).stringValue = "Character";
                tagManager.ApplyModifiedProperties();
            }
        }

        private static void ConfigureTextureImporter(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.mipmapEnabled = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
        }

        private static void ConfigureAllFbxImporters()
        {
            if (!Directory.Exists(ModulesDir)) return;
            var files = Directory.GetFiles(ModulesDir, "*.fbx");
            foreach (var f in files)
            {
                string rel = f.Replace('\\', '/');
                var importer = AssetImporter.GetAtPath(rel) as ModelImporter;
                if (importer != null)
                {
                    importer.globalScale = 1.0f;
                    importer.importCameras = false;
                    importer.importLights = false;
                    importer.materialImportMode = ModelImporterMaterialImportMode.None;
                    importer.bakeAxisConversion = true;
                    importer.SaveAndReimport();
                }
            }

            if (File.Exists(AssembledFbxPath))
            {
                var importer = AssetImporter.GetAtPath(AssembledFbxPath) as ModelImporter;
                if (importer != null)
                {
                    importer.globalScale = 1.0f;
                    importer.importCameras = false;
                    importer.importLights = false;
                    importer.materialImportMode = ModelImporterMaterialImportMode.None;
                    importer.bakeAxisConversion = true;
                    importer.SaveAndReimport();
                }
            }
        }

        private static Dictionary<string, Material> SetupMaterials()
        {
            var dict = new Dictionary<string, Material>();
            var trimTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TrimSheetPath);

            dict["MetalDark"]     = CreateOrUpdateMat("MAT_Lab_Metal_Dark",     new Color(0.106f, 0.141f, 0.188f), 0.15f, 0.30f, trimTex, Color.black, 0f);
            dict["MetalWorn"]     = CreateOrUpdateMat("MAT_Lab_Metal_Worn",     new Color(0.145f, 0.176f, 0.212f), 0.12f, 0.20f, trimTex, Color.black, 0f);
            dict["Floor"]         = CreateOrUpdateMat("MAT_Lab_Floor",         new Color(0.045f, 0.055f, 0.075f), 0.18f, 0.15f, trimTex, Color.black, 0f);
            dict["Grating"]       = CreateOrUpdateMat("MAT_Lab_Grating",       new Color(0.090f, 0.115f, 0.150f), 0.12f, 0.35f, trimTex, Color.black, 0f);
            dict["Glass"]         = CreateGlassMat("MAT_Lab_Glass");
            dict["CyanEmission"]  = CreateOrUpdateMat("MAT_Lab_CyanEmission",  new Color(0.039f, 0.047f, 0.063f), 0.05f, 0.00f, null, new Color(0.435f, 0.890f, 1.0f), 2.5f);
            dict["Console"]       = CreateOrUpdateMat("MAT_Lab_Console",       new Color(0.063f, 0.094f, 0.125f), 0.20f, 0.10f, trimTex, new Color(0.435f * 0.4f, 0.890f * 0.4f, 1.0f * 0.4f), 1.0f);
            dict["Cable"]         = CreateOrUpdateMat("MAT_Lab_Cable",         new Color(0.039f, 0.047f, 0.063f), 0.08f, 0.00f, null, Color.black, 0f);
            dict["Concrete"]      = CreateOrUpdateMat("MAT_Lab_Concrete",      new Color(0.102f, 0.118f, 0.141f), 0.05f, 0.00f, null, Color.black, 0f);
            dict["Warning"]       = CreateOrUpdateMat("MAT_Lab_Warning",       new Color(0.85f, 0.35f, 0.20f), 0.10f, 0.00f, trimTex, new Color(0.85f * 0.8f, 0.35f * 0.8f, 0.20f * 0.8f), 1.2f);

            return dict;
        }

        private static Material CreateOrUpdateMat(string name, Color baseColor, float smoothness, float metallic, Texture2D mainTex, Color emissionColor, float emissionIntensity)
        {
            string path = $"{MaterialsDir}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            if (mainTex != null) mat.SetTexture("_BaseMap", mainTex);
            if (emissionIntensity > 0f)
            {
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                mat.SetColor("_EmissionColor", Color.black);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material CreateGlassMat(string name)
        {
            string path = $"{MaterialsDir}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", new Color(0.20f, 0.45f, 0.60f, 0.25f));
            mat.SetFloat("_Metallic", 0.05f);
            mat.SetFloat("_Smoothness", 0.90f);
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.renderQueue = 3000;
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.20f, 0.50f, 0.70f) * 0.40f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void CreateModularPrefabs(Dictionary<string, Material> mats)
        {
            var files = Directory.GetFiles(ModulesDir, "*.fbx");
            foreach (var f in files)
            {
                string modName = Path.GetFileNameWithoutExtension(f);
                string prefabPath = $"{PrefabsDir}/PF_{modName}.prefab";
                var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModulesDir}/{modName}.fbx");
                if (fbxAsset != null)
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset);
                    AssignMultiMaterial(instance, modName, mats, 0, false);
                    PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                    Object.DestroyImmediate(instance);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
