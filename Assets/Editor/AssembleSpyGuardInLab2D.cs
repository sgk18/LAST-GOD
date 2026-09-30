// AssembleSpyGuardInLab2D.cs
// Integrates the 2D Spy Guard NPC into Act 1 Lab 2D scene and captures verified camera render.

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using LastGod.Player.Guard;

public static class AssembleSpyGuardInLab2D
{
    const string SCENE_PATH = "Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D.unity";
    const string SCREENSHOT_PATH = "Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D_Spy_Screenshot.png";

    [MenuItem("The Last God/Assemble Spy Guard in 2D Lab", false, 13)]
    public static void AssembleAndCapture()
    {
        Debug.Log(">>> [AssembleSpyGuardInLab2D] Opening scene " + SCENE_PATH);
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        // 1. Locate or create Spy Guard GameObject
        GameObject spyGO = GameObject.Find("NPC_Spy_Guard");
        if (spyGO == null)
        {
            spyGO = new GameObject("NPC_Spy_Guard");
        }

        spyGO.transform.position = new Vector3(2.6f, 0.40f, 0f);
        spyGO.transform.rotation = Quaternion.identity;
        spyGO.transform.localScale = Vector3.one;

        // 2. Setup SpriteRenderer
        var sr = spyGO.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = spyGO.AddComponent<SpriteRenderer>();

        Sprite baseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Characters/SpyGuard/SPY_GUARD_IDLE_0.png");
        if (baseSprite != null)
        {
            sr.sprite = baseSprite;
            Debug.Log("Assigned base sprite: " + baseSprite.name);
        }
        else
        {
            Debug.LogWarning("Base sprite not found at Assets/Art/Characters/SpyGuard/SPY_GUARD_IDLE_0.png");
        }

        sr.sortingLayerName = "Player";
        sr.sortingOrder = 1;

        // 3. Setup Animator
        var anim = spyGO.GetComponent<Animator>();
        if (anim == null)
            anim = spyGO.AddComponent<Animator>();

        RuntimeAnimatorController ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Characters/SpyGuard/Spy_Guard_AnimatorController.controller");
        if (ctrl != null)
        {
            anim.runtimeAnimatorController = ctrl;
            Debug.Log("Assigned AnimatorController: " + ctrl.name);
        }

        // 4. Setup SpyGuardNPC component
        var spyComp = spyGO.GetComponent<SpyGuardNPC>();
        if (spyComp == null)
            spyComp = spyGO.AddComponent<SpyGuardNPC>();

        EditorUtility.SetDirty(spyGO);
        EditorSceneManager.SaveScene(scene);
        Debug.Log(">>> [AssembleSpyGuardInLab2D] Scene saved with NPC_Spy_Guard successfully!");

        // 5. In-Engine Camera Screenshot Capture
        var cam = Camera.main;
        if (cam != null)
        {
            RenderTexture rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            var prevTarget = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            tex.Apply();

            cam.targetTexture = prevTarget;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(SCREENSHOT_PATH, bytes);
            Object.DestroyImmediate(tex);
            Debug.Log(">>> [AssembleSpyGuardInLab2D] In-engine camera screenshot saved: " + SCREENSHOT_PATH);
        }
        else
        {
            Debug.LogWarning("Main camera not found for screenshot capture.");
        }
    }
}
