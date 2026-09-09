using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;
using LastGod.Core;

namespace LastGod.EditorTools
{
    public static class AssembleLabScene
    {
        private const string ScenePath = "Assets/Scenes/2.5D_Lab_Scene.unity";
        private const string AeronFbxPath = "Assets/Characters/Aeron/Aeron.fbx";
        private const string GuardFbxPath = "Assets/Characters/Guard/Guard.fbx";
        private const string LabFbxPath = "Assets/Environment/Lab/Lab_BlockOut.fbx";

        [MenuItem("LastGod/Assemble 2.5D Lab Scene", false, 10)]
        public static void Assemble()
        {
            Debug.Log("[Assemble] Starting 2.5D Lab Scene Assembly...");

            // 1. Setup Layers
            SetupLayers();

            // 2. Setup Folders
            EnsureFolders();

            // 3. Configure Importers
            ConfigureModelImporter(AeronFbxPath);
            ConfigureModelImporter(GuardFbxPath);
            ConfigureModelImporter(LabFbxPath);

            // 4. Create / Configure Materials
            Material matAeron = CreateOrUpdateMaterial("Assets/Materials/Characters/M_Aeron.mat", "Assets/Characters/Aeron/Aeron_Albedo.png", "Assets/Characters/Aeron/Aeron_RoughMetal.png", 0.20f);
            Material matGuard = CreateOrUpdateMaterial("Assets/Materials/Characters/M_Guard.mat", "Assets/Characters/Guard/Guard_Albedo.png", "Assets/Characters/Guard/Guard_RoughMetal.png", 0.22f);
            Material matLab = CreateOrUpdateMaterial("Assets/Materials/Environment/M_Lab_Trim.mat", "Assets/Environment/Lab/Lab_TrimSheet.png", null, 0.20f);

            // 5. Create Animator Controllers
            AnimatorController aeronController = CreateAnimatorController("Assets/Characters/Aeron/Aeron_AnimatorController.controller", AeronFbxPath, "Aeron_Idle");
            AnimatorController guardController = CreateAnimatorController("Assets/Characters/Guard/Guard_AnimatorController.controller", GuardFbxPath, "Guard_Idle");

            // 6. Open / Create Scene
            var scene = EditorSceneManager.OpenScene(ScenePath);

            // Clear any lingering root GameObjects
            foreach (var root in scene.GetRootGameObjects())
            {
                Object.DestroyImmediate(root);
            }

            int groundLayer = LayerMask.NameToLayer("Ground");
            int characterLayer = LayerMask.NameToLayer("Character");
            if (characterLayer == -1) characterLayer = 11;
            if (groundLayer == -1) groundLayer = 8;

            // -------------------------------------------------------------
            // Lab Environment
            // -------------------------------------------------------------
            GameObject envParent = new GameObject("Lab_Environment");
            GameObject labPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LabFbxPath);
            GameObject labInstance = null;
            if (labPrefab != null)
            {
                labInstance = (GameObject)PrefabUtility.InstantiatePrefab(labPrefab, envParent.transform);
                labInstance.name = "Lab_BlockOut";
                labInstance.transform.localPosition = Vector3.zero;
                labInstance.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

                // Assign material and colliders
                foreach (var mr in labInstance.GetComponentsInChildren<MeshRenderer>(true))
                {
                    mr.sharedMaterial = matLab;
                    if (mr.gameObject.name.Contains("Floor"))
                    {
                        mr.gameObject.layer = groundLayer;
                        var mc = mr.gameObject.GetComponent<MeshCollider>();
                        if (mc == null) mc = mr.gameObject.AddComponent<MeshCollider>();
                        mc.convex = false;
                    }
                }
            }

            // -------------------------------------------------------------
            // Characters Parent
            // -------------------------------------------------------------
            GameObject charactersParent = new GameObject("Characters");

