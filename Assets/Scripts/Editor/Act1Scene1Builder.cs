using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;
using LastGod.Core;
using LastGod.Player;

namespace LastGod.Editor
{
    /// <summary>
    /// Programmatically generates and updates Act1_Scene1.unity with flickering lab background,
    /// solid catwalk ground geometry, and Aeron player setup.
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
            if (!File.Exists(ScenePath))
            {
                Debug.Log("[Act1Scene1Builder] Act1_Scene1.unity missing. Triggering automatic build...");
                BuildAct1Scene1();
            }
        }

        [MenuItem("Tools/LAST-GOD/Build Act 1 Scene 1")]
        public static void BuildAct1Scene1()
        {
            Debug.Log("[Act1Scene1Builder] === Assembling Act 1 Scene 1 (Lab Background & Catwalk Geometry) ===");

            // 1. Create a new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Setup Layer Indices
            int groundLayer = 8;
            int playerLayer = 9;

            // -------------------------------------------------------------
            // 3. BACKGROUND & AMBIENT FLICKER
            // -------------------------------------------------------------
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.position = Vector3.zero;

            SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
            bgRenderer.sortingLayerName = "Background";
            bgRenderer.sortingOrder = 0;

            Sprite f1 = AssetDatabase.LoadAssetAtPath<Sprite>(Frame1Path);
            Sprite f2 = AssetDatabase.LoadAssetAtPath<Sprite>(Frame2Path);

            if (f1 != null) bgRenderer.sprite = f1;

            AmbientBackgroundFlicker flicker = bgObj.AddComponent<AmbientBackgroundFlicker>();
            SerializedObject flickerSO = new SerializedObject(flicker);
            SerializedProperty framesProp = flickerSO.FindProperty("frameSprites");
            framesProp.arraySize = 2;
            framesProp.GetArrayElementAtIndex(0).objectReferenceValue = f1;
            framesProp.GetArrayElementAtIndex(1).objectReferenceValue = f2;
            flickerSO.FindProperty("minFlickerInterval").floatValue = 0.15f;
            flickerSO.FindProperty("maxFlickerInterval").floatValue = 0.60f;
            flickerSO.ApplyModifiedProperties();

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

            Grid grid = groundRoot.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            GameObject tilemapObj = new GameObject("Tilemap_Collision");
            tilemapObj.layer = groundLayer;
            tilemapObj.transform.SetParent(groundRoot.transform, false);

            Tilemap tilemap = tilemapObj.AddComponent<Tilemap>();
            TilemapRenderer tmRenderer = tilemapObj.AddComponent<TilemapRenderer>();
            TilemapCollider2D tmCollider = tilemapObj.AddComponent<TilemapCollider2D>();
            CompositeCollider2D composite = tilemapObj.AddComponent<CompositeCollider2D>();
            tmCollider.usedByComposite = true;

            // Main Catwalk Deck Collider (runs from x = -60 to +60 at Y = -14.2)
            // Top surface of collider at Y = -14.2, height = 2.0 -> Center Y = -15.2
            GameObject mainDeck = new GameObject("Collider_MainDeck");
            mainDeck.layer = groundLayer;
            mainDeck.transform.SetParent(groundRoot.transform, false);
            mainDeck.transform.position = new Vector3(0f, -15.2f, 0f);
            BoxCollider2D mainDeckBox = mainDeck.AddComponent<BoxCollider2D>();
            mainDeckBox.size = new Vector2(120f, 2.0f); // Top edge at Y = -14.2

            // Ledge / Crate Obstacle Break (at x = 12.8, top edge at Y = -12.0)
            GameObject crateStep = new GameObject("Collider_CrateStep");
            crateStep.layer = groundLayer;
            crateStep.transform.SetParent(groundRoot.transform, false);
            crateStep.transform.position = new Vector3(12.8f, -13.1f, 0f);
            BoxCollider2D crateBox = crateStep.AddComponent<BoxCollider2D>();
            crateBox.size = new Vector2(3.0f, 2.2f); // Top edge at Y = -12.0

            // Left Elevated Platform Step (at x = -58.0, top edge at Y = -11.7)
            GameObject leftStep = new GameObject("Collider_LeftStep");
            leftStep.layer = groundLayer;
            leftStep.transform.SetParent(groundRoot.transform, false);
            leftStep.transform.position = new Vector3(-58.0f, -12.95f, 0f);
            BoxCollider2D leftStepBox = leftStep.AddComponent<BoxCollider2D>();
            leftStepBox.size = new Vector2(8.0f, 2.5f);

            // Right Elevated Platform Step (at x = +58.0, top edge at Y = -11.7)
            GameObject rightStep = new GameObject("Collider_RightStep");
            rightStep.layer = groundLayer;
            rightStep.transform.SetParent(groundRoot.transform, false);
            rightStep.transform.position = new Vector3(58.0f, -12.95f, 0f);
            BoxCollider2D rightStepBox = rightStep.AddComponent<BoxCollider2D>();
            rightStepBox.size = new Vector2(8.0f, 2.5f);

            // Left Wall Barrier (x = -64)
            GameObject leftWall = new GameObject("Collider_LeftWall");
            leftWall.layer = groundLayer;
            leftWall.transform.SetParent(groundRoot.transform, false);
            leftWall.transform.position = new Vector3(-62.5f, 0f, 0f);
            BoxCollider2D leftWallBox = leftWall.AddComponent<BoxCollider2D>();
            leftWallBox.size = new Vector2(1f, 50f);

            // Right Wall Barrier (x = +64)
            GameObject rightWall = new GameObject("Collider_RightWall");
            rightWall.layer = groundLayer;
            rightWall.transform.SetParent(groundRoot.transform, false);
            rightWall.transform.position = new Vector3(62.5f, 0f, 0f);
            BoxCollider2D rightWallBox = rightWall.AddComponent<BoxCollider2D>();
            rightWallBox.size = new Vector2(1f, 50f);

            // -------------------------------------------------------------
            // 6. PLAYER (AERON)
            // -------------------------------------------------------------
            GameObject playerObj = new GameObject("Player");
            playerObj.tag = "Player";
            playerObj.layer = playerLayer;
            // Spawn standing squarely ON top of catwalk collider at Y = -14.2
            // CapsuleCollider2D height = 1.4, offset Y = 0.7 -> bottom of collider at transform.Y
            // Therefore transform.position.y = -14.20f
            playerObj.transform.position = new Vector3(0f, -14.20f, 0f);

            SpriteRenderer playerSR = playerObj.AddComponent<SpriteRenderer>();
            playerSR.sortingLayerName = "Default";
            playerSR.sortingOrder = 10;
            // Try loading Aeron sprite
            Sprite playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Aeron_Concept.png");
            if (playerSprite != null) playerSR.sprite = playerSprite;

            Rigidbody2D playerRb = playerObj.AddComponent<Rigidbody2D>();
            playerRb.bodyType = RigidbodyType2D.Dynamic;
            playerRb.mass = 1.0f;
            playerRb.gravityScale = 3.0f;
            playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;

            CapsuleCollider2D playerCol = playerObj.AddComponent<CapsuleCollider2D>();
            playerCol.size = new Vector2(0.5f, 1.4f);
            playerCol.offset = new Vector2(0f, 0.7f);

            PlayerController pCtrl = playerObj.AddComponent<PlayerController>();
            Health pHealth = playerObj.AddComponent<Health>();

            // -------------------------------------------------------------
            // 7. CAMERA SETUP
            // -------------------------------------------------------------
            GameObject mainCamObj = new GameObject("Main Camera");
            mainCamObj.tag = "MainCamera";
            mainCamObj.transform.position = new Vector3(0f, -9.5f, -10f);

            Camera cam = mainCamObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.0f;
            cam.backgroundColor = Color.black;
            cam.clearFlags = CameraClearFlags.SolidColor;

            PixelPerfectCamera ppCam = mainCamObj.AddComponent<PixelPerfectCamera>();
            ppCam.assetsPPU = 16;
            ppCam.refResolutionX = 240;
            ppCam.refResolutionY = 160;
            ppCam.upscaleRT = true;
            ppCam.pixelSnapping = true;

            CameraFollow camFollow = mainCamObj.AddComponent<CameraFollow>();
            SerializedObject camSO = new SerializedObject(camFollow);
            camSO.FindProperty("target").objectReferenceValue = playerObj.transform;
            camSO.ApplyModifiedProperties();

            // Save Scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Act1Scene1Builder] Successfully generated and saved scene at: {ScenePath}");
        }
    }
}
