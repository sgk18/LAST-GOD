#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using LastGod.ThirdPerson.Player;
using LastGod.ThirdPerson.AI;
using LastGod.ThirdPerson.Combat;
using LastGod.ThirdPerson.Cinematics;
using LastGod.ThirdPerson.Environment;
using LastGod.ThirdPerson.Dialogue;
using LastGod.ThirdPerson.UI;
using LastGod.ThirdPerson.Audio;
using LastGod.Characters.Aeron;

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
            string triggerFile = "Temp/rebuild_act1_origin.trigger";
            bool forceRebuild = System.IO.File.Exists(triggerFile);
            if (forceRebuild)
            {
                try { System.IO.File.Delete(triggerFile); } catch {}
            }

            if (!System.IO.File.Exists("Assets/Scenes/MainMenu_Origin.unity"))
            {
                Debug.Log("[Act1OriginSceneBuilder] MainMenu_Origin.unity missing. Building cinematic main menu scene...");
                BuildMainMenuScene();
            }

            if (forceRebuild || !System.IO.File.Exists("Assets/Scenes/Act1_Origin.unity"))
            {
                Debug.Log("[Act1OriginSceneBuilder] Building complete Act 1 (Origin) scene with authentic Aeron Idle Animation...");
                BuildAct1OriginScene();
            }
            else
            {
                EditorSceneManager.OpenScene("Assets/Scenes/Act1_Origin.unity", OpenSceneMode.Single);
            }
        }

        [MenuItem("The Last God/Open Act 1 (Origin) Scene", false, 0)]
        public static void OpenAct1OriginScene()
        {
            if (System.IO.File.Exists("Assets/Scenes/Act1_Origin.unity"))
            {
                EditorSceneManager.OpenScene("Assets/Scenes/Act1_Origin.unity", OpenSceneMode.Single);
            }
            else
            {
                BuildAct1OriginScene();
            }
        }

        [MenuItem("The Last God/Build Complete Act 1 (Origin) Scene", false, 1)]
        public static void BuildAct1OriginScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Lighting Environment & Ambient
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.14f, 0.17f, 0.22f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.04f, 0.05f, 0.08f);
            RenderSettings.fogDensity = 0.025f;

            // Directional Fill Light for Atmospheric Volume & Shadows
            GameObject dirLightObj = new GameObject("Lab_Directional_Fill");
            dirLightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light dirLight = dirLightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.65f, 0.75f, 0.90f);
            dirLight.intensity = 0.55f;
            dirLight.shadows = LightShadows.Soft;

            // Audio Clips
            AudioClip alarmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/alarm.wav") ?? AudioManager.GenerateAlarmKlaxonClip();
            AudioClip shatterClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/glass_shatter.wav") ?? AudioManager.GenerateGlassShatterClip();
            AudioClip droneClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/machine_hum.wav") ?? AudioManager.GenerateAmbientHumClip();
            AudioClip heartbeatClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/heartbeat.wav") ?? AudioManager.GenerateHeartbeatClip();
            AudioClip combatClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/lab_bg_music.wav") ?? AudioManager.GenerateCombatPulseClip();
            AudioClip gunshotClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/gunshot.wav") ?? AudioManager.GenerateGunshotClip();

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
            alarmSource.clip = alarmClip;
            alarmSource.loop = true;
            SetSerializedField(lightingCtrl, "alarmAudioSource", alarmSource);
            SetSerializedField(lightingCtrl, "alarmKlaxonSFX", alarmClip);

            // 5. Containment Chamber (Central Dais at 0, 0.7, 0)
            GameObject chamberObj = new GameObject("StasisChamber_Root");
            chamberObj.transform.position = new Vector3(0f, 0.7f, 0f);
            var chamber = chamberObj.AddComponent<StasisChamber>();

            // Glass Cylinder (collider removed so player is not blocked)
            GameObject glassCyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            glassCyl.name = "GlassCylinder";
            glassCyl.transform.SetParent(chamberObj.transform);
            glassCyl.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            glassCyl.transform.localScale = new Vector3(2.2f, 1.4f, 2.2f);
            Object.DestroyImmediate(glassCyl.GetComponent<Collider>());
            var glassMat = CreateGlassMaterial();
            glassCyl.GetComponent<Renderer>().material = glassMat;

            // Liquid Surface (collider removed)
            GameObject liquidObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            liquidObj.name = "StasisLiquid";
            liquidObj.transform.SetParent(chamberObj.transform);
            liquidObj.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            liquidObj.transform.localScale = new Vector3(2.1f, 1.3f, 2.1f);
            Object.DestroyImmediate(liquidObj.GetComponent<Collider>());
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

            // Chamber Particle Systems
            var bubbles = CreateBubblesParticleSystem(chamberObj.transform);
            var shatterVFX = CreateBurstParticleSystem(chamberObj.transform, "ShatterGlass_VFX", new Color(0.8f, 0.95f, 1f, 0.7f), 60);
            var liquidVFX = CreateBurstParticleSystem(chamberObj.transform, "LiquidBurst_VFX", new Color(0.1f, 0.8f, 0.95f, 0.8f), 80);

            AudioSource chAudio = chamberObj.AddComponent<AudioSource>();
            SetSerializedField(chamber, "glassCylinder", glassCyl);
            SetSerializedField(chamber, "liquidSurface", liquidObj);
            SetSerializedField(chamber, "chamberInternalLight", chLight);
            SetSerializedField(chamber, "audioSource", chAudio);
            SetSerializedField(chamber, "glassShatterSFX", shatterClip);
            SetSerializedField(chamber, "glassCrackSFX", shatterClip);
            SetSerializedField(chamber, "liquidDrainSFX", droneClip);
            SetSerializedField(chamber, "bubblesVFX", bubbles);
            SetSerializedField(chamber, "shatterGlassVFX", shatterVFX);
            SetSerializedField(chamber, "liquidBurstVFX", liquidVFX);

            // 6. Aeron (Playable Protagonist with Authentic Seamless Idle Animation)
            GameObject aeron = CreateAeronCharacter(new Vector3(0f, 0.7f, 0f));

            // 7. Third-Person Main Camera
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 300f;
            cam.clearFlags = CameraClearFlags.Color;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.08f);
            camObj.AddComponent<AudioListener>();

            var camData = camObj.AddComponent<UniversalAdditionalCameraData>();
            SetSerializedField(camData, "m_RendererIndex", 1);
            camData.renderPostProcessing = true;

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
                GameObject gObj = CreateGuardEnemy($"Guard_Security_{i + 1}", guardSpawns[i], gunshotClip);
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
            SetSerializedField(director, "heartbeatSFX", heartbeatClip);
            SetSerializedField(director, "ambientLabDrone", droneClip);
            SetSerializedField(director, "combatPulseMusic", combatClip);
            SetSerializedField(director, "ominousEndingDrone", droneClip);

            // Save Scene
            string scenePath = "Assets/Scenes/Act1_Origin.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"<color=cyan>[The Last God]</color> Successfully built master playable scene: {scenePath}");

            try
            {
                string syncPath = Application.dataPath.Replace("\\", "/").Contains("/LAST-GOD/Assets")
                    ? System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../../Assets/Scenes/Act1_Origin.unity"))
                    : System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../LAST-GOD/Assets/Scenes/Act1_Origin.unity"));
                if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(syncPath)))
                {
                    System.IO.File.Copy(scenePath, syncPath, true);
                }
            }
            catch {}

            // Open Scene
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Auto-launch into Play Mode
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlaying)
                {
                    Debug.Log("<color=green>[The Last God]</color> Auto-launching Act 1 (Origin) into Play Mode...");
                    EditorApplication.isPlaying = true;
                }
            };
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

            var camData = camObj.AddComponent<UniversalAdditionalCameraData>();
            SetSerializedField(camData, "m_RendererIndex", 1);
            camData.renderPostProcessing = true;

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

            try
            {
                string syncMenuPath = Application.dataPath.Replace("\\", "/").Contains("/LAST-GOD/Assets")
                    ? System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../../Assets/Scenes/MainMenu_Origin.unity"))
                    : System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../LAST-GOD/Assets/Scenes/MainMenu_Origin.unity"));
                if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(syncMenuPath)))
                {
                    System.IO.File.Copy(menuPath, syncMenuPath, true);
                }
            }
            catch {}
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

            // Visual mesh body with Authentic Seamless Idle Animation
            GameObject visual = new GameObject("Aeron_Visual");
            visual.transform.SetParent(aeron.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);

            // 1. Sprite Renderer with Aeron Idle Frame
            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            Sprite idleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Characters/Aeron/Sprites/Idle/Aeron_Idle_01.png");
            if (idleSprite != null)
            {
                sr.sprite = idleSprite;
            }
            sr.sortingOrder = 10;
            Shader spriteShader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Unlit/Texture");
            if (spriteShader != null)
            {
                sr.material = new Material(spriteShader);
            }

            // 2. Animator with Aeron Runtime Controller
            Animator anim = visual.AddComponent<Animator>();
            RuntimeAnimatorController animController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Characters/Aeron/Animator/Aeron.controller");
            if (animController != null)
            {
                anim.runtimeAnimatorController = animController;
            }

            // 3. Procedural Idle Motion Nodes
            GameObject headNode = new GameObject("HeadNode");
            headNode.transform.SetParent(visual.transform);
            headNode.transform.localPosition = new Vector3(0f, 5.8f, 0f);

            GameObject torsoNode = new GameObject("TorsoNode");
            torsoNode.transform.SetParent(visual.transform);
            torsoNode.transform.localPosition = new Vector3(0f, 3.5f, 0f);

            GameObject backTubesNode = new GameObject("BackTubesNode");
            backTubesNode.transform.SetParent(visual.transform);
            backTubesNode.transform.localPosition = new Vector3(0f, 3.8f, 0f);

            // 4. Aeron Idle Controller
            AeronIdleController idleController = visual.AddComponent<AeronIdleController>();
            SetSerializedField(idleController, "animator", anim);
            SetSerializedField(idleController, "spriteRenderer", sr);
            SetSerializedField(idleController, "snapToGround", false);
            SetSerializedField(idleController, "headNode", headNode.transform);
            SetSerializedField(idleController, "torsoNode", torsoNode.transform);
            SetSerializedField(idleController, "backTubesNode", backTubesNode.transform);

            // 5. Billboard Alignment to Third-Person Camera
            AeronBillboard billboard = visual.AddComponent<AeronBillboard>();

            // 6. Eye glow for Ascension Surge
            GameObject eyeLight = new GameObject("EyeGlowLight");
            eyeLight.transform.SetParent(visual.transform);
            eyeLight.transform.localPosition = new Vector3(0f, 5.8f, 0.5f);
            Light eLight = eyeLight.AddComponent<Light>();
            eLight.type = LightType.Point;
            eLight.color = new Color(0.2f, 0.9f, 1.0f);
            eLight.intensity = 2.0f;
            eLight.range = 2.5f;
            eLight.enabled = false;
            SetSerializedField(surge, "eyeGlowLight", eLight);

            // 7. Third-Person Player Animator Link
            var pAnim = visual.AddComponent<ThirdPersonPlayerAnimator>();
            SetSerializedField(pAnim, "torso", torsoNode.transform);
            SetSerializedField(pAnim, "head", headNode.transform);
            SetSerializedField(pAnim, "characterRenderer", sr);

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

        private static GameObject CreateGuardEnemy(string name, Vector3 position, AudioClip gunshotClip)
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
            SetSerializedField(weapon, "gunshotSFX", gunshotClip);

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

        private static ParticleSystem CreateBubblesParticleSystem(Transform parent)
        {
            GameObject pObj = new GameObject("Bubbles_VFX");
            pObj.transform.SetParent(parent);
            pObj.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            ParticleSystem ps = pObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0.3f, 0.85f, 1f, 0.5f);
            main.startSize = 0.08f;
            main.startSpeed = 0.6f;
            main.startLifetime = 2.0f;
            main.maxParticles = 80;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.8f;
            var emission = ps.emission;
            emission.rateOverTime = 20f;
            return ps;
        }

        private static ParticleSystem CreateBurstParticleSystem(Transform parent, string name, Color color, int count)
        {
            GameObject pObj = new GameObject(name);
            pObj.transform.SetParent(parent);
            pObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            ParticleSystem ps = pObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startSize = 0.15f;
            main.startSpeed = 4.0f;
            main.startLifetime = 1.0f;
            main.loop = false;
            main.playOnAwake = false;
            main.maxParticles = count;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, count) });
            return ps;
        }

        private static void SetSerializedField(Object target, string fieldName, object value)
        {
            if (target == null) return;
            var serializedObj = new SerializedObject(target);
            var prop = serializedObj.FindProperty(fieldName);
            if (prop != null)
            {
                if (value is Object uObj)
                {
                    prop.objectReferenceValue = uObj;
                }
                else if (value is System.Collections.IList list)
                {
                    prop.arraySize = list.Count;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var elementProp = prop.GetArrayElementAtIndex(i);
                        if (list[i] is Object elemObj)
                        {
                            elementProp.objectReferenceValue = elemObj;
                        }
                    }
                }
                else if (value is float f) prop.floatValue = f;
                else if (value is int num) prop.intValue = num;
                else if (value is bool b) prop.boolValue = b;
                else if (value is string s) prop.stringValue = s;

                serializedObj.ApplyModifiedPropertiesWithoutUndo();
            }

            // Always assign directly via reflection as well to ensure runtime consistency
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                try { field.SetValue(target, value); } catch {}
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
            mat.color = new Color(0.18f, 0.65f, 0.9f, 0.25f);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.95f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.1f);
            return mat;
        }

        private static Material CreateLiquidMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.color = new Color(0.08f, 0.75f, 0.92f, 0.45f);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", new Color(0.05f, 0.45f, 0.65f) * 0.8f);
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
