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

            // Main Catwalk Deck Collider (runs from x = -65.5 to +65.5, top edge at Y = -13.03)
            // Height = 4.5 -> Center Y = -15.28f, covering Y = -17.53 to -13.03 (matching visual walkway slab)
            BoxCollider2D groundBox = groundRoot.AddComponent<BoxCollider2D>();
            groundBox.offset = new Vector2(0f, -15.28f);
            groundBox.size = new Vector2(131.0f, 4.5f); // Top edge at Y = -13.03

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
            chamberObj.transform.position = new Vector3(-34.0f, -11.53f, 0f);

            SpriteRenderer chamberSR = chamberObj.AddComponent<SpriteRenderer>();
            chamberSR.sortingLayerName = "Default";
            chamberSR.sortingOrder = 5;
            Sprite chamberSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Glass_Chamber.png");
            if (chamberSprite == null) chamberSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Chamber_Intact.png");
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
            // Spawn standing squarely ON top of catwalk collider at Y = -13.03
            // CapsuleCollider2D height = 1.4, offset Y = 0.7 -> bottom of collider at transform.Y
            playerObj.transform.position = new Vector3(0f, -13.03f, 0f);

            SpriteRenderer playerSR = playerObj.AddComponent<SpriteRenderer>();
            playerSR.sortingLayerName = "Default";
            playerSR.sortingOrder = 10;
            // Try loading post-chamber Aeron sprite (aeron onside idle/01.png)
            Sprite playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/aeron onside idle/01.png");
            if (playerSprite == null) playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/aeron outside idle/01.png");
            if (playerSprite == null) playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Aeron_Concept.png");
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
            camSO.FindProperty("offset").vector3Value = new Vector3(0f, 3.5f, -10f);
            camSO.ApplyModifiedProperties();

            // Save Scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Act1Scene1Builder] Successfully generated and saved scene at: {ScenePath}");
        }
    }
}
