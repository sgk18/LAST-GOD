using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using LastGod.FirstPerson.Player;
using LastGod.Core;

namespace LastGod.EditorTools
{
    public static class AssembleFirstPersonAeronPass
    {
        private const string TextureDir = "Assets/Characters/Aeron/Textures";
        private const string MaterialDir = "Assets/Characters/Aeron/Materials";
        private const string MeshesDir = "Assets/Characters/Aeron/Meshes";
        private const string PrefabDir = "Assets/Prefabs/Player";
        private const string ScenePath = "Assets/Scenes/2.5D_Lab_Scene.unity";

        [MenuItem("The Last God/Assemble First-Person Aeron Pass", false, 10)]
        public static void ExecutePass()
        {
            Debug.Log("[AssembleFP] Starting Full 3D First-Person Aeron Assembly Pass...");

            Directory.CreateDirectory(MaterialDir);
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory("scratch");

            // 1. Configure Texture Importers
            ConfigureTextures();

            // 2. Setup URP Lit Materials
            var matHead = SetupMaterial("MAT_Aeron_3D_Head", 0.30f, false);
            var matSuit = SetupMaterial("MAT_Aeron_3D_Suit", 0.15f, false);
            var matArms = SetupMaterial("MAT_Aeron_3D_FPArms", 0.25f, false);
            var matEyes = SetupEyesMaterial();

            // 3. Configure FBX Importers
            ConfigureFBXImporters();

            // 4. Assemble Player Prefab
            GameObject playerPrefab = CreateFirstPersonPlayerPrefab(matHead, matSuit, matArms, matEyes);

            // 5. Update Active Lab Scene
            UpdateLabScene(playerPrefab);

            // 6. Capture Multi-Angle First-Person Verifications
            CaptureVerificationFrames();

            Debug.Log("[AssembleFP] ✅ Full 3D First-Person Aeron Assembly Pass Complete!");
        }

        private static void ConfigureTextures()
        {
            ConfigureTexture(Path.Combine(TextureDir, "Aeron_3D_Albedo.png"), false, true);
            ConfigureTexture(Path.Combine(TextureDir, "Aeron_3D_Masks.png"), false, false); // Linear mask
            ConfigureTexture(Path.Combine(TextureDir, "Aeron_3D_Normal.png"), true, false);  // Normal map
            ConfigureTexture(Path.Combine(TextureDir, "Aeron_3D_Emission.png"), false, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void ConfigureTexture(string assetPath, bool isNormal, bool isSRGB)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[AssembleFP] Texture importer not found: {assetPath}");
                return;
            }

            importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = isSRGB;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled = true;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            Debug.Log($"[AssembleFP] Configured texture: {assetPath}");
        }

