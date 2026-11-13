using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using LastGod.Act1;

namespace LastGod.Editor
{
    public static class BuildAct1Scene
    {
        [MenuItem("The Last God/Build Act 1 Scene")]
        public static void Build()
        {
            Debug.Log("[BuildAct1Scene] Starting generation of Act 1 Lab Origin scene...");

            // Ensure directories exist
            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Act1")) AssetDatabase.CreateFolder("Assets/Prefabs", "Act1");

            // Setup Tags and Layers
            SetupTagsAndLayers();

            // Create new empty scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera (Orthographic 180p height = 11.25 units at 16 PPU => size = 5.625)
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.625f;
            cam.backgroundColor = new Color(0.043f, 0.051f, 0.078f, 1f); // #0b0d14
            cam.clearFlags = CameraClearFlags.SolidColor;
            camObj.AddComponent<AudioListener>();
            var camFollow = camObj.AddComponent<CameraFollow2D>();
            camFollow.minBounds = new Vector2(-6f, -1.0f);
            camFollow.maxBounds = new Vector2(6f, 1.0f);
            camFollow.offset = new Vector2(0f, 0.5f);
            camObj.transform.position = new Vector3(-4.5f, 0f, -10f);

            // 2. Load core sprite assets
            Sprite bgFarSprite = LoadSprite("Assets/Art/Act1/environment/bg_far.png");
            Sprite bgMidSprite = LoadSprite("Assets/Art/Act1/environment/bg_mid.png");
            Sprite labBgSprite = LoadSprite("Assets/Art/Act1/scenes/lab_background.png");

            Sprite chamberIntactSprite = LoadSprite("Assets/Art/Act1/sprites/chamber_intact.png");
            Sprite chamberCrackedSprite = LoadSprite("Assets/Art/Act1/sprites/chamber_cracked.png");
            Sprite chamberShatteredSprite = LoadSprite("Assets/Art/Act1/sprites/chamber_shattered.png");

            Sprite aeronSprite = LoadSprite("Assets/Characters/Aeron/Sprites/Idle/aeron_idle_0.png") ?? LoadSprite("Assets/Art/Act1/sprites/aeron_idle.png");
            Sprite guardSprite = LoadSprite("Assets/Art/Act1/sprites/guard_idle.png");
            Sprite eliteSprite = LoadSprite("Assets/Art/Act1/sprites/guard_elite_idle.png");
            Sprite bulletSprite = LoadSprite("Assets/Art/Act1/sprites/bullet_normal.png");
            Sprite bulletSlowedSprite = LoadSprite("Assets/Art/Act1/sprites/bullet_slowed.png");
            Sprite slashSprite = LoadSprite("Assets/Art/Act1/sprites/slash_0.png");

            Sprite consoleSprite = LoadSprite("Assets/Art/Act1/props/console_desk.png");
            Sprite serverSprite = LoadSprite("Assets/Art/Act1/props/server_rack.png");
            Sprite ventSprite = LoadSprite("Assets/Art/Act1/props/steam_vent.png");
            Sprite signSprite = LoadSprite("Assets/Art/Act1/props/warning_sign.png");

            Sprite dialogBoxSprite = LoadSprite("Assets/Art/Act1/ui/dialog_box.png");
            Sprite portraitAeron = LoadSprite("Assets/Art/Act1/sprites/portrait_aeron.png");
            Sprite portraitVoss = LoadSprite("Assets/Art/Act1/sprites/portrait_voss.png");
            Sprite portraitVoice = LoadSprite("Assets/Art/Act1/sprites/portrait_voice.png");
            Sprite portraitGuard = LoadSprite("Assets/Art/Act1/sprites/portrait_guard.png");

            // Ensure shared unlit sprite material for pixel-perfect palette fidelity
            if (!AssetDatabase.IsValidFolder("Assets/Art/Act1/Materials"))
            {
                AssetDatabase.CreateFolder("Assets/Art/Act1", "Materials");
            }
            string unlitMatPath = "Assets/Art/Act1/Materials/Sprite_Unlit.mat";
            Material unlitMat = AssetDatabase.LoadAssetAtPath<Material>(unlitMatPath);
            if (unlitMat == null)
            {
                Shader s = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (s == null) s = Shader.Find("Sprites/Default");
                unlitMat = new Material(s);
                AssetDatabase.CreateAsset(unlitMat, unlitMatPath);
                AssetDatabase.SaveAssets();
            }

            // 3. Environment Hierarchy
            var envRoot = new GameObject("[Environment]");

            // Far Background (Parallax 0.2)
            var bgFarObj = new GameObject("BG_Far_Skyline");
            bgFarObj.transform.parent = envRoot.transform;
            bgFarObj.transform.position = new Vector3(0f, 0.8f, 5f);
            var srFar = bgFarObj.AddComponent<SpriteRenderer>();
            srFar.sprite = bgFarSprite;
            srFar.sharedMaterial = unlitMat;
            srFar.sortingOrder = -20;
            camFollow.farBackground = bgFarObj.transform;

            // Mid Background (Server Racks, Pipes)
            var bgMidObj = new GameObject("BG_Mid_Racks");
            bgMidObj.transform.parent = envRoot.transform;
            bgMidObj.transform.position = new Vector3(0f, 0.5f, 3f);
            var srMid = bgMidObj.AddComponent<SpriteRenderer>();
            srMid.sprite = bgMidSprite;
            srMid.sharedMaterial = unlitMat;
            srMid.sortingOrder = -10;
            camFollow.midBackground = bgMidObj.transform;

            // Foreground Lab Structure
            var labFgObj = new GameObject("Lab_Architecture");
            labFgObj.transform.parent = envRoot.transform;
            labFgObj.transform.position = new Vector3(0f, 0f, 0f);
            var srLab = labFgObj.AddComponent<SpriteRenderer>();
            srLab.sprite = labBgSprite;
            srLab.sharedMaterial = unlitMat;
            srLab.sortingOrder = -2;

            // Lab Floor Colliders (solid platform: top edge at y = -2.75)
            var floorCol = new GameObject("Floor_Collider");
            floorCol.transform.parent = envRoot.transform;
            floorCol.transform.position = new Vector3(0f, -3.25f, 0f);
            floorCol.tag = "Environment";
            var bcFloor = floorCol.AddComponent<BoxCollider2D>();
            bcFloor.size = new Vector2(30f, 1.0f);

            // Left & Right boundary walls
            var wallLeft = new GameObject("Wall_Left");
            wallLeft.transform.parent = envRoot.transform;
            wallLeft.transform.position = new Vector3(-10.5f, 0f, 0f);
            var bcWallL = wallLeft.AddComponent<BoxCollider2D>();
            bcWallL.size = new Vector2(1f, 12f);

            var wallRight = new GameObject("Wall_Right");
            wallRight.transform.parent = envRoot.transform;
            wallRight.transform.position = new Vector3(10.5f, 0f, 0f);
            var bcWallR = wallRight.AddComponent<BoxCollider2D>();
            bcWallR.size = new Vector2(1f, 12f);

            // Props (grounded at floor y = -2.75)
            CreateProp("Console_01", consoleSprite, new Vector3(-1.5f, -2.06f, 0f), envRoot.transform, 1, unlitMat);
            CreateProp("Server_01", serverSprite, new Vector3(2.0f, -1.38f, 0f), envRoot.transform, 1, unlitMat);
            CreateProp("Server_02", serverSprite, new Vector3(3.2f, -1.38f, 0f), envRoot.transform, 1, unlitMat);
            CreateProp("Vent_01", ventSprite, new Vector3(6.5f, -2.0f, 0f), envRoot.transform, 1, unlitMat);
            CreateProp("Sign_01", signSprite, new Vector3(5.0f, 0.8f, 0f), envRoot.transform, 1, unlitMat);

            // Elevated catwalks & platforms for Jumping & Pogo combat
            var platformLow = new GameObject("Platform_Catwalk_Mid");
            platformLow.transform.parent = envRoot.transform;
            platformLow.transform.position = new Vector3(-0.5f, -0.6f, 0f);
            var colPlat1 = platformLow.AddComponent<BoxCollider2D>();
            colPlat1.size = new Vector2(3.6f, 0.25f);
            CreateProp("Platform_Grating_Mid", consoleSprite, new Vector3(-0.5f, -0.6f, 0f), platformLow.transform, 2, unlitMat);

            var platformHigh = new GameObject("Platform_Catwalk_High");
            platformHigh.transform.parent = envRoot.transform;
            platformHigh.transform.position = new Vector3(3.5f, 0.9f, 0f);
            var colPlat2 = platformHigh.AddComponent<BoxCollider2D>();
            colPlat2.size = new Vector2(3.8f, 0.25f);
            CreateProp("Platform_Grating_High", consoleSprite, new Vector3(3.5f, 0.9f, 0f), platformHigh.transform, 2, unlitMat);

            // Stasis Chamber (Centerpiece)
            var chamberRoot = new GameObject("Stasis_Chamber_System");
            chamberRoot.transform.parent = envRoot.transform;
            chamberRoot.transform.position = new Vector3(-5.0f, -0.25f, 0f);

            var chIntact = CreateProp("Chamber_Intact", chamberIntactSprite, Vector3.zero, chamberRoot.transform, 3, unlitMat);
            var chCracked = CreateProp("Chamber_Cracked", chamberCrackedSprite, Vector3.zero, chamberRoot.transform, 3, unlitMat);
            chCracked.SetActive(false);
            var chShattered = CreateProp("Chamber_Shattered", chamberShatteredSprite, Vector3.zero, chamberRoot.transform, 3, unlitMat);
            chShattered.SetActive(false);

            // 4. Create Slash & Bullet Prefabs
            GameObject slashPrefab = CreateSlashEffectPrefab(slashSprite, unlitMat);
            GameObject bulletPrefab = CreateBulletPrefab(bulletSprite, bulletSlowedSprite, unlitMat);

            // 5. Create Guard Prefabs
            GameObject standardGuardPrefab = CreateGuardPrefab("StandardGuardPrefab", guardSprite, bulletPrefab, GuardAI.GuardType.Standard, unlitMat);
            GameObject eliteGuardPrefab = CreateGuardPrefab("EliteGuardPrefab", eliteSprite, bulletPrefab, GuardAI.GuardType.Elite, unlitMat);

            // 6. Create Player: Aeron
            var aeronObj = new GameObject("Aeron");
            aeronObj.tag = "Player";
            aeronObj.layer = LayerMask.NameToLayer("Player") != -1 ? LayerMask.NameToLayer("Player") : 0;
            aeronObj.transform.position = new Vector3(-5.0f, -0.75f, 0f); // inside chamber initially

            var srAeron = aeronObj.AddComponent<SpriteRenderer>();
            srAeron.sprite = aeronSprite;
            srAeron.sharedMaterial = unlitMat;
            srAeron.sortingOrder = 2; // inside/behind chamber glass

            var animAeron = aeronObj.AddComponent<Animator>();
            var aeronAnimCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Characters/Aeron/Animator/Aeron.controller");
            if (aeronAnimCtrl != null)
            {
                animAeron.runtimeAnimatorController = aeronAnimCtrl;
            }

            var rbAeron = aeronObj.AddComponent<Rigidbody2D>();
            rbAeron.freezeRotation = true;
            rbAeron.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var colAeron = aeronObj.AddComponent<BoxCollider2D>();
            colAeron.size = new Vector2(0.6f, 1.4f);
            colAeron.offset = new Vector2(0f, 0.7f);

            var aeronCtrl = aeronObj.AddComponent<AeronController>();
            aeronCtrl.animator = animAeron;
            aeronCtrl.slashEffectPrefab = slashPrefab;
            aeronCtrl.bulletLayer = 1 << LayerMask.NameToLayer("Default");
            aeronCtrl.enemyLayer = 1 << LayerMask.NameToLayer("Default");
            aeronCtrl.groundLayer = 1 << LayerMask.NameToLayer("Default");

            // GroundCheck point for jump & pogo
            var groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.parent = aeronObj.transform;
            groundCheckObj.transform.localPosition = Vector3.zero;
            aeronCtrl.groundCheck = groundCheckObj.transform;

            // Attack point
            var atkPointObj = new GameObject("AttackPoint");
            atkPointObj.transform.parent = aeronObj.transform;
            atkPointObj.transform.localPosition = new Vector3(0.8f, 0.7f, 0f);
            aeronCtrl.attackPoint = atkPointObj.transform;

            camFollow.target = aeronObj.transform;

            // 7. Lighting (URP 2D Light2D)
            var lightsRoot = new GameObject("[Lighting]");
            
            var ambientLight = new GameObject("Global_Lab_Light_2D");
            ambientLight.transform.parent = lightsRoot.transform;
            var gLight = ambientLight.AddComponent<Light2D>();
            gLight.lightType = Light2D.LightType.Global;
            gLight.color = new Color(0.75f, 0.88f, 0.95f);
            gLight.intensity = 0.85f;

            var chamberLight = new GameObject("Chamber_Glow_Light_2D");
            chamberLight.transform.parent = lightsRoot.transform;
            chamberLight.transform.position = new Vector3(-5.0f, -0.25f, 0f);
            var chLight = chamberLight.AddComponent<Light2D>();
            chLight.lightType = Light2D.LightType.Point;
            chLight.color = new Color(0.2f, 0.95f, 0.8f);
            chLight.pointLightOuterRadius = 4.5f;
            chLight.intensity = 1.6f;

            var alarmGroup = new GameObject("Alarm_Lights_Group");
            alarmGroup.transform.parent = lightsRoot.transform;
            alarmGroup.transform.position = new Vector3(-5.0f, 2.0f, 0f);
            var alarmLight = alarmGroup.AddComponent<Light2D>();
            alarmLight.lightType = Light2D.LightType.Point;
            alarmLight.color = new Color(1f, 0.15f, 0.25f);
            alarmLight.pointLightOuterRadius = 7.0f;
            alarmLight.intensity = 2.0f;
            alarmLight.enabled = false;

            // 8. UI Canvas & HUD
            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            var uiMgr = canvasObj.AddComponent<UIManager>();
            BuildUIHierarchy(canvasObj, uiMgr, dialogBoxSprite, portraitAeron, portraitVoss, portraitVoice, portraitGuard);

            // 9. Director and Game Manager
            var gmObj = new GameObject("GameManager");
            var gm = gmObj.AddComponent<GameManager>();
            gm.standardGuardPrefab = standardGuardPrefab;
            gm.eliteGuardPrefab = eliteGuardPrefab;

            var dirObj = new GameObject("Act1LabDirector");
            var director = dirObj.AddComponent<Act1LabDirector>();
            director.aeron = aeronCtrl;
            director.stasisChamberIntact = chIntact;
            director.stasisChamberCracked = chCracked;
            director.stasisChamberShattered = chShattered;
            director.alarmLightsGroup = alarmGroup;
            director.mainLabLight2D = gLight;
            director.alarmLight2D = alarmLight;

            // Spawn markers
            var spRight = new GameObject("Spawn_Right_01");
            spRight.transform.position = new Vector3(8.5f, -2.0f, 0f);
            var spLeft = new GameObject("Spawn_Left_01");
            spLeft.transform.position = new Vector3(-8.5f, -2.0f, 0f);
            gm.spawnPointsRight = new Transform[] { spRight.transform };
            gm.spawnPointsLeft = new Transform[] { spLeft.transform };

            // Save Scene
            string scenePath = "Assets/Scenes/Act1_Lab_Origin.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[BuildAct1Scene] Successfully created and saved Act 1 scene at: {scenePath}");
        }

