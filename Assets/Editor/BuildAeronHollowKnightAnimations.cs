using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace LastGod.Editor
{
    public static class BuildAeronHollowKnightAnimations
    {
        [MenuItem("The Last God/Build Aeron Hollow Knight Animations")]
        public static void Build()
        {
            Debug.Log("[BuildAeronHollowKnightAnimations] Starting pipeline...");

            // 1. Configure and verify sprite importers
            ConfigureSpriteImporters();

            // 2. Ensure destination directories exist
            EnsureDirectories();

            // 3. Build Idle Animation Clip
            AnimationClip idleClip = BuildIdleClip();

            // 4. Build Walk Animation Clip
            AnimationClip walkClip = BuildWalkClip();

            // 5. Build Animator Controller
            BuildAnimatorController(idleClip, walkClip);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Debug.Log("[BuildAeronHollowKnightAnimations] Completed successfully!");
        }

        private static void ConfigureSpriteImporters()
        {
            // Reference image
            string refPath = "Assets/Characters/Aeron/Reference/aeron_reference.png";
            if (File.Exists(refPath))
            {
                var importer = AssetImporter.GetAtPath(refPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 100f;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }
            }

            // Walk sprites
            for (int i = 0; i < 8; i++)
            {
                string path = $"Assets/Characters/Aeron/Sprites/Walk/aeron_walk_{i}.png";
                ConfigureSprite(path);
            }

            // Idle sprites
            for (int i = 0; i < 6; i++)
            {
                string path = $"Assets/Characters/Aeron/Sprites/Idle/aeron_idle_{i}.png";
                ConfigureSprite(path);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void ConfigureSprite(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[BuildAeronHollowKnightAnimations] Sprite not found: {path}");
                return;
            }

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                
                // Align pivot with feet contact line (y = 15 in 260px canvas)
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(0.5f, 15f / 260f);
                importer.SetTextureSettings(settings);

                importer.SaveAndReimport();
            }
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists("Assets/Characters/Aeron/Animations"))
            {
                Directory.CreateDirectory("Assets/Characters/Aeron/Animations");
            }
            if (!Directory.Exists("Assets/Characters/Aeron/Animator"))
            {
                Directory.CreateDirectory("Assets/Characters/Aeron/Animator");
            }
        }

        private static AnimationClip BuildIdleClip()
        {
            string clipPath = "Assets/Characters/Aeron/Animations/Aeron_Idle_1.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            clip.frameRate = 8f;

            // Set looping
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            // Load idle sprites
            Sprite[] sprites = new Sprite[6];
            for (int i = 0; i < 6; i++)
            {
                string spritePath = $"Assets/Characters/Aeron/Sprites/Idle/aeron_idle_{i}.png";
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (sprites[i] == null)
                {
                    Debug.LogError($"[BuildAeronHollowKnightAnimations] Could not load sprite at {spritePath}");
                }
            }

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = (float)i / clip.frameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            EditorUtility.SetDirty(clip);
            Debug.Log($"[BuildAeronHollowKnightAnimations] Built Idle Clip: {clipPath}");
            return clip;
        }

        private static AnimationClip BuildWalkClip()
        {
            string clipPath = "Assets/Characters/Aeron/Animations/Aeron_Walk.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            clip.frameRate = 10f;

            // Set looping
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            // Load walk sprites
            Sprite[] sprites = new Sprite[8];
            for (int i = 0; i < 8; i++)
            {
                string spritePath = $"Assets/Characters/Aeron/Sprites/Walk/aeron_walk_{i}.png";
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (sprites[i] == null)
                {
                    Debug.LogError($"[BuildAeronHollowKnightAnimations] Could not load sprite at {spritePath}");
                }
            }

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = (float)i / clip.frameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            EditorUtility.SetDirty(clip);
            Debug.Log($"[BuildAeronHollowKnightAnimations] Built Walk Clip: {clipPath}");
            return clip;
        }

        private static void BuildAnimatorController(AnimationClip idleClip, AnimationClip walkClip)
        {
            string controllerPath = "Assets/Characters/Aeron/Animator/Aeron.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            // Ensure "Speed" parameter exists
            bool hasSpeedParam = false;
            foreach (var param in controller.parameters)
            {
                if (param.name == "Speed")
                {
                    hasSpeedParam = true;
                    break;
                }
            }

            if (!hasSpeedParam)
            {
                controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            }

            var rootStateMachine = controller.layers[0].stateMachine;

            // Clear old states or find existing
            AnimatorState idleState = null;
            AnimatorState walkState = null;

            foreach (var childState in rootStateMachine.states)
            {
                if (childState.state.name == "Idle") idleState = childState.state;
                if (childState.state.name == "Walk") walkState = childState.state;
            }

            if (idleState == null)
            {
                idleState = rootStateMachine.AddState("Idle");
            }
            idleState.motion = idleClip;

            if (walkState == null)
            {
                walkState = rootStateMachine.AddState("Walk");
            }
            walkState.motion = walkClip;

            rootStateMachine.defaultState = idleState;

            // Clear transitions to rebuild cleanly
            idleState.transitions = new AnimatorStateTransition[0];
            walkState.transitions = new AnimatorStateTransition[0];

            // Transition: Idle -> Walk (Speed > 0.1)
            var toWalk = idleState.AddTransition(walkState);
            toWalk.hasExitTime = false;
            toWalk.duration = 0.1f;
            toWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

            // Transition: Walk -> Idle (Speed < 0.1)
            var toIdle = walkState.AddTransition(idleState);
            toIdle.hasExitTime = false;
            toIdle.duration = 0.1f;
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

            EditorUtility.SetDirty(controller);
            Debug.Log($"[BuildAeronHollowKnightAnimations] Built Animator Controller: {controllerPath}");
        }
    }
}