            // Aeron Instance
            GameObject aeronPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AeronFbxPath);
            if (aeronPrefab != null)
            {
                GameObject aeronInstance = (GameObject)PrefabUtility.InstantiatePrefab(aeronPrefab, charactersParent.transform);
                aeronInstance.name = "Aeron_Instance";
                aeronInstance.transform.position = new Vector3(-1.2f, 0f, 0f);
                aeronInstance.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
                SetLayerRecursively(aeronInstance, characterLayer);

                // Assign Material
                foreach (var smr in aeronInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    smr.sharedMaterial = matAeron;
                }

                // Animator
                Animator anim = aeronInstance.GetComponent<Animator>();
                if (anim == null) anim = aeronInstance.AddComponent<Animator>();
                anim.runtimeAnimatorController = aeronController;
                anim.applyRootMotion = false;

                // CapsuleCollider
                CapsuleCollider col = aeronInstance.GetComponent<CapsuleCollider>();
                if (col == null) col = aeronInstance.AddComponent<CapsuleCollider>();
                col.center = new Vector3(0f, 0.90f, 0f);
                col.radius = 0.32f;
                col.height = 1.80f;

                // Health
                Health hp = aeronInstance.GetComponent<Health>();
                if (hp == null) hp = aeronInstance.AddComponent<Health>();
                var so = new SerializedObject(hp);
                so.FindProperty("maxHP").intValue = 10;
                so.ApplyModifiedProperties();
            }

