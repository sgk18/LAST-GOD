using UnityEngine;

namespace LastGod.ThirdPerson.Environment
{
    /// <summary>
    /// Procedurally constructs the complete, high-fidelity Act 1 Research Laboratory facility.
    /// Features modular architecture, open industrial ceiling trusses, multi-tiered central dais,
    /// cryo-pylons, science workstations, mainframe server banks, holographic telemetry,
    /// cable conduits, observation control deck, and security blast doors.
    /// </summary>
    public class LabEnvironmentBuilder : MonoBehaviour
    {
        public static GameObject BuildModularLab(Transform parent)
        {
            GameObject labRoot = new GameObject("Environment_Laboratory");
            if (parent != null) labRoot.transform.SetParent(parent);

            // Palette of materials
            Material floorMat = CreateColorMaterial(new Color(0.10f, 0.12f, 0.15f), 0.35f, 0.65f);
            Material wallMat = CreateColorMaterial(new Color(0.15f, 0.18f, 0.21f), 0.2f, 0.5f);
            Material metalTrimMat = CreateColorMaterial(new Color(0.07f, 0.08f, 0.09f), 0.7f, 0.8f);
            Material glassMat = CreateGlassMaterial();
            Material cyanScreenMat = CreateEmissiveMaterial(new Color(0.1f, 0.85f, 1.0f), 2.8f);
            Material amberScreenMat = CreateEmissiveMaterial(new Color(1.0f, 0.65f, 0.15f), 2.4f);
            Material redWarningMat = CreateEmissiveMaterial(new Color(1.0f, 0.15f, 0.1f), 3.0f);
            Material greenStatusMat = CreateEmissiveMaterial(new Color(0.1f, 1.0f, 0.35f), 2.5f);
            Material hazardMat = CreateColorMaterial(new Color(0.92f, 0.74f, 0.10f), 0.1f, 0.4f);
            Material cyanCircuitMat = CreateEmissiveMaterial(new Color(0.05f, 0.9f, 1.0f), 2.2f);

            // =========================================================
            // AREA A: CONTAINMENT CHAMBER ARENA (x: -15 to +15, z: -15 to +15)
            // =========================================================
            GameObject areaA = new GameObject("Area_A_ContainmentArena");
            areaA.transform.SetParent(labRoot.transform);

            // 1. Floor
            CreateFloor(areaA.transform, new Vector3(0f, 0f, 0f), new Vector3(30f, 0.5f, 30f), floorMat);

            // 2. Open Industrial Ceiling Trusses (Open-top for cinematic visibility & overhead depth)
            CreateCeilingTrusses(areaA.transform, 0f, 0f, 30f, 30f, 6.2f, metalTrimMat, cyanCircuitMat);

            // 3. Perimeter Enclosing Walls with Structural Support Pillars
            CreateWall(areaA.transform, new Vector3(0f, 3.1f, -15f), new Vector3(30f, 6.2f, 0.8f), wallMat); // South
            CreateWall(areaA.transform, new Vector3(-15f, 3.1f, 0f), new Vector3(0.8f, 6.2f, 30f), wallMat); // West
            // East wall has opening for Security Corridor at center
            CreateWall(areaA.transform, new Vector3(15f, 3.1f, 10f), new Vector3(0.8f, 6.2f, 10f), wallMat);
            CreateWall(areaA.transform, new Vector3(15f, 3.1f, -10f), new Vector3(0.8f, 6.2f, 10f), wallMat);
            CreateWall(areaA.transform, new Vector3(15f, 5.2f, 0f), new Vector3(0.8f, 2.0f, 10f), wallMat); // Header above door

            // Perimeter Structural Pillars
            Vector3[] arenaPillarPositions = new Vector3[]
            {
                new Vector3(-14.2f, 3.1f, -14.2f),
                new Vector3(14.2f, 3.1f, -14.2f),
                new Vector3(-14.2f, 3.1f, 14.2f),
                new Vector3(14.2f, 3.1f, 14.2f),
                new Vector3(-14.2f, 3.1f, 0f),
                new Vector3(0f, 3.1f, -14.2f)
            };
            foreach (var pos in arenaPillarPositions)
            {
                CreatePillar(areaA.transform, pos, 6.2f, metalTrimMat, hazardMat);
            }

            // 4. Central Multi-Tiered Stasis Dais
            // Base tier with hazard border
            CreatePlatform(areaA.transform, new Vector3(0f, 0.15f, 0f), new Vector3(8.0f, 0.3f, 8.0f), metalTrimMat, "Dais_BaseTier");
            CreateBorderStrips(areaA.transform, new Vector3(0f, 0.31f, 0f), 8.0f, 8.0f, 0.35f, hazardMat);
            // Mid tier with cyan circuit ring
            CreatePlatform(areaA.transform, new Vector3(0f, 0.45f, 0f), new Vector3(6.2f, 0.3f, 6.2f), floorMat, "Dais_MidTier");
            CreateBorderStrips(areaA.transform, new Vector3(0f, 0.61f, 0f), 6.2f, 6.2f, 0.18f, cyanCircuitMat);
            // Top cylinder dais pedestal
            GameObject topPedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            topPedestal.name = "Dais_TopPedestal";
            topPedestal.transform.SetParent(areaA.transform);
            topPedestal.transform.position = new Vector3(0f, 0.65f, 0f);
            topPedestal.transform.localScale = new Vector3(4.2f, 0.12f, 4.2f);
            topPedestal.GetComponent<Renderer>().material = metalTrimMat;

            // 5. Floor Conduit Lines radiating from Dais to Pylons & Consoles
            CreateFloorConduit(areaA.transform, new Vector3(0f, 0.01f, -8f), new Vector3(1.0f, 0.06f, 8f), metalTrimMat, cyanCircuitMat);
            CreateFloorConduit(areaA.transform, new Vector3(-8f, 0.01f, 0f), new Vector3(8f, 0.06f, 1.0f), metalTrimMat, cyanCircuitMat);
            CreateFloorConduit(areaA.transform, new Vector3(0f, 0.01f, 8f), new Vector3(1.0f, 0.06f, 8f), metalTrimMat, cyanCircuitMat);
            CreateFloorConduit(areaA.transform, new Vector3(8f, 0.01f, 0f), new Vector3(8f, 0.06f, 1.0f), metalTrimMat, cyanCircuitMat);

            // 6. Four High-Voltage Cryo-Pylons at the Diagonals
            CreateCryoPylon(areaA.transform, new Vector3(-5.2f, 0f, -5.2f), metalTrimMat, cyanCircuitMat);
            CreateCryoPylon(areaA.transform, new Vector3(5.2f, 0f, -5.2f), metalTrimMat, cyanCircuitMat);
            CreateCryoPylon(areaA.transform, new Vector3(-5.2f, 0f, 5.2f), metalTrimMat, cyanCircuitMat);
            CreateCryoPylon(areaA.transform, new Vector3(5.2f, 0f, 5.2f), metalTrimMat, cyanCircuitMat);

            // 7. Science Telemetry Workstations & Computer Consoles in Area A
            CreateScienceStation(areaA.transform, new Vector3(-9.5f, 0f, -7.5f), 45f, metalTrimMat, cyanScreenMat, amberScreenMat);
            CreateScienceStation(areaA.transform, new Vector3(9.5f, 0f, -7.5f), -45f, metalTrimMat, cyanScreenMat, amberScreenMat);
            CreateScienceStation(areaA.transform, new Vector3(-9.5f, 0f, 7.5f), 135f, metalTrimMat, cyanScreenMat, amberScreenMat);

            // 8. Mainframe Server Banks with Activity LEDs
            CreateServerBank(areaA.transform, new Vector3(-13.8f, 0f, -6f), 90f, metalTrimMat, cyanCircuitMat, greenStatusMat, amberScreenMat);
            CreateServerBank(areaA.transform, new Vector3(-13.8f, 0f, 6f), 90f, metalTrimMat, cyanCircuitMat, greenStatusMat, amberScreenMat);
            CreateServerBank(areaA.transform, new Vector3(0f, 0f, -13.8f), 0f, metalTrimMat, cyanCircuitMat, greenStatusMat, amberScreenMat);

            // 9. Wall Diagnostic Holographic Displays on South Wall
            CreateWallDisplay(areaA.transform, new Vector3(-6f, 3.5f, -14.5f), new Vector3(4.2f, 2.2f, 0.1f), cyanScreenMat, metalTrimMat);
            CreateWallDisplay(areaA.transform, new Vector3(6f, 3.5f, -14.5f), new Vector3(4.2f, 2.2f, 0.1f), amberScreenMat, metalTrimMat);

            // Overhead Stasis Apparatus / Gantry
            GameObject gantry = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gantry.name = "Overhead_Stasis_Gantry";
            gantry.transform.SetParent(areaA.transform);
            gantry.transform.position = new Vector3(0f, 5.6f, 0f);
            gantry.transform.localScale = new Vector3(3.6f, 0.8f, 3.6f);
            gantry.GetComponent<Renderer>().material = metalTrimMat;

            // =========================================================
            // AREA B: ELEVATED OBSERVATION CONTROL ROOM (x: -12 to +12, z: 15 to 26, y: 2.6)
            // =========================================================
            GameObject areaB = new GameObject("Area_B_ElevatedControlDeck");
            areaB.transform.SetParent(labRoot.transform);

            // Elevated Floor & Base Structure
            CreatePlatform(areaB.transform, new Vector3(0f, 1.3f, 20.5f), new Vector3(22f, 2.6f, 11f), metalTrimMat, "ControlDeck_BaseSolid");
            CreateFloor(areaB.transform, new Vector3(0f, 2.6f, 20.5f), new Vector3(22f, 0.4f, 11f), floorMat);

            // Roof Trusses over Area B
            CreateCeilingTrusses(areaB.transform, 0f, 20.5f, 22f, 11f, 7.8f, metalTrimMat, amberScreenMat);

            // Enclosing Walls
            CreateWall(areaB.transform, new Vector3(0f, 5.2f, 26f), new Vector3(22f, 5.2f, 0.8f), wallMat); // Back
            CreateWall(areaB.transform, new Vector3(-11f, 5.2f, 20.5f), new Vector3(0.8f, 5.2f, 11f), wallMat); // Left
            CreateWall(areaB.transform, new Vector3(11f, 5.2f, 20.5f), new Vector3(0.8f, 5.2f, 11f), wallMat);  // Right

            // Reinforced Heavy Observation Glass looking down on Area A
            GameObject glassPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glassPanel.name = "ObservationWindow_Glass";
            glassPanel.transform.SetParent(areaB.transform);
            glassPanel.transform.position = new Vector3(0f, 4.4f, 15f);
            glassPanel.transform.localScale = new Vector3(18f, 3.2f, 0.25f);
            glassPanel.GetComponent<Renderer>().material = glassMat;

            // Observation Deck Railing Frame
            GameObject railTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            railTop.name = "WindowFrame_Top";
            railTop.transform.SetParent(areaB.transform);
            railTop.transform.position = new Vector3(0f, 6.1f, 15f);
            railTop.transform.localScale = new Vector3(18.5f, 0.3f, 0.45f);
            railTop.GetComponent<Renderer>().material = metalTrimMat;

            // Command Consoles overlooking Area A
            CreateConsole(areaB.transform, new Vector3(-5f, 3.2f, 16.5f), cyanScreenMat, metalTrimMat);
            CreateConsole(areaB.transform, new Vector3(0f, 3.2f, 16.5f), amberScreenMat, metalTrimMat);
            CreateConsole(areaB.transform, new Vector3(5f, 3.2f, 16.5f), cyanScreenMat, metalTrimMat);

            // Server stacks in control room
            CreateServerBank(areaB.transform, new Vector3(-9.5f, 2.6f, 24f), 0f, metalTrimMat, cyanCircuitMat, greenStatusMat, amberScreenMat);
            CreateServerBank(areaB.transform, new Vector3(9.5f, 2.6f, 24f), 0f, metalTrimMat, cyanCircuitMat, greenStatusMat, amberScreenMat);

            // =========================================================
            // AREA C: SECURITY CORRIDOR (x: 15 to 35, z: -4 to +4)
            // =========================================================
            GameObject areaC = new GameObject("Area_C_SecurityCorridor");
            areaC.transform.SetParent(labRoot.transform);

            CreateFloor(areaC.transform, new Vector3(25f, 0f, 0f), new Vector3(20f, 0.5f, 8f), floorMat);
            CreateBorderStrips(areaC.transform, new Vector3(25f, 0.01f, 0f), 20f, 7.8f, 0.2f, cyanCircuitMat);

            // Open ceiling girders
            CreateCeilingTrusses(areaC.transform, 25f, 0f, 20f, 8f, 5.2f, metalTrimMat, redWarningMat);

            CreateWall(areaC.transform, new Vector3(25f, 2.6f, 4f), new Vector3(20f, 5.2f, 0.8f), wallMat);
            CreateWall(areaC.transform, new Vector3(25f, 2.6f, -4f), new Vector3(20f, 5.2f, 0.8f), wallMat);

            // Overhead pipes and cable racks along corridor
            CreatePipes(areaC.transform, new Vector3(25f, 4.4f, 3.2f), 18f, metalTrimMat);
            CreatePipes(areaC.transform, new Vector3(25f, 4.4f, -3.2f), 18f, metalTrimMat);

            // Wall Lockers & Emergency Hydrants
            CreateProp(areaC.transform, new Vector3(20f, 1.2f, 3.4f), new Vector3(2.4f, 2.2f, 0.6f), metalTrimMat, "EquipmentLocker_01");
            CreateProp(areaC.transform, new Vector3(30f, 1.2f, -3.4f), new Vector3(2.4f, 2.2f, 0.6f), metalTrimMat, "EquipmentLocker_02");

            // =========================================================
            // AREA D: MEDICAL & RESEARCH BAY (x: 35 to 55, z: -15 to +15)
            // =========================================================
            GameObject areaD = new GameObject("Area_D_MedicalResearchBay");
            areaD.transform.SetParent(labRoot.transform);

            CreateFloor(areaD.transform, new Vector3(45f, 0f, 0f), new Vector3(20f, 0.5f, 30f), floorMat);
            CreateCeilingTrusses(areaD.transform, 45f, 0f, 20f, 30f, 5.8f, metalTrimMat, cyanCircuitMat);

            CreateWall(areaD.transform, new Vector3(45f, 2.9f, -15f), new Vector3(20f, 5.8f, 0.8f), wallMat);
            CreateWall(areaD.transform, new Vector3(45f, 2.9f, 15f), new Vector3(20f, 5.8f, 0.8f), wallMat);

            // Examination tables & specimen capsules
            CreateProp(areaD.transform, new Vector3(42f, 0.8f, 6f), new Vector3(3.2f, 1.2f, 1.6f), metalTrimMat, "ExamTable_01");
            CreateProp(areaD.transform, new Vector3(48f, 0.8f, 6f), new Vector3(3.2f, 1.2f, 1.6f), metalTrimMat, "ExamTable_02");
            CreateProp(areaD.transform, new Vector3(42f, 0.8f, -6f), new Vector3(3.2f, 1.2f, 1.6f), metalTrimMat, "ExamTable_03");

            // Crates and containment barrels
            CreateProp(areaD.transform, new Vector3(52f, 0.8f, -12f), new Vector3(2.0f, 1.6f, 2.0f), metalTrimMat, "CargoCrate_01");
            CreateProp(areaD.transform, new Vector3(52f, 0.8f, -9.5f), new Vector3(1.8f, 1.6f, 1.8f), metalTrimMat, "CargoCrate_02");

            // =========================================================
            // AREA E: EMERGENCY EXIT CORRIDOR (x: 55 to 75, z: -4 to +4)
            // =========================================================
            GameObject areaE = new GameObject("Area_E_EmergencyExitCorridor");
            areaE.transform.SetParent(labRoot.transform);

            CreateFloor(areaE.transform, new Vector3(65f, 0f, 0f), new Vector3(20f, 0.5f, 8f), floorMat);
            CreateCeilingTrusses(areaE.transform, 65f, 0f, 20f, 8f, 5.0f, metalTrimMat, redWarningMat);
            CreateWall(areaE.transform, new Vector3(65f, 2.5f, 4f), new Vector3(20f, 5.0f, 0.8f), wallMat);
            CreateWall(areaE.transform, new Vector3(65f, 2.5f, -4f), new Vector3(20f, 5.0f, 0.8f), wallMat);

            // =========================================================
            // AREA F: FINAL ESCAPE AIRLOCK GATE (x: 75 to 90, z: -10 to +10)
            // =========================================================
            GameObject areaF = new GameObject("Area_F_FinalEscapeAirlock");
            areaF.transform.SetParent(labRoot.transform);

            CreateFloor(areaF.transform, new Vector3(82f, 0f, 0f), new Vector3(14f, 0.5f, 18f), floorMat);
            CreateCeilingTrusses(areaF.transform, 82f, 0f, 14f, 18f, 5.8f, metalTrimMat, greenStatusMat);
            CreateWall(areaF.transform, new Vector3(89f, 2.9f, 0f), new Vector3(0.8f, 5.8f, 18f), metalTrimMat); // Heavy blast gate
            CreateWall(areaF.transform, new Vector3(82f, 2.9f, 9f), new Vector3(14f, 5.8f, 0.8f), wallMat);
            CreateWall(areaF.transform, new Vector3(82f, 2.9f, -9f), new Vector3(14f, 5.8f, 0.8f), wallMat);

            // Interactive Terminal station in Area F
            CreateTerminalStand(areaF.transform, new Vector3(86f, 0f, 0f), cyanScreenMat, metalTrimMat);

            return labRoot;
        }

