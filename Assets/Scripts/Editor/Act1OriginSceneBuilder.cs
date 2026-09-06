#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using LastGod.ThirdPerson.Player;
using LastGod.ThirdPerson.AI;
using LastGod.ThirdPerson.Combat;
using LastGod.ThirdPerson.Cinematics;
using LastGod.ThirdPerson.Environment;
using LastGod.ThirdPerson.Dialogue;
using LastGod.ThirdPerson.UI;
using LastGod.ThirdPerson.Audio;

namespace LastGod.ThirdPerson.Editor
{
    [InitializeOnLoad]
    public static class Act1OriginSceneBuilder
    {
        static Act1OriginSceneBuilder()
        {
            EditorApplication.delayCall += AutoBuildIfNeeded;
        }

        private static void AutoBuildIfNeeded()
        {
            if (!System.IO.File.Exists("Assets/Scenes/Act1_Origin.unity"))
            {
                Debug.Log("[Act1OriginSceneBuilder] Act1_Origin.unity missing. Building complete Act 1 (Origin) scene...");
                BuildAct1OriginScene();
            }

            if (!System.IO.File.Exists("Assets/Scenes/MainMenu_Origin.unity"))
            {
                Debug.Log("[Act1OriginSceneBuilder] MainMenu_Origin.unity missing. Building cinematic main menu scene...");
                BuildMainMenuScene();
            }
        }
        [MenuItem("The Last God/Build Complete Act 1 (Origin) Scene", false, 1)]
        public static void BuildAct1OriginScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Lighting Environment & Ambient
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.08f, 0.10f, 0.14f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.04f, 0.05f, 0.08f);
            RenderSettings.fogDensity = 0.035f;

            // 2. Core Managers Root
            GameObject managersRoot = new GameObject("=== CORE MANAGERS ===");
            var combatManager = managersRoot.AddComponent<CombatManager>();
            var dialogueSystem = managersRoot.AddComponent<DialogueSystem>();
            var audioManager = managersRoot.AddComponent<AudioManager>();
            var pauseMenu = managersRoot.AddComponent<PauseMenuUI>();

            // 3. Build Laboratory Architecture (Areas A through F)
            GameObject labRoot = LabEnvironmentBuilder.BuildModularLab(null);

            // 4. Lab Lighting Controller & Overhead Luminaires
            GameObject lightingObj = new GameObject("Lighting_System");
            var lightingCtrl = lightingObj.AddComponent<LabLightingController>();

            List<Light> sterileLights = new();
            List<Light> emergencyLights = new();

            // Sterile overhead lights in Area A (Arena)
            Vector3[] sterilePositions = new Vector3[]
            {
                new Vector3(-6f, 5.8f, -6f),
                new Vector3(6f, 5.8f, -6f),
                new Vector3(-6f, 5.8f, 6f),
                new Vector3(6f, 5.8f, 6f),
                new Vector3(25f, 4.8f, 0f),
                new Vector3(45f, 5.5f, 0f),
                new Vector3(65f, 4.5f, 0f),
                new Vector3(82f, 5.5f, 0f)
            };

            foreach (var pos in sterilePositions)
            {
                GameObject lObj = new GameObject("Light_Sterile_Overhead");
                lObj.transform.SetParent(lightingObj.transform);
                lObj.transform.position = pos;
                Light l = lObj.AddComponent<Light>();
                l.type = LightType.Point;
                l.range = 14f;
                l.color = new Color(0.7f, 0.85f, 1.0f);
                l.intensity = 1.8f;
                sterileLights.Add(l);
            }

            // Emergency Red Beacon Lights in Area A & Corridor
            Vector3[] emergencyPositions = new Vector3[]
            {
                new Vector3(0f, 6.0f, 0f),
                new Vector3(-10f, 5.5f, 0f),
                new Vector3(10f, 5.5f, 0f),
                new Vector3(25f, 5.0f, 0f),
                new Vector3(45f, 5.5f, 0f)
            };

