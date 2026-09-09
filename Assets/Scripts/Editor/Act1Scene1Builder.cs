using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;
using LastGod.Core;
using LastGod.Player;
using LastGod.Enemies;
using LastGod.Combat;

namespace LastGod.Editor
{
    /// <summary>
    /// Programmatically generates and updates Act1_Scene1.unity with flickering lab background,
    /// solid catwalk ground geometry, Aeron player setup, 2 guard enemies, and master sequence director.
    /// </summary>
    [InitializeOnLoad]
    public static class Act1Scene1Builder
    {
        private const string ScenePath = "Assets/Scenes/Act1_Scene1.unity";
        private const string Frame1Path = "Assets/Art/Backgrounds/frame-1.png";
        private const string Frame2Path = "Assets/Art/Backgrounds/frame-2.png";

        static Act1Scene1Builder()
        {
            EditorApplication.delayCall += AutoBuildIfNeeded;
        }

        private static void AutoBuildIfNeeded()
        {
            string flagPath = "Assets/Scripts/Editor/.rebuild_act1_scene1";
            if (!File.Exists(ScenePath) || File.Exists(flagPath))
            {
                if (File.Exists(flagPath))
                {
                    try { File.Delete(flagPath); } catch { }
                }
                Debug.Log("[Act1Scene1Builder] Rebuild requested. Generating updated Act1_Scene1.unity...");
                BuildAct1Scene1();
            }
        }

