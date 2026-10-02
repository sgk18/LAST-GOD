using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using LastGod.UI;
using LastGod.ThirdPerson.Save;

namespace LastGod.Tests
{
    [TestFixture]
    public class UIArchitectureTests
    {
        [Test]
        public void PaletteTokens_MatchMasterCreativeBriefSpecs()
        {
            // Verify Section 04 Master Palette Hex specifications
            Assert.AreEqual(new Color(0x08 / 255f, 0x0B / 255f, 0x0F / 255f, 1f), UITheme.NearBlack, "NearBlack mismatch (#080B0F)");
            Assert.AreEqual(new Color(0x11 / 255f, 0x16 / 255f, 0x1C / 255f, 1f), UITheme.DeepCharcoal, "DeepCharcoal mismatch (#11161C)");
            Assert.AreEqual(new Color(0x1B / 255f, 0x24 / 255f, 0x30 / 255f, 1f), UITheme.DarkBlueGrey, "DarkBlueGrey mismatch (#1B2430)");
            Assert.AreEqual(new Color(0x30 / 255f, 0x38 / 255f, 0x41 / 255f, 1f), UITheme.IndustrialGrey, "IndustrialGrey mismatch (#303841)");
            Assert.AreEqual(new Color(0x4A / 255f, 0x53 / 255f, 0x5C / 255f, 1f), UITheme.LightMetal, "LightMetal mismatch (#4A535C)");
            Assert.AreEqual(new Color(0x6F / 255f, 0xE3 / 255f, 0xFF / 255f, 1f), UITheme.TechnologyCyan, "TechnologyCyan mismatch (#6FE3FF)");
            Assert.AreEqual(new Color(0xCF / 255f, 0xF4 / 255f, 0xFF / 255f, 1f), UITheme.BrightCyan, "BrightCyan mismatch (#CFF4FF)");
            Assert.AreEqual(new Color(0x24 / 255f, 0x5B / 255f, 0x70 / 255f, 1f), UITheme.DarkCyan, "DarkCyan mismatch (#245B70)");
            Assert.AreEqual(new Color(0xC4 / 255f, 0x50 / 255f, 0x2E / 255f, 1f), UITheme.WarningOrange, "WarningOrange mismatch (#C4502E)");
            Assert.AreEqual(new Color(0xB3 / 255f, 0x9A / 255f, 0x45 / 255f, 1f), UITheme.IndustrialYellow, "IndustrialYellow mismatch (#B39A45)");
        }

        [Test]
        public void SaveSystem_MultiSlotArchival_FunctionsCorrectly()
        {
            // Test slot saving
            SaveSystem.ActiveSlot = 2;
            SaveSystem.Data.playerHealth = 85f;
            SaveSystem.Data.sectorName = "SECTOR B-03 CATWALK";
            SaveSystem.Data.currentCheckpoint = 3;
            SaveSystem.SaveSlot(2);

            Assert.IsTrue(SaveSystem.HasSlot(2), "Slot 2 should exist after SaveSlot(2)");

            var loadedData = SaveSystem.GetSlotData(2);
            Assert.IsNotNull(loadedData, "Loaded data for Slot 2 should not be null");
            Assert.AreEqual(85f, loadedData.playerHealth, "Player health in Slot 2 mismatch");
            Assert.AreEqual("SECTOR B-03 CATWALK", loadedData.sectorName, "Sector name mismatch");
            Assert.AreEqual(3, loadedData.currentCheckpoint, "Checkpoint number mismatch");
            Assert.IsNotEmpty(loadedData.timestamp, "Timestamp should be generated automatically");

            // Clean up
            SaveSystem.DeleteSlot(2);
            Assert.IsFalse(SaveSystem.HasSlot(2), "Slot 2 should be purged after DeleteSlot(2)");
        }

        [Test]
        public void UIRootPrefab_LoadsAndHasAllRequiredComponents()
        {
            string prefabPath = "Assets/Art/UI/Prefabs/UI_Root_TheLastGod.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.IsNotNull(prefab, $"Prefab must exist at {prefabPath}");

            Canvas canvas = prefab.GetComponent<Canvas>();
            Assert.IsNotNull(canvas, "UI_Root prefab must contain a Canvas component");

            CanvasScaler scaler = prefab.GetComponent<CanvasScaler>();
            Assert.IsNotNull(scaler, "UI_Root prefab must contain a CanvasScaler component");
            Assert.AreEqual(new Vector2(1920, 1080), scaler.referenceResolution, "CanvasScaler reference resolution must be 1920x1080");

            UIRootManager rootManager = prefab.GetComponent<UIRootManager>();
            Assert.IsNotNull(rootManager, "UI_Root prefab must contain UIRootManager coordinator");

            MainMenuController mainMenu = prefab.GetComponentInChildren<MainMenuController>(true);
            Assert.IsNotNull(mainMenu, "UI_Root prefab must contain MainMenuController");

            LoadingScreenController loading = prefab.GetComponentInChildren<LoadingScreenController>(true);
            Assert.IsNotNull(loading, "UI_Root prefab must contain LoadingScreenController");

            NewGameDossierController dossier = prefab.GetComponentInChildren<NewGameDossierController>(true);
            Assert.IsNotNull(dossier, "UI_Root prefab must contain NewGameDossierController");
        }

