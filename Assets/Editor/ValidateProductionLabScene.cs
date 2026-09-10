using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace LastGod.EditorTools
{
    public static class ValidateProductionLabScene
    {
        private const string ScenePath = "Assets/Scenes/2.5D_Lab_Scene.unity";
        private const string CaptureOutputPath = "scratch/unity_lab_camera_capture.png";

        [MenuItem("The Last God/Validate Production Lab Scene", false, 6)]
        public static void ValidateAndCapture()
        {
            Debug.Log("[ValidateLab] Starting validation of 2.5D Production Lab Scene...");

            var scene = EditorSceneManager.OpenScene(ScenePath);
            if (!scene.IsValid())
            {
                Debug.LogError($"[ValidateLab] Failed to open scene at {ScenePath}");
                EditorApplication.Exit(1);
                return;
            }

            int errors = 0;

            // 1. Verify Environment Hierarchy
            var rootEnv = GameObject.Find("Lab_Environment");
            if (rootEnv == null)
            {
                Debug.LogError("[ValidateLab] Root GameObject 'Lab_Environment' NOT found!");
                errors++;
            }
            else
            {
                string[] requiredGroups = {
                    "Architecture", "Platforms", "Containment", "Doors",
                    "Machinery", "Props", "PipesAndCables", "Lighting", "Collision"
                };

                foreach (var grp in requiredGroups)
                {
                    var child = rootEnv.transform.Find(grp);
                    if (child == null)
                    {
                        Debug.LogError($"[ValidateLab] Missing required group '{grp}' under Lab_Environment!");
                        errors++;
                    }
                    else
                    {
                        Debug.Log($"[ValidateLab] Found group '{grp}' with {child.childCount} children.");
                    }
                }
            }

            // 2. Verify Characters
            var charRoot = GameObject.Find("Characters");
            if (charRoot == null)
            {
                Debug.LogError("[ValidateLab] 'Characters' GameObject NOT found!");
                errors++;
            }
            else
            {
                var aeron = charRoot.transform.Find("Aeron_Instance");
                if (aeron == null)
                {
                    Debug.LogError("[ValidateLab] 'Aeron_Instance' NOT found!");
                    errors++;
                }
                else
                {
                    var anim = aeron.GetComponent<Animator>();
                    var col = aeron.GetComponent<Collider>();
                    Debug.Log($"[ValidateLab] Aeron_Instance verified at {aeron.position}. Animator present: {anim != null}, Collider present: {col != null}");
                }

                var guard = charRoot.transform.Find("Guard_Instance");
                if (guard == null)
                {
                    Debug.LogError("[ValidateLab] 'Guard_Instance' NOT found!");
                    errors++;
                }
                else
                {
                    var anim = guard.GetComponent<Animator>();
                    var col = guard.GetComponent<Collider>();
                    Debug.Log($"[ValidateLab] Guard_Instance verified at {guard.position}. Animator present: {anim != null}, Collider present: {col != null}");
                }
            }

            // 3. Verify Renderers and Shaders
            var renderers = UnityEngine.Object.FindObjectsByType<MeshRenderer>();
            Debug.Log($"[ValidateLab] Found {renderers.Length} MeshRenderers in scene.");
            int missingMats = 0;
            int errorShaders = 0;

            foreach (var r in renderers)
            {
                var mats = r.sharedMaterials;
                if (mats == null || mats.Length == 0)
                {
                    missingMats++;
                    continue;
                }

                foreach (var m in mats)
                {
                    if (m == null)
                    {
                        missingMats++;
                        Debug.LogWarning($"[ValidateLab] Renderer '{r.gameObject.name}' has null material slot!");
                        continue;
                    }

                    if (m.shader == null || m.shader.name.Contains("InternalErrorShader"))
                    {
                        errorShaders++;
                        Debug.LogError($"[ValidateLab] Material '{m.name}' on '{r.gameObject.name}' has Error Shader!");
                    }
                }
            }

            Debug.Log($"[ValidateLab] Material Check: Missing Materials = {missingMats}, Error Shaders = {errorShaders}");
            if (errorShaders > 0) errors += errorShaders;

            // 4. Verify Camera & Render In-Engine Screenshot
            var camGo = GameObject.Find("Main_Camera");
            if (camGo == null && Camera.main != null) camGo = Camera.main.gameObject;

            if (camGo != null)
            {
                var cam = camGo.GetComponent<Camera>();
                Debug.Log($"[ValidateLab] Camera found: pos={camGo.transform.position}, rot={camGo.transform.eulerAngles}, FOV={cam.fieldOfView}");

                // Render offscreen texture
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

                Directory.CreateDirectory("scratch");
                File.WriteAllBytes(CaptureOutputPath, pngBytes);
                Debug.Log($"[ValidateLab] Rendered in-engine camera frame to {CaptureOutputPath} ({pngBytes.Length} bytes).");
            }
            else
            {
                Debug.LogError("[ValidateLab] Main Camera NOT found!");
                errors++;
            }

            if (errors > 0)
            {
                Debug.LogError($"[ValidateLab] ❌ Validation failed with {errors} errors.");
                EditorApplication.Exit(1);
            }
            else
            {
                Debug.Log("[ValidateLab] ✅ Validation PASSED: All 2.5D Production Lab environment systems verified!");
            }
        }
    }
}
