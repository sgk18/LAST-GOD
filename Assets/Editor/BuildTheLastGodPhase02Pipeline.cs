// BuildTheLastGodPhase02Pipeline.cs
// Production Pipeline for THE LAST GOD — UI Visual Identity Phase 02
// In-Game HUD, Pause Menu, Settings Matrix, Save/Load Terminal, Dialogue Terminal, Game Over, Inventory Codex
// =========================================================================================================

using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using LastGod.UI;
using LastGod.ThirdPerson.Save;

public static class BuildTheLastGodPhase02Pipeline
{
    const string INGAME_PREFAB_PATH = "Assets/Art/UI/Prefabs/UI_InGame_TheLastGod.prefab";
    const string TITLE_PREFAB_PATH = "Assets/Art/UI/Prefabs/UI_Root_TheLastGod.prefab";
    const string GAMEPLAY_SCENE = "Assets/Scenes/Act1_Origin.unity";
    const string SCREENSHOT_DIR = "Assets/Art/UI/Screenshots";
    const string DREAM_LOOP_DIR = ".dream-loop";
    const string BRAIN_DIR = "C:/Users/Surya VM/.gemini/antigravity-cli/brain/bbbf2775-130c-4986-890f-7f89f3a0b51b";

    [MenuItem("The Last God/Execute Phase 02 UI Pipeline", false, 20)]
    public static void ExecutePipeline()
    {
        Debug.Log(">>> [BuildTheLastGodPhase02Pipeline] Beginning Phase 02 Pipeline...");

        // 1. Configure Phase 02 Texture Importers
        ConfigureTextureImporters();

        // 2. Build In-Game UI Prefab Hierarchy
        GameObject inGameRootGO = BuildInGameUIHierarchy();

        // 3. Save In-Game Prefab
        EnsureDirectory("Assets/Art/UI/Prefabs");
        GameObject inGamePrefabAsset = PrefabUtility.SaveAsPrefabAsset(inGameRootGO, INGAME_PREFAB_PATH);
        Debug.Log(">>> Saved canonical In-Game UI Prefab: " + INGAME_PREFAB_PATH);
        UnityEngine.Object.DestroyImmediate(inGameRootGO);

        // 4. Integrate into Gameplay Scene (Act1_Origin.unity)
        GameObject sceneInstance = IntegrateIntoGameplayScene(inGamePrefabAsset);

        // 5. Render Multi-Resolution Screenshots for all Phase 02 screens
        RenderPhase02Screenshots(sceneInstance);

        Debug.Log(">>> [BuildTheLastGodPhase02Pipeline] Phase 02 Pipeline finished successfully!");
    }

    // =========================================================================
    // STEP 1: CONFIGURE TEXTURE IMPORTERS
    // =========================================================================
    public static void ConfigureTextureImporters()
    {
        Debug.Log(">>> Step 1: Configuring texture importers for Phase 02...");
        string[] texPaths = new string[]
        {
            "Assets/Art/UI/Textures/UI_Speaker_Silhouette_01.png",
            "Assets/Art/UI/Textures/UI_Speaker_Silhouette_02.png",
            "Assets/Art/UI/Textures/UI_Bullet_Icon.png",
            "Assets/Art/UI/Textures/UI_Slider_Handle.png",
            "Assets/Art/UI/Textures/UI_Checkbox_Frame.png",
            "Assets/Art/UI/Textures/UI_Check_Mark.png"
        };

        foreach (string path in texPaths)
        {
            if (!File.Exists(path)) continue;
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    // =========================================================================
    // STEP 2: BUILD IN-GAME UI HIERARCHY
    // =========================================================================
    public static GameObject BuildInGameUIHierarchy()
    {
        Debug.Log(">>> Step 2: Assembling In-Game UI GameObject hierarchy...");

        // Load procedural sprites
        Sprite spWhite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/UI_White.png");
        Sprite spBrutalistFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/UI_Brutalist_Frame.png");
        Sprite spCrosshair = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/UI_Crosshair.png");
        Sprite spChevrons = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/UI_Hazard_Chevrons.png");
        Sprite spScanline = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/UI_Scanline.png");
        Sprite spBullet = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/UI_Bullet_Icon.png");
        Sprite spDot = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/UI_Dot_Cyan.png");
        Sprite spSpeaker1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/UI_Speaker_Silhouette_01.png");
        Sprite spSpeaker2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/UI_Speaker_Silhouette_02.png");

        TMP_FontAsset fontBase = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (fontBase == null)
        {
            fontBase = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Packages/com.unity.textmeshpro/Editor Resources/LiberationSans SDF.asset");
        }

        // Root
        GameObject root = new GameObject("UI_InGame_Root");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<GraphicRaycaster>();
        UIInGameManager inGameManager = root.AddComponent<UIInGameManager>();
        AudioSource audioSource = root.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // ---------------------------------------------------------------------
        // LAYER 1: IN-GAME HUD
        // ---------------------------------------------------------------------
        GameObject hudGO = CreateUIChild(root, "01_InGameHUD", Vector2.zero, Vector2.one);
        InGameHUDController hudCtrl = hudGO.AddComponent<InGameHUDController>();

        // Top-Left Aeron Vitality & Surge Panel
        GameObject vitalsPanelGO = CreateUIChild(hudGO, "VitalsPanel", new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -134), new Vector2(344, -24));
        Image vitalsBg = vitalsPanelGO.AddComponent<Image>();
        vitalsBg.sprite = spBrutalistFrame;
        vitalsBg.type = Image.Type.Sliced;
        vitalsBg.color = UITheme.DeepCharcoal;

        // Subject Aeron Tag
        GameObject subjGO = CreateUIChild(vitalsPanelGO, "SubjectLabel", new Vector2(0, 1), new Vector2(1, 1), new Vector2(14, -26), new Vector2(-14, -8));
        TextMeshProUGUI subjTxt = subjGO.AddComponent<TextMeshProUGUI>();
        subjTxt.font = fontBase;
        subjTxt.fontSize = 11;
        subjTxt.fontStyle = FontStyles.Bold;
        subjTxt.color = UITheme.BrightCyan;
        subjTxt.text = "SUBJECT // AERON   [INTEGRITY MATRIX]";

        // Health Bar Track
        GameObject hpTrack = CreateUIChild(vitalsPanelGO, "HealthTrack", new Vector2(0, 1), new Vector2(1, 1), new Vector2(14, -48), new Vector2(-14, -30));
        Image hpTrackImg = hpTrack.AddComponent<Image>();
        hpTrackImg.sprite = spWhite;
        hpTrackImg.color = UITheme.DarkBlueGrey;

        // Health Bar Fill
        GameObject hpFill = CreateUIChild(hpTrack, "HealthFill", Vector2.zero, Vector2.one);
        Image hpFillImg = hpFill.AddComponent<Image>();
        hpFillImg.sprite = spWhite;
        hpFillImg.type = Image.Type.Filled;
        hpFillImg.fillMethod = Image.FillMethod.Horizontal;
        hpFillImg.fillOrigin = 0;
        hpFillImg.fillAmount = 1.0f;
        hpFillImg.color = UITheme.TechnologyCyan;

        // Critical Chevrons Overlay
        GameObject hpChevrons = CreateUIChild(hpTrack, "CriticalChevrons", Vector2.zero, Vector2.one);
        Image hpChevronsImg = hpChevrons.AddComponent<Image>();
        hpChevronsImg.sprite = spChevrons;
        hpChevronsImg.type = Image.Type.Tiled;
        hpChevronsImg.color = UITheme.WarningOrange;
        hpChevrons.SetActive(false);

        // Numeric HP
        GameObject hpNumGO = CreateUIChild(vitalsPanelGO, "HealthNumeric", new Vector2(0, 1), new Vector2(1, 1), new Vector2(14, -68), new Vector2(-14, -50));
        TextMeshProUGUI hpNumTxt = hpNumGO.AddComponent<TextMeshProUGUI>();
        hpNumTxt.font = fontBase;
        hpNumTxt.fontSize = 11;
        hpNumTxt.fontStyle = FontStyles.Bold;
        hpNumTxt.alignment = TextAlignmentOptions.MidlineRight;
        hpNumTxt.color = UITheme.BrightCyan;
        hpNumTxt.text = "100 / 100 HP  [100%]";

        // Surge Track
        GameObject surgeTrack = CreateUIChild(vitalsPanelGO, "SurgeTrack", new Vector2(0, 1), new Vector2(1, 1), new Vector2(14, -84), new Vector2(-14, -76));
        Image surgeTrackImg = surgeTrack.AddComponent<Image>();
        surgeTrackImg.sprite = spWhite;
        surgeTrackImg.color = UITheme.NearBlack;

        GameObject surgeFill = CreateUIChild(surgeTrack, "SurgeFill", Vector2.zero, Vector2.one);
        Image surgeFillImg = surgeFill.AddComponent<Image>();
        surgeFillImg.sprite = spWhite;
        surgeFillImg.type = Image.Type.Filled;
        surgeFillImg.fillMethod = Image.FillMethod.Horizontal;
        surgeFillImg.fillOrigin = 0;
        surgeFillImg.fillAmount = 1.0f;
        surgeFillImg.color = UITheme.TechnologyCyan;

        // Surge Status
        GameObject surgeStatGO = CreateUIChild(vitalsPanelGO, "SurgeStatus", new Vector2(0, 1), new Vector2(1, 1), new Vector2(14, -104), new Vector2(-14, -86));
        TextMeshProUGUI surgeStatTxt = surgeStatGO.AddComponent<TextMeshProUGUI>();
        surgeStatTxt.font = fontBase;
        surgeStatTxt.fontSize = 10;
        surgeStatTxt.fontStyle = FontStyles.Bold;
        surgeStatTxt.color = UITheme.LightMetal;
        surgeStatTxt.text = "SURGE READY [Q / 1]";

        // Bottom-Right Weapon & Ammunition Module
        GameObject ammoPanelGO = CreateUIChild(hudGO, "AmmoPanel", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-284, 24), new Vector2(-24, 114));
        Image ammoBg = ammoPanelGO.AddComponent<Image>();
        ammoBg.sprite = spBrutalistFrame;
        ammoBg.type = Image.Type.Sliced;
        ammoBg.color = UITheme.DeepCharcoal;

        GameObject wpnNameGO = CreateUIChild(ammoPanelGO, "WeaponName", new Vector2(0, 1), new Vector2(1, 1), new Vector2(12, -24), new Vector2(-12, -6));
        TextMeshProUGUI wpnNameTxt = wpnNameGO.AddComponent<TextMeshProUGUI>();
        wpnNameTxt.font = fontBase;
        wpnNameTxt.fontSize = 10;
        wpnNameTxt.fontStyle = FontStyles.Bold;
        wpnNameTxt.color = UITheme.LightMetal;
        wpnNameTxt.text = "SEC-P9 // STASIS INTERCEPTOR";

        // Bullet Icons Container
        GameObject bulletsContainer = CreateUIChild(ammoPanelGO, "Bullets", new Vector2(0, 0), new Vector2(0.6f, 1), new Vector2(12, 10), new Vector2(0, -28));
        Image[] bulletImgArray = new Image[6];
        for (int b = 0; b < 6; b++)
        {
            GameObject bObj = CreateUIChild(bulletsContainer, $"Bullet_{b}", new Vector2(b * 0.16f, 0), new Vector2((b + 1) * 0.16f, 1));
            Image bImg = bObj.AddComponent<Image>();
            bImg.sprite = spBullet;
            bImg.preserveAspect = true;
            bImg.color = UITheme.TechnologyCyan;
            bulletImgArray[b] = bImg;
        }

        // Numeric Ammo
        GameObject ammoNumGO = CreateUIChild(ammoPanelGO, "AmmoNumeric", new Vector2(0.6f, 0), new Vector2(1, 1), new Vector2(0, 10), new Vector2(-12, -24));
        TextMeshProUGUI ammoNumTxt = ammoNumGO.AddComponent<TextMeshProUGUI>();
        ammoNumTxt.font = fontBase;
        ammoNumTxt.fontSize = 24;
        ammoNumTxt.fontStyle = FontStyles.Bold;
        ammoNumTxt.alignment = TextAlignmentOptions.MidlineRight;
        ammoNumTxt.color = UITheme.BrightCyan;
        ammoNumTxt.text = "06 / 06";

        // Reload alert
        GameObject reloadGO = CreateUIChild(ammoPanelGO, "ReloadAlert", new Vector2(0, 0), new Vector2(1, 0), new Vector2(12, 6), new Vector2(-12, 22));
        TextMeshProUGUI reloadTxt = reloadGO.AddComponent<TextMeshProUGUI>();
        reloadTxt.font = fontBase;
        reloadTxt.fontSize = 10;
        reloadTxt.fontStyle = FontStyles.Bold;
        reloadTxt.alignment = TextAlignmentOptions.MidlineRight;
        reloadTxt.color = UITheme.WarningOrange;
        reloadTxt.text = "[R] RELOAD REQUIRED";
        reloadGO.SetActive(false);

        // Bottom-Left Tactical Controls Legend
        GameObject guidePanelGO = CreateUIChild(hudGO, "ControlsGuidePanel", new Vector2(0, 0), new Vector2(0, 0), new Vector2(24, 24), new Vector2(254, 184));
        Image guideBg = guidePanelGO.AddComponent<Image>();
        guideBg.sprite = spBrutalistFrame;
        guideBg.type = Image.Type.Sliced;
        guideBg.color = new Color(UITheme.DeepCharcoal.r, UITheme.DeepCharcoal.g, UITheme.DeepCharcoal.b, 0.9f);

        GameObject guideHeaderGO = CreateUIChild(guidePanelGO, "Header", new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -24), new Vector2(-10, -6));
        TextMeshProUGUI guideHdrTxt = guideHeaderGO.AddComponent<TextMeshProUGUI>();
        guideHdrTxt.font = fontBase;
        guideHdrTxt.fontSize = 10;
        guideHdrTxt.fontStyle = FontStyles.Bold;
        guideHdrTxt.color = UITheme.BrightCyan;
        guideHdrTxt.text = "TACTICAL LINK // [H] TOGGLE";