            foreach (var pos in emergencyPositions)
            {
                GameObject eObj = new GameObject("Light_Emergency_Red");
                eObj.transform.SetParent(lightingObj.transform);
                eObj.transform.position = pos;
                Light l = eObj.AddComponent<Light>();
                l.type = LightType.Point;
                l.range = 16f;
                l.color = new Color(1.0f, 0.05f, 0.05f);
                l.intensity = 0f;
                emergencyLights.Add(l);
            }

            SetSerializedField(lightingCtrl, "sterileLights", sterileLights);
            SetSerializedField(lightingCtrl, "emergencyRedLights", emergencyLights);
            AudioSource alarmSource = lightingObj.AddComponent<AudioSource>();
            alarmSource.clip = AudioManager.GenerateAlarmKlaxonClip();
            alarmSource.loop = true;
            SetSerializedField(lightingCtrl, "alarmAudioSource", alarmSource);
            SetSerializedField(lightingCtrl, "alarmKlaxonSFX", alarmSource.clip);

            // 5. Containment Chamber (Central Dais at 0, 0.7, 0)
            GameObject chamberObj = new GameObject("StasisChamber_Root");
            chamberObj.transform.position = new Vector3(0f, 0.7f, 0f);
            var chamber = chamberObj.AddComponent<StasisChamber>();

            // Glass Cylinder
            GameObject glassCyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            glassCyl.name = "GlassCylinder";
            glassCyl.transform.SetParent(chamberObj.transform);
            glassCyl.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            glassCyl.transform.localScale = new Vector3(2.2f, 1.4f, 2.2f);
            var glassMat = CreateGlassMaterial();
            glassCyl.GetComponent<Renderer>().material = glassMat;

            // Liquid Surface
            GameObject liquidObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            liquidObj.name = "StasisLiquid";
            liquidObj.transform.SetParent(chamberObj.transform);
            liquidObj.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            liquidObj.transform.localScale = new Vector3(2.1f, 1.3f, 2.1f);
            var liquidMat = CreateLiquidMaterial();
            liquidObj.GetComponent<Renderer>().material = liquidMat;

            // Chamber Internal Glow Light
            GameObject chLightObj = new GameObject("Chamber_Internal_Light");
            chLightObj.transform.SetParent(chamberObj.transform);
            chLightObj.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            Light chLight = chLightObj.AddComponent<Light>();
            chLight.type = LightType.Point;
            chLight.range = 7f;
            chLight.color = new Color(0.15f, 0.9f, 1.0f);
            chLight.intensity = 2.5f;

            AudioSource chAudio = chamberObj.AddComponent<AudioSource>();
            AudioClip shatterClip = AudioManager.GenerateGlassShatterClip();
            SetSerializedField(chamber, "glassCylinder", glassCyl);
            SetSerializedField(chamber, "liquidSurface", liquidObj);
            SetSerializedField(chamber, "chamberInternalLight", chLight);
            SetSerializedField(chamber, "audioSource", chAudio);
            SetSerializedField(chamber, "glassShatterSFX", shatterClip);

            // 6. Aeron (Playable Protagonist)
            GameObject aeron = CreateAeronCharacter(new Vector3(0f, 0.7f, 0f));

            // 7. Third-Person Main Camera
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 300f;
            camObj.AddComponent<AudioListener>();
            var camCtrl = camObj.AddComponent<ThirdPersonCameraController>();
            camCtrl.SetTarget(aeron.transform);

            // HUD
            camObj.AddComponent<VitalStabilityHUD>();

            // 8. Dr. Ilya Voss NPC in Area B (Elevated Control Room)
            GameObject vossObj = CreateDrVossCharacter(new Vector3(0f, 2.75f, 18f));

            // 9. Sliding Security Blast Door (between Area A and Area C)
            GameObject doorObj = new GameObject("SecurityDoor_AreaA_to_AreaC");
            doorObj.transform.position = new Vector3(15f, 1.5f, 0f);
            var labDoor = doorObj.AddComponent<LabDoor>();