        private static GameObject CreateProp(string name, Sprite sprite, Vector3 localPos, Transform parent, int sortOrder, Material mat = null)
        {
            var obj = new GameObject(name);
            obj.transform.parent = parent;
            obj.transform.localPosition = localPos;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortOrder;
            if (mat != null) sr.sharedMaterial = mat;
            return obj;
        }

        private static GameObject CreateSlashEffectPrefab(Sprite slashSprite, Material mat = null)
        {
            var obj = new GameObject("SlashEffect");
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = slashSprite;
            sr.sortingOrder = 10;
            if (mat != null) sr.sharedMaterial = mat;
            string path = "Assets/Prefabs/Act1/SlashEffect.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateBulletPrefab(Sprite bulletSprite, Sprite bulletSlowedSprite, Material mat = null)
        {
            var obj = new GameObject("Bullet");
            obj.tag = "Bullet";
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = bulletSprite;
            sr.sortingOrder = 8;
            if (mat != null) sr.sharedMaterial = mat;

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.5f, 0.2f);

            var b = obj.AddComponent<Bullet>();
            b.normalSprite = bulletSprite;
            b.slowedSprite = bulletSlowedSprite;

            string path = "Assets/Prefabs/Act1/Bullet.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateGuardPrefab(string name, Sprite guardSprite, GameObject bulletPrefab, GuardAI.GuardType type, Material mat = null)
        {
            var obj = new GameObject(name);
            obj.tag = "Enemy";
            obj.layer = LayerMask.NameToLayer("Enemy") != -1 ? LayerMask.NameToLayer("Enemy") : 0;

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = guardSprite;
            sr.sortingOrder = 4;
            if (mat != null) sr.sharedMaterial = mat;

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;

            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.6f, 1.4f);
            col.offset = new Vector2(0f, 0.7f);

            var ai = obj.AddComponent<GuardAI>();
            ai.guardType = type;
            ai.bulletPrefab = bulletPrefab;
            ai.tacticalIdleSprite = LoadSprite("Assets/Art/Act1/characters/guard_tactical_idle.png");
            ai.tacticalAimSprite = LoadSprite("Assets/Art/Act1/characters/guard_tactical_aim.png");
            ai.tacticalShootSprite = LoadSprite("Assets/Art/Act1/characters/guard_tactical_shoot.png");
            ai.tacticalRecoilSprite = LoadSprite("Assets/Art/Act1/characters/guard_tactical_recoil.png");
            ai.tacticalHurtSprite = LoadSprite("Assets/Art/Act1/characters/guard_tactical_hurt.png");
            ai.tacticalBrokenSprite = LoadSprite("Assets/Art/Act1/characters/guard_tactical_broken.png");
            if (ai.tacticalIdleSprite != null) sr.sprite = ai.tacticalIdleSprite;

            // Gun Muzzle with Blue Tip
            var muzzle = new GameObject("Muzzle");
            muzzle.transform.parent = obj.transform;
            muzzle.transform.localPosition = new Vector3(-0.6f, 0.65f, 0f);
            ai.gunMuzzle = muzzle.transform;

            // LineRenderer for Blue Capacitor Aim Laser
            var lr = obj.AddComponent<LineRenderer>();
            lr.startWidth = 0.04f;
            lr.endWidth = 0.02f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(0.15f, 0.85f, 1f, 0.9f);
            lr.endColor = new Color(0.1f, 0.65f, 1f, 0.2f);
            lr.enabled = false;
            ai.laserAimLine = lr;

            string path = $"Assets/Prefabs/Act1/{name}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static void BuildUIHierarchy(GameObject canvasObj, UIManager ui, Sprite dialogBox, Sprite pAeron, Sprite pVoss, Sprite pVoice, Sprite pGuard)
        {
            // HUD Root
            var hud = new GameObject("HUD_Panel");
            hud.transform.SetParent(canvasObj.transform, false);
            ui.hudPanel = hud;

            // Life bar
            var lifeObj = CreateUIBar(hud, "LifeBar", new Vector2(20, -20), new Color(0.85f, 0.2f, 0.3f));
            ui.lifeBarFill = lifeObj.GetComponent<Image>();

            // Focus bar
            var focusObj = CreateUIBar(hud, "FocusBar", new Vector2(20, -36), new Color(0.45f, 0.95f, 0.85f));
            ui.focusBarFill = focusObj.GetComponent<Image>();

            // Humanity bar
            var humanityObj = CreateUIBar(hud, "HumanityBar", new Vector2(20, -52), new Color(0.8f, 0.9f, 0.95f));
            ui.humanityBarFill = humanityObj.GetComponent<Image>();

            // Humanity text
            var huText = CreateUIText(hud, "HumanityText", "HUMANITY 100%", new Vector2(170, -52), 12, Color.white);
            ui.humanityText = huText.GetComponent<Text>();

            // Wave text
            var waveText = CreateUIText(hud, "WaveText", "WAVE 1/3", new Vector2(Screen.width - 120, -20), 14, Color.cyan);
            ui.waveText = waveText.GetComponent<Text>();

            // Controls Hint
            var hint = CreateUIText(hud, "ControlsHint", "A/D: MOVE  |  W/SPACE: JUMP  |  S+J (AIR): POGO  |  U/RMB: DIVINE LASER  |  J: SLASH  |  K: DASH  |  L: TAKE", new Vector2(Screen.width * 0.5f - 340, -Screen.height + 40), 11, Color.yellow);
            ui.controlsHintText = hint.GetComponent<Text>();

            // Broken prompt
            var brokenPrompt = CreateUIText(hud, "BrokenPrompt", "[L] TAKE  or  [J] CUT DOWN", new Vector2(Screen.width * 0.5f - 100, -Screen.height * 0.5f), 16, Color.green);
            ui.brokenPromptObject = brokenPrompt;

            // Dialogue Panel
            var dlgObj = new GameObject("Dialogue_Panel");
            dlgObj.transform.SetParent(canvasObj.transform, false);
            var rtDlg = dlgObj.AddComponent<RectTransform>();
            rtDlg.anchorMin = new Vector2(0.5f, 0f);
            rtDlg.anchorMax = new Vector2(0.5f, 0f);
            rtDlg.anchoredPosition = new Vector2(0, 80);
            rtDlg.sizeDelta = new Vector2(620, 110);
            var dlgImg = dlgObj.AddComponent<Image>();
            dlgImg.sprite = dialogBox;
            dlgImg.type = Image.Type.Sliced;
            dlgImg.color = new Color(0.9f, 0.95f, 1f, 0.95f);
            ui.dialoguePanel = dlgObj;

            var portObj = new GameObject("Portrait_Image");
            portObj.transform.SetParent(dlgObj.transform, false);
            var rtPort = portObj.AddComponent<RectTransform>();
            rtPort.anchorMin = new Vector2(0f, 0.5f);
            rtPort.anchorMax = new Vector2(0f, 0.5f);
            rtPort.anchoredPosition = new Vector2(50, 0);
            rtPort.sizeDelta = new Vector2(64, 64);
            var imgPort = portObj.AddComponent<Image>();
            imgPort.sprite = pVoice;
            ui.portraitImage = imgPort;
            ui.aeronPortrait = pAeron;
            ui.vossPortrait = pVoss;
            ui.voicePortrait = pVoice;
            ui.guardPortrait = pGuard;

            var spk = CreateUIText(dlgObj, "SpeakerName", "VOICE (V.O.)", new Vector2(95, 75), 14, Color.cyan);
            ui.speakerNameText = spk.GetComponent<Text>();

            var body = CreateUIText(dlgObj, "MessageBody", "...", new Vector2(95, 45), 15, Color.white);
            ui.dialogueBodyText = body.GetComponent<Text>();

            // Results Panel
            var resObj = new GameObject("Results_Panel");
            resObj.transform.SetParent(canvasObj.transform, false);
            var rtRes = resObj.AddComponent<RectTransform>();
            rtRes.anchorMin = Vector2.zero;
            rtRes.anchorMax = Vector2.one;
            var resBg = resObj.AddComponent<Image>();
            resBg.color = new Color(0f, 0f, 0f, 0.96f);
            ui.resultsPanel = resObj;

            var title = CreateUIText(resObj, "Title", "ACT 1 COMPLETE", new Vector2(Screen.width * 0.5f - 110, -60), 24, Color.red);
            var rHu = CreateUIText(resObj, "HumanityResult", "HUMANITY: 100% [STILL HUMAN]", new Vector2(Screen.width * 0.5f - 150, -120), 16, Color.white);
            ui.resultsHumanityText = rHu.GetComponent<Text>();

            var rStats = CreateUIText(resObj, "Stats", "TAKEN: 0  |  CUT DOWN: 0", new Vector2(Screen.width * 0.5f - 160, -180), 14, Color.cyan);
            ui.resultsStatsText = rStats.GetComponent<Text>();

            var rQuote = CreateUIText(resObj, "Quote", "\"THEY WERE A WARNING.\"", new Vector2(Screen.width * 0.5f - 200, -260), 13, Color.gray);
            ui.resultsQuoteText = rQuote.GetComponent<Text>();

            // Game Over Panel
            var goObj = new GameObject("GameOver_Panel");
            goObj.transform.SetParent(canvasObj.transform, false);
            var rtGo = goObj.AddComponent<RectTransform>();
            rtGo.anchorMin = Vector2.zero;
            rtGo.anchorMax = Vector2.one;
            var goBg = goObj.AddComponent<Image>();
            goBg.color = new Color(0.1f, 0f, 0f, 0.9f);
            ui.gameOverPanel = goObj;

            CreateUIText(goObj, "GoText", "NOT YET.\n\nPRESS R TO RISE AGAIN", new Vector2(Screen.width * 0.5f - 120, -Screen.height * 0.5f + 40), 20, Color.red);
        }

        private static GameObject CreateUIBar(GameObject parent, string name, Vector2 pos, Color color)
        {
            var bgObj = new GameObject(name + "_BG");
            bgObj.transform.SetParent(parent.transform, false);
            var rtBg = bgObj.AddComponent<RectTransform>();
            rtBg.anchorMin = new Vector2(0, 1);
            rtBg.anchorMax = new Vector2(0, 1);
            rtBg.anchoredPosition = pos;
            rtBg.sizeDelta = new Vector2(130, 10);
            var imgBg = bgObj.AddComponent<Image>();
            imgBg.color = new Color(0.1f, 0.12f, 0.18f, 0.9f);

            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(bgObj.transform, false);
            var rtFill = fillObj.AddComponent<RectTransform>();
            rtFill.anchorMin = Vector2.zero;
            rtFill.anchorMax = Vector2.one;
            rtFill.sizeDelta = Vector2.zero;
            var imgFill = fillObj.AddComponent<Image>();
            imgFill.color = color;
            imgFill.type = Image.Type.Filled;
            imgFill.fillMethod = Image.FillMethod.Horizontal;
            return fillObj;
        }

        private static GameObject CreateUIText(GameObject parent, string name, string content, Vector2 pos, int fontSize, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(600, 30);
            var txt = obj.AddComponent<Text>();
            txt.text = content;
            txt.fontSize = fontSize;
            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.font = f;
            txt.color = color;
            return obj;
        }

        private static Sprite LoadSprite(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                bool dirty = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    dirty = true;
                }
                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    dirty = true;
                }
                if (importer.spritePixelsPerUnit != 16f)
                {
                    importer.spritePixelsPerUnit = 16f;
                    dirty = true;
                }
                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    dirty = true;
                }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    dirty = true;
                }
                if (dirty)
                {
                    importer.SaveAndReimport();
                }
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
            {
                Debug.LogError($"[BuildAct1Scene] CRITICAL: Could not load persistent Sprite at path: {assetPath}");
            }
            return sprite;
        }

        private static void SetupTagsAndLayers()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            string[] neededTags = { "Enemy", "Bullet", "Environment" };
            foreach (var t in neededTags)
            {
                bool found = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    if (tagsProp.GetArrayElementAtIndex(i).stringValue == t) { found = true; break; }
                }
                if (!found)
                {
                    tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                    tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = t;
                }
            }
            tagManager.ApplyModifiedProperties();
        }
    }
}
