// BuildTheLastGodUIPipeline.cs
// Master Production Pipeline for THE LAST GOD — UI Visual Identity Phase 01
// Dark Ink | Industrial Brutalism | Graphic Novel | Scientific Documentation
// =========================================================================

using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using LastGod.UI;

public static class BuildTheLastGodUIPipeline
{
    const string PREFAB_PATH = "Assets/Art/UI/Prefabs/UI_Root_TheLastGod.prefab";
    const string MAIN_MENU_SCENE = "Assets/Scenes/MainMenu_Origin.unity";
    const string SCREENSHOT_DIR = "Assets/Art/UI/Screenshots";
    const string DREAM_LOOP_DIR = ".dream-loop";

    [MenuItem("The Last God/Execute Full UI Pipeline (Phase 01)", false, 10)]
    public static void ExecutePipeline()
    {
        Debug.Log(">>> [BuildTheLastGodUIPipeline] Beginning Phase 01 UI Pipeline...");

        // 1. Generate procedural UI sprite assets (borders, crosshairs, chevrons, dots)
        GenerateProceduralSprites();

        // 2. Configure texture importers for all UI textures
        ConfigureTextureImporters();

        // 3. Setup / Create TMP Font Assets
        SetupFontAssets();

        // 4. Build canonical UI_Root GameObject Hierarchy
        GameObject uiRootGO = BuildUIRootHierarchy();

        // 5. Save canonical prefab
        EnsureDirectory("Assets/Art/UI/Prefabs");
        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(uiRootGO, PREFAB_PATH);
        Debug.Log(">>> [BuildTheLastGodUIPipeline] Saved canonical UI Prefab: " + PREFAB_PATH);

        // 6. Integrate into MainMenu_Origin Scene
        GameObject sceneInstance = IntegrateIntoMainMenuScene(prefabAsset);

        // 7. Render Multi-Resolution Screenshots for all 3 screens
        RenderMultiResolutionScreenshots(sceneInstance);

        Debug.Log(">>> [BuildTheLastGodUIPipeline] Pipeline Phase 01 finished successfully!");
    }

    // =========================================================================
    // STEP 1: GENERATE PROCEDURAL SPRITES
    // =========================================================================
    public static void GenerateProceduralSprites()
    {
        Debug.Log(">>> Step 1: Generating procedural graphic novel UI sprites...");
        string texDir = "Assets/Art/UI/Textures";
        EnsureDirectory(texDir);

        // A. 1x1 Pure White Sprite (for solid fills, tintable panels, thin rules)
        CreateSolidTexture(Path.Combine(texDir, "UI_White.png"), 4, 4, Color.white);

        // B. 64x64 Brutalist Frame with 2px ink stroke and corner ticks
        CreateBrutalistFrameTexture(Path.Combine(texDir, "UI_Brutalist_Frame.png"), 64, 64);

        // C. 32x32 Registration Crosshair
        CreateCrosshairTexture(Path.Combine(texDir, "UI_Crosshair.png"), 32, 32);

        // D. 128x32 Industrial Hazard Chevrons (Warning Orange & Near Black)
        CreateHazardChevronTexture(Path.Combine(texDir, "UI_Hazard_Chevrons.png"), 128, 32);

        // E. 32x32 Soft Glowing Cyan Dot
        CreateGlowingDotTexture(Path.Combine(texDir, "UI_Dot_Cyan.png"), 32, UITheme.TechnologyCyan);

        // F. 8x8 Scanline Pattern
        CreateScanlineTexture(Path.Combine(texDir, "UI_Scanline.png"), 8, 8);

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    private static void CreateSolidTexture(string path, int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
    }

    private static void CreateBrutalistFrameTexture(string path, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color transparent = new Color(0, 0, 0, 0);
        Color stroke = new Color(0.7f, 0.75f, 0.8f, 1f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isBorder = (x < 2 || x >= width - 2 || y < 2 || y >= height - 2);
                bool isCornerTick = (x < 8 && (y == 4 || y == 5)) ||
                                    (x >= width - 8 && (y == 4 || y == 5)) ||
                                    (y < 8 && (x == 4 || x == 5)) ||
                                    (y >= height - 8 && (x == 4 || x == 5));

                if (isBorder || isCornerTick)
                    tex.SetPixel(x, y, stroke);
                else
                    tex.SetPixel(x, y, transparent);
            }
        }
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
    }