        [Test]
        public void UIInGamePrefab_LoadsAndHasAllRequiredComponents()
        {
            string prefabPath = "Assets/Art/UI/Prefabs/UI_InGame_TheLastGod.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.IsNotNull(prefab, $"Prefab must exist at {prefabPath}");

            UIInGameManager mgr = prefab.GetComponent<UIInGameManager>();
            Assert.IsNotNull(mgr, "UI_InGame prefab must contain UIInGameManager coordinator");

            InGameHUDController hud = prefab.GetComponentInChildren<InGameHUDController>(true);
            Assert.IsNotNull(hud, "UI_InGame prefab must contain InGameHUDController");

            PauseMenuController pause = prefab.GetComponentInChildren<PauseMenuController>(true);
            Assert.IsNotNull(pause, "UI_InGame prefab must contain PauseMenuController");

            SettingsMenuController settings = prefab.GetComponentInChildren<SettingsMenuController>(true);
            Assert.IsNotNull(settings, "UI_InGame prefab must contain SettingsMenuController");

            SaveLoadMenuController saveLoad = prefab.GetComponentInChildren<SaveLoadMenuController>(true);
            Assert.IsNotNull(saveLoad, "UI_InGame prefab must contain SaveLoadMenuController");

            DialogueTerminalController dial = prefab.GetComponentInChildren<DialogueTerminalController>(true);
            Assert.IsNotNull(dial, "UI_InGame prefab must contain DialogueTerminalController");

            GameOverController gameOver = prefab.GetComponentInChildren<GameOverController>(true);
            Assert.IsNotNull(gameOver, "UI_InGame prefab must contain GameOverController");

            InventoryCodexController codex = prefab.GetComponentInChildren<InventoryCodexController>(true);
            Assert.IsNotNull(codex, "UI_InGame prefab must contain InventoryCodexController");
        }

        [Test]
        public void MainMenuScene_HasActiveUIRoot_WithObsoleteMenusRemoved()
        {
            string scenePath = "Assets/Scenes/MainMenu_Origin.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Obsolete menus must be absent
            var oldMenus = UnityEngine.Object.FindObjectsByType<LastGod.ThirdPerson.UI.MainMenuUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(0, oldMenus.Length, "Obsolete OnGUI MainMenuUI components must not exist in MainMenu_Origin");

            // Canonical UI_Root must exist
            var uiRoots = UnityEngine.Object.FindObjectsByType<UIRootManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(1, uiRoots.Length, "Exactly one UIRootManager must exist in MainMenu_Origin");
        }

        [Test]
        public void Act1OriginScene_HasActiveInGameUIRoot_WithObsoleteMenusRemoved()
        {
            string scenePath = "Assets/Scenes/Act1_Origin.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Obsolete HUDs must be absent
            var oldHuds = UnityEngine.Object.FindObjectsByType<LastGod.Player.PlayerControlsHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(0, oldHuds.Length, "Obsolete PlayerControlsHUD components must not exist in Act1_Origin");

            var oldPauses = UnityEngine.Object.FindObjectsByType<LastGod.ThirdPerson.UI.PauseMenuUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(0, oldPauses.Length, "Obsolete PauseMenuUI components must not exist in Act1_Origin");

            var oldVitals = UnityEngine.Object.FindObjectsByType<LastGod.ThirdPerson.UI.VitalStabilityHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(0, oldVitals.Length, "Obsolete VitalStabilityHUD components must not exist in Act1_Origin");

            // Canonical UI_InGame_Root must exist
            var inGameRoots = UnityEngine.Object.FindObjectsByType<UIInGameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(1, inGameRoots.Length, "Exactly one UIInGameManager must exist in Act1_Origin");
        }

