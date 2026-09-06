using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using LastGod.Core;

namespace LastGod.Editor
{
    /// <summary>
    /// Adds and configures the Glass Chamber (stasis pod) at Pillar B-3 in Act1_Scene1.
    /// Operates automatically on assembly load and provides a menu command.
    /// </summary>
    [InitializeOnLoad]
    public static class AddChamberToScene
    {
        private const string ScenePath = "Assets/Scenes/Act1_Scene1.unity";
        private const string LabScenePath = "Assets/Scenes/Act1_Scene1_Lab.unity";
        private const string ChamberSpritePath = "Assets/Art/Sprites/Glass_Chamber.png";
        private const string ChamberIntactPath = "Assets/Art/Sprites/Chamber_Intact.png";
        private const string CrackedSpritePath = "Assets/Art/Sprites/Chamber_Cracked.png";
        private const string ShatteredSpritePath = "Assets/Art/Sprites/Chamber_Shattered.png";

        // Position: Pillar B-3 at floor level (X = -34.0, Y = -12.95 for 48px sprite resting on Y = -14.2 deck)
        public static readonly Vector3 ChamberPosition = new Vector3(-34.0f, -12.95f, 0f);

        static AddChamberToScene()
        {
            EditorApplication.delayCall += EnsureChamberInOpenScene;
        }

        [MenuItem("Tools/LAST-GOD/Add Glass Chamber To Scene")]
        public static void EnsureChamberInOpenScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.isLoaded) return;

            bool isAct1 = activeScene.path.Contains("Act1_Scene1") || activeScene.name.Contains("Act1_Scene1");
            if (!isAct1) return;

            ConfigureChamberInScene(activeScene);
        }

        public static GameObject ConfigureChamberInScene(UnityEngine.SceneManagement.Scene scene)
        {
            // 1. Look for existing Chamber or create new
            GameObject chamberObj = GameObject.Find("Glass_Chamber");
            if (chamberObj == null)
            {
                chamberObj = GameObject.Find("Chamber_Glass");
            }
            if (chamberObj == null)
            {
                chamberObj = new GameObject("Glass_Chamber");
                Undo.RegisterCreatedObjectUndo(chamberObj, "Create Glass Chamber");
            }

            chamberObj.name = "Glass_Chamber";
            chamberObj.transform.position = ChamberPosition;
            chamberObj.transform.localScale = Vector3.one;

            // 2. SpriteRenderer
            SpriteRenderer sr = chamberObj.GetComponent<SpriteRenderer>();
            if (sr == null) sr = chamberObj.AddComponent<SpriteRenderer>();

            Sprite chamberSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ChamberSpritePath);
            if (chamberSprite == null) chamberSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ChamberIntactPath);
            if (chamberSprite != null) sr.sprite = chamberSprite;

            sr.sortingLayerName = "Default";
            sr.sortingOrder = 5; // In front of background (order 0), behind player (order 10)

            // 3. Collider
            BoxCollider2D boxCol = chamberObj.GetComponent<BoxCollider2D>();
            if (boxCol == null) boxCol = chamberObj.AddComponent<BoxCollider2D>();
            boxCol.size = new Vector2(2.2f, 2.7f);
            boxCol.offset = new Vector2(0f, 0.125f);
            boxCol.isTrigger = false;

            // 4. Stasis Glow Light (Light2D)
            Transform existingLight = chamberObj.transform.Find("Chamber_StasisLight");
            GameObject lightObj = existingLight != null ? existingLight.gameObject : new GameObject("Chamber_StasisLight");
            lightObj.transform.SetParent(chamberObj.transform, false);
            lightObj.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            Light2D stasisLight = lightObj.GetComponent<Light2D>();
            if (stasisLight == null) stasisLight = lightObj.AddComponent<Light2D>();
            stasisLight.lightType = Light2D.LightType.Point;
            stasisLight.color = new Color(0.25f, 0.85f, 1.0f, 1.0f);
            stasisLight.intensity = 1.3f;
            stasisLight.pointLightInnerRadius = 0.4f;
            stasisLight.pointLightOuterRadius = 3.2f;

            // 5. Wire into Act1Scene1SequenceManager if present
            Act1Scene1SequenceManager seqMgr = UnityEngine.Object.FindAnyObjectByType<Act1Scene1SequenceManager>();
            if (seqMgr != null)
            {
                SerializedObject so = new SerializedObject(seqMgr);
                SerializedProperty rendProp = so.FindProperty("chamberRenderer");
                if (rendProp != null) rendProp.objectReferenceValue = sr;

                SerializedProperty crackProp = so.FindProperty("crackedChamberSprite");
                if (crackProp != null)
                {
                    Sprite crackSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CrackedSpritePath);
                    if (crackSprite != null) crackProp.objectReferenceValue = crackSprite;
                }

                SerializedProperty shatProp = so.FindProperty("shatteredChamberSprite");
                if (shatProp != null)
                {
                    Sprite shatSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ShatteredSpritePath);
                    if (shatSprite != null) shatProp.objectReferenceValue = shatSprite;
                }

                so.ApplyModifiedProperties();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[AddChamberToScene] Successfully placed and saved Glass Chamber at Pillar B-3 (x={ChamberPosition.x}, y={ChamberPosition.y}) in {scene.name}!");

            return chamberObj;
        }
    }
}
