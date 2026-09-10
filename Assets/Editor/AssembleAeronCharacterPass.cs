using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LastGod.Core;
using LastGod.Characters.Aeron;

namespace LastGod.EditorTools
{
    public static class AssembleAeronCharacterPass
    {
        private const string ScenePath = "Assets/Scenes/2.5D_Lab_Scene.unity";
        private const string PrefabDir = "Assets/Characters/Aeron/Prefabs";
        private const string MatDir = "Assets/Characters/Aeron/Materials";
        private const string SpriteDir = "Assets/Characters/Aeron/Sprites";
        private const string MeshDir = "Assets/Characters/Aeron/Meshes";
        private const string CaptureOutputPath = "scratch/unity_lab_camera_capture.png";

        [MenuItem("The Last God/Assemble Aeron Character Pass", false, 7)]
        public static void ExecutePass()
        {
            Debug.Log("[AssembleAeron] Starting Aeron Character Production Pass...");

            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(MatDir);
            Directory.CreateDirectory(MeshDir);
            Directory.CreateDirectory("scratch");

            // 1. Configure Texture Importers
            ConfigureTextures();

            // 2. Setup Materials
            Material aeronMat = SetupAeronMaterial();
            Material shadowMat = SetupShadowMaterial();

            // 3. Create Prefab
            GameObject prefabAsset = CreateAeronPrefab(aeronMat, shadowMat);

            // 4. Update Scene
            UpdateSceneWithAeron(prefabAsset);

            // 5. Render Camera Capture
            RenderCameraVerification();

            Debug.Log("[AssembleAeron] ✅ Aeron Character Production Pass Complete!");
        }

        private static void ConfigureTextures()
        {
            string[] textures = {
                $"{SpriteDir}/Aeron_Idle_0.png",
                $"{SpriteDir}/Aeron_Base_256.png",
                $"{SpriteDir}/Aeron_Emission_256.png",
                $"{SpriteDir}/Aeron_ContactShadow.png"
            };

            foreach (var path in textures)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.SaveAndReimport();
                    Debug.Log($"[AssembleAeron] Configured texture importer: {path}");
                }
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static Material SetupAeronMaterial()
        {
            string matPath = $"{MatDir}/MAT_Aeron_Sprite.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");

            if (mat == null)
            {
                mat = new Material(litShader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = litShader;
            }

            Texture2D baseTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{SpriteDir}/Aeron_Idle_0.png");
            Texture2D emissionTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{SpriteDir}/Aeron_Emission_256.png");

            mat.SetTexture("_BaseMap", baseTex);
            mat.SetTexture("_MainTex", baseTex);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetColor("_Color", Color.white);

            if (emissionTex != null)
            {
                mat.SetTexture("_EmissionMap", emissionTex);
                mat.SetColor("_EmissionColor", new Color(0.435f, 0.890f, 1.0f, 1.0f) * 2.8f);
                mat.EnableKeyword("_EMISSION");
            }

            // Alpha Clipping setup
            mat.SetFloat("_Surface", 0f);
            mat.SetFloat("_AlphaClip", 1f);
            mat.SetFloat("_Cutoff", 0.20f);
            mat.SetFloat("_Cull", 0f); // Double-sided
            mat.SetFloat("_Smoothness", 0.15f);
            mat.SetFloat("_Metallic", 0.10f);

            mat.EnableKeyword("_ALPHATEST_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;

            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AssembleAeron] Configured Aeron sprite material at {matPath}");
            return mat;
        }

        private static Material SetupShadowMaterial()
        {
            string matPath = $"{MatDir}/MAT_Aeron_Shadow.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");

            if (mat == null)
            {
                mat = new Material(litShader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = litShader;
            }

            Texture2D shadowTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{SpriteDir}/Aeron_ContactShadow.png");

            mat.SetTexture("_BaseMap", shadowTex);
            mat.SetTexture("_MainTex", shadowTex);
            mat.SetColor("_BaseColor", new Color(0.04f, 0.05f, 0.07f, 0.80f));

            // Transparent setup
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_Cull", 0f);
            mat.SetFloat("_Smoothness", 0f);
            mat.SetFloat("_Metallic", 0f);

            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AssembleAeron] Configured Contact Shadow material at {matPath}");
            return mat;
        }