        string[] guideRows = new[]
        {
            "<color=#6FE3FF>[A / D]</color>  MOVE / PATROL",
            "<color=#6FE3FF>[SPACE]</color>  JUMP / EVADE",
            "<color=#6FE3FF>[L-CLICK]</color>  STASIS FIRE",
            "<color=#6FE3FF>[SHIFT]</color>  DODGE DASH",
            "<color=#6FE3FF>[Q]</color>  ASCENSION SURGE",
            "<color=#6FE3FF>[ESC]</color>  SUSPEND PROTOCOL"
        };
        for (int g = 0; g < guideRows.Length; g++)
        {
            GameObject rObj = CreateUIChild(guidePanelGO, $"Row_{g}", new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -44 - (g * 18)), new Vector2(-10, -28 - (g * 18)));
            TextMeshProUGUI rTxt = rObj.AddComponent<TextMeshProUGUI>();
            rTxt.font = fontBase;
            rTxt.fontSize = 9;
            rTxt.color = UITheme.TextMuted;
            rTxt.text = guideRows[g];
        }

        // Lock-On Reticle
        GameObject reticleGO = CreateUIChild(hudGO, "LockOnReticle", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-24, -24), new Vector2(24, 24));
        Image reticleImg = reticleGO.AddComponent<Image>();
        reticleImg.sprite = spCrosshair;
        reticleImg.color = UITheme.TechnologyCyan;

        GameObject reticleTagGO = CreateUIChild(reticleGO, "TargetTag", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-100, -22), new Vector2(100, -4));
        TextMeshProUGUI reticleTagTxt = reticleTagGO.AddComponent<TextMeshProUGUI>();
        reticleTagTxt.font = fontBase;
        reticleTagTxt.fontSize = 9;
        reticleTagTxt.fontStyle = FontStyles.Bold;
        reticleTagTxt.alignment = TextAlignmentOptions.Center;
        reticleTagTxt.color = UITheme.BrightCyan;
        reticleTagTxt.text = "[TARGET: BIO-SPECIMEN // GUARD]";
        reticleGO.SetActive(false);

        // Wire InGameHUDController
        SerializedObject soHud = new SerializedObject(hudCtrl);
        soHud.FindProperty("vitalPanel").objectReferenceValue = vitalsPanelGO.GetComponent<RectTransform>();
        soHud.FindProperty("subjectLabel").objectReferenceValue = subjTxt;
        soHud.FindProperty("healthBarFill").objectReferenceValue = hpFillImg;
        soHud.FindProperty("healthBarCriticalChevrons").objectReferenceValue = hpChevronsImg;
        soHud.FindProperty("healthNumericText").objectReferenceValue = hpNumTxt;
        soHud.FindProperty("surgeBarFill").objectReferenceValue = surgeFillImg;
        soHud.FindProperty("surgeStatusText").objectReferenceValue = surgeStatTxt;
        soHud.FindProperty("ammoPanel").objectReferenceValue = ammoPanelGO.GetComponent<RectTransform>();
        soHud.FindProperty("weaponNameText").objectReferenceValue = wpnNameTxt;
        soHud.FindProperty("ammoNumericText").objectReferenceValue = ammoNumTxt;
        SerializedProperty bArrayProp = soHud.FindProperty("bulletIcons");
        bArrayProp.arraySize = 6;
        for (int i = 0; i < 6; i++) bArrayProp.GetArrayElementAtIndex(i).objectReferenceValue = bulletImgArray[i];
        soHud.FindProperty("reloadAlertText").objectReferenceValue = reloadTxt;
        soHud.FindProperty("controlsGuidePanel").objectReferenceValue = guidePanelGO;
        soHud.FindProperty("lockOnReticle").objectReferenceValue = reticleGO.GetComponent<RectTransform>();
        soHud.FindProperty("lockOnTargetLabel").objectReferenceValue = reticleTagTxt;
        soHud.ApplyModifiedProperties();

        // ---------------------------------------------------------------------
        // LAYER 2: PAUSE MENU & MODAL TERMINALS
        // ---------------------------------------------------------------------
        GameObject pauseGO = CreateUIChild(root, "02_PauseMenu", Vector2.zero, Vector2.one);
        PauseMenuController pauseCtrl = pauseGO.AddComponent<PauseMenuController>();

        // Backdrop
        GameObject pBackdrop = CreateUIChild(pauseGO, "Backdrop", Vector2.zero, Vector2.one);
        Image pBackdropImg = pBackdrop.AddComponent<Image>();
        pBackdropImg.sprite = spWhite;
        pBackdropImg.color = new Color(UITheme.NearBlack.r, UITheme.NearBlack.g, UITheme.NearBlack.b, 0.88f);

        // Scanlines overlay
        GameObject pScan = CreateUIChild(pauseGO, "Scanlines", Vector2.zero, Vector2.one);
        Image pScanImg = pScan.AddComponent<Image>();
        pScanImg.sprite = spScanline;
        pScanImg.type = Image.Type.Tiled;
        pScanImg.color = new Color(1, 1, 1, 0.04f);

        // Pause Main Panel
        GameObject pMainPanel = CreateUIChild(pauseGO, "PauseMainPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-270, -290), new Vector2(270, 290));
        Image pMainBg = pMainPanel.AddComponent<Image>();
        pMainBg.sprite = spBrutalistFrame;
        pMainBg.type = Image.Type.Sliced;
        pMainBg.color = UITheme.DeepCharcoal;

        // Top Classification Bar
        GameObject pSecHdr = CreateUIChild(pMainPanel, "SecHeader", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -36), new Vector2(-24, -16));
        TextMeshProUGUI pSecTxt = pSecHdr.AddComponent<TextMeshProUGUI>();
        pSecTxt.font = fontBase;
        pSecTxt.fontSize = 11;
        pSecTxt.fontStyle = FontStyles.Bold;
        pSecTxt.color = UITheme.WarningOrange;
        pSecTxt.text = "FACILITY // SUBTERRANEAN SECTOR B-03   [PROTOCOL SUSPENDED]";

        // Title
        GameObject pTitleGO = CreateUIChild(pMainPanel, "Title", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -90), new Vector2(-24, -38));
        TextMeshProUGUI pTitleTxt = pTitleGO.AddComponent<TextMeshProUGUI>();
        pTitleTxt.font = fontBase;
        pTitleTxt.fontSize = 38;
        pTitleTxt.fontStyle = FontStyles.Bold;
        pTitleTxt.color = UITheme.TextBright;
        pTitleTxt.text = "SYSTEM SUSPENDED";

        // Piercing Rule
        GameObject pRule = CreateUIChild(pMainPanel, "Rule", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -96), new Vector2(-24, -94));
        Image pRuleImg = pRule.AddComponent<Image>();
        pRuleImg.sprite = spWhite;
        pRuleImg.color = UITheme.TechnologyCyan;

        // Menu items container
        GameObject pMenuContainer = CreateUIChild(pMainPanel, "MenuItems", new Vector2(0, 0), new Vector2(1, 1), new Vector2(24, 50), new Vector2(-24, -110));
        UIMenuItem pResume = CreateMenuItem(pMenuContainer, "01_Resume", "01", "RESUME PROTOCOL", "CONTINUE ACTIVE CONTAINMENT SIMULATION", 0, spWhite, spDot, fontBase);
        UIMenuItem pRestart = CreateMenuItem(pMenuContainer, "02_Restart", "02", "RESTART CHECKPOINT", "RELOAD ACTIVE SECTOR INCEPTION POINT", 1, spWhite, spDot, fontBase);
        UIMenuItem pSettings = CreateMenuItem(pMenuContainer, "03_Settings", "03", "TERMINAL SETTINGS", "CALIBRATE AUDIO, DISPLAY & NEURAL CONTROLS", 2, spWhite, spDot, fontBase);
        UIMenuItem pArchive = CreateMenuItem(pMenuContainer, "04_Archive", "04", "ARCHIVE MATRIX", "OPEN CLASSIFIED DATA STORAGE VAULT", 3, spWhite, spDot, fontBase);
        UIMenuItem pQuit = CreateMenuItem(pMenuContainer, "05_Quit", "05", "QUIT TO TITLE", "DISENGAGE SESSION AND RETURN TO ARCHIVE GATE", 4, spWhite, spDot, fontBase);

        // Settings Sub-Terminal
        GameObject settingsPanelGO = CreateSettingsTerminal(pauseGO, spWhite, spBrutalistFrame, fontBase);
        SettingsMenuController settingsCtrl = settingsPanelGO.GetComponent<SettingsMenuController>();

        // Save/Load Sub-Terminal
        GameObject saveLoadPanelGO = CreateSaveLoadTerminal(pauseGO, spWhite, spBrutalistFrame, fontBase);
        SaveLoadMenuController saveLoadCtrl = saveLoadPanelGO.GetComponent<SaveLoadMenuController>();

        // Wire PauseMenuController
        SerializedObject soPause = new SerializedObject(pauseCtrl);
        soPause.FindProperty("pausePanel").objectReferenceValue = pMainPanel;
        soPause.FindProperty("resumeItem").objectReferenceValue = pResume;
        soPause.FindProperty("restartItem").objectReferenceValue = pRestart;
        soPause.FindProperty("settingsItem").objectReferenceValue = pSettings;
        soPause.FindProperty("archiveItem").objectReferenceValue = pArchive;
        soPause.FindProperty("quitItem").objectReferenceValue = pQuit;
        soPause.FindProperty("settingsMenu").objectReferenceValue = settingsCtrl;
        soPause.FindProperty("saveLoadMenu").objectReferenceValue = saveLoadCtrl;
        soPause.ApplyModifiedProperties();

        // ---------------------------------------------------------------------
        // LAYER 3: DIALOGUE TERMINAL
        // ---------------------------------------------------------------------
        GameObject dialGO = CreateUIChild(root, "03_DialogueTerminal", Vector2.zero, Vector2.one);
        DialogueTerminalController dialCtrl = dialGO.AddComponent<DialogueTerminalController>();

        GameObject dialBox = CreateUIChild(dialGO, "DialogueBox", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-540, 36), new Vector2(540, 216));
        Image dialBoxBg = dialBox.AddComponent<Image>();
        dialBoxBg.sprite = spBrutalistFrame;
        dialBoxBg.type = Image.Type.Sliced;
        dialBoxBg.color = UITheme.DeepCharcoal;

        // Speaker Portrait
        GameObject spkPortObj = CreateUIChild(dialBox, "SpeakerPortrait", new Vector2(0, 0), new Vector2(0, 1), new Vector2(16, 16), new Vector2(164, -16));
        Image spkPortImg = spkPortObj.AddComponent<Image>();
        spkPortImg.sprite = spSpeaker1;
        spkPortImg.preserveAspect = true;

        // Speaker Name
        GameObject spkNameGO = CreateUIChild(dialBox, "SpeakerName", new Vector2(0, 1), new Vector2(1, 1), new Vector2(184, -40), new Vector2(-20, -14));
        TextMeshProUGUI spkNameTxt = spkNameGO.AddComponent<TextMeshProUGUI>();
        spkNameTxt.font = fontBase;
        spkNameTxt.fontSize = 16;
        spkNameTxt.fontStyle = FontStyles.Bold;
        spkNameTxt.color = UITheme.BrightCyan;
        spkNameTxt.text = "DR. V. MALKHOV [CHIEF DIRECTOR]";

        // Frequency Tag
        GameObject spkFreqGO = CreateUIChild(dialBox, "FreqTag", new Vector2(0, 1), new Vector2(1, 1), new Vector2(184, -62), new Vector2(-20, -42));
        TextMeshProUGUI spkFreqTxt = spkFreqGO.AddComponent<TextMeshProUGUI>();
        spkFreqTxt.font = fontBase;
        spkFreqTxt.fontSize = 11;
        spkFreqTxt.fontStyle = FontStyles.Bold;
        spkFreqTxt.color = UITheme.WarningOrange;
        spkFreqTxt.text = "TRANSMISSION // DIRECTORIAL OVERRIDE [SEC-01]";

        // Dialogue Subtitle Text
        GameObject dialBodyGO = CreateUIChild(dialBox, "BodyText", new Vector2(0, 0), new Vector2(1, 1), new Vector2(184, 40), new Vector2(-20, -66));
        TextMeshProUGUI dialBodyTxt = dialBodyGO.AddComponent<TextMeshProUGUI>();
        dialBodyTxt.font = fontBase;
        dialBodyTxt.fontSize = 17;
        dialBodyTxt.enableWordWrapping = true;
        dialBodyTxt.color = UITheme.TextBright;
        dialBodyTxt.text = "Subject Aeron exhibits severe divine divergence. Engage stasis clamps immediately before the containment matrix fractures completely.";

        // Advance Prompt
        GameObject advPromptGO = CreateUIChild(dialBox, "AdvancePrompt", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-260, 12), new Vector2(-20, 32));
        TextMeshProUGUI advPromptTxt = advPromptGO.AddComponent<TextMeshProUGUI>();
        advPromptTxt.font = fontBase;
        advPromptTxt.fontSize = 10;
        advPromptTxt.fontStyle = FontStyles.Bold;
        advPromptTxt.alignment = TextAlignmentOptions.MidlineRight;
        advPromptTxt.color = UITheme.TechnologyCyan;
        advPromptTxt.text = "[SPACE / CLICK TO ADVANCE ▶]";

        // Wire DialogueTerminalController
        SerializedObject soDial = new SerializedObject(dialCtrl);
        soDial.FindProperty("speakerPortraitImage").objectReferenceValue = spkPortImg;
        soDial.FindProperty("speakerNameText").objectReferenceValue = spkNameTxt;
        soDial.FindProperty("frequencyTagText").objectReferenceValue = spkFreqTxt;
        soDial.FindProperty("dialogueBodyText").objectReferenceValue = dialBodyTxt;
        soDial.FindProperty("advancePromptObject").objectReferenceValue = advPromptGO;
        soDial.FindProperty("advancePromptText").objectReferenceValue = advPromptTxt;
        soDial.FindProperty("scientistSprite").objectReferenceValue = spSpeaker1;
        soDial.FindProperty("overwatchSprite").objectReferenceValue = spSpeaker2;
        soDial.ApplyModifiedProperties();

        // ---------------------------------------------------------------------
        // LAYER 4: GAME OVER SCREEN
        // ---------------------------------------------------------------------
        GameObject gameOverGO = CreateUIChild(root, "04_GameOverScreen", Vector2.zero, Vector2.one);
        GameOverController gameOverCtrl = gameOverGO.AddComponent<GameOverController>();

        GameObject goBackdrop = CreateUIChild(gameOverGO, "Backdrop", Vector2.zero, Vector2.one);
        Image goBackdropImg = goBackdrop.AddComponent<Image>();
        goBackdropImg.sprite = spWhite;
        goBackdropImg.color = UITheme.NearBlack;

        GameObject goMainPanel = CreateUIChild(gameOverGO, "GameOverPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-380, -310), new Vector2(380, 310));
        Image goMainBg = goMainPanel.AddComponent<Image>();
        goMainBg.sprite = spBrutalistFrame;
        goMainBg.type = Image.Type.Sliced;
        goMainBg.color = UITheme.DeepCharcoal;

        // Emergency Warning Hazard Strip
        GameObject goHazard = CreateUIChild(goMainPanel, "HazardStrip", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -30), new Vector2(-24, -14));
        Image goHazardImg = goHazard.AddComponent<Image>();
        goHazardImg.sprite = spChevrons;
        goHazardImg.type = Image.Type.Tiled;
        goHazardImg.color = UITheme.WarningOrange;

        GameObject goHdr = CreateUIChild(goMainPanel, "Header", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -98), new Vector2(-24, -36));
        TextMeshProUGUI goHdrTxt = goHdr.AddComponent<TextMeshProUGUI>();
        goHdrTxt.font = fontBase;
        goHdrTxt.fontSize = 26;
        goHdrTxt.fontStyle = FontStyles.Bold;
        goHdrTxt.color = UITheme.WarningOrange;
        goHdrTxt.text = "CONTAINMENT BREACH // CRITICAL FAILURE";

        GameObject goSub = CreateUIChild(goMainPanel, "Subheader", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -126), new Vector2(-24, -100));
        TextMeshProUGUI goSubTxt = goSub.AddComponent<TextMeshProUGUI>();
        goSubTxt.font = fontBase;
        goSubTxt.fontSize = 12;
        goSubTxt.fontStyle = FontStyles.Bold;
        goSubTxt.color = UITheme.BrightCyan;
        goSubTxt.text = "SUBJECT AERON TERMINATED — CELLULAR INTEGRITY COLLAPSED";

        GameObject goIncident = CreateUIChild(goMainPanel, "IncidentReport", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -196), new Vector2(-24, -132));
        TextMeshProUGUI goIncTxt = goIncident.AddComponent<TextMeshProUGUI>();
        goIncTxt.font = fontBase;
        goIncTxt.fontSize = 11;
        goIncTxt.color = UITheme.TextMuted;
        goIncTxt.text = "LOCATION: SUBTERRANEAN SECTOR B-03 CATWALK\nFATAL VECTOR: TRAUMATIC BLUNT DISRUPTION & CELLULAR DECAY\nFACILITY STATUS: EMERGENCY LOCKDOWN ACTIVE // AIRLOCKS SEALED";

        GameObject goMenuContainer = CreateUIChild(goMainPanel, "MenuItems", new Vector2(0, 0), new Vector2(1, 1), new Vector2(24, 20), new Vector2(-24, -206));
        UIMenuItem goRetry = CreateMenuItem(goMenuContainer, "01_Retry", "01", "RETRY CHECKPOINT", "RESTORE RECENT INCIDENT CHECKPOINT", 0, spWhite, spDot, fontBase);
        UIMenuItem goLoad = CreateMenuItem(goMenuContainer, "02_LoadArchive", "02", "RESTORE ARCHIVE", "SELECT CLASSIFIED VAULT FILE TO RESTORE", 1, spWhite, spDot, fontBase);
        UIMenuItem goQuit = CreateMenuItem(goMenuContainer, "03_Quit", "03", "TERMINATE SESSION", "ABANDON SIMULATION AND RETURN TO TITLE GATE", 2, spWhite, spDot, fontBase);

        // Wire GameOverController
        SerializedObject soGO = new SerializedObject(gameOverCtrl);
        soGO.FindProperty("retryCheckpointItem").objectReferenceValue = goRetry;
        soGO.FindProperty("loadArchiveItem").objectReferenceValue = goLoad;
        soGO.FindProperty("quitToTitleItem").objectReferenceValue = goQuit;
        soGO.FindProperty("saveLoadMenu").objectReferenceValue = saveLoadCtrl;
        soGO.ApplyModifiedProperties();

        // ---------------------------------------------------------------------
        // LAYER 5: INVENTORY & SPECIMEN CODEX
        // ---------------------------------------------------------------------
        GameObject codexGO = CreateUIChild(root, "05_InventoryCodex", Vector2.zero, Vector2.one);
        InventoryCodexController codexCtrl = codexGO.AddComponent<InventoryCodexController>();

        GameObject codexBackdrop = CreateUIChild(codexGO, "Backdrop", Vector2.zero, Vector2.one);
        Image codexBackdropImg = codexBackdrop.AddComponent<Image>();
        codexBackdropImg.sprite = spWhite;
        codexBackdropImg.color = new Color(UITheme.NearBlack.r, UITheme.NearBlack.g, UITheme.NearBlack.b, 0.94f);

        GameObject codexPanel = CreateUIChild(codexGO, "CodexPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-600, -340), new Vector2(600, 340));
        Image codexBg = codexPanel.AddComponent<Image>();
        codexBg.sprite = spBrutalistFrame;
        codexBg.type = Image.Type.Sliced;
        codexBg.color = UITheme.DeepCharcoal;

        // Header
        GameObject codexHdr = CreateUIChild(codexPanel, "Header", new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -50), new Vector2(-30, -20));
        TextMeshProUGUI codexHdrTxt = codexHdr.AddComponent<TextMeshProUGUI>();
        codexHdrTxt.font = fontBase;
        codexHdrTxt.fontSize = 24;
        codexHdrTxt.fontStyle = FontStyles.Bold;
        codexHdrTxt.color = UITheme.BrightCyan;
        codexHdrTxt.text = "CLASSIFIED SPECIMEN LOADOUT & RESEARCH CODEX";

        // Left Equipment List
        GameObject eqList = CreateUIChild(codexPanel, "EquipmentList", new Vector2(0, 0), new Vector2(0.36f, 1), new Vector2(30, 40), new Vector2(0, -70));
        Button wpnBtn = CreateCodexSlotButton(eqList, "Slot_01", "01  SEC-P9 PISTOL", 0, spWhite, fontBase);
        Button surgeBtn = CreateCodexSlotButton(eqList, "Slot_02", "02  DIVINE CATALYST", 1, spWhite, fontBase);
        Button collarBtn = CreateCodexSlotButton(eqList, "Slot_03", "03  STASIS COLLAR", 2, spWhite, fontBase);

        // Right Detail Dossier
        GameObject detPanel = CreateUIChild(codexPanel, "DetailDossier", new Vector2(0.38f, 0), new Vector2(1, 1), new Vector2(0, 40), new Vector2(-30, -70));
        Image detBg = detPanel.AddComponent<Image>();
        detBg.sprite = spWhite;
        detBg.color = UITheme.NearBlack;

        GameObject itmTitle = CreateUIChild(detPanel, "Title", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -40), new Vector2(-240, -14));
        TextMeshProUGUI itmTitleTxt = itmTitle.AddComponent<TextMeshProUGUI>();
        itmTitleTxt.font = fontBase;
        itmTitleTxt.fontSize = 20;
        itmTitleTxt.fontStyle = FontStyles.Bold;
        itmTitleTxt.color = UITheme.TextBright;
        itmTitleTxt.text = "SEC-P9 // STASIS INTERCEPTOR";

        // Classification Stamp
        GameObject itmStamp = CreateUIChild(detPanel, "Stamp", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-240, -40), new Vector2(-24, -14));
        TextMeshProUGUI itmStampTxt = itmStamp.AddComponent<TextMeshProUGUI>();
        itmStampTxt.font = fontBase;
        itmStampTxt.fontSize = 11;
        itmStampTxt.fontStyle = FontStyles.Bold;
        itmStampTxt.alignment = TextAlignmentOptions.MidlineRight;
        itmStampTxt.color = UITheme.WarningOrange;
        itmStampTxt.text = "[RESTRICTED SPECIMEN RECORD]";

        GameObject itmSerial = CreateUIChild(detPanel, "Serial", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -64), new Vector2(-24, -42));
        TextMeshProUGUI itmSerialTxt = itmSerial.AddComponent<TextMeshProUGUI>();
        itmSerialTxt.font = fontBase;
        itmSerialTxt.fontSize = 11;
        itmSerialTxt.fontStyle = FontStyles.Bold;
        itmSerialTxt.color = UITheme.TechnologyCyan;
        itmSerialTxt.text = "SERIAL: #09-441-K // CLEARANCE LEVEL 04";

        // Technical Schematic Box
        GameObject schemBox = CreateUIChild(detPanel, "SchematicBox", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -160), new Vector2(-24, -72));
        Image schemBoxBg = schemBox.AddComponent<Image>();
        schemBoxBg.sprite = spBrutalistFrame;
        schemBoxBg.type = Image.Type.Sliced;
        schemBoxBg.color = UITheme.DeepCharcoal;

        GameObject schemCross = CreateUIChild(schemBox, "Crosshair", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-20, -20), new Vector2(20, 20));
        Image schemCrossImg = schemCross.AddComponent<Image>();
        schemCrossImg.sprite = spCrosshair;
        schemCrossImg.color = UITheme.TechnologyCyan;

        GameObject schemBullet = CreateUIChild(schemBox, "BulletSilhouette", new Vector2(0.35f, 0.5f), new Vector2(0.35f, 0.5f), new Vector2(-12, -24), new Vector2(12, 24));
        Image schemBulletImg = schemBullet.AddComponent<Image>();
        schemBulletImg.sprite = spBullet;
        schemBulletImg.color = UITheme.BrightCyan;

        GameObject schemTag = CreateUIChild(schemBox, "SchemTag", new Vector2(0, 0), new Vector2(1, 0), new Vector2(12, 6), new Vector2(-12, 22));
        TextMeshProUGUI schemTagTxt = schemTag.AddComponent<TextMeshProUGUI>();
        schemTagTxt.font = fontBase;
        schemTagTxt.fontSize = 9;
        schemTagTxt.color = UITheme.LightMetal;
        schemTagTxt.text = "ORTHOGRAPHIC SCHEMATIC // BLUEPRINT REV 02.4";

        GameObject itmSpecs = CreateUIChild(detPanel, "Specs", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -250), new Vector2(-24, -170));
        TextMeshProUGUI itmSpecsTxt = itmSpecs.AddComponent<TextMeshProUGUI>();
        itmSpecsTxt.font = fontBase;
        itmSpecsTxt.fontSize = 11;
        itmSpecsTxt.color = UITheme.BrightCyan;
        itmSpecsTxt.text = "CALIBER: 9×19mm STASIS HOLLOW-POINT\nMAGAZINE: 6 ROUNDS CAPACITY\nVELOCITY: 380 M/S SUB-SONIC\nFIRE-RATE: SEMI-AUTOMATIC INTERCEPT";

        GameObject itmLore = CreateUIChild(detPanel, "Lore", new Vector2(0, 0), new Vector2(1, 1), new Vector2(24, 20), new Vector2(-24, -260));
        TextMeshProUGUI itmLoreTxt = itmLore.AddComponent<TextMeshProUGUI>();
        itmLoreTxt.font = fontBase;
        itmLoreTxt.fontSize = 12;
        itmLoreTxt.enableWordWrapping = true;
        itmLoreTxt.color = UITheme.TextMuted;
        itmLoreTxt.text = "Standard issue facility sidearm. Modified with pressurized liquid stasis reservoirs to neutralize biological specimens exhibiting divine divergence.";

        GameObject codexClose = CreateUIChild(codexPanel, "CloseButton", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-200, 10), new Vector2(-30, 42));
        Image cCloseImg = codexClose.AddComponent<Image>();
        cCloseImg.sprite = spWhite;
        cCloseImg.color = UITheme.DarkBlueGrey;
        Button cCloseBtn = codexClose.AddComponent<Button>();
        GameObject cCloseTxtObj = CreateUIChild(codexClose, "Text", Vector2.zero, Vector2.one);
        TextMeshProUGUI cCloseTxt = cCloseTxtObj.AddComponent<TextMeshProUGUI>();
        cCloseTxt.font = fontBase;
        cCloseTxt.fontSize = 12;
        cCloseTxt.fontStyle = FontStyles.Bold;
        cCloseTxt.alignment = TextAlignmentOptions.Center;
        cCloseTxt.color = UITheme.BrightCyan;
        cCloseTxt.text = "[CLOSE ARCHIVE // ESC]";

        // Wire InventoryCodexController
        SerializedObject soCodex = new SerializedObject(codexCtrl);
        soCodex.FindProperty("weaponSlotButton").objectReferenceValue = wpnBtn;
        soCodex.FindProperty("surgeSlotButton").objectReferenceValue = surgeBtn;
        soCodex.FindProperty("collarSlotButton").objectReferenceValue = collarBtn;
        soCodex.FindProperty("itemTitleText").objectReferenceValue = itmTitleTxt;
        soCodex.FindProperty("itemSerialText").objectReferenceValue = itmSerialTxt;
        soCodex.FindProperty("itemSpecsText").objectReferenceValue = itmSpecsTxt;
        soCodex.FindProperty("itemLoreText").objectReferenceValue = itmLoreTxt;
        soCodex.FindProperty("closeButton").objectReferenceValue = cCloseBtn;
        soCodex.ApplyModifiedProperties();

        // ---------------------------------------------------------------------
        // WIRE UI IN-GAME MANAGER
        // ---------------------------------------------------------------------
        SerializedObject soManager = new SerializedObject(inGameManager);
        soManager.FindProperty("hudController").objectReferenceValue = hudCtrl;
        soManager.FindProperty("pauseMenu").objectReferenceValue = pauseCtrl;
        soManager.FindProperty("settingsMenu").objectReferenceValue = settingsCtrl;
        soManager.FindProperty("saveLoadMenu").objectReferenceValue = saveLoadCtrl;
        soManager.FindProperty("dialogueTerminal").objectReferenceValue = dialCtrl;
        soManager.FindProperty("gameOverScreen").objectReferenceValue = gameOverCtrl;
        soManager.FindProperty("inventoryCodex").objectReferenceValue = codexCtrl;
        soManager.FindProperty("uiAudioSource").objectReferenceValue = audioSource;
        soManager.ApplyModifiedProperties();

        // Set initial modal visibility
        pauseGO.SetActive(false);
        dialogueTerminalGO_SetActive(dialGO, false);
        gameOverGO.SetActive(false);
        codexGO.SetActive(false);

        return root;
    }

    private static void dialogueTerminalGO_SetActive(GameObject go, bool active)
    {
        go.SetActive(active);
    }

    // =========================================================================
    // HELPER: CREATE SETTINGS TERMINAL
    // =========================================================================
    private static GameObject CreateSettingsTerminal(GameObject parent, Sprite spWhite, Sprite spBrutalistFrame, TMP_FontAsset fontBase)
    {
        GameObject panel = CreateUIChild(parent, "SettingsSubTerminal", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-380, -290), new Vector2(380, 290));
        Image bg = panel.AddComponent<Image>();
        bg.sprite = spBrutalistFrame;
        bg.type = Image.Type.Sliced;
        bg.color = UITheme.DeepCharcoal;

        SettingsMenuController ctrl = panel.AddComponent<SettingsMenuController>();

        // Header
        GameObject hdr = CreateUIChild(panel, "Header", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -40), new Vector2(-24, -14));
        TextMeshProUGUI hdrTxt = hdr.AddComponent<TextMeshProUGUI>();
        hdrTxt.font = fontBase;
        hdrTxt.fontSize = 18;
        hdrTxt.fontStyle = FontStyles.Bold;
        hdrTxt.color = UITheme.BrightCyan;
        hdrTxt.text = "SYSTEM CALIBRATION // FACILITY SETTINGS";

        // Tab Buttons Container
        GameObject tabsContainer = CreateUIChild(panel, "Tabs", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -80), new Vector2(-24, -46));
        (Button aBtn, TextMeshProUGUI aTxt) = CreateTabButton(tabsContainer, "TabAudio", "01 AUDIO", 0, spWhite, fontBase);
        (Button dBtn, TextMeshProUGUI dTxt) = CreateTabButton(tabsContainer, "TabDisplay", "02 DISPLAY // CRT", 1, spWhite, fontBase);
        (Button cBtn, TextMeshProUGUI cTxt) = CreateTabButton(tabsContainer, "TabControls", "03 NEURAL LINK", 2, spWhite, fontBase);

        // Content Area
        GameObject contentArea = CreateUIChild(panel, "ContentArea", new Vector2(0, 0), new Vector2(1, 1), new Vector2(24, 60), new Vector2(-24, -90));

        // Audio Panel
        GameObject audioP = CreateUIChild(contentArea, "AudioPanel", Vector2.zero, Vector2.one);
        (Slider mSld, TextMeshProUGUI mLbl) = CreateSliderRow(audioP, "MasterVolume", "MASTER VOLUME", 0, spWhite, fontBase);
        (Slider sSld, TextMeshProUGUI sLbl) = CreateSliderRow(audioP, "SfxVolume", "SFX / COMBAT AUDIO", 1, spWhite, fontBase);
        (Slider muSld, TextMeshProUGUI muLbl) = CreateSliderRow(audioP, "MusicVolume", "AMBIENT / LAB RESONANCE", 2, spWhite, fontBase);

        // Display Panel
        GameObject dispP = CreateUIChild(contentArea, "DisplayPanel", Vector2.zero, Vector2.one);
        (Toggle fsTgl, TextMeshProUGUI fsLbl) = CreateToggleRow(dispP, "Fullscreen", "DISPLAY MODE", "[ FULLSCREEN: ENABLED ]", 0, spWhite, fontBase);
        (Button resBtn, TextMeshProUGUI resLbl) = CreateButtonRow(dispP, "Resolution", "SCREEN RESOLUTION", "1920 × 1080", 1, spWhite, fontBase);
        (Toggle vsTgl, TextMeshProUGUI vsLbl) = CreateToggleRow(dispP, "Vsync", "VERTICAL SYNC", "[ V-SYNC: LOCKED 60HZ ]", 2, spWhite, fontBase);
        (Slider scSld, TextMeshProUGUI scLbl) = CreateSliderRow(dispP, "Scanlines", "CRT SCANLINE INTENSITY", 3, spWhite, fontBase);
        dispP.SetActive(false);

        // Controls Panel
        GameObject ctrlP = CreateUIChild(contentArea, "ControlsPanel", Vector2.zero, Vector2.one);
        (Slider sensSld, TextMeshProUGUI sensLbl) = CreateSliderRow(ctrlP, "Sensitivity", "MOUSE SENSITIVITY", 0, spWhite, fontBase);
        (Toggle invTgl, TextMeshProUGUI invLbl) = CreateToggleRow(ctrlP, "InvertY", "INVERT PITCH (Y-AXIS)", "[ INVERT Y: NORMAL ]", 1, spWhite, fontBase);
        ctrlP.SetActive(false);

        // Bottom Action Buttons
        GameObject actionArea = CreateUIChild(panel, "ActionButtons", new Vector2(0, 0), new Vector2(1, 0), new Vector2(24, 16), new Vector2(-24, 52));
        Button applyBtn = CreateActionButton(actionArea, "ApplyBtn", "APPLY & PERSIST", new Vector2(0, 0), new Vector2(0.38f, 1), spWhite, fontBase);
        Button defBtn = CreateActionButton(actionArea, "DefaultsBtn", "RESTORE DEFAULTS", new Vector2(0.42f, 0), new Vector2(0.72f, 1), spWhite, fontBase);
        Button backBtn = CreateActionButton(actionArea, "BackBtn", "RETURN", new Vector2(0.76f, 0), new Vector2(1f, 1), spWhite, fontBase);

        // Wire SettingsMenuController
        SerializedObject so = new SerializedObject(ctrl);
        so.FindProperty("audioTabButton").objectReferenceValue = aBtn;
        so.FindProperty("displayTabButton").objectReferenceValue = dBtn;
        so.FindProperty("controlsTabButton").objectReferenceValue = cBtn;
        so.FindProperty("audioTabText").objectReferenceValue = aTxt;
        so.FindProperty("displayTabText").objectReferenceValue = dTxt;
        so.FindProperty("controlsTabText").objectReferenceValue = cTxt;
        so.FindProperty("audioPanel").objectReferenceValue = audioP;
        so.FindProperty("displayPanel").objectReferenceValue = dispP;
        so.FindProperty("controlsPanel").objectReferenceValue = ctrlP;
        so.FindProperty("masterVolumeSlider").objectReferenceValue = mSld;
        so.FindProperty("masterVolumeLabel").objectReferenceValue = mLbl;
        so.FindProperty("sfxVolumeSlider").objectReferenceValue = sSld;
        so.FindProperty("sfxVolumeLabel").objectReferenceValue = sLbl;
        so.FindProperty("musicVolumeSlider").objectReferenceValue = muSld;
        so.FindProperty("musicVolumeLabel").objectReferenceValue = muLbl;
        so.FindProperty("fullscreenToggle").objectReferenceValue = fsTgl;
        so.FindProperty("fullscreenToggleLabel").objectReferenceValue = fsLbl;
        so.FindProperty("resolutionButton").objectReferenceValue = resBtn;
        so.FindProperty("resolutionLabel").objectReferenceValue = resLbl;
        so.FindProperty("vsyncToggle").objectReferenceValue = vsTgl;
        so.FindProperty("vsyncToggleLabel").objectReferenceValue = vsLbl;
        so.FindProperty("scanlineSlider").objectReferenceValue = scSld;
        so.FindProperty("scanlineLabel").objectReferenceValue = scLbl;
        so.FindProperty("sensitivitySlider").objectReferenceValue = sensSld;
        so.FindProperty("sensitivityLabel").objectReferenceValue = sensLbl;
        so.FindProperty("invertYToggle").objectReferenceValue = invTgl;
        so.FindProperty("invertYLabel").objectReferenceValue = invLbl;
        so.FindProperty("applyButton").objectReferenceValue = applyBtn;
        so.FindProperty("defaultsButton").objectReferenceValue = defBtn;
        so.FindProperty("backButton").objectReferenceValue = backBtn;
        so.ApplyModifiedProperties();

        panel.SetActive(false);
        return panel;
    }

    // =========================================================================
    // HELPER: CREATE SAVE / LOAD TERMINAL
    // =========================================================================
    private static GameObject CreateSaveLoadTerminal(GameObject parent, Sprite spWhite, Sprite spBrutalistFrame, TMP_FontAsset fontBase)
    {
        GameObject panel = CreateUIChild(parent, "SaveLoadSubTerminal", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-420, -310), new Vector2(420, 310));
        Image bg = panel.AddComponent<Image>();
        bg.sprite = spBrutalistFrame;
        bg.type = Image.Type.Sliced;
        bg.color = UITheme.DeepCharcoal;

        SaveLoadMenuController ctrl = panel.AddComponent<SaveLoadMenuController>();

        // Header
        GameObject hdr = CreateUIChild(panel, "HeaderTitle", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -40), new Vector2(-24, -14));
        TextMeshProUGUI hdrTxt = hdr.AddComponent<TextMeshProUGUI>();
        hdrTxt.font = fontBase;
        hdrTxt.fontSize = 20;
        hdrTxt.fontStyle = FontStyles.Bold;
        hdrTxt.color = UITheme.BrightCyan;
        hdrTxt.text = "DATA ARCHIVE // RECORD MATRIX";

        GameObject subHdr = CreateUIChild(panel, "SubHeader", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -66), new Vector2(-24, -44));
        TextMeshProUGUI subTxt = subHdr.AddComponent<TextMeshProUGUI>();
        subTxt.font = fontBase;
        subTxt.fontSize = 11;
        subTxt.fontStyle = FontStyles.Bold;
        subTxt.color = UITheme.WarningOrange;
        subTxt.text = "FACILITY B-03 // SELECT SECTOR SLOT TO OVERWRITE DATA RECORD";

        // Slots Container
        GameObject slotsArea = CreateUIChild(panel, "SlotsArea", new Vector2(0, 0), new Vector2(1, 1), new Vector2(24, 66), new Vector2(-24, -76));
        (Button s1Btn, TextMeshProUGUI s1Title, TextMeshProUGUI s1Det, Button s1Purge) = CreateSlotRow(slotsArea, "Slot_01", 1, 0, spWhite, fontBase);
        (Button s2Btn, TextMeshProUGUI s2Title, TextMeshProUGUI s2Det, Button s2Purge) = CreateSlotRow(slotsArea, "Slot_02", 2, 1, spWhite, fontBase);
        (Button s3Btn, TextMeshProUGUI s3Title, TextMeshProUGUI s3Det, Button s3Purge) = CreateSlotRow(slotsArea, "Slot_03", 3, 2, spWhite, fontBase);

        // Return Button
        GameObject returnArea = CreateUIChild(panel, "ReturnArea", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-180, 18), new Vector2(-24, 52));
        Button backBtn = CreateActionButton(returnArea, "ReturnBtn", "RETURN", Vector2.zero, Vector2.one, spWhite, fontBase);

        // Confirmation Modal
        GameObject confModal = CreateUIChild(panel, "ConfirmModal", Vector2.zero, Vector2.one);
        Image confBg = confModal.AddComponent<Image>();
        confBg.sprite = spWhite;
        confBg.color = new Color(UITheme.NearBlack.r, UITheme.NearBlack.g, UITheme.NearBlack.b, 0.95f);

        GameObject confCard = CreateUIChild(confModal, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-250, -110), new Vector2(250, 110));
        Image confCardBg = confCard.AddComponent<Image>();
        confCardBg.sprite = spBrutalistFrame;
        confCardBg.type = Image.Type.Sliced;
        confCardBg.color = UITheme.DeepCharcoal;

        GameObject confPrompt = CreateUIChild(confCard, "Prompt", new Vector2(0, 0.4f), new Vector2(1, 1), new Vector2(16, 0), new Vector2(-16, -16));
        TextMeshProUGUI confPromptTxt = confPrompt.AddComponent<TextMeshProUGUI>();
        confPromptTxt.font = fontBase;
        confPromptTxt.fontSize = 13;
        confPromptTxt.fontStyle = FontStyles.Bold;
        confPromptTxt.alignment = TextAlignmentOptions.Center;
        confPromptTxt.color = UITheme.TextBright;
        confPromptTxt.text = "CONFIRM OVERWRITE // EXISTING ARCHIVE WILL BE PURGED.";

        Button confYes = CreateActionButton(confCard, "YesBtn", "CONFIRM [PROCEED]", new Vector2(0.08f, 0.12f), new Vector2(0.46f, 0.4f), spWhite, fontBase);
        Button confNo = CreateActionButton(confCard, "NoBtn", "CANCEL", new Vector2(0.54f, 0.12f), new Vector2(0.92f, 0.4f), spWhite, fontBase);
        confModal.SetActive(false);

        // Wire SaveLoadMenuController
        SerializedObject so = new SerializedObject(ctrl);
        so.FindProperty("headerTitleText").objectReferenceValue = hdrTxt;
        so.FindProperty("modeSubheaderText").objectReferenceValue = subTxt;
        so.FindProperty("slot1Button").objectReferenceValue = s1Btn;
        so.FindProperty("slot1TitleText").objectReferenceValue = s1Title;
        so.FindProperty("slot1DetailsText").objectReferenceValue = s1Det;
        so.FindProperty("slot1PurgeButton").objectReferenceValue = s1Purge;
        so.FindProperty("slot2Button").objectReferenceValue = s2Btn;
        so.FindProperty("slot2TitleText").objectReferenceValue = s2Title;
        so.FindProperty("slot2DetailsText").objectReferenceValue = s2Det;
        so.FindProperty("slot2PurgeButton").objectReferenceValue = s2Purge;
        so.FindProperty("slot3Button").objectReferenceValue = s3Btn;
        so.FindProperty("slot3TitleText").objectReferenceValue = s3Title;
        so.FindProperty("slot3DetailsText").objectReferenceValue = s3Det;
        so.FindProperty("slot3PurgeButton").objectReferenceValue = s3Purge;
        so.FindProperty("backButton").objectReferenceValue = backBtn;
        so.FindProperty("confirmOverlay").objectReferenceValue = confModal;
        so.FindProperty("confirmPromptText").objectReferenceValue = confPromptTxt;
        so.FindProperty("confirmYesButton").objectReferenceValue = confYes;
        so.FindProperty("confirmNoButton").objectReferenceValue = confNo;
        so.ApplyModifiedProperties();

        panel.SetActive(false);
        return panel;
    }

    // =========================================================================
    // STEP 4: INTEGRATE INTO GAMEPLAY SCENE (Act1_Origin.unity)
    // =========================================================================
    public static GameObject IntegrateIntoGameplayScene(GameObject inGamePrefabAsset)
    {
        Debug.Log(">>> Step 4: Integrating In-Game UI into " + GAMEPLAY_SCENE);
        var scene = EditorSceneManager.OpenScene(GAMEPLAY_SCENE, OpenSceneMode.Single);

        // Remove obsolete legacy IMGUI components if present
        var oldHud = UnityEngine.Object.FindObjectsByType<LastGod.Player.PlayerControlsHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var oh in oldHud)
        {
            Debug.Log(">>> Removing obsolete PlayerControlsHUD: " + oh.gameObject.name);
            UnityEngine.Object.DestroyImmediate(oh);
        }

        var oldPause = UnityEngine.Object.FindObjectsByType<LastGod.ThirdPerson.UI.PauseMenuUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var op in oldPause)
        {
            Debug.Log(">>> Removing obsolete PauseMenuUI: " + op.gameObject.name);
            UnityEngine.Object.DestroyImmediate(op);
        }

        var oldVital = UnityEngine.Object.FindObjectsByType<LastGod.ThirdPerson.UI.VitalStabilityHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var ov in oldVital)
        {
            Debug.Log(">>> Removing obsolete VitalStabilityHUD: " + ov.gameObject.name);
            UnityEngine.Object.DestroyImmediate(ov);
        }

        var oldAmmo = UnityEngine.Object.FindObjectsByType<LastGod.UI.AmmoHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var oa in oldAmmo)
        {
            Debug.Log(">>> Removing obsolete AmmoHUD: " + oa.gameObject.name);
            UnityEngine.Object.DestroyImmediate(oa);
        }

        // Remove old UI_InGame_Root if present
        var existingRoots = UnityEngine.Object.FindObjectsByType<UIInGameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var er in existingRoots)
        {
            UnityEngine.Object.DestroyImmediate(er.gameObject);
        }

        // Instantiate fresh Prefab
        GameObject instance = PrefabUtility.InstantiatePrefab(inGamePrefabAsset) as GameObject;
        instance.name = "UI_InGame_Root";

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log(">>> Gameplay scene updated and saved successfully: " + GAMEPLAY_SCENE);
        return instance;
    }

    // =========================================================================
    // STEP 5: RENDER MULTI-RESOLUTION SCREENSHOTS
    // =========================================================================
    public static void RenderPhase02Screenshots(GameObject inGameInstance)
    {
        Debug.Log(">>> Step 5: Capturing high-resolution screenshots for Phase 02 screens...");
        EnsureDirectory(SCREENSHOT_DIR);
        EnsureDirectory(DREAM_LOOP_DIR);
        EnsureDirectory(BRAIN_DIR);

        UIInGameManager mgr = inGameInstance.GetComponent<UIInGameManager>();
        Canvas canvas = inGameInstance.GetComponent<Canvas>();
        CanvasScaler scaler = inGameInstance.GetComponent<CanvasScaler>();

        GameObject camObj = new GameObject("Screenshot_Render_Cam");
        Camera cam = camObj.AddComponent<Camera>();
        Camera sceneCam = Camera.main;
        if (sceneCam != null)
        {
            cam.transform.position = sceneCam.transform.position;
            cam.transform.rotation = sceneCam.transform.rotation;
            cam.orthographic = sceneCam.orthographic;
            cam.orthographicSize = sceneCam.orthographicSize;
            cam.fieldOfView = sceneCam.fieldOfView;
            cam.nearClipPlane = sceneCam.nearClipPlane;
            cam.farClipPlane = sceneCam.farClipPlane;
            cam.clearFlags = sceneCam.clearFlags;
            cam.backgroundColor = sceneCam.backgroundColor;
        }
        else
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = UITheme.NearBlack;
        }
        cam.cullingMask = ~0;

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;

        (int width, int height, string label)[] resolutions = new[]
        {
            (1920, 1080, "1080p_FHD"),
            (1280, 720,  "720p_HD"),
            (2560, 1440, "1440p_2K"),
            (2560, 1080, "21x9_Ultrawide")
        };

        // All Phase 02 screens to capture
        string[] screenNames = new[]
        {
            "InGameHUD",
            "PauseMenu",
            "SettingsMatrix",
            "ArchiveTerminal",
            "DialogueTransmission",
            "GameOverBreach",
            "InventoryCodex"
        };

        foreach (string scr in screenNames)
        {
            SetScreenVisible(mgr, scr);

            foreach (var res in resolutions)
            {
                scaler.referenceResolution = new Vector2(res.width, res.height);
                Canvas.ForceUpdateCanvases();
                foreach (var tmp in inGameInstance.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    tmp.ForceMeshUpdate(true, true);
                }

                RenderTexture rt = new RenderTexture(res.width, res.height, 24, RenderTextureFormat.ARGB32);
                cam.targetTexture = rt;
                cam.Render();

                RenderTexture.active = rt;
                Texture2D capture = new Texture2D(res.width, res.height, TextureFormat.RGB24, false);
                capture.ReadPixels(new Rect(0, 0, res.width, res.height), 0, 0);
                capture.Apply();

                byte[] pngBytes = capture.EncodeToPNG();
                string fileName = $"{scr}_{res.label}.png";

                // Save to project screenshots, dream-loop, and brain directory
                File.WriteAllBytes(Path.Combine(SCREENSHOT_DIR, fileName), pngBytes);
                File.WriteAllBytes(Path.Combine(BRAIN_DIR, fileName), pngBytes);

                if (res.label == "1080p_FHD")
                {
                    File.WriteAllBytes(Path.Combine(DREAM_LOOP_DIR, $"{scr}_1080p.png"), pngBytes);
                }

                UnityEngine.Object.DestroyImmediate(capture);
                cam.targetTexture = null;
                RenderTexture.active = null;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }

        // Restore canvas to overlay
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = null;
        UnityEngine.Object.DestroyImmediate(camObj);

        // Reset to gameplay HUD
        SetScreenVisible(mgr, "InGameHUD");

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log(">>> All Phase 02 screens successfully captured across 1080p, 720p, 1440p, and 21:9 Ultrawide!");
    }

    private static void SetScreenVisible(UIInGameManager mgr, string screen)
    {
        mgr.HUD.SetVisible(screen == "InGameHUD");
        mgr.PauseMenu.gameObject.SetActive(screen == "PauseMenu" || screen == "SettingsMatrix" || screen == "ArchiveTerminal");
        mgr.PauseMenu.transform.Find("PauseMainPanel").gameObject.SetActive(screen == "PauseMenu");
        mgr.SettingsMenu.SetVisible(screen == "SettingsMatrix");
        mgr.SaveLoadMenu.SetVisible(screen == "ArchiveTerminal", SaveLoadMenuController.TerminalMode.Save);
        mgr.Dialogue.SetVisible(screen == "DialogueTransmission");
        mgr.GameOver.SetVisible(screen == "GameOverBreach");
        mgr.Codex.SetVisible(screen == "InventoryCodex");
    }

    // =========================================================================
    // UI BUILDER PRIMITIVES & HELPERS
    // =========================================================================
    private static GameObject CreateUIChild(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        return go;
    }

    private static GameObject CreateUIChild(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        return CreateUIChild(parent, name, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
    }

    private static UIMenuItem CreateMenuItem(GameObject parent, string name, string number, string title, string sublabel, int index, Sprite spWhite, Sprite spDot, TMP_FontAsset font)
    {
        float itemHeight = 64f;
        float itemSpacing = 10f;
        float topOffset = index * (itemHeight + itemSpacing);

        GameObject itemGO = CreateUIChild(parent, name, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -(topOffset + itemHeight)), new Vector2(0, -topOffset));
        UIMenuItem menuItem = itemGO.AddComponent<UIMenuItem>();

        GameObject bgObj = CreateUIChild(itemGO, "BackgroundHighlight", Vector2.zero, Vector2.one);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.sprite = spWhite;
        bgImg.color = UITheme.DeepCharcoal;

        GameObject numObj = CreateUIChild(itemGO, "ItemNumber", new Vector2(0, 0), new Vector2(0.12f, 1), new Vector2(16, 0), new Vector2(0, 0));
        TextMeshProUGUI numTxt = numObj.AddComponent<TextMeshProUGUI>();
        numTxt.font = font;
        numTxt.fontSize = 20;
        numTxt.fontStyle = FontStyles.Bold;
        numTxt.alignment = TextAlignmentOptions.MidlineLeft;
        numTxt.color = UITheme.LightMetal;
        numTxt.text = number;

        GameObject titleObj = CreateUIChild(itemGO, "ItemTitle", new Vector2(0.14f, 0.38f), new Vector2(1, 1), new Vector2(0, 0), new Vector2(-16, 0));
        TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
        titleTxt.font = font;
        titleTxt.fontSize = 20;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.alignment = TextAlignmentOptions.BottomLeft;
        titleTxt.color = UITheme.TextBright;
        titleTxt.text = title;

        GameObject subObj = CreateUIChild(itemGO, "ItemSublabel", new Vector2(0.14f, 0.05f), new Vector2(1, 0.38f), new Vector2(0, 0), new Vector2(-16, 0));
        TextMeshProUGUI subTxt = subObj.AddComponent<TextMeshProUGUI>();
        subTxt.font = font;
        subTxt.fontSize = 10;
        subTxt.alignment = TextAlignmentOptions.TopLeft;
        subTxt.color = Color.clear;
        subTxt.text = sublabel;

        GameObject ruleObj = CreateUIChild(itemGO, "CyanRule", new Vector2(0.14f, 0.02f), new Vector2(1, 0.05f));
        Image ruleImg = ruleObj.AddComponent<Image>();
        ruleImg.sprite = spWhite;
        ruleImg.color = Color.clear;

        GameObject tickObj = CreateUIChild(itemGO, "IndicatorTick", new Vector2(0.09f, 0.50f), new Vector2(0.09f, 0.50f));
        RectTransform tickRect = tickObj.GetComponent<RectTransform>();
        tickRect.sizeDelta = new Vector2(8, 8);
        Image tickImg = tickObj.AddComponent<Image>();
        tickImg.sprite = spDot != null ? spDot : spWhite;
        tickImg.color = Color.clear;

        SerializedObject so = new SerializedObject(menuItem);
        so.FindProperty("numberText").objectReferenceValue = numTxt;
        so.FindProperty("titleText").objectReferenceValue = titleTxt;
        so.FindProperty("sublabelText").objectReferenceValue = subTxt;
        so.FindProperty("cyanRule").objectReferenceValue = ruleImg;
        so.FindProperty("indicatorTick").objectReferenceValue = tickImg;
        so.FindProperty("backgroundHighlight").objectReferenceValue = bgImg;
        so.FindProperty("itemNumber").stringValue = number;
        so.FindProperty("itemTitle").stringValue = title;
        so.FindProperty("itemSublabel").stringValue = sublabel;
        so.ApplyModifiedProperties();

        return menuItem;
    }

    private static (Button, TextMeshProUGUI) CreateTabButton(GameObject parent, string name, string label, int index, Sprite spWhite, TMP_FontAsset font)
    {
        float tabW = 0.32f;
        GameObject btnObj = CreateUIChild(parent, name, new Vector2(index * 0.34f, 0), new Vector2(index * 0.34f + tabW, 1));
        Image img = btnObj.AddComponent<Image>();
        img.sprite = spWhite;
        img.color = UITheme.DarkBlueGrey;
        Button btn = btnObj.AddComponent<Button>();

        GameObject txtObj = CreateUIChild(btnObj, "Text", Vector2.zero, Vector2.one);
        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.font = font;
        txt.fontSize = 11;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = index == 0 ? UITheme.BrightCyan : UITheme.LightMetal;
        txt.text = label;

        return (btn, txt);
    }

    private static (Slider, TextMeshProUGUI) CreateSliderRow(GameObject parent, string name, string label, int index, Sprite spWhite, TMP_FontAsset font)
    {
        float rowH = 46f;
        float yOffset = index * 52f;
        GameObject row = CreateUIChild(parent, name, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -(yOffset + rowH)), new Vector2(0, -yOffset));

        GameObject lblObj = CreateUIChild(row, "Label", new Vector2(0, 0), new Vector2(0.48f, 1));
        TextMeshProUGUI lbl = lblObj.AddComponent<TextMeshProUGUI>();
        lbl.font = font;
        lbl.fontSize = 12;
        lbl.fontStyle = FontStyles.Bold;
        lbl.color = UITheme.BrightCyan;
        lbl.text = label;

        GameObject sldObj = CreateUIChild(row, "Slider", new Vector2(0.50f, 0.25f), new Vector2(0.82f, 0.75f));
        Slider sld = sldObj.AddComponent<Slider>();

        GameObject track = CreateUIChild(sldObj, "Background", Vector2.zero, Vector2.one);
        Image trackImg = track.AddComponent<Image>();
        trackImg.sprite = spWhite;
        trackImg.color = UITheme.NearBlack;

        GameObject fillArea = CreateUIChild(sldObj, "Fill Area", Vector2.zero, Vector2.one);
        GameObject fill = CreateUIChild(fillArea, "Fill", Vector2.zero, Vector2.one);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.sprite = spWhite;
        fillImg.color = UITheme.TechnologyCyan;
        sld.fillRect = fill.GetComponent<RectTransform>();
        sld.value = 0.8f;

        GameObject valObj = CreateUIChild(row, "ValText", new Vector2(0.84f, 0), new Vector2(1, 1));
        TextMeshProUGUI valTxt = valObj.AddComponent<TextMeshProUGUI>();
        valTxt.font = fontBaseFallback(font);
        valTxt.fontSize = 12;
        valTxt.fontStyle = FontStyles.Bold;
        valTxt.alignment = TextAlignmentOptions.MidlineRight;
        valTxt.color = UITheme.BrightCyan;
        valTxt.text = "80%";

        return (sld, valTxt);
    }

    private static (Toggle, TextMeshProUGUI) CreateToggleRow(GameObject parent, string name, string label, string stateText, int index, Sprite spWhite, TMP_FontAsset font)
    {
        float rowH = 46f;
        float yOffset = index * 52f;
        GameObject row = CreateUIChild(parent, name, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -(yOffset + rowH)), new Vector2(0, -yOffset));

        GameObject lblObj = CreateUIChild(row, "Label", new Vector2(0, 0), new Vector2(0.48f, 1));
        TextMeshProUGUI lbl = lblObj.AddComponent<TextMeshProUGUI>();
        lbl.font = font;
        lbl.fontSize = 12;
        lbl.fontStyle = FontStyles.Bold;
        lbl.color = UITheme.BrightCyan;
        lbl.text = label;

        GameObject tglObj = CreateUIChild(row, "Toggle", new Vector2(0.50f, 0.1f), new Vector2(1, 0.9f));
        Toggle tgl = tglObj.AddComponent<Toggle>();

        GameObject valObj = CreateUIChild(tglObj, "ValText", Vector2.zero, Vector2.one);
        TextMeshProUGUI valTxt = valObj.AddComponent<TextMeshProUGUI>();
        valTxt.font = fontBaseFallback(font);
        valTxt.fontSize = 12;
        valTxt.fontStyle = FontStyles.Bold;
        valTxt.alignment = TextAlignmentOptions.MidlineLeft;
        valTxt.color = UITheme.BrightCyan;
        valTxt.text = stateText;

        return (tgl, valTxt);
    }

    private static (Button, TextMeshProUGUI) CreateButtonRow(GameObject parent, string name, string label, string btnText, int index, Sprite spWhite, TMP_FontAsset font)
    {
        float rowH = 46f;
        float yOffset = index * 52f;
        GameObject row = CreateUIChild(parent, name, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -(yOffset + rowH)), new Vector2(0, -yOffset));

        GameObject lblObj = CreateUIChild(row, "Label", new Vector2(0, 0), new Vector2(0.48f, 1));
        TextMeshProUGUI lbl = lblObj.AddComponent<TextMeshProUGUI>();
        lbl.font = font;
        lbl.fontSize = 12;
        lbl.fontStyle = FontStyles.Bold;
        lbl.color = UITheme.BrightCyan;
        lbl.text = label;

        GameObject btnObj = CreateUIChild(row, "Btn", new Vector2(0.50f, 0.1f), new Vector2(0.95f, 0.9f));
        Image bImg = btnObj.AddComponent<Image>();
        bImg.sprite = spWhite;
        bImg.color = UITheme.DarkBlueGrey;
        Button btn = btnObj.AddComponent<Button>();

        GameObject valObj = CreateUIChild(btnObj, "Text", Vector2.zero, Vector2.one);
        TextMeshProUGUI valTxt = valObj.AddComponent<TextMeshProUGUI>();
        valTxt.font = fontBaseFallback(font);
        valTxt.fontSize = 12;
        valTxt.fontStyle = FontStyles.Bold;
        valTxt.alignment = TextAlignmentOptions.Center;
        valTxt.color = UITheme.BrightCyan;
        valTxt.text = btnText;

        return (btn, valTxt);
    }

    private static Button CreateActionButton(GameObject parent, string name, string label, Vector2 aMin, Vector2 aMax, Sprite spWhite, TMP_FontAsset font)
    {
        GameObject btnObj = CreateUIChild(parent, name, aMin, aMax);
        Image img = btnObj.AddComponent<Image>();
        img.sprite = spWhite;
        img.color = UITheme.DarkBlueGrey;
        Button btn = btnObj.AddComponent<Button>();

        GameObject txtObj = CreateUIChild(btnObj, "Text", Vector2.zero, Vector2.one);
        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.font = fontBaseFallback(font);
        txt.fontSize = 12;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = UITheme.BrightCyan;
        txt.text = label;

        return btn;
    }

    private static (Button, TextMeshProUGUI, TextMeshProUGUI, Button) CreateSlotRow(GameObject parent, string name, int slotNum, int index, Sprite spWhite, TMP_FontAsset font)
    {
        float rowH = 68f;
        float yOffset = index * 76f;
        GameObject row = CreateUIChild(parent, name, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -(yOffset + rowH)), new Vector2(0, -yOffset));
        Image rBg = row.AddComponent<Image>();
        rBg.sprite = spWhite;
        rBg.color = UITheme.NearBlack;
        Button mainBtn = row.AddComponent<Button>();

        GameObject titleObj = CreateUIChild(row, "Title", new Vector2(0, 0.45f), new Vector2(0.85f, 1), new Vector2(16, 0), new Vector2(0, -6));
        TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
        titleTxt.font = fontBaseFallback(font);
        titleTxt.fontSize = 14;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.color = UITheme.BrightCyan;
        titleTxt.text = $"ARCHIVE // 0{slotNum}   [CLASSIFIED RECORD]";

        GameObject detObj = CreateUIChild(row, "Details", new Vector2(0, 0), new Vector2(0.85f, 0.45f), new Vector2(16, 6), new Vector2(0, 0));
        TextMeshProUGUI detTxt = detObj.AddComponent<TextMeshProUGUI>();
        detTxt.font = fontBaseFallback(font);
        detTxt.fontSize = 10;
        detTxt.color = UITheme.TextMuted;
        detTxt.text = "LOCATION: SECTOR B-03 CATWALK   CHECKPOINT: CP-01\nVITAL INTEGRITY: 100%   TIMESTAMP: 2026-10-02 09:15:32 UTC";

        GameObject purgeObj = CreateUIChild(row, "PurgeBtn", new Vector2(0.86f, 0.15f), new Vector2(0.98f, 0.85f));
        Image pImg = purgeObj.AddComponent<Image>();
        pImg.sprite = spWhite;
        pImg.color = UITheme.DarkBlueGrey;
        Button purgeBtn = purgeObj.AddComponent<Button>();
        GameObject pTxtObj = CreateUIChild(purgeObj, "Text", Vector2.zero, Vector2.one);
        TextMeshProUGUI pTxt = pTxtObj.AddComponent<TextMeshProUGUI>();
        pTxt.font = fontBaseFallback(font);
        pTxt.fontSize = 10;
        pTxt.fontStyle = FontStyles.Bold;
        pTxt.alignment = TextAlignmentOptions.Center;
        pTxt.color = UITheme.WarningOrange;
        pTxt.text = "PURGE";

        return (mainBtn, titleTxt, detTxt, purgeBtn);
    }

    private static Button CreateCodexSlotButton(GameObject parent, string name, string label, int index, Sprite spWhite, TMP_FontAsset font)
    {
        float rowH = 50f;
        float yOffset = index * 58f;
        GameObject btnObj = CreateUIChild(parent, name, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -(yOffset + rowH)), new Vector2(0, -yOffset));
        Image img = btnObj.AddComponent<Image>();
        img.sprite = spWhite;
        img.color = UITheme.NearBlack;
        Button btn = btnObj.AddComponent<Button>();

        GameObject txtObj = CreateUIChild(btnObj, "Text", Vector2.zero, Vector2.one, new Vector2(14, 0), new Vector2(-14, 0));
        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.font = fontBaseFallback(font);
        txt.fontSize = 13;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.MidlineLeft;
        txt.color = UITheme.BrightCyan;
        txt.text = label;

        return btn;
    }

    private static TMP_FontAsset fontBaseFallback(TMP_FontAsset font)
    {
        return font;
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