        private static Material SetupMaterial(string matName, float smoothness, bool isCutout)
        {
            string matPath = Path.Combine(MaterialDir, $"{matName}.mat");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            var albedoTex = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(TextureDir, "Aeron_3D_Albedo.png"));
            var masksTex = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(TextureDir, "Aeron_3D_Masks.png"));
            var normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(TextureDir, "Aeron_3D_Normal.png"));

            if (albedoTex != null) mat.SetTexture("_BaseMap", albedoTex);
            if (masksTex != null)
            {
                mat.SetTexture("_MetallicSpecGlossMap", masksTex);
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            }
            if (normalTex != null)
            {
                mat.SetTexture("_BumpMap", normalTex);
                mat.EnableKeyword("_NORMALMAP");
            }

            mat.SetFloat("_Smoothness", smoothness);
            mat.SetColor("_BaseColor", Color.white);

            EditorUtility.SetDirty(mat);
            Debug.Log($"[AssembleFP] Configured material: {matPath}");
            return mat;
        }

        private static Material SetupEyesMaterial()
        {
            string matPath = Path.Combine(MaterialDir, "MAT_Aeron_3D_Eyes.mat");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            var albedoTex = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(TextureDir, "Aeron_3D_Albedo.png"));
            var emissionTex = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(TextureDir, "Aeron_3D_Emission.png"));

            if (albedoTex != null) mat.SetTexture("_BaseMap", albedoTex);
            if (emissionTex != null)
            {
                mat.SetTexture("_EmissionMap", emissionTex);
                mat.SetColor("_EmissionColor", new Color(0.435f, 0.890f, 1.0f) * 1.5f); // #6FE3FF cyan glow
                mat.EnableKeyword("_EMISSION");
            }

            mat.SetFloat("_Smoothness", 0.95f);
            EditorUtility.SetDirty(mat);
            Debug.Log($"[AssembleFP] Configured eyes material: {matPath}");
            return mat;
        }

        private static void ConfigureFBXImporters()
        {
            string[] fbxFiles = {
                Path.Combine(MeshesDir, "Aeron_FullBody_3D.fbx"),
                Path.Combine(MeshesDir, "Aeron_FirstPerson_Arms.fbx")
            };

            foreach (var fbxPath in fbxFiles)
            {
                var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
                if (importer != null)
                {
                    importer.bakeAxisConversion = true;
                    importer.importAnimation = true;
                    importer.animationType = ModelImporterAnimationType.Generic;
                    importer.SaveAndReimport();
                    Debug.Log($"[AssembleFP] Configured ModelImporter: {fbxPath}");
                }
            }
        }

        private static GameObject CreateFirstPersonPlayerPrefab(Material matHead, Material matSuit, Material matArms, Material matEyes)
        {
            string prefabPath = Path.Combine(PrefabDir, "Aeron_FirstPerson_Player.prefab");

            // Root Player GameObject
            var root = new GameObject("Aeron_FirstPerson_Player");
            root.tag = "Player";
            root.layer = 11; // Character Layer

            // CharacterController
            var cc = root.AddComponent<CharacterController>();
            cc.height = 1.80f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0, 0.90f, 0);
            cc.stepOffset = 0.30f;
            cc.slopeLimit = 45f;

            // PlayerController
            var pc = root.AddComponent<FirstPersonPlayerController>();

            // Health
            root.AddComponent<Health>();

            // 1. CharacterBody
            var body = new GameObject("CharacterBody");
            body.transform.SetParent(root.transform, false);

            var fullBodyFbx = AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MeshesDir, "Aeron_FullBody_3D.fbx"));
            if (fullBodyFbx != null)
            {
                var fullBodyInst = UnityEngine.Object.Instantiate(fullBodyFbx, body.transform);
                fullBodyInst.name = "FullBodyModel";
                fullBodyInst.transform.localPosition = Vector3.zero;
                fullBodyInst.transform.localRotation = Quaternion.identity;

                // Assign materials to renderers
                var renderers = fullBodyInst.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    if (r.gameObject.name.Contains("Head"))
                    {
                        r.sharedMaterial = matHead;
                        r.shadowCastingMode = ShadowCastingMode.ShadowsOnly; // Eliminates face/chin clipping in FP!
                    }
                    else
                    {
                        r.sharedMaterials = new Material[] { matSuit, matArms };
                        r.shadowCastingMode = ShadowCastingMode.On;
                    }
                    r.receiveShadows = true;
                }
            }

            // 2. CameraHolder & FirstPersonCamera
            var camHolder = new GameObject("CameraHolder");
            camHolder.transform.SetParent(root.transform, false);
            camHolder.transform.localPosition = new Vector3(0, 1.70f, 0); // Eye height 1.70m

            var camGo = new GameObject("FirstPersonCamera");
            camGo.transform.SetParent(camHolder.transform, false);
            camGo.tag = "MainCamera";

            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 80f; // Section 28 requirement
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 1000f;
            cam.clearFlags = CameraClearFlags.Color;
            cam.backgroundColor = new Color(0.02f, 0.024f, 0.039f, 1f); // Dark atmosphere

            camGo.AddComponent<AudioListener>();

            var camCtrl = camHolder.AddComponent<FirstPersonCameraController>();

            // 3. FirstPersonArms under CameraHolder
            var fpArmsFbx = AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MeshesDir, "Aeron_FirstPerson_Arms.fbx"));
            if (fpArmsFbx != null)
            {
                var fpArmsInst = UnityEngine.Object.Instantiate(fpArmsFbx, camHolder.transform);
                fpArmsInst.name = "FirstPersonArms";
                fpArmsInst.transform.localPosition = Vector3.zero;
                fpArmsInst.transform.localRotation = Quaternion.identity;

                var armsRenderers = fpArmsInst.GetComponentsInChildren<Renderer>(true);
                foreach (var ar in armsRenderers)
                {
                    ar.sharedMaterial = matArms;
                    ar.shadowCastingMode = ShadowCastingMode.Off;
                }
            }

            // 4. GroundCheck
            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(root.transform, false);
            groundCheck.transform.localPosition = new Vector3(0, 0.05f, 0);

            // 5. VisualEffects (Subtle eye glow point light)
            var vfxGo = new GameObject("VisualEffects");
            vfxGo.transform.SetParent(camHolder.transform, false);
            vfxGo.transform.localPosition = new Vector3(0.04f, 0, 0.15f);

            var eyeLight = vfxGo.AddComponent<Light>();
            eyeLight.type = LightType.Point;
            eyeLight.color = new Color(0.435f, 0.890f, 1.0f); // #6FE3FF
            eyeLight.intensity = 0.6f;
            eyeLight.range = 1.5f;

            // Save Prefab
            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);

            Debug.Log($"[AssembleFP] Saved First-Person Player Prefab to: {prefabPath}");
            return savedPrefab;
        }

        private static void UpdateLabScene(GameObject playerPrefab)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            if (!scene.IsValid())
            {
                Debug.LogError($"[AssembleFP] Failed to open scene: {ScenePath}");
                return;
            }

            // Disable or remove old 2.5D stationary Main_Camera
            var oldCam = GameObject.Find("Main_Camera");
            if (oldCam != null)
            {
                oldCam.SetActive(false);
                Debug.Log("[AssembleFP] Disabled old stationary Main_Camera in favor of FirstPersonCamera.");
            }

            // Remove legacy 2.5D Aeron_Instance
            var oldAeron = GameObject.Find("Aeron_Instance");
            if (oldAeron != null)
            {
                UnityEngine.Object.DestroyImmediate(oldAeron);
                Debug.Log("[AssembleFP] Removed legacy 2.5D Aeron_Instance sprite card.");
            }

            // Also check under "Characters"
            var charRoot = GameObject.Find("Characters");
            if (charRoot != null)
            {
                var charAeron = charRoot.transform.Find("Aeron_Instance");
                if (charAeron != null) UnityEngine.Object.DestroyImmediate(charAeron.gameObject);
            }

            // Instantiate First-Person Player
            var playerInstance = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            playerInstance.name = "Aeron_Player_FirstPerson";
            playerInstance.transform.position = new Vector3(-1.20f, 0.00f, 0.50f);
            playerInstance.transform.rotation = Quaternion.Euler(0, 20f, 0);

            if (charRoot != null)
            {
                playerInstance.transform.SetParent(charRoot.transform, true);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[AssembleFP] Instantiated Aeron_Player_FirstPerson in 2.5D_Lab_Scene.unity at (-1.20, 0.00, 0.50).");
        }

        private static void CaptureVerificationFrames()
        {
            var player = GameObject.Find("Aeron_Player_FirstPerson");
            if (player == null)
            {
                Debug.LogWarning("[AssembleFP] Aeron_Player_FirstPerson not found for frame capture.");
                return;
            }

            var fpCam = player.GetComponentInChildren<Camera>();
            if (fpCam == null)
            {
                Debug.LogWarning("[AssembleFP] FirstPersonCamera not found for frame capture.");
                return;
            }

            var holder = fpCam.transform.parent;

            // Frame 1: Forward view (looking at stasis chamber)
            RenderCameraToPNG(fpCam, "scratch/fp_view_forward.png");

            // Frame 2: Look Down view (looking at chest, legs, boots)
            if (holder != null)
            {
                var origRot = holder.localRotation;
                holder.localRotation = Quaternion.Euler(65f, 0, 0);
                RenderCameraToPNG(fpCam, "scratch/fp_view_down.png");

                // Frame 3: Look Slightly Down view (showing hands)
                holder.localRotation = Quaternion.Euler(20f, 0, 0);
                RenderCameraToPNG(fpCam, "scratch/fp_view_hands.png");

                holder.localRotation = origRot;
            }
        }

        private static void RenderCameraToPNG(Camera cam, string outputPath)
        {
            int width = 1280;
            int height = 720;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);

            byte[] pngBytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);

            File.WriteAllBytes(outputPath, pngBytes);
            Debug.Log($"[AssembleFP] Rendered frame to {outputPath} ({pngBytes.Length} bytes).");
        }
    }
}