        private static void CreateFloor(Transform parent, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(parent);
            floor.transform.position = pos - Vector3.up * (size.y * 0.5f);
            floor.transform.localScale = size;
            floor.GetComponent<Renderer>().material = mat;
            floor.layer = LayerMask.NameToLayer("Default");
        }

        private static void CreateCeilingTrusses(Transform parent, float centerX, float centerZ, float widthX, float lengthZ, float heightY, Material trussMat, Material luminaireMat)
        {
            GameObject trusses = new GameObject("Ceiling_Girders_Trusses");
            trusses.transform.SetParent(parent);

            int beamCount = Mathf.Max(2, Mathf.RoundToInt(lengthZ / 5f));
            float step = lengthZ / beamCount;
            float startZ = centerZ - (lengthZ * 0.5f) + (step * 0.5f);

            for (int i = 0; i < beamCount; i++)
            {
                float z = startZ + i * step;

                // Transverse steel girder
                GameObject girder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                girder.name = $"Girder_{i + 1}";
                girder.transform.SetParent(trusses.transform);
                girder.transform.position = new Vector3(centerX, heightY, z);
                girder.transform.localScale = new Vector3(widthX, 0.45f, 0.45f);
                Object.DestroyImmediate(girder.GetComponent<Collider>());
                girder.GetComponent<Renderer>().material = trussMat;

                // Longitudinal luminaire strip under girder
                GameObject lightStrip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lightStrip.name = $"LuminaireStrip_{i + 1}";
                lightStrip.transform.SetParent(girder.transform);
                lightStrip.transform.localPosition = new Vector3(0f, -0.25f, 0f);
                lightStrip.transform.localScale = new Vector3(0.7f, 0.15f, 0.35f);
                Object.DestroyImmediate(lightStrip.GetComponent<Collider>());
                lightStrip.GetComponent<Renderer>().material = luminaireMat;
            }
        }