        // =========================================================================
        // AUTOMATED BATCH RUNNER & LIVE BOOT VISUAL CAPTURE
        // =========================================================================
        [MenuItem("The Last God/Run All Architecture Tests & Capture Live Screen", false, 1)]
        public static void RunAllTestsAndCaptureBootScreen()
        {
            Debug.Log("=================================================================");
            Debug.Log(">>> [THE LAST GOD] INITIATING AUTOMATED TEST SUITE & SYSTEM AUDIT");
            Debug.Log("=================================================================");

            var testSuite = new UIArchitectureTests();
            var testCases = new (string Name, Action Action)[]
            {
                ("PaletteTokens_MatchMasterCreativeBriefSpecs", () => testSuite.PaletteTokens_MatchMasterCreativeBriefSpecs()),
                ("SaveSystem_MultiSlotArchival_FunctionsCorrectly", () => testSuite.SaveSystem_MultiSlotArchival_FunctionsCorrectly()),
                ("UIRootPrefab_LoadsAndHasAllRequiredComponents", () => testSuite.UIRootPrefab_LoadsAndHasAllRequiredComponents()),
                ("UIInGamePrefab_LoadsAndHasAllRequiredComponents", () => testSuite.UIInGamePrefab_LoadsAndHasAllRequiredComponents()),
                ("MainMenuScene_HasActiveUIRoot_WithObsoleteMenusRemoved", () => testSuite.MainMenuScene_HasActiveUIRoot_WithObsoleteMenusRemoved()),
                ("Act1OriginScene_HasActiveInGameUIRoot_WithObsoleteMenusRemoved", () => testSuite.Act1OriginScene_HasActiveInGameUIRoot_WithObsoleteMenusRemoved())
            };

            int passed = 0;
            int failed = 0;
            var swTotal = System.Diagnostics.Stopwatch.StartNew();
            var testReportList = new System.Collections.Generic.List<string>();
            var xmlTestCases = new System.Text.StringBuilder();

            foreach (var tc in testCases)
            {
                var swTest = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    tc.Action.Invoke();
                    swTest.Stop();
                    passed++;
                    Debug.Log($"[TEST PASS] {tc.Name} ({swTest.ElapsedMilliseconds} ms)");
                    testReportList.Add($"{{\"name\": \"{tc.Name}\", \"status\": \"PASSED\", \"durationMs\": {swTest.ElapsedMilliseconds}}}");
                    xmlTestCases.AppendLine($"    <testcase classname=\"LastGod.Tests.UIArchitectureTests\" name=\"{tc.Name}\" time=\"{(swTest.ElapsedMilliseconds / 1000f):F3}\" />");
                }
                catch (Exception ex)
                {
                    swTest.Stop();
                    failed++;
                    Debug.LogError($"[TEST FAIL] {tc.Name} ({swTest.ElapsedMilliseconds} ms): {ex.Message}");
                    testReportList.Add($"{{\"name\": \"{tc.Name}\", \"status\": \"FAILED\", \"durationMs\": {swTest.ElapsedMilliseconds}, \"error\": \"{ex.Message.Replace("\"", "\\\"")}\"}}");
                    xmlTestCases.AppendLine($"    <testcase classname=\"LastGod.Tests.UIArchitectureTests\" name=\"{tc.Name}\" time=\"{(swTest.ElapsedMilliseconds / 1000f):F3}\">");
                    xmlTestCases.AppendLine($"      <failure message=\"{System.Security.SecurityElement.Escape(ex.Message)}\">{System.Security.SecurityElement.Escape(ex.ToString())}</failure>");
                    xmlTestCases.AppendLine("    </testcase>");
                }
            }

            swTotal.Stop();
            Debug.Log("-----------------------------------------------------------------");
            Debug.Log($">>> [TEST SUMMARY] Total: {testCases.Length} | Passed: {passed} | Failed: {failed} | Time: {swTotal.ElapsedMilliseconds} ms");
            Debug.Log("-----------------------------------------------------------------");

            // Persist structured test results
            if (!Directory.Exists("Logs")) Directory.CreateDirectory("Logs");

            string xmlReport = $"<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<testsuites tests=\"{testCases.Length}\" failures=\"{failed}\" time=\"{(swTotal.ElapsedMilliseconds / 1000f):F3}\">\n  <testsuite name=\"LastGod.Tests.UIArchitectureTests\" tests=\"{testCases.Length}\" failures=\"{failed}\" time=\"{(swTotal.ElapsedMilliseconds / 1000f):F3}\">\n{xmlTestCases}  </testsuite>\n</testsuites>";
            File.WriteAllText("Logs/test_results.xml", xmlReport);

            string jsonReport = $"{{\n  \"total\": {testCases.Length},\n  \"passed\": {passed},\n  \"failed\": {failed},\n  \"durationMs\": {swTotal.ElapsedMilliseconds},\n  \"results\": [\n    {string.Join(",\n    ", testReportList)}\n  ]\n}}";
            File.WriteAllText("Logs/test_summary.json", jsonReport);

            if (failed > 0)
            {
                throw new Exception($"Test suite finished with {failed} failures!");
            }

            // =========================================================================
            // BOOT UP & CAPTURE LIVE MAIN MENU SCREEN
            // =========================================================================
            Debug.Log(">>> [THE LAST GOD] BOOTING MAIN MENU ORIGIN SCENE FOR LIVE PROOF...");

            string scenePath = "Assets/Scenes/MainMenu_Origin.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var rootManager = UnityEngine.Object.FindAnyObjectByType<UIRootManager>();
            if (rootManager == null)
            {
                throw new Exception("UIRootManager not found in MainMenu_Origin!");
            }

            rootManager.SwitchScreen(UIRootManager.ScreenState.MainMenu);

            // Update live telemetry clock & containment beacon
            var mainMenu = rootManager.GetComponentInChildren<MainMenuController>(true);
            if (mainMenu != null)
            {
                var clockField = typeof(MainMenuController).GetField("systemClockText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (clockField != null)
                {
                    var clockText = clockField.GetValue(mainMenu) as TextMeshProUGUI;
                    if (clockText != null)
                    {
                        clockText.text = $"UTC {DateTime.Now:HH:mm:ss.ff}";
                    }
                }

                var cyanDotField = typeof(MainMenuController).GetField("cyanContainmentDot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (cyanDotField != null)
                {
                    var dot = cyanDotField.GetValue(mainMenu) as Image;
                    if (dot != null)
                    {
                        dot.color = new Color(UITheme.TechnologyCyan.r, UITheme.TechnologyCyan.g, UITheme.TechnologyCyan.b, 0.95f);
                    }
                }

                // Simulate brutalist hover on 01 NEW GAME item to show active rule, orange numeral & offset
                var newItemField = typeof(MainMenuController).GetField("itemNewGame", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (newItemField != null)
                {
                    var item = newItemField.GetValue(mainMenu) as UIMenuItem;
                    if (item != null)
                    {
                        item.SetHovered(true);
                    }
                }
            }

            // Render 1080p FHD Screenshot
            GameObject camObj = new GameObject("Boot_Screenshot_Camera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = UITheme.NearBlack;
            cam.cullingMask = ~0;

            Canvas canvas = rootManager.GetComponent<Canvas>();
            CanvasScaler scaler = rootManager.GetComponent<CanvasScaler>();

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            scaler.referenceResolution = new Vector2(1920, 1080);

            Canvas.ForceUpdateCanvases();
            foreach (var tmp in rootManager.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.ForceMeshUpdate(true, true);
            }

            RenderTexture rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D capture = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            capture.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            capture.Apply();

            byte[] pngBytes = capture.EncodeToPNG();

            string screenshotDir = "Assets/Art/UI/Screenshots";
            if (!Directory.Exists(screenshotDir)) Directory.CreateDirectory(screenshotDir);

            string liveBootPath = Path.Combine(screenshotDir, "MainMenu_Live_Boot.png");
            File.WriteAllBytes(liveBootPath, pngBytes);

            // Copy to brain artifacts directory and dream-loop
            string brainArtifactDir = "C:/Users/Surya VM/.gemini/antigravity-cli/brain/bbbf2775-130c-4986-890f-7f89f3a0b51b";
            if (Directory.Exists(brainArtifactDir))
            {
                File.WriteAllBytes(Path.Combine(brainArtifactDir, "MainMenu_Live_Boot.png"), pngBytes);
            }

            string dreamLoopDir = ".dream-loop";
            if (!Directory.Exists(dreamLoopDir)) Directory.CreateDirectory(dreamLoopDir);
            File.WriteAllBytes(Path.Combine(dreamLoopDir, "target.png"), pngBytes);
            File.WriteAllBytes(Path.Combine(dreamLoopDir, "MainMenu_Live_Boot.png"), pngBytes);

            // Cleanup rendering resources
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
            cam.targetTexture = null;
            RenderTexture.active = null;
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(capture);
            UnityEngine.Object.DestroyImmediate(camObj);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Debug.Log($">>> [LIVE PROOF SAVED] Successfully captured live Main Menu at: {liveBootPath}");
            Debug.Log("=================================================================");
            Debug.Log(">>> [THE LAST GOD] AUDIT & LIVE BOOT COMPLETED WITH ZERO ERRORS");
            Debug.Log("=================================================================");
        }
    }
}