    private static void CreateCrosshairTexture(string path, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color transparent = new Color(0, 0, 0, 0);
        Color mark = new Color(0.43f, 0.89f, 1.0f, 0.9f); // Cyan mark

        int midX = width / 2;
        int midY = height / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isCross = (x == midX && (y >= 4 && y <= height - 5 && Mathf.Abs(y - midY) > 2)) ||
                               (y == midY && (x >= 4 && x <= width - 5 && Mathf.Abs(x - midX) > 2));
                bool isCenterDot = (Mathf.Abs(x - midX) <= 1 && Mathf.Abs(y - midY) <= 1);

                if (isCross || isCenterDot)
                    tex.SetPixel(x, y, mark);
                else
                    tex.SetPixel(x, y, transparent);
            }
        }
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
    }

    private static void CreateHazardChevronTexture(string path, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color orange = UITheme.WarningOrange;
        Color black = UITheme.NearBlack;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int stripe = ((x + y) / 10) % 2;
                tex.SetPixel(x, y, stripe == 0 ? orange : black);
            }
        }
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
    }

    private static void CreateGlowingDotTexture(string path, int size, Color baseColor)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01(1f - (dist / radius));
                alpha = Mathf.Pow(alpha, 1.8f);
                tex.SetPixel(x, y, new Color(baseColor.r, baseColor.g, baseColor.b, alpha));
            }
        }
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
    }

    private static void CreateScanlineTexture(string path, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color transparent = new Color(0, 0, 0, 0);
        Color line = new Color(0, 0, 0, 0.45f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tex.SetPixel(x, y, (y % 2 == 0) ? line : transparent);
            }
        }
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
    }

    // =========================================================================
    // STEP 2: CONFIGURE TEXTURE IMPORTERS
    // =========================================================================
    public static void ConfigureTextureImporters()
    {
        Debug.Log(">>> Step 2: Configuring Texture Importers for all UI assets...");
        string[] texPaths = Directory.GetFiles("Assets/Art/UI/Textures", "*.*", SearchOption.AllDirectories);

        foreach (string fullPath in texPaths)
        {
            if (fullPath.EndsWith(".meta")) continue;
            string assetPath = fullPath.Replace('\\', '/');
            if (assetPath.StartsWith(Application.dataPath))
            {
                assetPath = "Assets" + assetPath.Substring(Application.dataPath.Length);
            }

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                bool modified = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    modified = true;
                }
                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    modified = true;
                }
                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    modified = true;
                }
                if (importer.wrapMode != TextureWrapMode.Clamp)
                {
                    importer.wrapMode = TextureWrapMode.Clamp;
                    modified = true;
                }
                if (importer.maxTextureSize < 2048)
                {
                    importer.maxTextureSize = 2048;
                    modified = true;
                }
                if (modified)
                {
                    importer.SaveAndReimport();
                }
            }
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    // =========================================================================
    // STEP 3: SETUP FONT ASSETS
    // =========================================================================
    private static TMP_FontAsset _industrialFont;
    private static TMP_FontAsset _grotesqueFont;
    private static TMP_FontAsset _monoFont;
    private static TMP_FontAsset _defaultFont;

    public static void SetupFontAssets()
    {
        Debug.Log(">>> Step 3: Setting up TextMesh Pro font assets...");
        _defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (_defaultFont == null)
        {
            _defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        }

        _industrialFont = _defaultFont;
        _grotesqueFont = _defaultFont;
        _monoFont = _defaultFont;

        Debug.Log($">>> TMP Fonts: Industrial={_industrialFont?.name}, Grotesque={_grotesqueFont?.name}, Mono={_monoFont?.name}");
    }

    private static bool IsValidTMPFont(TMP_FontAsset font)
    {
        if (font == null) return false;
        if (font.atlasTextures == null || font.atlasTextures.Length == 0 || font.atlasTextures[0] == null) return false;
        return true;
    }

    private static TMP_FontAsset GetOrCreateTMPFont(string ttfPath, string assetName)
    {
        string assetPath = $"Assets/Art/UI/Fonts/{assetName}.asset";
        TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
        if (IsValidTMPFont(existing)) return existing;

        Font font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
        if (font != null)
        {
            try
            {
                TMP_FontAsset created = TMP_FontAsset.CreateFontAsset(font, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
                if (created != null && IsValidTMPFont(created))
                {
                    created.name = assetName;
                    AssetDatabase.CreateAsset(created, assetPath);
                    if (created.atlasTextures != null && created.atlasTextures.Length > 0 && created.atlasTextures[0] != null)
                    {
                        created.atlasTextures[0].name = assetName + " Atlas";
                        AssetDatabase.AddObjectToAsset(created.atlasTextures[0], created);
                    }
                    AssetDatabase.SaveAssets();
                    return created;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Could not auto-generate SDF for {ttfPath}: {ex.Message}");
            }
        }
        return null;
    }

    // =========================================================================
    // STEP 4: BUILD CANONICAL UI_ROOT HIERARCHY
    // =========================================================================
    public static GameObject BuildUIRootHierarchy()
    {
        Debug.Log(">>> Step 4: Assembling canonical UI_Root GameObject hierarchy...");

        // Load Sprites
        Sprite spWhite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/UI_White.png");
        Sprite spFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/UI_Brutalist_Frame.png");
        Sprite spCrosshair = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/UI_Crosshair.png");
        Sprite spHazard = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/UI_Hazard_Chevrons.png");
        Sprite spDotCyan = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/UI_Dot_Cyan.png");
        Sprite spScanline = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/UI_Scanline.png");

        Sprite spLabSilo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/Lab_Silo_03.jpg");
        Sprite spLabContainment = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/Lab_Containment_01.jpg");
        Sprite spEmblemCrown = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/Emblem_IndustrialCrown_04.jpg");
        Sprite spEmblemSeal = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/Emblem_ContainmentSeal_01.jpg");
        Sprite spDossierSheet = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/Dossier_SpecimenSheet_02.jpg");
        Sprite spDossierFolder = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Textures/Dossier_ClassifiedCutout_03.jpg");

        // UI_Root
        GameObject root = new GameObject("UI_Root");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<GraphicRaycaster>();
        UIRootManager rootManager = root.AddComponent<UIRootManager>();

        // Audio Source
        AudioSource audioSrc = root.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;

        // =====================================================================
        // VIEW B: MAIN MENU (Section 07, 08, 09, 11, 12, 24)
        // =====================================================================
        GameObject viewMainMenu = CreateUIChild(root, "View_MainMenu", Vector2.zero, Vector2.one);
        CanvasGroup menuGroup = viewMainMenu.AddComponent<CanvasGroup>();
        MainMenuController menuCtrl = viewMainMenu.AddComponent<MainMenuController>();

        // Layer 1: Background
        GameObject bgLayer = CreateUIChild(viewMainMenu, "01_Background", Vector2.zero, Vector2.one);
        Image bgImg = bgLayer.AddComponent<Image>();
        bgImg.sprite = spWhite;
        bgImg.color = UITheme.NearBlack; // #080B0F

        // Layer 2: Illustration (Section 08, 09)
        GameObject illustLayer = CreateUIChild(viewMainMenu, "02_Illustration", new Vector2(0.24f, 0f), new Vector2(1f, 1f));
        Image illustImg = illustLayer.AddComponent<Image>();
        illustImg.sprite = spLabSilo;
        illustImg.preserveAspect = true;
        illustImg.color = new Color(0.92f, 0.94f, 0.96f, 0.88f);

        // Dark gradient overlay to blend illustration left edge into pure black
        GameObject illustFade = CreateUIChild(illustLayer, "FadeOverlay", Vector2.zero, Vector2.one);
        Image fadeImg = illustFade.AddComponent<Image>();
        fadeImg.sprite = spWhite;
        fadeImg.color = new Color(UITheme.NearBlack.r, UITheme.NearBlack.g, UITheme.NearBlack.b, 0.35f);

        // Layer 3: GraphicLayer (Brutalist border, corner crosshairs, stamped emblem)
        GameObject graphicLayer = CreateUIChild(viewMainMenu, "03_GraphicLayer", Vector2.zero, Vector2.one);

        // Outer Structural Border
        GameObject borderObj = CreateUIChild(graphicLayer, "ScreenBorder", Vector2.zero, Vector2.one, new Vector2(28, 28), new Vector2(-28, -28));
        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.sprite = spFrame;
        borderImg.type = Image.Type.Sliced;
        borderImg.color = new Color(UITheme.IndustrialGrey.r, UITheme.IndustrialGrey.g, UITheme.IndustrialGrey.b, 0.6f);

        // Corner Crosshairs
        CreateCrosshairElement(graphicLayer, "Crosshair_TL", new Vector2(0, 1), new Vector2(36, -36), spCrosshair);
        CreateCrosshairElement(graphicLayer, "Crosshair_TR", new Vector2(1, 1), new Vector2(-36, -36), spCrosshair);
        CreateCrosshairElement(graphicLayer, "Crosshair_BL", new Vector2(0, 0), new Vector2(36, 36), spCrosshair);
        CreateCrosshairElement(graphicLayer, "Crosshair_BR", new Vector2(1, 0), new Vector2(-36, 36), spCrosshair);

        // Stamped Classified Emblem (Lower Left)
        GameObject emblemObj = CreateUIChild(graphicLayer, "EmblemWatermark", new Vector2(0.045f, 0.08f), new Vector2(0.045f, 0.08f), Vector2.zero, Vector2.zero);
        RectTransform emblemRect = emblemObj.GetComponent<RectTransform>();
        emblemRect.sizeDelta = new Vector2(210, 210);
        emblemRect.pivot = new Vector2(0, 0);
        Image emblemImg = emblemObj.AddComponent<Image>();
        emblemImg.sprite = spEmblemCrown;
        emblemImg.color = new Color(1f, 1f, 1f, 0.35f);

        // Hazard Banner (Top Left)
        GameObject hazardBanner = CreateUIChild(graphicLayer, "HazardBanner", new Vector2(0.05f, 0.93f), new Vector2(0.26f, 0.945f));
        Image hazardImg = hazardBanner.AddComponent<Image>();
        hazardImg.sprite = spHazard;
        hazardImg.type = Image.Type.Tiled;

        // Layer 4: TitleLayer (Section 06: Custom Architectural Stacked Title)
        GameObject titleLayer = CreateUIChild(viewMainMenu, "04_TitleLayer", new Vector2(0.05f, 0.34f), new Vector2(0.48f, 0.90f));

        // Subline: PROJECT // A-07
        GameObject preTitleObj = CreateUIChild(titleLayer, "ClassificationCode", new Vector2(0, 0.92f), new Vector2(1, 0.98f));
        TextMeshProUGUI preTitle = preTitleObj.AddComponent<TextMeshProUGUI>();
        preTitle.font = _monoFont;
        preTitle.text = "PROJECT // A-07  [CLASSIFIED RESEARCH INITIATIVE]";
        preTitle.fontSize = 15;
        preTitle.color = UITheme.TechnologyCyan;
        preTitle.characterSpacing = 8;

        // Stacked Title: THE
        GameObject titleTheObj = CreateUIChild(titleLayer, "Title_THE", new Vector2(0, 0.76f), new Vector2(1, 0.90f));
        TextMeshProUGUI titleThe = titleTheObj.AddComponent<TextMeshProUGUI>();
        titleThe.font = _grotesqueFont;
        titleThe.text = "T  H  E";
        titleThe.fontSize = 38;
        titleThe.fontStyle = FontStyles.Bold;
        titleThe.color = UITheme.LightMetal;
        titleThe.characterSpacing = 35;

        // Stacked Title: LAST
        GameObject titleLastObj = CreateUIChild(titleLayer, "Title_LAST", new Vector2(0, 0.44f), new Vector2(1, 0.76f));
        TextMeshProUGUI titleLast = titleLastObj.AddComponent<TextMeshProUGUI>();
        titleLast.font = _grotesqueFont;
        titleLast.text = "LAST";
        titleLast.fontSize = 114;
        titleLast.fontStyle = FontStyles.Bold;
        titleLast.color = UITheme.TextBright;
        titleLast.characterSpacing = -4;

        // Stacked Title: GOD
        GameObject titleGodObj = CreateUIChild(titleLayer, "Title_GOD", new Vector2(0, 0.12f), new Vector2(1, 0.46f));
        TextMeshProUGUI titleGod = titleGodObj.AddComponent<TextMeshProUGUI>();
        titleGod.font = _grotesqueFont;
        titleGod.text = "GOD";
        titleGod.fontSize = 126;
        titleGod.fontStyle = FontStyles.Bold;
        titleGod.color = UITheme.TextBright;
        titleGod.characterSpacing = -6;

        // Horizontal piercing cyan rule
        GameObject ruleObj = CreateUIChild(titleLayer, "TitleCyanRule", new Vector2(0, 0.10f), new Vector2(0.95f, 0.105f));
        Image ruleImg = ruleObj.AddComponent<Image>();
        ruleImg.sprite = spWhite;
        ruleImg.color = UITheme.TechnologyCyan;

        // Facility subtitle
        GameObject subTitleObj = CreateUIChild(titleLayer, "TitleSubtitle", new Vector2(0, 0.01f), new Vector2(1, 0.08f));
        TextMeshProUGUI subTitle = subTitleObj.AddComponent<TextMeshProUGUI>();
        subTitle.font = _monoFont;
        subTitle.text = "FACILITY // SUBTERRANEAN SECTOR B-03\nCONTAINMENT MATRIX: PROTOCOL ZERO";
        subTitle.fontSize = 13;
        subTitle.color = UITheme.TextMuted;
        subTitle.lineSpacing = 12;

        // Layer 5: MenuLayer (Section 07, 11, 12: Asymmetric Right 35%)
        GameObject menuLayer = CreateUIChild(viewMainMenu, "05_MenuLayer", new Vector2(0.58f, 0.22f), new Vector2(0.94f, 0.78f));
        VerticalLayoutGroup menuLayout = menuLayer.AddComponent<VerticalLayoutGroup>();
        menuLayout.spacing = 18;
        menuLayout.childControlHeight = false;
        menuLayout.childControlWidth = true;
        menuLayout.childForceExpandHeight = false;
        menuLayout.childForceExpandWidth = true;

        UIMenuItem itemNew = CreateMenuItem(menuLayer, "MenuItem_01_NewGame", "01", "NEW GAME", "SYSTEM INITIALIZATION // SPECIMEN DOSSIER", spWhite, spDotCyan);
        UIMenuItem itemContinue = CreateMenuItem(menuLayer, "MenuItem_02_Continue", "02", "CONTINUE", "RESUME CONTAINMENT LOG // CHECKPOINT ALPHA", spWhite, spDotCyan);
        UIMenuItem itemSettings = CreateMenuItem(menuLayer, "MenuItem_03_Settings", "03", "SETTINGS", "FACILITY TERMINAL CONFIGURATION", spWhite, spDotCyan);
        UIMenuItem itemQuit = CreateMenuItem(menuLayer, "MenuItem_04_Quit", "04", "QUIT", "TERMINATE SECURE SESSION", spWhite, spDotCyan);

        // Layer 6: TechnicalLayer (Section 26: Microdetails)
        GameObject techLayer = CreateUIChild(viewMainMenu, "06_TechnicalLayer", Vector2.zero, Vector2.one);

        // Upper Right Telemetry
        GameObject upperRightObj = CreateUIChild(techLayer, "TelemetryUpperRight", new Vector2(0.60f, 0.93f), new Vector2(0.96f, 0.97f));
        TextMeshProUGUI upperRightText = upperRightObj.AddComponent<TextMeshProUGUI>();
        upperRightText.font = _monoFont;
        upperRightText.fontSize = 12;
        upperRightText.alignment = TextAlignmentOptions.TopRight;
        upperRightText.color = UITheme.TextMuted;
        upperRightText.text = "FACILITY // B-03  |  CLEARANCE // LEVEL 05  |  ARCHIVE // RESTRICTED";

        // Bottom Left Telemetry (Live clock & pressure)
        GameObject bottomLeftObj = CreateUIChild(techLayer, "TelemetryBottomLeft", new Vector2(0.05f, 0.035f), new Vector2(0.50f, 0.075f));
        TextMeshProUGUI bottomLeftText = bottomLeftObj.AddComponent<TextMeshProUGUI>();
        bottomLeftText.font = _monoFont;
        bottomLeftText.fontSize = 12;
        bottomLeftText.color = UITheme.TextMuted;
        bottomLeftText.text = "SYS_CLOCK: UTC 21:42:00.00  |  PRESSURE: 418.2 PSI  |  MUTATION: ACTIVE";

        // Bottom Right Integrity
        GameObject bottomRightObj = CreateUIChild(techLayer, "TelemetryBottomRight", new Vector2(0.60f, 0.035f), new Vector2(0.96f, 0.075f));
        TextMeshProUGUI bottomRightText = bottomRightObj.AddComponent<TextMeshProUGUI>();
        bottomRightText.font = _monoFont;
        bottomRightText.fontSize = 12;
        bottomRightText.alignment = TextAlignmentOptions.BottomRight;
        bottomRightText.color = UITheme.TextClassified;
        bottomRightText.text = "DEICIDE PROTOCOL IN EFFECT // ZERO ESCAPE PERMITTED";

        // Layer 7: EffectsLayer (Cyan pulsing containment dot & scanline)
        GameObject effectsLayer = CreateUIChild(viewMainMenu, "07_EffectsLayer", Vector2.zero, Vector2.one);
        GameObject cyanDotObj = CreateUIChild(effectsLayer, "CyanContainmentLight", new Vector2(0.765f, 0.320f), new Vector2(0.765f, 0.320f));
        RectTransform cyanDotRect = cyanDotObj.GetComponent<RectTransform>();
        cyanDotRect.sizeDelta = new Vector2(24, 24);
        Image cyanDotImg = cyanDotObj.AddComponent<Image>();
        cyanDotImg.sprite = spDotCyan;
        cyanDotImg.color = UITheme.TechnologyCyan;

        GameObject scanlineObj = CreateUIChild(effectsLayer, "ScanlineOverlay", Vector2.zero, Vector2.one);
        Image scanlineImg = scanlineObj.AddComponent<Image>();
        scanlineImg.sprite = spScanline;
        scanlineImg.type = Image.Type.Tiled;
        scanlineImg.color = new Color(1, 1, 1, 0.04f);

        // Inject MainMenuController fields via SerializedObject
        SerializedObject soMenu = new SerializedObject(menuCtrl);
        soMenu.FindProperty("itemNewGame").objectReferenceValue = itemNew;
        soMenu.FindProperty("itemContinue").objectReferenceValue = itemContinue;
        soMenu.FindProperty("itemSettings").objectReferenceValue = itemSettings;
        soMenu.FindProperty("itemQuit").objectReferenceValue = itemQuit;
        soMenu.FindProperty("menuCanvasGroup").objectReferenceValue = menuGroup;
        soMenu.FindProperty("laboratoryIllustration").objectReferenceValue = illustImg;
        soMenu.FindProperty("cyanContainmentDot").objectReferenceValue = cyanDotImg;
        soMenu.FindProperty("emblemWatermark").objectReferenceValue = emblemImg;
        soMenu.FindProperty("facilityCodeText").objectReferenceValue = upperRightText;
        soMenu.FindProperty("systemClockText").objectReferenceValue = bottomLeftText;
        soMenu.FindProperty("containmentIntegrityText").objectReferenceValue = bottomRightText;
        soMenu.ApplyModifiedProperties();

        // =====================================================================
        // VIEW A: LOADING SCREEN (Section 13, 14, 15)
        // =====================================================================
        GameObject viewLoading = CreateUIChild(root, "View_LoadingScreen", Vector2.zero, Vector2.one);
        CanvasGroup loadingGroup = viewLoading.AddComponent<CanvasGroup>();
        LoadingScreenController loadingCtrl = viewLoading.AddComponent<LoadingScreenController>();

        // Background
        GameObject lBg = CreateUIChild(viewLoading, "LoadingBackground", Vector2.zero, Vector2.one);
        Image lBgImg = lBg.AddComponent<Image>();
        lBgImg.sprite = spWhite;
        lBgImg.color = UITheme.NearBlack;

        // Illustration: Containment chamber disappearing into darkness (25-30% of field)
        GameObject lIllust = CreateUIChild(viewLoading, "ContainmentChamber", new Vector2(0.35f, 0.25f), new Vector2(0.65f, 0.75f));
        Image lIllustImg = lIllust.AddComponent<Image>();
        lIllustImg.sprite = spLabContainment;
        lIllustImg.preserveAspect = true;
        lIllustImg.color = new Color(0.9f, 0.92f, 0.95f, 0.85f);

        // Subtle Cyan Light Source
        GameObject lPulse = CreateUIChild(viewLoading, "CyanPulseLight", new Vector2(0.50f, 0.48f), new Vector2(0.50f, 0.48f));
        RectTransform lPulseRect = lPulse.GetComponent<RectTransform>();
        lPulseRect.sizeDelta = new Vector2(38, 38);
        Image lPulseImg = lPulse.AddComponent<Image>();
        lPulseImg.sprite = spDotCyan;
        lPulseImg.color = UITheme.TechnologyCyan;

        // Frame
        GameObject lBorder = CreateUIChild(viewLoading, "LoadingBorder", Vector2.zero, Vector2.one, new Vector2(36, 36), new Vector2(-36, -36));
        Image lBorderImg = lBorder.AddComponent<Image>();
        lBorderImg.sprite = spFrame;
        lBorderImg.type = Image.Type.Sliced;
        lBorderImg.color = new Color(UITheme.IndustrialGrey.r, UITheme.IndustrialGrey.g, UITheme.IndustrialGrey.b, 0.5f);

        // Loading Header / Project Code
        GameObject lHeaderObj = CreateUIChild(viewLoading, "LoadingHeader", new Vector2(0.10f, 0.82f), new Vector2(0.90f, 0.90f));
        TextMeshProUGUI lHeader = lHeaderObj.AddComponent<TextMeshProUGUI>();
        lHeader.font = _monoFont;
        lHeader.fontSize = 20;
        lHeader.alignment = TextAlignmentOptions.Center;
        lHeader.color = UITheme.TechnologyCyan;
        lHeader.text = "PROJECT // A-07  [INITIALIZING CONTAINMENT MATRIX]";
        lHeader.characterSpacing = 10;

        // Title
        GameObject lTitleObj = CreateUIChild(viewLoading, "LoadingTitle", new Vector2(0.10f, 0.74f), new Vector2(0.90f, 0.82f));
        TextMeshProUGUI lTitle = lTitleObj.AddComponent<TextMeshProUGUI>();
        lTitle.font = _grotesqueFont;
        lTitle.fontSize = 32;
        lTitle.alignment = TextAlignmentOptions.Center;
        lTitle.fontStyle = FontStyles.Bold;
        lTitle.color = UITheme.TextBright;
        lTitle.text = "THE LAST GOD";
        lTitle.characterSpacing = 14;

        // Progress Bar (Thin, crisp line, Section 14)
        GameObject barBackObj = CreateUIChild(viewLoading, "ProgressBarBack", new Vector2(0.30f, 0.18f), new Vector2(0.70f, 0.185f));
        Image barBackImg = barBackObj.AddComponent<Image>();
        barBackImg.sprite = spWhite;
        barBackImg.color = UITheme.DarkBlueGrey;

        GameObject barFillObj = CreateUIChild(barBackObj, "ProgressBarFill", Vector2.zero, Vector2.one);
        Image barFillImg = barFillObj.AddComponent<Image>();
        barFillImg.sprite = spWhite;
        barFillImg.type = Image.Type.Filled;
        barFillImg.fillMethod = Image.FillMethod.Horizontal;
        barFillImg.fillOrigin = 0;
        barFillImg.fillAmount = 0.4f;
        barFillImg.color = UITheme.TechnologyCyan;

        // Progress Text
        GameObject lPctObj = CreateUIChild(viewLoading, "ProgressPercent", new Vector2(0.30f, 0.14f), new Vector2(0.70f, 0.175f));
        TextMeshProUGUI lPct = lPctObj.AddComponent<TextMeshProUGUI>();
        lPct.font = _monoFont;
        lPct.fontSize = 14;
        lPct.alignment = TextAlignmentOptions.Center;
        lPct.color = UITheme.BrightCyan;
        lPct.text = "[ 40.0% ]";

        // Cycling Telemetry Status (Section 14 point 6)
        GameObject lStatusObj = CreateUIChild(viewLoading, "StatusTelemetry", new Vector2(0.15f, 0.08f), new Vector2(0.85f, 0.13f));
        TextMeshProUGUI lStatus = lStatusObj.AddComponent<TextMeshProUGUI>();
        lStatus.font = _monoFont;
        lStatus.fontSize = 13;
        lStatus.alignment = TextAlignmentOptions.Center;
        lStatus.color = UITheme.TextMuted;
        lStatus.text = "CONTAINMENT STATUS: STABLE // BIO-CHAMBER 07";

        // Blinking warning ticks
        GameObject blink1 = CreateUIChild(viewLoading, "Blinker1", new Vector2(0.28f, 0.178f), new Vector2(0.285f, 0.187f));
        Image b1Img = blink1.AddComponent<Image>();
        b1Img.sprite = spWhite;
        b1Img.color = UITheme.WarningOrange;

        GameObject blink2 = CreateUIChild(viewLoading, "Blinker2", new Vector2(0.715f, 0.178f), new Vector2(0.720f, 0.187f));
        Image b2Img = blink2.AddComponent<Image>();
        b2Img.sprite = spWhite;
        b2Img.color = UITheme.WarningOrange;

        // Inject LoadingScreenController fields via SerializedObject
        SerializedObject soLoad = new SerializedObject(loadingCtrl);
        soLoad.FindProperty("screenGroup").objectReferenceValue = loadingGroup;
        soLoad.FindProperty("illustrationImage").objectReferenceValue = lIllustImg;
        soLoad.FindProperty("cyanPulseLight").objectReferenceValue = lPulseImg;
        soLoad.FindProperty("progressBarRect").objectReferenceValue = barBackObj.GetComponent<RectTransform>();
        soLoad.FindProperty("progressBarFill").objectReferenceValue = barFillImg;
        soLoad.FindProperty("progressPercentText").objectReferenceValue = lPct;
        soLoad.FindProperty("statusTelemetryText").objectReferenceValue = lStatus;
        soLoad.FindProperty("indicatorBlinkers").ClearArray();
        soLoad.FindProperty("indicatorBlinkers").InsertArrayElementAtIndex(0);
        soLoad.FindProperty("indicatorBlinkers").GetArrayElementAtIndex(0).objectReferenceValue = b1Img;
        soLoad.FindProperty("indicatorBlinkers").InsertArrayElementAtIndex(1);
        soLoad.FindProperty("indicatorBlinkers").GetArrayElementAtIndex(1).objectReferenceValue = b2Img;
        soLoad.ApplyModifiedProperties();

        // =====================================================================
        // VIEW C: NEW GAME SCREEN / CLASSIFIED DOSSIER (Section 16, 17, 18)
        // =====================================================================
        GameObject viewDossier = CreateUIChild(root, "View_NewGameDossier", Vector2.zero, Vector2.one);
        CanvasGroup dossierGroup = viewDossier.AddComponent<CanvasGroup>();
        NewGameDossierController dossierCtrl = viewDossier.AddComponent<NewGameDossierController>();

        // Background
        GameObject dBg = CreateUIChild(viewDossier, "DossierBackground", Vector2.zero, Vector2.one);
        Image dBgImg = dBg.AddComponent<Image>();
        dBgImg.sprite = spWhite;
        dBgImg.color = UITheme.NearBlack;

        // Archival Dossier Sheet (Orthographic Graphic-Novel Scan, Section 16)
        GameObject dSheet = CreateUIChild(viewDossier, "DossierSheet", new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.94f));
        Image dSheetImg = dSheet.AddComponent<Image>();
        dSheetImg.sprite = spDossierSheet;
        dSheetImg.preserveAspect = true;
        dSheetImg.color = new Color(0.85f, 0.88f, 0.92f, 0.9f);

        // Frame
        GameObject dBorder = CreateUIChild(viewDossier, "DossierBorder", Vector2.zero, Vector2.one, new Vector2(36, 36), new Vector2(-36, -36));
        Image dBorderImg = dBorder.AddComponent<Image>();
        dBorderImg.sprite = spFrame;
        dBorderImg.type = Image.Type.Sliced;
        dBorderImg.color = new Color(UITheme.IndustrialGrey.r, UITheme.IndustrialGrey.g, UITheme.IndustrialGrey.b, 0.7f);

        // Header Stamp
        GameObject dHeader = CreateUIChild(viewDossier, "DossierHeader", new Vector2(0.24f, 0.83f), new Vector2(0.77f, 0.89f));
        TextMeshProUGUI dHeaderText = dHeader.AddComponent<TextMeshProUGUI>();
        dHeaderText.font = _monoFont;
        dHeaderText.fontSize = 16;
        dHeaderText.color = UITheme.WarningOrange;
        dHeaderText.text = "CLASSIFIED ARCHIVE // CLEARANCE LEVEL 05 // PROJECT A-07";
        dHeaderText.characterSpacing = 6;

        // Subject Specification Panel (Left Side of Dossier)
        GameObject specPanel = CreateUIChild(viewDossier, "SpecimenDataPanel", new Vector2(0.28f, 0.28f), new Vector2(0.57f, 0.81f));
        VerticalLayoutGroup specLayout = specPanel.AddComponent<VerticalLayoutGroup>();
        specLayout.spacing = 14;
        specLayout.childControlHeight = false;
        specLayout.childControlWidth = true;
        specLayout.childForceExpandHeight = false;

        CreateDataRow(specPanel, "Row_Name", "SUBJECT IDENTIFIER:", "AERON", _monoFont, _industrialFont, UITheme.NearBlack);
        CreateDataRow(specPanel, "Row_Status", "CONTAINMENT STATUS:", "STABILIZED IN ARTIFICIAL COMA", _monoFont, _industrialFont, UITheme.DarkCyan);
        UIRedactionBlock redOrigin = CreateRedactionRow(specPanel, "Row_Origin", "ORIGIN CODE:", "FORBIDDEN DIVINITY // SECTOR ZERO", spWhite, _monoFont);
        UIRedactionBlock redAge = CreateRedactionRow(specPanel, "Row_Age", "CHRONOLOGICAL AGE:", "CYCLE 419 // UNMEASURABLE BIOMASS", spWhite, _monoFont);
        UIRedactionBlock redDirector = CreateRedactionRow(specPanel, "Row_Director", "PROJECT DIRECTOR:", "DR. V. MALKHOV [TERMINATED]", spWhite, _monoFont);
        UIRedactionBlock redObjective = CreateRedactionRow(specPanel, "Row_Objective", "PRIMARY DIRECTIVE:", "CELLULAR EXTRACTION & DIVINE DEICIDE", spWhite, _monoFont);

        // Right Side: Specimen Stasis Chamber Viewport
        GameObject viewportObj = CreateUIChild(viewDossier, "SpecimenViewport", new Vector2(0.59f, 0.30f), new Vector2(0.77f, 0.78f));
        Image viewportBg = viewportObj.AddComponent<Image>();
        viewportBg.sprite = spEmblemSeal;
        viewportBg.preserveAspect = true;
        viewportBg.color = new Color(0.9f, 0.95f, 1f, 0.75f);

        GameObject viewportBorder = CreateUIChild(viewportObj, "ViewportBorder", Vector2.zero, Vector2.one);
        Image vpBorderImg = viewportBorder.AddComponent<Image>();
        vpBorderImg.sprite = spFrame;
        vpBorderImg.type = Image.Type.Sliced;
        vpBorderImg.color = UITheme.IndustrialGrey;

        // Viewport label
        GameObject vpLabelObj = CreateUIChild(viewportObj, "ViewportLabel", new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.14f));
        TextMeshProUGUI vpLabel = vpLabelObj.AddComponent<TextMeshProUGUI>();
        vpLabel.font = _monoFont;
        vpLabel.fontSize = 11;
        vpLabel.alignment = TextAlignmentOptions.Center;
        vpLabel.color = UITheme.TechnologyCyan;
        vpLabel.text = "STASIS APERTURE // CAMERA 07-B [ACTIVE]";

        // Action Buttons (Initialize & Return)
        GameObject btnInitObj = CreateUIChild(viewDossier, "Button_Initialize", new Vector2(0.48f, 0.13f), new Vector2(0.77f, 0.23f));
        Image btnInitImg = btnInitObj.AddComponent<Image>();
        btnInitImg.sprite = spWhite;
        btnInitImg.color = UITheme.DarkBlueGrey;
        Button btnInit = btnInitObj.AddComponent<Button>();
        btnInit.targetGraphic = btnInitImg;

        GameObject btnInitBorder = CreateUIChild(btnInitObj, "InitBorder", Vector2.zero, Vector2.one);
        Image bibImg = btnInitBorder.AddComponent<Image>();
        bibImg.sprite = spFrame;
        bibImg.type = Image.Type.Sliced;
        bibImg.color = UITheme.WarningOrange;

        GameObject btnInitTextObj = CreateUIChild(btnInitObj, "InitText", Vector2.zero, Vector2.one);
        TextMeshProUGUI btnInitText = btnInitTextObj.AddComponent<TextMeshProUGUI>();
        btnInitText.font = _industrialFont;
        btnInitText.fontSize = 18;
        btnInitText.fontStyle = FontStyles.Bold;
        btnInitText.alignment = TextAlignmentOptions.Center;
        btnInitText.color = UITheme.TextBright;
        btnInitText.text = "INITIALIZE // BREAK CONTAINMENT MATRIX";
        btnInitText.characterSpacing = 8;

        // Back Button
        GameObject btnBackObj = CreateUIChild(viewDossier, "Button_Back", new Vector2(0.24f, 0.13f), new Vector2(0.45f, 0.23f));
        Image btnBackImg = btnBackObj.AddComponent<Image>();
        btnBackImg.sprite = spWhite;
        btnBackImg.color = UITheme.DeepCharcoal;
        Button btnBack = btnBackObj.AddComponent<Button>();
        btnBack.targetGraphic = btnBackImg;

        GameObject btnBackBorder = CreateUIChild(btnBackObj, "BackBorder", Vector2.zero, Vector2.one);
        Image bbbImg = btnBackBorder.AddComponent<Image>();
        bbbImg.sprite = spFrame;
        bbbImg.type = Image.Type.Sliced;
        bbbImg.color = UITheme.IndustrialGrey;

        GameObject btnBackTextObj = CreateUIChild(btnBackObj, "BackText", Vector2.zero, Vector2.one);
        TextMeshProUGUI btnBackText = btnBackTextObj.AddComponent<TextMeshProUGUI>();
        btnBackText.font = _industrialFont;
        btnBackText.fontSize = 15;
        btnBackText.alignment = TextAlignmentOptions.Center;
        btnBackText.color = UITheme.TextMuted;
        btnBackText.text = "< RETURN TO TERMINAL";

        GameObject transOverlayObj = CreateUIChild(viewDossier, "TransitionOverlay", Vector2.zero, Vector2.one);
        CanvasGroup transOverlay = transOverlayObj.AddComponent<CanvasGroup>();
        transOverlay.alpha = 0f;
        transOverlay.blocksRaycasts = false;
        transOverlayObj.SetActive(false);
        Image transImg = transOverlayObj.AddComponent<Image>();
        transImg.sprite = spWhite;
        transImg.color = UITheme.NearBlack;

        GameObject scanSweepObj = CreateUIChild(transOverlayObj, "ScanLineSweep", new Vector2(0, 0.98f), new Vector2(1, 1f));
        Image scanSweepImg = scanSweepObj.AddComponent<Image>();
        scanSweepImg.sprite = spWhite;
        scanSweepImg.color = UITheme.TechnologyCyan;

        GameObject transTextObj = CreateUIChild(transOverlayObj, "TransitionTerminalText", new Vector2(0.2f, 0.4f), new Vector2(0.8f, 0.6f));
        TextMeshProUGUI transText = transTextObj.AddComponent<TextMeshProUGUI>();
        transText.font = _monoFont;
        transText.fontSize = 18;
        transText.alignment = TextAlignmentOptions.Center;
        transText.color = UITheme.TechnologyCyan;
        transText.text = "PROJECT A-07 // INITIALIZATION COMMENCING...";

        // Inject NewGameDossierController fields
        SerializedObject soDossier = new SerializedObject(dossierCtrl);
        soDossier.FindProperty("dossierCanvasGroup").objectReferenceValue = dossierGroup;
        soDossier.FindProperty("dossierBackgroundImage").objectReferenceValue = dSheetImg;
        soDossier.FindProperty("specimenSilhouetteImage").objectReferenceValue = viewportBg;
        soDossier.FindProperty("redactionOrigin").objectReferenceValue = redOrigin;
        soDossier.FindProperty("redactionAge").objectReferenceValue = redAge;
        soDossier.FindProperty("redactionDirector").objectReferenceValue = redDirector;
        soDossier.FindProperty("redactionObjective").objectReferenceValue = redObjective;
        soDossier.FindProperty("initializeButton").objectReferenceValue = btnInit;
        soDossier.FindProperty("initializeButtonText").objectReferenceValue = btnInitText;
        soDossier.FindProperty("backButton").objectReferenceValue = btnBack;
        soDossier.FindProperty("transitionOverlay").objectReferenceValue = transOverlay;
        soDossier.FindProperty("scanLineSweep").objectReferenceValue = scanSweepObj.GetComponent<RectTransform>();
        soDossier.FindProperty("transitionTerminalText").objectReferenceValue = transText;
        soDossier.ApplyModifiedProperties();

        // Inject UIRootManager fields
        SerializedObject soRoot = new SerializedObject(rootManager);
        soRoot.FindProperty("loadingScreen").objectReferenceValue = loadingCtrl;
        soRoot.FindProperty("mainMenu").objectReferenceValue = menuCtrl;
        soRoot.FindProperty("newGameDossier").objectReferenceValue = dossierCtrl;
        soRoot.FindProperty("uiAudioSource").objectReferenceValue = audioSrc;
        soRoot.ApplyModifiedProperties();

        // Default: Show Main Menu
        rootManager.SwitchScreen(UIRootManager.ScreenState.MainMenu);

        return root;
    }

    private static GameObject CreateUIChild(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin = default, Vector2 offsetMax = default)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        return go;
    }

    private static void CreateCrosshairElement(GameObject parent, string name, Vector2 anchor, Vector2 offset, Sprite crosshairSprite)
    {
        GameObject ch = CreateUIChild(parent, name, anchor, anchor);
        RectTransform rt = ch.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(24, 24);
        rt.anchoredPosition = offset;
        Image img = ch.AddComponent<Image>();
        img.sprite = crosshairSprite;
        img.color = UITheme.TechnologyCyan;
    }

    private static UIMenuItem CreateMenuItem(GameObject parent, string name, string number, string title, string sublabel, Sprite spWhite, Sprite spDot)
    {
        GameObject itemObj = CreateUIChild(parent, name, Vector2.zero, Vector2.one);
        RectTransform itemRect = itemObj.GetComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(0, 68);

        UIMenuItem menuItem = itemObj.AddComponent<UIMenuItem>();

        // Background Highlight
        GameObject bgObj = CreateUIChild(itemObj, "Highlight", Vector2.zero, Vector2.one);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.sprite = spWhite;
        bgImg.color = Color.clear;

        // Number Box (e.g. 01)
        GameObject numObj = CreateUIChild(itemObj, "NumberText", new Vector2(0, 0.40f), new Vector2(0.12f, 0.95f));
        TextMeshProUGUI numText = numObj.AddComponent<TextMeshProUGUI>();
        numText.font = _industrialFont;
        numText.fontSize = 24;
        numText.fontStyle = FontStyles.Bold;
        numText.color = UITheme.LightMetal;
        numText.text = number;

        // Main Title (e.g. NEW GAME)
        GameObject titleObj = CreateUIChild(itemObj, "TitleText", new Vector2(0.14f, 0.40f), new Vector2(1, 0.95f));
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.font = _industrialFont;
        titleText.fontSize = 24;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = UITheme.TextBright;
        titleText.text = title;
        titleText.characterSpacing = 6;

        // Sublabel (e.g. SYSTEM INITIALIZATION)
        GameObject subObj = CreateUIChild(itemObj, "SublabelText", new Vector2(0.14f, 0.05f), new Vector2(1, 0.38f));
        TextMeshProUGUI subText = subObj.AddComponent<TextMeshProUGUI>();
        subText.font = _monoFont;
        subText.fontSize = 11;
        subText.color = Color.clear;
        subText.text = sublabel;

        // Thin Cyan Indicator Rule
        GameObject ruleObj = CreateUIChild(itemObj, "CyanRule", new Vector2(0.14f, 0.02f), new Vector2(1, 0.05f));
        Image ruleImg = ruleObj.AddComponent<Image>();
        ruleImg.sprite = spWhite;
        ruleImg.color = Color.clear;

        // Indicator Tick
        GameObject tickObj = CreateUIChild(itemObj, "IndicatorTick", new Vector2(0.09f, 0.50f), new Vector2(0.09f, 0.50f));
        RectTransform tickRect = tickObj.GetComponent<RectTransform>();
        tickRect.sizeDelta = new Vector2(8, 8);
        Image tickImg = tickObj.AddComponent<Image>();
        tickImg.sprite = spDot;
        tickImg.color = Color.clear;

        // Wire serialized fields
        SerializedObject so = new SerializedObject(menuItem);
        so.FindProperty("numberText").objectReferenceValue = numText;
        so.FindProperty("titleText").objectReferenceValue = titleText;
        so.FindProperty("sublabelText").objectReferenceValue = subText;
        so.FindProperty("cyanRule").objectReferenceValue = ruleImg;
        so.FindProperty("indicatorTick").objectReferenceValue = tickImg;
        so.FindProperty("backgroundHighlight").objectReferenceValue = bgImg;
        so.FindProperty("itemNumber").stringValue = number;
        so.FindProperty("itemTitle").stringValue = title;
        so.FindProperty("itemSublabel").stringValue = sublabel;
        so.ApplyModifiedProperties();

        return menuItem;
    }

    private static void CreateDataRow(GameObject parent, string name, string label, string value, TMP_FontAsset monoFont, TMP_FontAsset valueFont, Color valueColor)
    {
        GameObject row = CreateUIChild(parent, name, Vector2.zero, Vector2.one);
        RectTransform rt = row.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 34);

        GameObject lblObj = CreateUIChild(row, "Label", new Vector2(0, 0), new Vector2(0.42f, 1));
        TextMeshProUGUI lbl = lblObj.AddComponent<TextMeshProUGUI>();
        lbl.font = monoFont;
        lbl.fontSize = 12;
        lbl.fontStyle = FontStyles.Bold;
        lbl.color = UITheme.DeepCharcoal;
        lbl.text = label;

        GameObject valObj = CreateUIChild(row, "Value", new Vector2(0.44f, 0), new Vector2(1, 1));
        TextMeshProUGUI val = valObj.AddComponent<TextMeshProUGUI>();
        val.font = valueFont;
        val.fontSize = 14;
        val.fontStyle = FontStyles.Bold;
        val.color = valueColor;
        val.text = value;
    }

    private static UIRedactionBlock CreateRedactionRow(GameObject parent, string name, string label, string secret, Sprite spWhite, TMP_FontAsset monoFont)
    {
        GameObject row = CreateUIChild(parent, name, Vector2.zero, Vector2.one);
        RectTransform rt = row.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 36);

        GameObject lblObj = CreateUIChild(row, "Label", new Vector2(0, 0), new Vector2(0.42f, 1));
        TextMeshProUGUI lbl = lblObj.AddComponent<TextMeshProUGUI>();
        lbl.font = monoFont;
        lbl.fontSize = 12;
        lbl.fontStyle = FontStyles.Bold;
        lbl.color = UITheme.DeepCharcoal;
        lbl.text = label;

        GameObject redBlockObj = CreateUIChild(row, "RedactionBlock", new Vector2(0.44f, 0.05f), new Vector2(1f, 0.95f));
        UIRedactionBlock redBlock = redBlockObj.AddComponent<UIRedactionBlock>();

        Image plate = redBlockObj.AddComponent<Image>();
        plate.sprite = spWhite;
        plate.color = UITheme.NearBlack;

        GameObject secretObj = CreateUIChild(redBlockObj, "SecretText", Vector2.zero, Vector2.one, new Vector2(8, 0), new Vector2(-8, 0));
        TextMeshProUGUI sTxt = secretObj.AddComponent<TextMeshProUGUI>();
        sTxt.font = monoFont;
        sTxt.fontSize = 12;
        sTxt.alignment = TextAlignmentOptions.MidlineLeft;
        sTxt.color = Color.clear;
        sTxt.text = secret;

        // Wire redaction block
        SerializedObject so = new SerializedObject(redBlock);
        so.FindProperty("redactionPlate").objectReferenceValue = plate;
        so.FindProperty("secretText").objectReferenceValue = sTxt;
        so.FindProperty("redactedContent").stringValue = secret;
        so.ApplyModifiedProperties();

        return redBlock;
    }

    // =========================================================================
    // STEP 6: INTEGRATE INTO MAIN MENU SCENE
    // =========================================================================
    public static GameObject IntegrateIntoMainMenuScene(GameObject prefabAsset)
    {
        Debug.Log(">>> Step 6: Integrating UI_Root into " + MAIN_MENU_SCENE);

        var scene = EditorSceneManager.OpenScene(MAIN_MENU_SCENE, OpenSceneMode.Single);

        // Remove old Legacy MainMenuUI if present
        var oldMenu = UnityEngine.Object.FindObjectsByType<LastGod.ThirdPerson.UI.MainMenuUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var om in oldMenu)
        {
            Debug.Log(">>> Removing obsolete OnGUI MainMenuUI component: " + om.gameObject.name);
            UnityEngine.Object.DestroyImmediate(om);
        }

        // Remove existing UI_Root if present
        var existingUIs = GameObject.FindObjectsByType<UIRootManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var ui in existingUIs)
        {
            UnityEngine.Object.DestroyImmediate(ui.gameObject);
        }

        // Instantiate fresh Prefab
        GameObject instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
        instance.name = "UI_Root";

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log(">>> Scene updated and saved successfully: " + MAIN_MENU_SCENE);
        return instance;
    }

    // =========================================================================
    // STEP 7: RENDER MULTI-RESOLUTION SCREENSHOTS
    // =========================================================================
    public static void RenderMultiResolutionScreenshots(GameObject uiRootInstance)
    {
        Debug.Log(">>> Step 7: Capturing high-resolution visual proof screenshots...");
        EnsureDirectory(SCREENSHOT_DIR);
        EnsureDirectory(DREAM_LOOP_DIR);

        UIRootManager rootManager = uiRootInstance.GetComponent<UIRootManager>();
        Canvas canvas = uiRootInstance.GetComponent<Canvas>();
        CanvasScaler scaler = uiRootInstance.GetComponent<CanvasScaler>();

        // Setup temporary camera for clean UI rendering
        GameObject camObj = new GameObject("Screenshot_Render_Cam");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = UITheme.NearBlack;
        cam.cullingMask = ~0;

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;

        // Resolution profiles to validate Section 25 (Responsive Design)
        (int width, int height, string label)[] resolutions = new[]
        {
            (1920, 1080, "1080p_FHD"),
            (1280, 720,  "720p_HD"),
            (2560, 1440, "1440p_2K"),
            (2560, 1080, "21x9_Ultrawide")
        };

        UIRootManager.ScreenState[] screens = new[]
        {
            UIRootManager.ScreenState.MainMenu,
            UIRootManager.ScreenState.Loading,
            UIRootManager.ScreenState.NewGameDossier
        };

        foreach (var screen in screens)
        {
            rootManager.SwitchScreen(screen);

            foreach (var res in resolutions)
            {
                scaler.referenceResolution = new Vector2(res.width, res.height);
                Canvas.ForceUpdateCanvases();
                foreach (var tmp in uiRootInstance.GetComponentsInChildren<TextMeshProUGUI>(true))
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
                string fileName = $"{screen}_{res.label}.png";
                string fullPath = Path.Combine(SCREENSHOT_DIR, fileName);
                File.WriteAllBytes(fullPath, pngBytes);

                // If 1080p Main Menu, also update target/reference in .dream-loop
                if (screen == UIRootManager.ScreenState.MainMenu && res.label == "1080p_FHD")
                {
                    File.WriteAllBytes(Path.Combine(DREAM_LOOP_DIR, "MainMenu_1080p.png"), pngBytes);
                }
                else if (screen == UIRootManager.ScreenState.Loading && res.label == "1080p_FHD")
                {
                    File.WriteAllBytes(Path.Combine(DREAM_LOOP_DIR, "LoadingScreen_1080p.png"), pngBytes);
                }
                else if (screen == UIRootManager.ScreenState.NewGameDossier && res.label == "1080p_FHD")
                {
                    File.WriteAllBytes(Path.Combine(DREAM_LOOP_DIR, "NewGameDossier_1080p.png"), pngBytes);
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

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log(">>> Visual screenshots captured across 1080p, 720p, 1440p, and 21:9 Ultrawide!");
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