        private static void CreateWall(Transform parent, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.SetParent(parent);
            wall.transform.position = pos;
            wall.transform.localScale = size;
            wall.GetComponent<Renderer>().material = mat;
        }

        private static void CreatePlatform(Transform parent, Vector3 pos, Vector3 size, Material mat, string name = "Platform")
        {
            GameObject plat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plat.name = name;
            plat.transform.SetParent(parent);
            plat.transform.position = pos;
            plat.transform.localScale = size;
            plat.GetComponent<Renderer>().material = mat;
        }

        private static void CreatePillar(Transform parent, Vector3 pos, float height, Material bodyMat, Material accentMat)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = "StructuralPillar";
            pillar.transform.SetParent(parent);
            pillar.transform.position = pos;
            pillar.transform.localScale = new Vector3(1.2f, height, 1.2f);
            pillar.GetComponent<Renderer>().material = bodyMat;

            // Accent hazard band
            GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cube);
            band.name = "HazardBand";
            band.transform.SetParent(pillar.transform);
            band.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            band.transform.localScale = new Vector3(1.02f, 0.18f, 1.02f);
            Object.DestroyImmediate(band.GetComponent<Collider>());
            band.GetComponent<Renderer>().material = accentMat;
        }

        private static void CreateBorderStrips(Transform parent, Vector3 center, float sizeX, float sizeZ, float thickness, Material mat)
        {
            // North
            CreateProp(parent, center + new Vector3(0f, 0f, sizeZ * 0.5f - thickness * 0.5f), new Vector3(sizeX, 0.05f, thickness), mat, "BorderStrip_N");
            // South
            CreateProp(parent, center + new Vector3(0f, 0f, -sizeZ * 0.5f + thickness * 0.5f), new Vector3(sizeX, 0.05f, thickness), mat, "BorderStrip_S");
            // East
            CreateProp(parent, center + new Vector3(sizeX * 0.5f - thickness * 0.5f, 0f, 0f), new Vector3(thickness, 0.05f, sizeZ), mat, "BorderStrip_E");
            // West
            CreateProp(parent, center + new Vector3(-sizeX * 0.5f + thickness * 0.5f, 0f, 0f), new Vector3(thickness, 0.05f, sizeZ), mat, "BorderStrip_W");
        }

        private static void CreateFloorConduit(Transform parent, Vector3 pos, Vector3 size, Material bodyMat, Material coreMat)
        {
            GameObject conduit = new GameObject("FloorConduitTrench");
            conduit.transform.SetParent(parent);
            conduit.transform.position = pos;

            GameObject casing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            casing.name = "Casing";
            casing.transform.SetParent(conduit.transform);
            casing.transform.localPosition = Vector3.zero;
            casing.transform.localScale = size;
            Object.DestroyImmediate(casing.GetComponent<Collider>());
            casing.GetComponent<Renderer>().material = bodyMat;

            GameObject neonCore = GameObject.CreatePrimitive(PrimitiveType.Cube);
            neonCore.name = "NeonCore";
            neonCore.transform.SetParent(conduit.transform);
            neonCore.transform.localPosition = new Vector3(0f, size.y * 0.55f, 0f);
            neonCore.transform.localScale = new Vector3(size.x > size.z ? size.x : size.x * 0.35f, 0.02f, size.z > size.x ? size.z : size.z * 0.35f);
            Object.DestroyImmediate(neonCore.GetComponent<Collider>());
            neonCore.GetComponent<Renderer>().material = coreMat;
        }

        private static void CreateCryoPylon(Transform parent, Vector3 pos, Material bodyMat, Material glowMat)
        {
            GameObject pylon = new GameObject("CryoContainmentPylon");
            pylon.transform.SetParent(parent);
            pylon.transform.position = pos;

            // Column base
            GameObject col = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            col.name = "Column";
            col.transform.SetParent(pylon.transform);
            col.transform.localPosition = new Vector3(0f, 2.0f, 0f);
            col.transform.localScale = new Vector3(0.9f, 2.0f, 0.9f);
            col.GetComponent<Renderer>().material = bodyMat;

            // 3 Emissive energy coil rings
            for (int i = 0; i < 3; i++)
            {
                GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = $"EnergyRing_{i + 1}";
                ring.transform.SetParent(pylon.transform);
                ring.transform.localPosition = new Vector3(0f, 1.0f + i * 1.0f, 0f);
                ring.transform.localScale = new Vector3(1.05f, 0.12f, 1.05f);
                Object.DestroyImmediate(ring.GetComponent<Collider>());
                ring.GetComponent<Renderer>().material = glowMat;
            }

            // Top beacon light
            GameObject capLight = new GameObject("PylonBeaconLight");
            capLight.transform.SetParent(pylon.transform);
            capLight.transform.localPosition = new Vector3(0f, 4.1f, 0f);
            Light l = capLight.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.1f, 0.85f, 1.0f);
            l.intensity = 1.6f;
            l.range = 5f;
        }

        private static void CreateScienceStation(Transform parent, Vector3 pos, float rotY, Material bodyMat, Material screenMatA, Material screenMatB)
        {
            GameObject station = new GameObject("ScienceWorkstation");
            station.transform.SetParent(parent);
            station.transform.position = pos;
            station.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

            // Curved/angled desk
            GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "DeskBody";
            desk.transform.SetParent(station.transform);
            desk.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            desk.transform.localScale = new Vector3(2.6f, 1.1f, 1.3f);
            desk.GetComponent<Renderer>().material = bodyMat;

            // Screen 1 (Cyan Telemetry)
            GameObject scr1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            scr1.name = "TelemetryScreen_A";
            scr1.transform.SetParent(station.transform);
            scr1.transform.localPosition = new Vector3(-0.65f, 1.45f, 0.35f);
            scr1.transform.localScale = new Vector3(1.1f, 0.7f, 0.1f);
            scr1.transform.localRotation = Quaternion.Euler(-15f, 10f, 0f);
            scr1.GetComponent<Renderer>().material = screenMatA;

            // Screen 2 (Amber Diagnostics)
            GameObject scr2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            scr2.name = "TelemetryScreen_B";
            scr2.transform.SetParent(station.transform);
            scr2.transform.localPosition = new Vector3(0.65f, 1.45f, 0.35f);
            scr2.transform.localScale = new Vector3(1.1f, 0.7f, 0.1f);
            scr2.transform.localRotation = Quaternion.Euler(-15f, -10f, 0f);
            scr2.GetComponent<Renderer>().material = screenMatB;

            // Soft console illumination
            GameObject lightObj = new GameObject("ConsoleGlow");
            lightObj.transform.SetParent(station.transform);
            lightObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.2f, 0.85f, 1.0f);
            l.intensity = 1.2f;
            l.range = 3.5f;
        }

        private static void CreateServerBank(Transform parent, Vector3 pos, float rotY, Material bodyMat, Material cyanLed, Material greenLed, Material amberLed)
        {
            GameObject bank = new GameObject("MainframeServerRack");
            bank.transform.SetParent(parent);
            bank.transform.position = pos;
            bank.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "RackChassis";
            body.transform.SetParent(bank.transform);
            body.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            body.transform.localScale = new Vector3(2.4f, 3.4f, 1.2f);
            body.GetComponent<Renderer>().material = bodyMat;

            // Blinking activity LED strips
            for (int r = 0; r < 4; r++)
            {
                Material ledMat = (r % 3 == 0) ? cyanLed : ((r % 3 == 1) ? greenLed : amberLed);
                GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                strip.name = $"LED_Strip_{r + 1}";
                strip.transform.SetParent(bank.transform);
                strip.transform.localPosition = new Vector3(0f, 0.8f + r * 0.65f, 0.61f);
                strip.transform.localScale = new Vector3(1.8f, 0.12f, 0.04f);
                Object.DestroyImmediate(strip.GetComponent<Collider>());
                strip.GetComponent<Renderer>().material = ledMat;
            }
        }

        private static void CreateWallDisplay(Transform parent, Vector3 pos, Vector3 size, Material screenMat, Material frameMat)
        {
            GameObject display = new GameObject("WallDiagnosticDisplay");
            display.transform.SetParent(parent);
            display.transform.position = pos;

            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame";
            frame.transform.SetParent(display.transform);
            frame.transform.localPosition = Vector3.zero;
            frame.transform.localScale = size + new Vector3(0.2f, 0.2f, 0.05f);
            frame.GetComponent<Renderer>().material = frameMat;

            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = "ScreenSurface";
            screen.transform.SetParent(display.transform);
            screen.transform.localPosition = new Vector3(0f, 0f, 0.05f);
            screen.transform.localScale = size;
            Object.DestroyImmediate(screen.GetComponent<Collider>());
            screen.GetComponent<Renderer>().material = screenMat;
        }

        private static void CreateProp(Transform parent, Vector3 pos, Vector3 size, Material mat, string name)
        {
            GameObject prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prop.name = name;
            prop.transform.SetParent(parent);
            prop.transform.position = pos;
            prop.transform.localScale = size;
            prop.GetComponent<Renderer>().material = mat;
        }

        private static void CreateConsole(Transform parent, Vector3 pos, Material screenMat, Material bodyMat)
        {
            GameObject console = new GameObject("WorkstationConsole");
            console.transform.SetParent(parent);
            console.transform.position = pos;

            GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "Desk";
            desk.transform.SetParent(console.transform);
            desk.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            desk.transform.localScale = new Vector3(2.4f, 1.1f, 1.2f);
            desk.GetComponent<Renderer>().material = bodyMat;

            GameObject monitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            monitor.name = "Screen";
            monitor.transform.SetParent(console.transform);
            monitor.transform.localPosition = new Vector3(0f, 1.45f, 0.3f);
            monitor.transform.localScale = new Vector3(1.8f, 0.9f, 0.15f);
            monitor.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
            monitor.GetComponent<Renderer>().material = screenMat;
        }

        private static void CreateTerminalStand(Transform parent, Vector3 pos, Material screenMat, Material bodyMat)
        {
            GameObject stand = new GameObject("Terminal_ProjectAscension");
            stand.transform.SetParent(parent);
            stand.transform.position = pos;

            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.transform.SetParent(stand.transform);
            pillar.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            pillar.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
            pillar.GetComponent<Renderer>().material = bodyMat;

            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.transform.SetParent(stand.transform);
            screen.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            screen.transform.localScale = new Vector3(1.4f, 0.8f, 0.12f);
            screen.transform.localRotation = Quaternion.Euler(-25f, 180f, 0f);
            screen.GetComponent<Renderer>().material = screenMat;

            var monitorComp = stand.AddComponent<InteractiveMonitor>();
            monitorComp.SetContent(InteractiveMonitor.MonitorContent.PhaseOneInitiated);
        }

        private static void CreatePipes(Transform parent, Vector3 pos, float length, Material mat)
        {
            GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = "IndustrialPipe";
            pipe.transform.SetParent(parent);
            pipe.transform.position = pos;
            pipe.transform.localScale = new Vector3(0.35f, length * 0.5f, 0.35f);
            pipe.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            pipe.GetComponent<Renderer>().material = mat;
        }

        private static Material CreateColorMaterial(Color color, float metallic = 0.1f, float smoothness = 0.5f)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.color = color;
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }

        private static Material CreateGlassMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.color = new Color(0.2f, 0.7f, 0.95f, 0.35f);

            // True URP Transparency setup
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f); // Transparent
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);     // Alpha
            if (mat.HasProperty("_SrcBlend")) mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend")) mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.05f);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.95f);
            return mat;
        }

        private static Material CreateEmissiveMaterial(Color emissiveColor, float intensity = 2.5f)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.color = emissiveColor * 0.6f;
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissiveColor * intensity);
            return mat;
        }
    }
}