        [MenuItem("Tools/LAST-GOD/Build Act 1 Scene 1")]
        public static void BuildAct1Scene1()
        {
            Debug.Log("[Act1Scene1Builder] === Assembling Act 1 Scene 1 (Lab Background, Catwalk, Aeron & Guards) ===");

            // 1. Create a new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Setup Layer Indices
            int groundLayer = 8;
            int playerLayer = 9;

            // -------------------------------------------------------------
            // 3. BACKGROUND & AMBIENT FLICKER
            // -------------------------------------------------------------
            // Solid Dark Opaque Backdrop (Bedrock at Z = 10, completely solid slate charcoal)
            GameObject backdropObj = new GameObject("Backdrop_Solid");
            backdropObj.transform.position = new Vector3(0f, 0f, 10f);
            SpriteRenderer backdropSR = backdropObj.AddComponent<SpriteRenderer>();
            backdropSR.sortingLayerName = "Background";
            backdropSR.sortingOrder = -100;
            backdropSR.color = new Color(0.04f, 0.05f, 0.08f, 1f);
            Sprite solidSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Backdrop_Solid_Dark.png");
            if (solidSprite != null) backdropSR.sprite = solidSprite;
            backdropSR.drawMode = SpriteDrawMode.Sliced;
            backdropSR.size = new Vector2(250f, 120f);

            // Lab Interior Ambient Flicker Background (100% Opaque painterly laboratory)
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.position = Vector3.zero;

            SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
            bgRenderer.sortingLayerName = "Background";
            bgRenderer.sortingOrder = 0;
            bgRenderer.color = Color.white;

            Sprite f1 = AssetDatabase.LoadAssetAtPath<Sprite>(Frame1Path);
            Sprite f2 = AssetDatabase.LoadAssetAtPath<Sprite>(Frame2Path);
            if (f1 != null) bgRenderer.sprite = f1;

            // AmbientBackgroundFlicker superseded by 2.5D pivot and removed

            // -------------------------------------------------------------
            // 4. LIGHTING
            // -------------------------------------------------------------
            GameObject globalLightObj = new GameObject("Global Light 2D");
            Light2D globalLight = globalLightObj.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            globalLight.intensity = 1.0f;
            globalLight.color = Color.white;

            // -------------------------------------------------------------
            // 5. CATWALK GROUND & COLLIDERS
            // -------------------------------------------------------------
            GameObject groundRoot = new GameObject("Ground");
            groundRoot.layer = groundLayer;
            groundRoot.transform.position = Vector3.zero;

            Rigidbody2D groundRb = groundRoot.AddComponent<Rigidbody2D>();
            groundRb.bodyType = RigidbodyType2D.Static;

            // Main Catwalk Deck Collider (top edge at visual walkway floor Y = -17.10)
            BoxCollider2D groundBox = groundRoot.AddComponent<BoxCollider2D>();
            groundBox.offset = new Vector2(0f, -19.35f);
            groundBox.size = new Vector2(131.0f, 4.5f); // Top edge at Y = -17.10

            // Raised Computer Console Platform
            GameObject consoleObj = new GameObject("Collider_ComputerConsole");
            consoleObj.layer = groundLayer;
            consoleObj.transform.SetParent(groundRoot.transform, false);
            consoleObj.transform.position = new Vector3(-0.5f, -15.065f, 0f);
            BoxCollider2D consoleBox = consoleObj.AddComponent<BoxCollider2D>();
            consoleBox.size = new Vector2(5.5f, 4.07f);

            // Left Wall Barrier (x = -65.5)
            GameObject leftWall = new GameObject("Collider_LeftWall");
            leftWall.layer = groundLayer;
            leftWall.transform.SetParent(groundRoot.transform, false);
            leftWall.transform.position = new Vector3(-65.5f, 0f, 0f);
            BoxCollider2D leftWallBox = leftWall.AddComponent<BoxCollider2D>();
            leftWallBox.size = new Vector2(2f, 45f);

            // Right Wall Barrier (x = +65.5)
            GameObject rightWall = new GameObject("Collider_RightWall");
            rightWall.layer = groundLayer;
            rightWall.transform.SetParent(groundRoot.transform, false);
            rightWall.transform.position = new Vector3(65.5f, 0f, 0f);
            BoxCollider2D rightWallBox = rightWall.AddComponent<BoxCollider2D>();
            rightWallBox.size = new Vector2(2f, 45f);

            // Ceiling Barrier (Y = 22.0)
            GameObject ceiling = new GameObject("Collider_Ceiling");
            ceiling.layer = groundLayer;
            ceiling.transform.SetParent(groundRoot.transform, false);
            ceiling.transform.position = new Vector3(0f, 22.0f, 0f);
            BoxCollider2D ceilingBox = ceiling.AddComponent<BoxCollider2D>();
            ceilingBox.size = new Vector2(131.0f, 4f);

            // Heavy Lab BGM AudioSource
            GameObject bgmObj = new GameObject("BGM_Player");
            bgmObj.layer = groundLayer;
            bgmObj.transform.SetParent(groundRoot.transform, false);
            bgmObj.transform.position = new Vector3(0f, -15f, 0f);
            AudioSource bgmSource = bgmObj.AddComponent<AudioSource>();
            AudioClip bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/lab_bg_music.wav");
            if (bgmClip != null) bgmSource.clip = bgmClip;
            bgmSource.playOnAwake = true;
            bgmSource.loop = true;
            bgmSource.volume = 0.75f;

            // -------------------------------------------------------------
            // 5b. GLASS CHAMBER (STASIS POD AT PILLAR B-3)
            // -------------------------------------------------------------
            GameObject chamberObj = new GameObject("Glass_Chamber");
            chamberObj.transform.position = new Vector3(-34.0f, -15.60f, 0f);

            SpriteRenderer chamberSR = chamberObj.AddComponent<SpriteRenderer>();
            chamberSR.sortingLayerName = "Default";
            chamberSR.sortingOrder = 5;
            Sprite chamberSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Chamber_Intact.png") ??
                                  AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Glass_Chamber.png");
            if (chamberSprite != null) chamberSR.sprite = chamberSprite;

            BoxCollider2D chamberCol = chamberObj.AddComponent<BoxCollider2D>();
            chamberCol.size = new Vector2(2.2f, 2.7f);
            chamberCol.offset = new Vector2(0f, 0.125f);
            chamberCol.isTrigger = false;

            GameObject stasisLightObj = new GameObject("Chamber_StasisLight");
            stasisLightObj.transform.SetParent(chamberObj.transform, false);
            stasisLightObj.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            Light2D stasisLight = stasisLightObj.AddComponent<Light2D>();
            stasisLight.lightType = Light2D.LightType.Point;
            stasisLight.color = new Color(0.25f, 0.85f, 1.0f, 1.0f);
            stasisLight.intensity = 1.3f;
            stasisLight.pointLightInnerRadius = 0.4f;
            stasisLight.pointLightOuterRadius = 3.2f;

            // -------------------------------------------------------------
            // 6. PLAYER (AERON)
            // -------------------------------------------------------------
            GameObject playerObj = new GameObject("Player");
            playerObj.tag = "Player";
            playerObj.layer = playerLayer;
            // Spawn standing cleanly ON the walkway deck floor at Y = -17.10 near chamber
            playerObj.transform.position = new Vector3(-31.5f, -17.10f, 0f);

            SpriteRenderer playerSR = playerObj.AddComponent<SpriteRenderer>();
            playerSR.sortingLayerName = "Default";
            playerSR.sortingOrder = 10;
            Sprite playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Characters/Aeron/Sprites/Idle/Aeron_Idle_01.png");
            if (playerSprite != null) playerSR.sprite = playerSprite;

            Rigidbody2D playerRb = playerObj.AddComponent<Rigidbody2D>();
            playerRb.bodyType = RigidbodyType2D.Dynamic;
            playerRb.mass = 1.0f;
            playerRb.gravityScale = 3.0f;
            playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;

            CapsuleCollider2D playerCol = playerObj.AddComponent<CapsuleCollider2D>();
            playerCol.size = new Vector2(1.8f, 5.5f);
            playerCol.offset = new Vector2(0f, 2.75f);

            Animator playerAnim = playerObj.AddComponent<Animator>();
            RuntimeAnimatorController aCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Characters/Aeron/Animator/Aeron.controller");
            if (aCtrl != null) playerAnim.runtimeAnimatorController = aCtrl;

            LastGod.Player.PlayerController pCtrl = playerObj.AddComponent<LastGod.Player.PlayerController>();
            pCtrl.LockToWalkAndJumpOnly = true;

            Health pHealth = playerObj.AddComponent<Health>();
            SerializedObject pHealthSO = new SerializedObject(pHealth);
            var maxHealthProp = pHealthSO.FindProperty("maxHP") ?? pHealthSO.FindProperty("maxHealth");
            if (maxHealthProp != null) maxHealthProp.intValue = 20;
            pHealthSO.ApplyModifiedProperties();

            playerObj.AddComponent<PrototypePowerController>();

            // -------------------------------------------------------------
            // 7. CAMERA SETUP
            // -------------------------------------------------------------
            GameObject mainCamObj = new GameObject("Main Camera");
            mainCamObj.tag = "MainCamera";
            mainCamObj.transform.position = new Vector3(-31.5f, -14.3f, -10f);

            Camera cam = mainCamObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7.5f;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.08f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;

            UniversalAdditionalCameraData camData = mainCamObj.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;

            CameraFollow camFollow = mainCamObj.AddComponent<CameraFollow>();
            SerializedObject camSO = new SerializedObject(camFollow);
            camSO.FindProperty("target").objectReferenceValue = playerObj.transform;
            camSO.FindProperty("offsetY").floatValue = 2.8f;
            camSO.FindProperty("cameraZ").floatValue = -10f;
            camSO.ApplyModifiedProperties();

            mainCamObj.AddComponent<LastGod.Core.CameraShake2D>();
            mainCamObj.AddComponent<AudioListener>();

            // -------------------------------------------------------------
            // 8. CUTSCENE UI CANVAS & OVERLAYS
            // -------------------------------------------------------------
            GameObject canvasObj = new GameObject("CutsceneCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Fullscreen Black Overlay
            GameObject blackObj = new GameObject("BlackOverlay");
            blackObj.transform.SetParent(canvasObj.transform, false);
            RectTransform blackRect = blackObj.AddComponent<RectTransform>();
            blackRect.anchorMin = Vector2.zero;
            blackRect.anchorMax = Vector2.one;
            blackRect.sizeDelta = Vector2.zero;
            Image blackImg = blackObj.AddComponent<Image>();
            blackImg.color = new Color(0f, 0f, 0f, 1f); // Pure black cold open

            // Red Emergency Alarm Overlay
            GameObject redObj = new GameObject("RedAlarmOverlay");
            redObj.transform.SetParent(canvasObj.transform, false);
            RectTransform redRect = redObj.AddComponent<RectTransform>();
            redRect.anchorMin = Vector2.zero;
            redRect.anchorMax = Vector2.one;
            redRect.sizeDelta = Vector2.zero;
            Image redImg = redObj.AddComponent<Image>();
            redImg.color = new Color(1f, 0f, 0f, 0f);

            // Typewriter Dialogue Text
            GameObject textObj = new GameObject("DialogueText");
            textObj.transform.SetParent(canvasObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.35f);
            textRect.anchorMax = new Vector2(0.9f, 0.65f);
            textRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontSize = 28;
            tmpText.color = Color.white;
            tmpText.text = "";

            CutsceneUIController uiCtrl = canvasObj.AddComponent<CutsceneUIController>();
            SerializedObject uiSO = new SerializedObject(uiCtrl);
            uiSO.FindProperty("blackOverlay").objectReferenceValue = blackImg;
            uiSO.FindProperty("redAlarmOverlay").objectReferenceValue = redImg;
            uiSO.FindProperty("dialogueText").objectReferenceValue = tmpText;
            uiSO.ApplyModifiedProperties();

            // -------------------------------------------------------------
            // 9. CYBER GUARDS (TACTICAL ENCOUNTER)
            // -------------------------------------------------------------
            GameObject guard1 = CreateGuard("Guard_01", new Vector3(-14.0f, -17.10f, 0f), playerObj.transform);
            GameObject guard2 = CreateGuard("Guard_02", new Vector3(-6.0f, -17.10f, 0f), playerObj.transform);

            // -------------------------------------------------------------
            // 10. MASTER SEQUENCE DIRECTOR
            // -------------------------------------------------------------
            GameObject seqObj = new GameObject("Act1Scene1SequenceManager");
            Act1Scene1SequenceManager seqMgr = seqObj.AddComponent<Act1Scene1SequenceManager>();

            AudioSource humSource = seqObj.AddComponent<AudioSource>();
            AudioClip humClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/machine_hum.wav");
            if (humClip != null) humSource.clip = humClip;
            humSource.loop = true;
            humSource.playOnAwake = false;

            AudioSource sfxSource = seqObj.AddComponent<AudioSource>();

            SerializedObject seqSO = new SerializedObject(seqMgr);
            seqSO.FindProperty("ambientHumSource").objectReferenceValue = humSource;
            seqSO.FindProperty("sfxSource").objectReferenceValue = sfxSource;
            seqSO.FindProperty("heartbeatSFX").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/heartbeat.wav");
            seqSO.FindProperty("glassShatterSFX").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/glass_shatter.wav");
            seqSO.FindProperty("alarmSFX").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/alarm.wav");
            seqSO.FindProperty("chamberRenderer").objectReferenceValue = chamberSR;
            seqSO.FindProperty("crackedChamberSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Chamber_Cracked.png");
            seqSO.FindProperty("shatteredChamberSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Chamber_Shattered.png");
            seqSO.FindProperty("cameraFollow").objectReferenceValue = camFollow;
            seqSO.FindProperty("uiController").objectReferenceValue = uiCtrl;
            seqSO.FindProperty("skipIntroSequence").boolValue = false;
            seqSO.ApplyModifiedProperties();

            // Save Scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Act1Scene1Builder] Successfully generated and saved scene at: {ScenePath}");
        }

        private static GameObject CreateGuard(string name, Vector3 position, Transform targetPlayer)
        {
            GameObject guardObj = new GameObject(name);
            guardObj.tag = "Enemy";
            guardObj.transform.position = position;

            SpriteRenderer sr = guardObj.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 9;
            Sprite guardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Guard_Spritesheet.png");
            if (guardSprite != null) sr.sprite = guardSprite;

            Rigidbody2D rb = guardObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3.0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CapsuleCollider2D col = guardObj.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(1.8f, 5.0f);
            col.offset = new Vector2(0f, 2.5f);

            Health hp = guardObj.AddComponent<Health>();
            SerializedObject hpSO = new SerializedObject(hp);
            var guardHealthProp = hpSO.FindProperty("maxHP") ?? hpSO.FindProperty("maxHealth");
            if (guardHealthProp != null) guardHealthProp.intValue = 6;
            hpSO.ApplyModifiedProperties();

            guardObj.AddComponent<AudioSource>();

            EnemyAI ai = guardObj.AddComponent<EnemyAI>();
            SerializedObject aiSO = new SerializedObject(ai);
            aiSO.FindProperty("moveSpeed").floatValue = 3.2f;
            aiSO.FindProperty("attackRange").floatValue = 7.0f;
            aiSO.FindProperty("stopDistance").floatValue = 2.5f;
            aiSO.FindProperty("attackCooldown").floatValue = 1.6f;
            aiSO.FindProperty("spriteRenderer").objectReferenceValue = sr;
            aiSO.FindProperty("deadSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Guard_Dead.png");
            aiSO.FindProperty("gunshotSFX").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/gunshot.wav");
            aiSO.ApplyModifiedProperties();

            ai.SetTarget(targetPlayer);
            return guardObj;
        }
    }
}
