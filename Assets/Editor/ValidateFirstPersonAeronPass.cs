using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using LastGod.FirstPerson.Player;

namespace LastGod.EditorTools
{
    public static class ValidateFirstPersonAeronPass
    {
        private const string ScenePath = "Assets/Scenes/2.5D_Lab_Scene.unity";

        [MenuItem("The Last God/Validate First-Person Aeron Pass", false, 11)]
        public static void ValidateAndCapture()
        {
            Debug.Log("[ValidateFP] Starting validation of Full 3D First-Person Aeron Architecture...");

            var scene = EditorSceneManager.OpenScene(ScenePath);
            if (!scene.IsValid())
            {
                Debug.LogError($"[ValidateFP] Failed to open scene at {ScenePath}");
                EditorApplication.Exit(1);
                return;
            }

            int errors = 0;

            // 1. Verify First-Person Player Root
            var playerGo = GameObject.Find("Aeron_Player_FirstPerson");
            if (playerGo == null)
            {
                playerGo = GameObject.FindWithTag("Player");
            }

            if (playerGo == null)
            {
                Debug.LogError("[ValidateFP] First-Person Player GameObject NOT found in scene!");
                errors++;
            }
            else
            {
                var cc = playerGo.GetComponent<CharacterController>();
                var pc = playerGo.GetComponent<FirstPersonPlayerController>();
                if (cc == null) { Debug.LogError("[ValidateFP] CharacterController missing on Player!"); errors++; }
                if (pc == null) { Debug.LogError("[ValidateFP] FirstPersonPlayerController missing on Player!"); errors++; }

                Debug.Log($"[ValidateFP] Player root verified at {playerGo.transform.position}. CC height={cc?.height}, radius={cc?.radius}.");

                // Check CameraHolder & FirstPersonCamera
                var camHolder = playerGo.transform.Find("CameraHolder");
                if (camHolder == null)
                {
                    Debug.LogError("[ValidateFP] 'CameraHolder' missing under Player root!");
                    errors++;
                }
                else
                {
                    float eyeHeight = camHolder.localPosition.y;
                    Debug.Log($"[ValidateFP] CameraHolder verified at eye height: {eyeHeight:F2}m (Expected ~1.70m).");
                    if (Mathf.Abs(eyeHeight - 1.70f) > 0.15f)
                    {
                        Debug.LogWarning($"[ValidateFP] CameraHolder eye height {eyeHeight} deviates from 1.70m standard.");
                    }

                    var camGo = camHolder.Find("FirstPersonCamera");
                    var cam = camGo != null ? camGo.GetComponent<Camera>() : null;
                    if (cam == null)
                    {
                        Debug.LogError("[ValidateFP] FirstPersonCamera missing under CameraHolder!");
                        errors++;
                    }
                    else
                    {
                        Debug.Log($"[ValidateFP] FirstPersonCamera verified: FOV={cam.fieldOfView} (Expected 80), NearClip={cam.nearClipPlane}.");
                        if (Mathf.Abs(cam.fieldOfView - 80f) > 5f)
                        {
                            Debug.LogWarning($"[ValidateFP] Camera FOV {cam.fieldOfView} deviates from 80° target.");
                        }
                    }

                    // Check FirstPersonArms
                    var fpArms = camHolder.Find("FirstPersonArms");
                    if (fpArms == null)
                    {
                        Debug.LogError("[ValidateFP] 'FirstPersonArms' missing under CameraHolder!");
                        errors++;
                    }
                    else
                    {
                        var armsRenderers = fpArms.GetComponentsInChildren<Renderer>(true);
                        Debug.Log($"[ValidateFP] FirstPersonArms verified with {armsRenderers.Length} renderers.");
                    }
                }

                // Check FullBodyModel
                var charBody = playerGo.transform.Find("CharacterBody");
                var fullBody = charBody != null ? charBody.Find("FullBodyModel") : null;
                if (fullBody == null)
                {
                    Debug.LogError("[ValidateFP] 'FullBodyModel' missing under CharacterBody!");
                    errors++;
                }
                else
                {
                    var bodyRenderers = fullBody.GetComponentsInChildren<Renderer>(true);
                    Debug.Log($"[ValidateFP] FullBodyModel verified with {bodyRenderers.Length} renderers.");
                }
            }

            // 2. Verify Guard Intact
            var charRoot = GameObject.Find("Characters");
            var guard = charRoot != null ? charRoot.transform.Find("Guard_Instance") : GameObject.Find("Guard_Instance")?.transform;
            if (guard == null)
            {
                Debug.LogError("[ValidateFP] 'Guard_Instance' NOT found!");
                errors++;
            }
            else
            {
                Debug.Log($"[ValidateFP] Guard_Instance verified at {guard.position}.");
            }

            // 3. Verify Environment Hierarchy
            var env = GameObject.Find("Lab_Environment");
            if (env == null)
            {
                Debug.LogError("[ValidateFP] 'Lab_Environment' NOT found!");
                errors++;
            }
            else
            {
                string[] groups = { "Architecture", "Platforms", "Containment", "Doors", "Machinery", "Props", "PipesAndCables", "Lighting", "Collision" };
                foreach (var g in groups)
                {
                    if (env.transform.Find(g) == null)
                    {
                        Debug.LogError($"[ValidateFP] Missing environment group: {g}");
                        errors++;
                    }
                }
            }

            // 4. Verify Materials & Shaders (0 missing, 0 error shaders)
            var allRenderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            int missingMats = 0;
            int errorShaders = 0;

            foreach (var r in allRenderers)
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
                        continue;
                    }

                    if (m.shader == null || m.shader.name.Contains("InternalErrorShader"))
                    {
                        errorShaders++;
                        Debug.LogError($"[ValidateFP] Error shader detected on {r.gameObject.name} with material {m.name}!");
                    }
                }
            }

            Debug.Log($"[ValidateFP] Renderer Material Check: Total Renderers = {allRenderers.Length}, Missing Mats = {missingMats}, Error Shaders = {errorShaders}");
            if (errorShaders > 0) errors += errorShaders;

            if (errors > 0)
            {
                Debug.LogError($"[ValidateFP] ❌ Validation FAILED with {errors} errors!");
                EditorApplication.Exit(1);
            }
            else
            {
                Debug.Log("[ValidateFP] ✅ Validation PASSED: Full 3D First-Person Aeron architecture verified!");
            }
        }
    }
}