            GameObject doorLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorLeft.name = "DoorPanel_Left";
            doorLeft.transform.SetParent(doorObj.transform);
            doorLeft.transform.localPosition = new Vector3(0f, 0f, -1.8f);
            doorLeft.transform.localScale = new Vector3(0.4f, 3.2f, 3.6f);

            GameObject doorRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorRight.name = "DoorPanel_Right";
            doorRight.transform.SetParent(doorObj.transform);
            doorRight.transform.localPosition = new Vector3(0f, 0f, 1.8f);
            doorRight.transform.localScale = new Vector3(0.4f, 3.2f, 3.6f);

            BoxCollider doorTrigger = doorObj.AddComponent<BoxCollider>();
            doorTrigger.isTrigger = true;
            doorTrigger.size = new Vector3(6f, 4f, 8f);

            SetSerializedField(labDoor, "leftPanel", doorLeft.transform);
            SetSerializedField(labDoor, "rightPanel", doorRight.transform);

            // 10. Security Guards (5 Tactical Guards deployed in Arena & Corridor)
            List<GuardAI> guardList = new();
            Vector3[] guardSpawns = new Vector3[]
            {
                new Vector3(18f, 0.5f, -1.5f),
                new Vector3(20f, 0.5f, 1.5f),
                new Vector3(23f, 0.5f, -2.5f),
                new Vector3(26f, 0.5f, 2.0f),
                new Vector3(28f, 0.5f, 0f)
            };

            for (int i = 0; i < guardSpawns.Length; i++)
            {
                GameObject gObj = CreateGuardEnemy($"Guard_Security_{i + 1}", guardSpawns[i]);
                guardList.Add(gObj.GetComponent<GuardAI>());
            }

            // 11. Interactive Monitor in Area F
            InteractiveMonitor endTerminal = Object.FindAnyObjectByType<InteractiveMonitor>();

            // 12. Master Act 1 Sequence Director
            GameObject directorObj = new GameObject("=== ACT 1 ORIGIN DIRECTOR ===");
            var director = directorObj.AddComponent<Act1OriginDirector>();
            AudioSource ambientSource = directorObj.AddComponent<AudioSource>();
            AudioSource sfxSource = directorObj.AddComponent<AudioSource>();

            SetSerializedField(director, "player", aeron.GetComponent<ThirdPersonPlayerController>());
            SetSerializedField(director, "cameraController", camCtrl);
            SetSerializedField(director, "chamber", chamber);
            SetSerializedField(director, "lighting", lightingCtrl);
            SetSerializedField(director, "endTerminalMonitor", endTerminal);
            SetSerializedField(director, "securityDoor", labDoor);
            SetSerializedField(director, "drVossNPC", vossObj.transform);
            SetSerializedField(director, "guards", guardList);
            SetSerializedField(director, "ambientSource", ambientSource);
            SetSerializedField(director, "sfxSource", sfxSource);
            SetSerializedField(director, "heartbeatSFX", AudioManager.GenerateHeartbeatClip());

            // Save Scene
            string scenePath = "Assets/Scenes/Act1_Origin.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"<color=cyan>[The Last God]</color> Successfully built master playable scene: {scenePath}");