            // Guard Instance
            GameObject guardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GuardFbxPath);
            if (guardPrefab != null)
            {
                GameObject guardInstance = (GameObject)PrefabUtility.InstantiatePrefab(guardPrefab, charactersParent.transform);
                guardInstance.name = "Guard_Instance";
                guardInstance.transform.position = new Vector3(1.2f, 0f, 0f);
                guardInstance.transform.rotation = Quaternion.Euler(0f, -25f, 0f);
                SetLayerRecursively(guardInstance, characterLayer);

                // Assign Material
                foreach (var smr in guardInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    smr.sharedMaterial = matGuard;
                }

                // Animator
                Animator anim = guardInstance.GetComponent<Animator>();
                if (anim == null) anim = guardInstance.AddComponent<Animator>();
                anim.runtimeAnimatorController = guardController;
                anim.applyRootMotion = false;

                // CapsuleCollider
                CapsuleCollider col = guardInstance.GetComponent<CapsuleCollider>();
                if (col == null) col = guardInstance.AddComponent<CapsuleCollider>();
                col.center = new Vector3(0f, 1.00f, 0f);
                col.radius = 0.42f;
                col.height = 2.00f;

                // Health
                Health hp = guardInstance.GetComponent<Health>();
                if (hp == null) hp = guardInstance.AddComponent<Health>();
                var so = new SerializedObject(hp);
                so.FindProperty("maxHP").intValue = 15;
                so.ApplyModifiedProperties();
            }

            // -------------------------------------------------------------
            // Lighting Parent
            // -------------------------------------------------------------
            GameObject lightingParent = new GameObject("Lighting");

            // Key Light (Cold Cyan #6FE3FF, intensity 1.2)
            GameObject keyLightObj = new GameObject("Key_Light");
            keyLightObj.transform.SetParent(lightingParent.transform);
            keyLightObj.transform.rotation = Quaternion.Euler(35f, -40f, 0f);
            Light keyLight = keyLightObj.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            ColorUtility.TryParseHtmlString("#6FE3FF", out Color keyColor);
            keyLight.color = keyColor;
            keyLight.intensity = 1.2f;
            keyLight.shadows = LightShadows.Soft;

            // Fill Light (Neutral Dark Blue-Grey #0E1420, intensity 0.18)
            GameObject fillLightObj = new GameObject("Fill_Light");
            fillLightObj.transform.SetParent(lightingParent.transform);
            fillLightObj.transform.rotation = Quaternion.Euler(45f, 140f, 0f);
            Light fillLight = fillLightObj.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            ColorUtility.TryParseHtmlString("#0E1420", out Color fillColor);
            fillLight.color = fillColor;
            fillLight.intensity = 0.18f;
            fillLight.shadows = LightShadows.None;

            // -------------------------------------------------------------
            // Main Camera
            // -------------------------------------------------------------
            GameObject cameraObj = new GameObject("Main_Camera");
            cameraObj.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0f, 1.6f, -6.0f);
            cameraObj.transform.rotation = Quaternion.Euler(5f, 0f, 0f);

            Camera cam = cameraObj.AddComponent<Camera>();
            cam.fieldOfView = 27f;
            cam.orthographic = false;
            cam.clearFlags = CameraClearFlags.SolidColor;
            ColorUtility.TryParseHtmlString("#05060A", out Color bgColor);
            cam.backgroundColor = bgColor;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            // Ensure UniversalAdditionalCameraData
            var camData = cameraObj.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null) camData = cameraObj.AddComponent<UniversalAdditionalCameraData>();
            camData.renderType = CameraRenderType.Base;

            // -------------------------------------------------------------
            // Physics Collision Matrix
            // -------------------------------------------------------------
            Physics.IgnoreLayerCollision(characterLayer, groundLayer, false);
            // Disable character colliding with non-gameplay layers
            for (int i = 0; i < 32; i++)
            {
                if (i != groundLayer && i != characterLayer)
                {
                    Physics.IgnoreLayerCollision(characterLayer, i, true);
                }
            }

            // Save Scene
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Assemble] 2.5D Lab Scene assembled successfully!");
        }

        private static void SetupLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProp = tagManager.FindProperty("layers");

            // Ensure Ground at 8
            var layer8 = layersProp.GetArrayElementAtIndex(8);
            if (string.IsNullOrEmpty(layer8.stringValue)) layer8.stringValue = "Ground";

            // Ensure Character at 11
            var layer11 = layersProp.GetArrayElementAtIndex(11);
            layer11.stringValue = "Character";

            tagManager.ApplyModifiedProperties();
        }

        private static void EnsureFolders()
        {
            string[] folders = {
                "Assets/Materials",
                "Assets/Materials/Characters",
                "Assets/Materials/Environment"
            };
            foreach (var f in folders)
            {
                if (!AssetDatabase.IsValidFolder(f))
                {
                    string parent = Path.GetDirectoryName(f).Replace("\\", "/");
                    string child = Path.GetFileName(f);
                    AssetDatabase.CreateFolder(parent, child);
                }
            }
        }

        private static void ConfigureModelImporter(string path)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) return;

            importer.importCameras = false;
            importer.importLights = false;
            importer.globalScale = 1.0f;
            importer.animationType = ModelImporterAnimationType.Generic;

            // Configure clips
            var clipAnims = importer.defaultClipAnimations;
            if (clipAnims != null && clipAnims.Length > 0)
            {
                foreach (var c in clipAnims)
                {
                    c.loopTime = true;
                }
                importer.clipAnimations = clipAnims;
            }

            importer.SaveAndReimport();
        }

        private static Material CreateOrUpdateMaterial(string matPath, string albedoPath, string roughMetalPath, float smoothness)
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) urpLit = Shader.Find("Standard");

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(urpLit);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = urpLit;
            }

            if (!string.IsNullOrEmpty(albedoPath))
            {
                Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
                if (albedo != null) mat.SetTexture("_BaseMap", albedo);
            }
            if (!string.IsNullOrEmpty(roughMetalPath))
            {
                Texture2D rm = AssetDatabase.LoadAssetAtPath<Texture2D>(roughMetalPath);
                if (rm != null) mat.SetTexture("_MetallicSpecGlossMap", rm);
            }

            mat.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static AnimatorController CreateAnimatorController(string controllerPath, string fbxPath, string clipName)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            // Find clip in FBX
            AnimationClip animClip = null;
            var assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            foreach (var a in assets)
            {
                if (a is AnimationClip c && !c.name.StartsWith("__preview__"))
                {
                    animClip = c;
                    break;
                }
            }

            var rootStateMachine = controller.layers[0].stateMachine;
            var states = rootStateMachine.states;
            AnimatorState idleState = null;
            foreach (var s in states)
            {
                if (s.state.name == "Idle")
                {
                    idleState = s.state;
                    break;
                }
            }

            if (idleState == null)
            {
                idleState = rootStateMachine.AddState("Idle");
            }

            if (animClip != null)
            {
                idleState.motion = animClip;
            }

            rootStateMachine.defaultState = idleState;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void SetLayerRecursively(GameObject obj, int layer)
        {
            if (obj == null) return;
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}
