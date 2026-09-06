using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using LastGod.Core;
using LastGod.Combat;

namespace LastGod.Editor
{
    /// <summary>
    /// Programmatically generates and configures the complete First Level (Act 1 Level 1 - Village)
    /// using Cainos Pixel Art Platformer - Village Props assets.
    /// Creates Assets/Scenes/Act1_Level1_Village.unity with zero manual placement errors.
    /// </summary>
    [InitializeOnLoad]
    public static class Level1Builder
    {
        private const string ScenePath = "Assets/Scenes/Act1_Level1_Village.unity";
        private const string CainosPrefabPath = "Assets/Cainos/Pixel Art Platformer - Village Props/Prefab/";

        static Level1Builder()
        {
            EditorApplication.delayCall += AutoBuildIfNeeded;
        }

        private static void AutoBuildIfNeeded()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.Log("[Level1Builder] Act1_Level1_Village.unity not detected. Triggering automatic build...");
                BuildLevel1Scene();
            }
        }

        [MenuItem("Tools/LAST-GOD/Build Level 1 (Village Scene)")]
        public static void BuildLevel1Scene()
        {
            Debug.Log("[Level1Builder] === Starting Generation of Act 1 Level 1 (Village Scene) ===");

            // 1. Create a new empty scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Setup Layer Masks: Ground is Layer 8, Player is Layer 9
            int groundLayerIndex = 8;
            int playerLayerIndex = 9;

            // 3. Create Root Organization Groups
            GameObject envRoot = new GameObject("=== ENVIRONMENT ===");
            GameObject terrainRoot = new GameObject("=== TERRAIN & COLLIDERS ===");
            GameObject platformsRoot = new GameObject("=== PLATFORMS & BRIDGES ===");
            GameObject propsRoot = new GameObject("=== VILLAGE PROPS & FOLIAGE ===");
            GameObject hazardsRoot = new GameObject("=== HAZARDS ===");
            GameObject actorsRoot = new GameObject("=== ACTORS ===");

            // -------------------------------------------------------------
            // SECTION A: BACKGROUND & LIGHTING
            // -------------------------------------------------------------
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(envRoot.transform);
            bgObj.transform.position = new Vector3(13f, 3.5f, 10f);
            bgObj.transform.localScale = new Vector3(1f, 1f, 1f);

            var bgRenderer = bgObj.AddComponent<SpriteRenderer>();
            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Village_Background.png");
            if (bgSprite != null)
            {
                bgRenderer.sprite = bgSprite;
                bgRenderer.drawMode = SpriteDrawMode.Tiled;
                bgRenderer.size = new Vector2(100f, 25f);
                bgRenderer.sortingOrder = -100;
                Debug.Log("[Level1Builder] Assigned Village_Background.png to Background.");
            }
            else
            {
                Debug.LogWarning("[Level1Builder] Could not find Assets/Art/Sprites/Village_Background.png");
            }

            // Global 2D Light
            GameObject globalLightObj = new GameObject("Global_Light2D");
            globalLightObj.transform.SetParent(envRoot.transform);
            var globalLight = globalLightObj.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            globalLight.color = new Color(0.95f, 0.90f, 0.82f, 1f); // Warm atmospheric twilight/dawn
            globalLight.intensity = 0.85f;

            // -------------------------------------------------------------
            // SECTION B: TERRAIN COLLIDERS (Layer 8: Ground)
            // -------------------------------------------------------------
            // Sector 1: Grove Floor (-22 to -2, surface at y = -2.0)
            CreateGroundBox(terrainRoot.transform, "Ground_Sector1_Grove", new Vector2(-12f, -3.5f), new Vector2(20f, 3f), groundLayerIndex);

            // Sector 2: Village Terrace Floor (0 to 17.5, surface at y = -0.5)
            CreateGroundBox(terrainRoot.transform, "Ground_Sector2_VillageTerrace", new Vector2(8.75f, -2.0f), new Vector2(17.5f, 3f), groundLayerIndex);

            // Sector 3: Chasm Pit Floor (17.5 to 28.5, surface at y = -6.5)
            CreateGroundBox(terrainRoot.transform, "Ground_Sector3_ChasmPit", new Vector2(23f, -8.0f), new Vector2(11f, 3f), groundLayerIndex);

            // Sector 4: Shrine Plateau Floor (29.5 to 48.5, surface at y = 1.5)
            CreateGroundBox(terrainRoot.transform, "Ground_Sector4_ShrinePlateau", new Vector2(39f, 0.0f), new Vector2(19f, 3f), groundLayerIndex);

            // Boundary Walls
            CreateWallBox(terrainRoot.transform, "Wall_Left_Boundary", new Vector2(-22.5f, 5f), new Vector2(1f, 24f), groundLayerIndex);
            CreateWallBox(terrainRoot.transform, "Wall_Right_Boundary", new Vector2(48.5f, 5f), new Vector2(1f, 24f), groundLayerIndex);

            // Lower Chasm Walls (below bridge deck)
            CreateWallBox(terrainRoot.transform, "Wall_Chasm_Left", new Vector2(17.5f, -4.5f), new Vector2(0.5f, 4f), groundLayerIndex);
            CreateWallBox(terrainRoot.transform, "Wall_Chasm_Right", new Vector2(28.5f, -4.5f), new Vector2(0.5f, 4f), groundLayerIndex);

            // Fall Catcher / Bottom Pit Trap
            GameObject bottomPit = new GameObject("Pit_Fall_Catcher");
            bottomPit.transform.SetParent(hazardsRoot.transform);
            bottomPit.transform.position = new Vector2(13f, -14f);
            var pitCol = bottomPit.AddComponent<BoxCollider2D>();
            pitCol.size = new Vector2(120f, 2f);
            pitCol.isTrigger = true;
            bottomPit.AddComponent<HazardSpike>();

            // -------------------------------------------------------------
            // SECTION C: CAINOS PLATFORMS, BRIDGES, STAIRS & LADDERS
            // -------------------------------------------------------------
            // 1. Two Static Wooden Bridges across Chasm (x: 17.4 to 28.8, surface at y = -0.5)
            SpawnCainosPrefab("PF_WoodenBridge_Static", new Vector3(17.4f, -0.45f, 0f), platformsRoot.transform);
            SpawnCainosPrefab("PF_WoodenBridge_Static", new Vector3(23.0f, -0.45f, 0f), platformsRoot.transform);
            CreateGroundBox(platformsRoot.transform, "Collider_Bridge", new Vector2(23.1f, -0.65f), new Vector2(11.5f, 0.3f), groundLayerIndex);

            // 2. Stairs connecting Grove to Village Terrace (x: -1, y: -2.0)
            SpawnCainosPrefab("PF Village Props - Stairs X32", new Vector3(-1f, -2.0f, 0f), platformsRoot.transform);
            CreateGroundSlopeCollider(terrainRoot.transform, "Slope_Grove_To_Terrace", new Vector2(-1f, -1.25f), new Vector2(2.0f, 1.5f), groundLayerIndex);

            // 3. Stairs connecting Bridge to Shrine Plateau (x: 28.5f, y: 0.5f)
            SpawnCainosPrefab("PF Village Props - Stairs X48", new Vector3(28.5f, 0.5f, 0f), platformsRoot.transform);
            CreateGroundSlopeCollider(terrainRoot.transform, "Slope_Bridge_To_Shrine", new Vector2(28.5f, 0.5f), new Vector2(2.0f, 2.0f), groundLayerIndex);

            // 4. Village Rooftop Platform 1 (x: 5, y: 1.5)
            var rooftop1 = SpawnCainosPrefab("PF Village Props - Platform 02 X4", new Vector3(5f, 1.5f, 0f), platformsRoot.transform);
            if (rooftop1 != null)
            {
                CreateOneWayPlatformCollider(rooftop1, new Vector2(0f, 0.05f), new Vector2(4.5f, 0.25f), groundLayerIndex);
            }

            // 5. Village High Lookout Platform (x: 11, y: 4.0)
            var lookout = SpawnCainosPrefab("PF Village Props - Platform 02 X3", new Vector3(11f, 4.0f, 0f), platformsRoot.transform);
            if (lookout != null)
            {
                CreateOneWayPlatformCollider(lookout, new Vector2(0f, 0.05f), new Vector2(3.5f, 0.25f), groundLayerIndex);
            }

            // 6. Intermediate acrobatic platforms
            var midPlat1 = SpawnCainosPrefab("PF Village Props - Platform 01 L X2", new Vector3(1f, 3.2f, 0f), platformsRoot.transform);
            if (midPlat1 != null) CreateOneWayPlatformCollider(midPlat1, new Vector2(0f, 0f), new Vector2(2.5f, 0.25f), groundLayerIndex);

            var midPlat2 = SpawnCainosPrefab("PF Village Props - Platform 01 R X2", new Vector3(15f, 2.8f, 0f), platformsRoot.transform);
            if (midPlat2 != null) CreateOneWayPlatformCollider(midPlat2, new Vector2(0f, 0f), new Vector2(2.5f, 0.25f), groundLayerIndex);

            // High acrobatic route over chasm
            var chasmPlat1 = SpawnCainosPrefab("PF Village Props - Platform 02 X2", new Vector3(20.5f, 3.6f, 0f), platformsRoot.transform);
            if (chasmPlat1 != null) CreateOneWayPlatformCollider(chasmPlat1, new Vector2(0f, 0f), new Vector2(2.5f, 0.25f), groundLayerIndex);

            var chasmPlat2 = SpawnCainosPrefab("PF Village Props - Platform 02 X2", new Vector3(25.5f, 3.6f, 0f), platformsRoot.transform);
            if (chasmPlat2 != null) CreateOneWayPlatformCollider(chasmPlat2, new Vector2(0f, 0f), new Vector2(2.5f, 0.25f), groundLayerIndex);

            // 7. Ladders
            SpawnCainosPrefab("PF Village Props - Ladder 01", new Vector3(2.5f, 0.2f, 0f), platformsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Ladder 02", new Vector3(8.8f, 2.5f, 0f), platformsRoot.transform);

            // -------------------------------------------------------------
            // SECTION D: FOLIAGE & NATURE (Trees, Bushes, Flowers, Rocks)
            // -------------------------------------------------------------
            // Large Trees
            SpawnCainosPrefab("PF Village Props - Tree 01", new Vector3(-20f, -0.6f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Tree 02", new Vector3(-16f, -0.6f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Tree 01", new Vector3(-6f, -0.6f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Tree 02", new Vector3(17f, 1.0f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Tree 01", new Vector3(32f, 3.0f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Tree 02", new Vector3(45f, 3.0f, 0f), propsRoot.transform);

            // Bushes
            SpawnCainosPrefab("PF Village Props - Bush 01", new Vector3(-18.5f, -2.2f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Bush 02", new Vector3(-14.5f, -2.2f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Bush 03", new Vector3(-7.5f, -2.2f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Bush 01", new Vector3(2f, -0.7f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Bush 02", new Vector3(14f, -0.7f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Bush 03", new Vector3(34f, 1.3f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Bush 01", new Vector3(43f, 1.3f, 0f), propsRoot.transform);

            // Rocks & Boulders
            SpawnCainosPrefab("PF Village Props - Rock 01", new Vector3(-21f, -2.2f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Rock 02", new Vector3(-3.5f, -2.2f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Rock 03", new Vector3(17.5f, -0.7f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Rock 01", new Vector3(28.5f, 1.3f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Rock 02", new Vector3(46.5f, 1.3f, 0f), propsRoot.transform);

            // Grass Tufts & Flowers
            for (float gx = -21f; gx < -3f; gx += 2.5f)
            {
                string grassPrefab = (Mathf.Abs((int)gx) % 2 == 0) ? "PF Village Props - Grass 01" : "PF Village Props - Grass 03";
                SpawnCainosPrefab(grassPrefab, new Vector3(gx, -2.3f, 0f), propsRoot.transform);
            }
            for (float gx = 1f; gx < 17f; gx += 3f)
            {
                SpawnCainosPrefab("PF Village Props - Grass 02", new Vector3(gx, -0.8f, 0f), propsRoot.transform);
            }
            for (float gx = 30f; gx < 46f; gx += 2.8f)
            {
                SpawnCainosPrefab("PF Village Props - Grass 04", new Vector3(gx, 1.2f, 0f), propsRoot.transform);
            }

            SpawnCainosPrefab("PF Village Props - Sunflower 01", new Vector3(-17f, -2.1f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Sunflower 02", new Vector3(-15.5f, -2.1f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Flower 01", new Vector3(-8f, -2.2f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Flower 02", new Vector3(7f, -0.8f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Wheat 01", new Vector3(31f, 1.2f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Wheat 02", new Vector3(32.5f, 1.2f, 0f), propsRoot.transform);

            // -------------------------------------------------------------
            // SECTION E: VILLAGE SETTLEMENT & PROPS
            // -------------------------------------------------------------
            // 1. Signs & Fences
            SpawnCainosPrefab("PF Village Props - Sign 01", new Vector3(-17.5f, -2.0f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Fence A", new Vector3(-11f, -2.1f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Road Sign 01", new Vector3(-0.2f, -0.6f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Fence B", new Vector3(13f, -0.6f, 0f), propsRoot.transform);

            // 2. Training Dummy (Practice Combat with Aeron's attacks!)
            var dummyObj = SpawnCainosPrefab("PF Village Props - Training Dummy", new Vector3(-13.5f, -2.0f, 0f), propsRoot.transform);
            if (dummyObj != null)
            {
                var col = dummyObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.8f, 1.4f);
                col.offset = new Vector2(0f, 0.7f);
                dummyObj.AddComponent<InteractiveDummy>();
                dummyObj.tag = "Untagged";
                Debug.Log("[Level1Builder] Attached InteractiveDummy to Training Dummy.");
            }

            // Weapon Rack & Anvil
            SpawnCainosPrefab("PF Village Props - Weapon Rack", new Vector3(-10f, -2.0f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Anvil 01", new Vector3(-8.5f, -2.2f, 0f), propsRoot.transform);

            // 3. Campfire Area (x: 3.5, y: -1.0)
            var campfire = SpawnCainosPrefab("PF Village Props - Campfire 01", new Vector3(3.5f, -0.7f, 0f), propsRoot.transform);
            if (campfire != null)
            {
                AddPointLight2D(campfire, new Color(1f, 0.6f, 0.25f), 1.5f, 4.5f);
            }
            SpawnCainosPrefab("PF Village Props - Log Bench", new Vector3(5.2f, -0.8f, 0f), propsRoot.transform);

            // 4. Village Market / Square
            SpawnCainosPrefab("PF Village Props - Well", new Vector3(8f, -0.5f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Stall", new Vector3(11.5f, -0.5f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Wheelbarrow", new Vector3(14.5f, -0.8f, 0f), propsRoot.transform);

            // Crates and Barrels
            SpawnCainosPrefab("PF Village Props - Barrel", new Vector3(0.5f, -0.7f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Crate Large", new Vector3(1.2f, -0.7f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Crate Small", new Vector3(1.2f, -0.1f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Barrel", new Vector3(10.2f, -0.7f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Hay Bale", new Vector3(16f, -0.6f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Wood Logs", new Vector3(-5f, -2.2f, 0f), propsRoot.transform);

            // 5. Street Lamps & Torches with 2D Point Lights
            var roadLamp1 = SpawnCainosPrefab("PF Village Props - Road Lamp", new Vector3(0f, -0.3f, 0f), propsRoot.transform);
            if (roadLamp1 != null) AddPointLight2D(roadLamp1, new Color(1f, 0.78f, 0.45f), 1.2f, 3.5f, new Vector2(0f, 1.2f));

            var roadLamp2 = SpawnCainosPrefab("PF Village Props - Road Lamp", new Vector3(16.5f, -0.3f, 0f), propsRoot.transform);
            if (roadLamp2 != null) AddPointLight2D(roadLamp2, new Color(1f, 0.78f, 0.45f), 1.2f, 3.5f, new Vector2(0f, 1.2f));

            var torch1 = SpawnCainosPrefab("PF Village Props - Torch", new Vector3(5f, 2.2f, 0f), propsRoot.transform);
            if (torch1 != null) AddPointLight2D(torch1, new Color(1f, 0.55f, 0.2f), 1.3f, 3.2f, new Vector2(0f, 0.5f));

            var torch2 = SpawnCainosPrefab("PF Village Props - Torch", new Vector3(11f, 4.7f, 0f), propsRoot.transform);
            if (torch2 != null) AddPointLight2D(torch2, new Color(1f, 0.55f, 0.2f), 1.3f, 3.2f, new Vector2(0f, 0.5f));

            // -------------------------------------------------------------
            // SECTION F: INTERACTIVE CHESTS
            // -------------------------------------------------------------
            // 1. Wooden Chest on Lookout platform (x: 11.5, y: 4.2)
            var chestWood = SpawnCainosPrefab("PF Village Props - Chest Wooden", new Vector3(11.5f, 4.2f, 0f), propsRoot.transform);
            if (chestWood != null)
            {
                var chestCol = chestWood.AddComponent<BoxCollider2D>();
                chestCol.size = new Vector2(1.2f, 1f);
                chestCol.isTrigger = true;
                chestWood.AddComponent<InteractiveChest>();
                Debug.Log("[Level1Builder] Configured Interactive Wooden Chest on Lookout.");
            }

            // 2. Secret Iron Chest down in Chasm secret alcove (x: 24, y: -6.8)
            var chestIron = SpawnCainosPrefab("PF Village Props - Chest Iron", new Vector3(24f, -6.8f, 0f), propsRoot.transform);
            if (chestIron != null)
            {
                var chestCol = chestIron.AddComponent<BoxCollider2D>();
                chestCol.size = new Vector2(1.2f, 1f);
                chestCol.isTrigger = true;
                chestIron.AddComponent<InteractiveChest>();
            }

            // 3. Golden Chest at the Ancient Shrine (x: 43, y: 1.3)
            var chestGold = SpawnCainosPrefab("PF Village Props - Chest Golden", new Vector3(43f, 1.3f, 0f), propsRoot.transform);
            if (chestGold != null)
            {
                var chestCol = chestGold.AddComponent<BoxCollider2D>();
                chestCol.size = new Vector2(1.2f, 1f);
                chestCol.isTrigger = true;
                chestGold.AddComponent<InteractiveChest>();
                AddPointLight2D(chestGold, new Color(1f, 0.85f, 0.2f), 1.4f, 3.2f, new Vector2(0f, 0.5f));
            }

            // -------------------------------------------------------------
            // SECTION G: HAZARDS (Spike Pit in Chasm)
            // -------------------------------------------------------------
            for (float sx = 18.5f; sx <= 27.5f; sx += 1.0f)
            {
                var spike = SpawnCainosPrefab("PF Village Props - Spike", new Vector3(sx, -6.8f, 0f), hazardsRoot.transform);
                if (spike != null)
                {
                    var spikeCol = spike.AddComponent<BoxCollider2D>();
                    spikeCol.size = new Vector2(0.9f, 0.5f);
                    spikeCol.offset = new Vector2(0f, 0.25f);
                    spikeCol.isTrigger = true;
                    spike.AddComponent<HazardSpike>();
                }
            }

            // -------------------------------------------------------------
            // SECTION H: ANCIENT SHRINE & GOAL AREA
            // -------------------------------------------------------------
            // Stone of Recall with ethereal cyan light
            var recallStone = SpawnCainosPrefab("PF Village Props - Stone of Recall", new Vector3(38f, 1.5f, 0f), propsRoot.transform);
            if (recallStone != null)
            {
                AddPointLight2D(recallStone, new Color(0.35f, 0.85f, 1f), 1.6f, 5.0f, new Vector2(0f, 0.8f));
            }

            // Village Guardian Statue
            SpawnCainosPrefab("PF Village Props - Statue", new Vector3(40.5f, 1.8f, 0f), propsRoot.transform);

            // Banners & Memorials
            SpawnCainosPrefab("PF Village Props - Banner", new Vector3(36f, 1.8f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Banner", new Vector3(45f, 1.8f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Gravestone 01", new Vector3(35f, 1.2f, 0f), propsRoot.transform);
            SpawnCainosPrefab("PF Village Props - Gravestone 02", new Vector3(46f, 1.2f, 0f), propsRoot.transform);

            // -------------------------------------------------------------
            // SECTION I: PLAYER (AERON - HERO KNIGHT) & BRINGER OF DEATH
            // -------------------------------------------------------------
            GameObject playerObj = new GameObject("Aeron");
            playerObj.transform.SetParent(actorsRoot.transform);
            playerObj.transform.position = new Vector3(-18f, -1.94f, 0f);
            playerObj.tag = "Player";
            playerObj.layer = playerLayerIndex;

            var playerRb = playerObj.AddComponent<Rigidbody2D>();
            playerRb.bodyType = RigidbodyType2D.Dynamic;
            playerRb.mass = 1f;
            playerRb.gravityScale = 3f;
            playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            playerRb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var playerCol = playerObj.AddComponent<BoxCollider2D>();
            playerCol.offset = new Vector2(0f, 0.662f);
            playerCol.size = new Vector2(0.73f, 1.2f);

            var playerHealth = playerObj.AddComponent<Health>();

            var playerRenderer = playerObj.AddComponent<SpriteRenderer>();
            var heroKnightSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Hero Knight - Pixel Art/Sprites/HeroKnight.png");
            if (heroKnightSprites != null && heroKnightSprites.Length > 0)
            {
                foreach (var obj in heroKnightSprites)
                {
                    if (obj is Sprite sp) { playerRenderer.sprite = sp; break; }
                }
            }
            playerRenderer.sortingOrder = 15;

            var playerAnim = playerObj.AddComponent<Animator>();
            var heroController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Hero Knight - Pixel Art/Animations/HeroKnight_AnimController.controller");
            if (heroController != null) playerAnim.runtimeAnimatorController = heroController;

            var playerAudio = playerObj.AddComponent<AudioSource>();

            var playerController = playerObj.AddComponent<LastGod.Player.PlayerController>();
            playerObj.AddComponent<LastGod.Player.PlayerControlsHUD>();

            var pcType = typeof(LastGod.Player.PlayerController);
            void SetPCField(string fieldName, object val) {
                var f = pcType.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (f != null) f.SetValue(playerController, val);
            }
            SetPCField("groundLayer", (LayerMask)((1 << groundLayerIndex) | (1 << 0)));
            SetPCField("animator", playerAnim);
            SetPCField("audioSource", playerAudio);
            SetPCField("slideDustPrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Hero Knight - Pixel Art/Demo/SlideDust.prefab"));
            SetPCField("slash1SFX", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/player_slash1.wav"));
            SetPCField("slash2SFX", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/player_slash2.wav"));
            SetPCField("slash3SFX", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/player_slash3.wav"));
            SetPCField("jumpSFX", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/player_jump.wav"));
            SetPCField("dashSFX", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/player_dash.wav"));
            SetPCField("hurtSFX", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/player_hurt.wav"));
            SetPCField("deathSFX", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/player_death.wav"));

            // Bringer of Death Enemies
            SpawnBringerOfDeath("BringerOfDeath_TerraceSentry", new Vector3(7f, -0.5f, 0f), 1f, 15, actorsRoot.transform);
            SpawnBringerOfDeath("BringerOfDeath_BridgeGuardian", new Vector3(23f, -0.5f, 0f), 1f, 20, actorsRoot.transform);
            SpawnBringerOfDeath("BringerOfDeath_ShrineWarden_Boss", new Vector3(38f, 1.5f, 0f), 1.5f, 45, actorsRoot.transform);

            // Main Camera
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(-18f, -1.2f, -10f);

            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.11f, 0.08f, 0.21f, 1f); // Deep dusk sky
            cam.clearFlags = CameraClearFlags.SolidColor;

            var ppCam = camObj.AddComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>();
            ppCam.assetsPPU = 16;
            ppCam.refResolutionX = 240;
            ppCam.refResolutionY = 160;
            ppCam.gridSnapping = UnityEngine.Rendering.Universal.PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;

            var camFollow = camObj.AddComponent<LastGod.Player.CameraFollow>();
            var targetField = typeof(LastGod.Player.CameraFollow).GetField("target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (targetField != null)
            {
                targetField.SetValue(camFollow, playerObj.transform);
            }

            camObj.AddComponent<LastGod.Core.CameraShake2D>();
            camObj.AddComponent<AudioListener>();

            // -------------------------------------------------------------
            // SECTION J: SAVE SCENE & REGISTER IN BUILD SETTINGS
            // -------------------------------------------------------------
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Level1Builder] ✅ Successfully saved new scene to {ScenePath}!");

            RegisterSceneInBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Level1Builder] === Act 1 Level 1 (Village Scene) Generation Complete! ===");
        }

        private static GameObject SpawnCainosPrefab(string prefabName, Vector3 position, Transform parent)
        {
            string fullPath = CainosPrefabPath + prefabName + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[Level1Builder] Failed to load prefab at {fullPath}");
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = position;
            instance.transform.SetParent(parent);
            return instance;
        }

        private static void CreateGroundBox(Transform parent, string name, Vector2 center, Vector2 size, int layer)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.position = center;
            obj.layer = layer;
            var col = obj.AddComponent<BoxCollider2D>();
            col.size = size;
        }

        private static void CreateWallBox(Transform parent, string name, Vector2 center, Vector2 size, int layer)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.position = center;
            obj.layer = layer;
            var col = obj.AddComponent<BoxCollider2D>();
            col.size = size;
        }

        private static void CreateGroundSlopeCollider(Transform parent, string name, Vector2 center, Vector2 size, int layer)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.position = center;
            obj.layer = layer;
            // Angled slope polygon or box
            var col = obj.AddComponent<PolygonCollider2D>();
            col.points = new Vector2[]
            {
                new Vector2(-size.x * 0.5f, -size.y * 0.5f),
                new Vector2(size.x * 0.5f, size.y * 0.5f),
                new Vector2(size.x * 0.5f, -size.y * 0.5f)
            };
        }

        private static void CreateOneWayPlatformCollider(GameObject target, Vector2 offset, Vector2 size, int layer)
        {
            target.layer = layer;
            var col = target.AddComponent<BoxCollider2D>();
            col.offset = offset;
            col.size = size;
            col.usedByEffector = true;

            var effector = target.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.useOneWayGrouping = true;
            effector.surfaceArc = 170f;
        }

        private static void AddPointLight2D(GameObject parent, Color color, float intensity, float outerRadius, Vector2? offset = null)
        {
            GameObject lightObj = new GameObject("Light2D_Point");
            lightObj.transform.SetParent(parent.transform);
            Vector2 off = offset ?? Vector2.zero;
            lightObj.transform.localPosition = new Vector3(off.x, off.y, 0f);

            var light2D = lightObj.AddComponent<Light2D>();
            light2D.lightType = Light2D.LightType.Point;
            light2D.color = color;
            light2D.intensity = intensity;
            light2D.pointLightOuterRadius = outerRadius;
            light2D.pointLightInnerRadius = outerRadius * 0.2f;
        }

        private static void RegisterSceneInBuildSettings(string scenePath)
        {
            var currentScenes = EditorBuildSettings.scenes;
            foreach (var s in currentScenes)
            {
                if (s.path == scenePath) return;
            }

            var newScenes = new EditorBuildSettingsScene[currentScenes.Length + 1];
            for (int i = 0; i < currentScenes.Length; i++)
            {
                newScenes[i] = currentScenes[i];
            }
            newScenes[currentScenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
            Debug.Log($"[Level1Builder] Registered {scenePath} in EditorBuildSettings.");
        }

        private static void SpawnBringerOfDeath(string name, Vector3 pos, float scale, int maxHp, Transform parent)
        {
            GameObject reaperObj = new GameObject(name);
            reaperObj.transform.SetParent(parent);
            reaperObj.transform.position = pos;
            reaperObj.transform.localScale = new Vector3(scale, scale, 1f);
            reaperObj.tag = "Enemy";
            reaperObj.layer = 10;

            var rb = reaperObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = 2f;
            rb.gravityScale = 2f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var col = reaperObj.AddComponent<CapsuleCollider2D>();
            col.offset = new Vector2(0f, 0.7f);
            col.size = new Vector2(0.8f, 1.4f);

            var hp = reaperObj.AddComponent<Health>();
            var maxHpField = typeof(Health).GetField("maxHP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (maxHpField != null) maxHpField.SetValue(hp, maxHp);

            var sr = reaperObj.AddComponent<SpriteRenderer>();
            var reaperSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Bringer Of Death/Sprite Sheet/Bringer-of-Death-SpritSheet.png");
            if (reaperSprites != null)
            {
                foreach (var obj in reaperSprites)
                {
                    if (obj is Sprite sp) { sr.sprite = sp; break; }
                }
            }
            sr.sortingOrder = 14;

            var anim = reaperObj.AddComponent<Animator>();
            var reaperController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Bringer Of Death/Animation/Bringer Of Death.controller");
            if (reaperController != null) anim.runtimeAnimatorController = reaperController;

            var audio = reaperObj.AddComponent<AudioSource>();

            var ai = reaperObj.AddComponent<LastGod.Enemies.BringerOfDeathAI>();
            var aiType = typeof(LastGod.Enemies.BringerOfDeathAI);
            void SetAIField(string fieldName, object val) {
                var f = aiType.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (f != null) f.SetValue(ai, val);
            }
            SetAIField("spriteRenderer", sr);
            SetAIField("animator", anim);
            SetAIField("audioSource", audio);
            SetAIField("darkMagicPrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DarkMagicOrb.prefab"));
            SetAIField("scytheAttackSFX", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/reaper_attack.wav"));
            SetAIField("spellCastSFX", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/reaper_spell.wav"));
            SetAIField("hurtSFX", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/reaper_hurt.wav"));
            SetAIField("deathSFX", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/reaper_death.wav"));
        }
    }
}