            // Also build Main Menu Scene
            BuildMainMenuScene();
        }

        [MenuItem("The Last God/Build Main Menu Scene", false, 2)]
        public static void BuildMainMenuScene()
        {
            Scene menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Lighting & Atmosphere
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.04f, 0.06f, 0.08f);

            // Camera
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            camObj.transform.position = new Vector3(0f, 2.0f, -4.5f);
            camObj.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            camObj.AddComponent<AudioListener>();

            // Main Menu UI Controller
            camObj.AddComponent<MainMenuUI>();

            // Background lab chamber silhouette
            GameObject dais = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dais.transform.position = new Vector3(0f, 0.3f, 0f);
            dais.transform.localScale = new Vector3(4f, 0.6f, 4f);

            GameObject pod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pod.transform.position = new Vector3(0f, 1.6f, 0f);
            pod.transform.localScale = new Vector3(1.8f, 1.4f, 1.8f);
            pod.GetComponent<Renderer>().material = CreateGlassMaterial();

            // Soft cyan light inside pod
            GameObject pLight = new GameObject("Pod_Light");
            pLight.transform.position = new Vector3(0f, 1.6f, 0f);
            Light l = pLight.AddComponent<Light>();
            l.color = new Color(0.2f, 0.85f, 1.0f);
            l.intensity = 3.0f;
            l.range = 8f;

            string menuPath = "Assets/Scenes/MainMenu_Origin.unity";
            EditorSceneManager.SaveScene(menuScene, menuPath);
            Debug.Log($"<color=cyan>[The Last God]</color> Successfully built cinematic main menu: {menuPath}");
        }

        private static GameObject CreateAeronCharacter(Vector3 position)
        {
            GameObject aeron = new GameObject("Aeron_Protagonist");
            aeron.tag = "Player";
            aeron.transform.position = position;

            var cc = aeron.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 0.95f, 0f);
            cc.height = 1.9f;
            cc.radius = 0.42f;

            aeron.AddComponent<ThirdPersonPlayerInput>();
            var health = aeron.AddComponent<Health3D>();
            var receiver = aeron.AddComponent<DamageReceiver>();
            var surge = aeron.AddComponent<AscensionSurge>();
            var controller = aeron.AddComponent<ThirdPersonPlayerController>();

            // Audio & SFX
            AudioSource audio = aeron.AddComponent<AudioSource>();
            SetSerializedField(controller, "audioSource", audio);
            SetSerializedField(controller, "punchSFX", AudioManager.GeneratePunchImpactClip());
            SetSerializedField(controller, "heavyStrikeSFX", AudioManager.GeneratePunchImpactClip());

            // Visual mesh body
            GameObject visual = new GameObject("Aeron_Visual");
            visual.transform.SetParent(aeron.transform);
            visual.transform.localPosition = Vector3.zero;

            // Torso
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(visual.transform);
            torso.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            torso.transform.localScale = new Vector3(0.55f, 0.55f, 0.35f);
            Object.DestroyImmediate(torso.GetComponent<Collider>());
            torso.GetComponent<Renderer>().material = CreateColorMaterial(new Color(0.08f, 0.08f, 0.10f), 0.2f, 0.5f); // Black containment suit

            // Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(visual.transform);
            head.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            head.transform.localScale = new Vector3(0.32f, 0.35f, 0.32f);
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().material = CreateColorMaterial(new Color(0.85f, 0.78f, 0.72f), 0.05f, 0.3f); // Pale skin

            // Eye glow for Ascension Surge
            GameObject eyeLight = new GameObject("EyeGlowLight");
            eyeLight.transform.SetParent(head.transform);
            eyeLight.transform.localPosition = new Vector3(0f, 0.05f, 0.2f);
            Light eLight = eyeLight.AddComponent<Light>();
            eLight.type = LightType.Point;
            eLight.color = new Color(0.2f, 0.9f, 1.0f);
            eLight.intensity = 2.0f;
            eLight.range = 2.5f;
            eLight.enabled = false;
            SetSerializedField(surge, "eyeGlowLight", eLight);

            // Procedural Animator Link
            var pAnim = visual.AddComponent<ThirdPersonPlayerAnimator>();
            SetSerializedField(pAnim, "torso", torso.transform);
            SetSerializedField(pAnim, "head", head.transform);

            return aeron;
        }

        private static GameObject CreateDrVossCharacter(Vector3 position)
        {
            GameObject voss = new GameObject("NPC_Dr_Ilya_Voss");
            voss.transform.position = position;

            // Scientist Body (White lab coat)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(voss.transform);
            body.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            body.transform.localScale = new Vector3(0.48f, 0.55f, 0.32f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().material = CreateColorMaterial(new Color(0.88f, 0.90f, 0.92f), 0.1f, 0.3f);

            // Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(voss.transform);
            head.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            head.transform.localScale = new Vector3(0.3f, 0.32f, 0.3f);
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().material = CreateColorMaterial(new Color(0.82f, 0.72f, 0.65f), 0.05f, 0.3f);

            return voss;
        }

        private static GameObject CreateGuardEnemy(string name, Vector3 position)
        {
            GameObject guard = new GameObject(name);
            guard.tag = "Enemy";
            guard.transform.position = position;

            var cc = guard.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 0.95f, 0f);
            cc.height = 1.9f;
            cc.radius = 0.45f;

            var health = guard.AddComponent<Health3D>();
            var receiver = guard.AddComponent<DamageReceiver>();
            var reaction = guard.AddComponent<HitReaction>();
            var guardAI = guard.AddComponent<GuardAI>();
            var weapon = guard.AddComponent<GuardWeapon>();

            AudioSource gAudio = guard.AddComponent<AudioSource>();
            SetSerializedField(weapon, "audioSource", gAudio);
            SetSerializedField(weapon, "gunshotSFX", AudioManager.GenerateGunshotClip());

            // Visual Tactical Suit
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "TacticalArmor";
            body.transform.SetParent(guard.transform);
            body.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            body.transform.localScale = new Vector3(0.6f, 0.6f, 0.45f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().material = CreateColorMaterial(new Color(0.18f, 0.22f, 0.24f), 0.4f, 0.7f); // Tactical grey/black

            // Tactical Helmet
            GameObject helmet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            helmet.name = "TacticalHelmet";
            helmet.transform.SetParent(guard.transform);
            helmet.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            helmet.transform.localScale = new Vector3(0.38f, 0.38f, 0.38f);
            Object.DestroyImmediate(helmet.GetComponent<Collider>());
            helmet.GetComponent<Renderer>().material = CreateColorMaterial(new Color(0.10f, 0.12f, 0.14f), 0.6f, 0.8f);

            // Visor glow
            GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "VisorGlow";
            visor.transform.SetParent(helmet.transform);
            visor.transform.localPosition = new Vector3(0f, 0.02f, 0.18f);
            visor.transform.localScale = new Vector3(0.24f, 0.08f, 0.08f);
            Object.DestroyImmediate(visor.GetComponent<Collider>());
            visor.GetComponent<Renderer>().material = CreateEmissiveMaterial(new Color(1.0f, 0.3f, 0.1f));

            // Rifle Prop
            GameObject rifle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rifle.name = "AssaultRifle";
            rifle.transform.SetParent(guard.transform);
            rifle.transform.localPosition = new Vector3(0.35f, 1.05f, 0.45f);
            rifle.transform.localScale = new Vector3(0.12f, 0.18f, 0.85f);
            Object.DestroyImmediate(rifle.GetComponent<Collider>());
            rifle.GetComponent<Renderer>().material = CreateColorMaterial(new Color(0.08f, 0.08f, 0.08f), 0.8f, 0.7f);

            // Laser Sight
            LineRenderer lr = rifle.AddComponent<LineRenderer>();
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;
            lr.material = CreateEmissiveMaterial(new Color(1.0f, 0.1f, 0.1f));
            lr.enabled = false;
            SetSerializedField(weapon, "laserSight", lr);

            return guard;
        }

        private static void SetSerializedField(Object target, string fieldName, object value)
        {
            var serializedObj = new SerializedObject(target);
            var prop = serializedObj.FindProperty(fieldName);
            if (prop != null)
            {
                if (value is Object uObj) prop.objectReferenceValue = uObj;
                serializedObj.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                // Reflection fallback for private fields
                var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                if (field != null)
                {
                    field.SetValue(target, value);
                }
            }
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
            mat.color = new Color(0.25f, 0.55f, 0.85f, 0.35f);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            return mat;
        }

        private static Material CreateLiquidMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.color = new Color(0.1f, 0.8f, 0.95f, 0.65f);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            return mat;
        }

        private static Material CreateEmissiveMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color * 2.0f);
            return mat;
        }
    }
}
#endif