        private static GameObject CreateAeronPrefab(Material aeronMat, Material shadowMat)
        {
            string prefabPath = $"{PrefabDir}/Aeron_Player.prefab";

            GameObject root = new GameObject("Aeron_Player");
            root.layer = 11;
            root.tag = "Player";

            // 1. Capsule Collider (1.8m height, 0.35m radius, feet at Y=0)
            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.90f, 0f);
            col.radius = 0.35f;
            col.height = 1.80f;

            // 2. Health component
            var hp = root.AddComponent<Health>();
            var hpField = typeof(Health).GetField("maxHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (hpField != null) hpField.SetValue(hp, 100);

            // 3. VisualRoot
            GameObject visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(root.transform, false);

            // 4. Aeron_SpriteCard
            GameObject cardGo = new GameObject("Aeron_SpriteCard");
            cardGo.transform.SetParent(visualRoot.transform, false);
            cardGo.layer = 11;

            Mesh cardMesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshDir}/Aeron_Card.fbx");
            if (cardMesh == null)
            {
                var allObjs = AssetDatabase.LoadAllAssetsAtPath($"{MeshDir}/Aeron_Card.fbx");
                foreach (var o in allObjs)
                {
                    if (o is Mesh m)
                    {
                        cardMesh = m;
                        break;
                    }
                }
            }

            var mf = cardGo.AddComponent<MeshFilter>();
            mf.sharedMesh = cardMesh;
            var mr = cardGo.AddComponent<MeshRenderer>();
            mr.sharedMaterial = aeronMat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
            mr.receiveShadows = true;

            cardGo.AddComponent<AeronBillboard>();

            // 5. Eye Glow Light
            GameObject eyeLightGo = new GameObject("Eye_Glow_Light");
            eyeLightGo.transform.SetParent(visualRoot.transform, false);
            eyeLightGo.transform.localPosition = new Vector3(0.04f, 1.55f, 0.05f);
            var light = eyeLightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.435f, 0.890f, 1.0f);
            light.intensity = 0.8f;
            light.range = 1.2f;
            light.shadows = LightShadows.None;

            // 6. Contact Shadow
            GameObject shadowGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            shadowGo.name = "ContactShadow";
            shadowGo.transform.SetParent(root.transform, false);
            shadowGo.transform.localPosition = new Vector3(0f, 0.012f, 0f);
            shadowGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            shadowGo.transform.localScale = new Vector3(0.85f, 0.45f, 1.0f);

            var shadowCol = shadowGo.GetComponent<Collider>();
            if (shadowCol != null) UnityEngine.Object.DestroyImmediate(shadowCol);

            var shadowMr = shadowGo.GetComponent<MeshRenderer>();
            shadowMr.sharedMaterial = shadowMat;
            shadowMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            shadowMr.receiveShadows = false;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            Debug.Log($"[AssembleAeron] Saved Aeron_Player prefab at {prefabPath}");
            return prefab;
        }

        private static void UpdateSceneWithAeron(GameObject prefabAsset)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            if (!scene.IsValid())
            {
                Debug.LogError($"[AssembleAeron] Could not open scene at {ScenePath}");
                return;
            }

            var charsRoot = GameObject.Find("Characters");
            if (charsRoot == null)
            {
                charsRoot = new GameObject("Characters");
            }

            var existingAeron = GameObject.Find("Aeron_Instance");
            Vector3 targetPos = new Vector3(-1.20f, 0.00f, 0.50f);
            if (existingAeron != null)
            {
                targetPos = existingAeron.transform.position;
                UnityEngine.Object.DestroyImmediate(existingAeron);
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
            instance.name = "Aeron_Instance";
            instance.transform.SetParent(charsRoot.transform, false);
            instance.transform.position = targetPos;
            instance.transform.rotation = Quaternion.identity;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[AssembleAeron] Instantiated Aeron_Instance at {targetPos} in {ScenePath}");
        }

        private static void RenderCameraVerification()
        {
            var camGo = GameObject.Find("Main_Camera");
            if (camGo == null)
            {
                Debug.LogError("[AssembleAeron] Main_Camera not found in scene!");
                return;
            }

            var cam = camGo.GetComponent<Camera>();
            cam.transform.position = new Vector3(0.0f, 2.0f, -8.5f);
            cam.transform.rotation = Quaternion.Euler(4.5f, 0.0f, 0.0f);
            cam.fieldOfView = 27f;

            int width = 1920;
            int height = 1080;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            rt.Create();

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);

            byte[] bytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);
            File.WriteAllBytes(CaptureOutputPath, bytes);
            Debug.Log($"[AssembleAeron] Captured verification frame ({bytes.Length} bytes) to {CaptureOutputPath}");
        }
    }
}
