using UnityEngine;

namespace LastGod.ThirdPerson.Environment
{
    public class LabEnvironmentBuilder : MonoBehaviour
    {
        [Header("Materials (Optional Overrides)")]
        [SerializeField] private Material floorMaterial;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material metalTrimMaterial;
        [SerializeField] private Material glassMaterial;
        [SerializeField] private Material emissiveScreenMaterial;
        [SerializeField] private Material warningStripeMaterial;

        public static GameObject BuildModularLab(Transform parent)
        {
            GameObject labRoot = new GameObject("Environment_Laboratory");
            if (parent != null) labRoot.transform.SetParent(parent);

            Material floorMat = CreateColorMaterial(new Color(0.12f, 0.14f, 0.16f), 0.2f, 0.6f);
            Material wallMat = CreateColorMaterial(new Color(0.18f, 0.20f, 0.22f), 0.1f, 0.4f);
            Material metalTrimMat = CreateColorMaterial(new Color(0.08f, 0.09f, 0.10f), 0.6f, 0.8f);
            Material glassMat = CreateGlassMaterial();
            Material screenMat = CreateEmissiveMaterial(new Color(0.1f, 0.8f, 1.0f));
            Material redWarningMat = CreateEmissiveMaterial(new Color(1.0f, 0.15f, 0.1f));

            // =========================================================
            // AREA A: CONTAINMENT CHAMBER ARENA (x: -15 to +15, z: -15 to +15)
            // =========================================================
            GameObject areaA = new GameObject("Area_A_ContainmentChamberArena");
            areaA.transform.SetParent(labRoot.transform);

            // Arena Main Floor
            CreateFloor(areaA.transform, new Vector3(0f, 0f, 0f), new Vector3(30f, 0.5f, 30f), floorMat);
            // Arena Ceiling
            CreateCeiling(areaA.transform, new Vector3(0f, 6.5f, 0f), new Vector3(30f, 0.5f, 30f), wallMat);
            // Surrounding Walls
            CreateWall(areaA.transform, new Vector3(0f, 3.25f, -15f), new Vector3(30f, 6.5f, 0.8f), wallMat); // South
            CreateWall(areaA.transform, new Vector3(-15f, 3.25f, 0f), new Vector3(0.8f, 6.5f, 30f), wallMat); // West
            CreateWall(areaA.transform, new Vector3(15f, 3.25f, 0f), new Vector3(0.8f, 6.5f, 30f), wallMat);  // East

            // Central Chamber Dais Platform
            CreatePlatform(areaA.transform, new Vector3(0f, 0.4f, 0f), new Vector3(6f, 0.6f, 6f), metalTrimMat);
            // Cable conduits running to dais
            CreateProp(areaA.transform, new Vector3(0f, 0.2f, -5f), new Vector3(1.2f, 0.25f, 6f), metalTrimMat, "CableTrench_South");
            CreateProp(areaA.transform, new Vector3(-5f, 0.2f, 0f), new Vector3(6f, 0.25f, 1.2f), metalTrimMat, "CableTrench_West");

            // Overhead Chamber Apparatus
            CreateProp(areaA.transform, new Vector3(0f, 5.2f, 0f), new Vector3(3.2f, 2.0f, 3.2f), metalTrimMat, "Overhead_Stasis_Apparatus");

            // =========================================================
            // AREA B: ELEVATED CONTROL ROOM (x: -10 to +10, z: 15 to 25, y: 2.5)
            // =========================================================
            GameObject areaB = new GameObject("Area_B_ElevatedControlRoom");
            areaB.transform.SetParent(labRoot.transform);

            // Elevated floor
            CreateFloor(areaB.transform, new Vector3(0f, 2.5f, 20f), new Vector3(20f, 0.5f, 10f), floorMat);
            CreateCeiling(areaB.transform, new Vector3(0f, 7.5f, 20f), new Vector3(20f, 0.5f, 10f), wallMat);
            CreateWall(areaB.transform, new Vector3(0f, 5.0f, 25f), new Vector3(20f, 5.0f, 0.8f), wallMat); // Back wall
            CreateWall(areaB.transform, new Vector3(-10f, 5.0f, 20f), new Vector3(0.8f, 5.0f, 10f), wallMat);
            CreateWall(areaB.transform, new Vector3(10f, 5.0f, 20f), new Vector3(0.8f, 5.0f, 10f), wallMat);

            // Reinforced Observation Glass overlooking Area A
            GameObject glassPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glassPanel.name = "ObservationWindow_Glass";
            glassPanel.transform.SetParent(areaB.transform);
            glassPanel.transform.position = new Vector3(0f, 4.2f, 15f);
            glassPanel.transform.localScale = new Vector3(16f, 3.2f, 0.3f);
            glassPanel.GetComponent<Renderer>().material = glassMat;

            // Control Room Workstations
            CreateConsole(areaB.transform, new Vector3(-4f, 3.3f, 16.2f), screenMat, metalTrimMat);
            CreateConsole(areaB.transform, new Vector3(0f, 3.3f, 16.2f), screenMat, metalTrimMat);
            CreateConsole(areaB.transform, new Vector3(4f, 3.3f, 16.2f), screenMat, metalTrimMat);

            // =========================================================
            // AREA C: SECURITY CORRIDOR (x: 15 to 35, z: -4 to +4)
            // =========================================================
            GameObject areaC = new GameObject("Area_C_SecurityCorridor");
            areaC.transform.SetParent(labRoot.transform);

            CreateFloor(areaC.transform, new Vector3(25f, 0f, 0f), new Vector3(20f, 0.5f, 8f), floorMat);
            CreateCeiling(areaC.transform, new Vector3(25f, 5.5f, 0f), new Vector3(20f, 0.5f, 8f), wallMat);
            CreateWall(areaC.transform, new Vector3(25f, 2.75f, 4f), new Vector3(20f, 5.5f, 0.8f), wallMat);
            CreateWall(areaC.transform, new Vector3(25f, 2.75f, -4f), new Vector3(20f, 5.5f, 0.8f), wallMat);

            // Overhead pipes and cable racks along corridor
            CreatePipes(areaC.transform, new Vector3(25f, 4.6f, 3.2f), 18f, metalTrimMat);
            CreatePipes(areaC.transform, new Vector3(25f, 4.6f, -3.2f), 18f, metalTrimMat);

            // =========================================================
            // AREA D: MEDICAL & RESEARCH LABORATORY (x: 35 to 55, z: -15 to +15)
            // =========================================================
            GameObject areaD = new GameObject("Area_D_MedicalResearchBay");
            areaD.transform.SetParent(labRoot.transform);

            CreateFloor(areaD.transform, new Vector3(45f, 0f, 0f), new Vector3(20f, 0.5f, 30f), floorMat);
            CreateCeiling(areaD.transform, new Vector3(45f, 6.0f, 0f), new Vector3(20f, 0.5f, 30f), wallMat);
            CreateWall(areaD.transform, new Vector3(45f, 3.0f, -15f), new Vector3(20f, 6.0f, 0.8f), wallMat);
            CreateWall(areaD.transform, new Vector3(45f, 3.0f, 15f), new Vector3(20f, 6.0f, 0.8f), wallMat);

            // Examination tables & medical containers
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
            CreateCeiling(areaE.transform, new Vector3(65f, 5.0f, 0f), new Vector3(20f, 0.5f, 8f), wallMat);
            CreateWall(areaE.transform, new Vector3(65f, 2.5f, 4f), new Vector3(20f, 5.0f, 0.8f), wallMat);
            CreateWall(areaE.transform, new Vector3(65f, 2.5f, -4f), new Vector3(20f, 5.0f, 0.8f), wallMat);

            // =========================================================
            // AREA F: FINAL ESCAPE AIRLOCK GATE (x: 75 to 90, z: -10 to +10)
            // =========================================================
            GameObject areaF = new GameObject("Area_F_FinalEscapeAirlock");
            areaF.transform.SetParent(labRoot.transform);

            CreateFloor(areaF.transform, new Vector3(82f, 0f, 0f), new Vector3(14f, 0.5f, 18f), floorMat);
            CreateCeiling(areaF.transform, new Vector3(82f, 6.0f, 0f), new Vector3(14f, 0.5f, 18f), wallMat);
            CreateWall(areaF.transform, new Vector3(89f, 3.0f, 0f), new Vector3(0.8f, 6.0f, 18f), wallMat); // Back airlock blast door
            CreateWall(areaF.transform, new Vector3(82f, 3.0f, 9f), new Vector3(14f, 6.0f, 0.8f), wallMat);
            CreateWall(areaF.transform, new Vector3(82f, 3.0f, -9f), new Vector3(14f, 6.0f, 0.8f), wallMat);

            // Terminal station in Area F
            CreateTerminalStand(areaF.transform, new Vector3(86f, 1.2f, 0f), screenMat, metalTrimMat);

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

        private static void CreateCeiling(Transform parent, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.SetParent(parent);
            ceiling.transform.position = pos;
            ceiling.transform.localScale = size;
            ceiling.GetComponent<Renderer>().material = mat;
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

        private static void CreatePlatform(Transform parent, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject plat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plat.name = "Platform";
            plat.transform.SetParent(parent);
            plat.transform.position = pos;
            plat.transform.localScale = size;
            plat.GetComponent<Renderer>().material = mat;
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
            desk.transform.localPosition = new Vector3(0f, 0f, 0f);
            desk.transform.localScale = new Vector3(2.4f, 1.1f, 1.2f);
            desk.GetComponent<Renderer>().material = bodyMat;

            GameObject monitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            monitor.name = "Screen";
            monitor.transform.SetParent(console.transform);
            monitor.transform.localPosition = new Vector3(0f, 0.9f, 0.3f);
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
            pillar.transform.localPosition = Vector3.zero;
            pillar.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
            pillar.GetComponent<Renderer>().material = bodyMat;

            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.transform.SetParent(stand.transform);
            screen.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            screen.transform.localScale = new Vector3(1.4f, 0.8f, 0.12f);
            screen.transform.localRotation = Quaternion.Euler(-20f, 180f, 0f);
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
            mat.color = new Color(0.3f, 0.6f, 0.8f, 0.35f);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f); // Transparent
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.1f);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.95f);
            return mat;
        }

        private static Material CreateEmissiveMaterial(Color emissiveColor)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.color = emissiveColor * 0.6f;
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissiveColor * 2.5f);
            return mat;
        }
    }
}
