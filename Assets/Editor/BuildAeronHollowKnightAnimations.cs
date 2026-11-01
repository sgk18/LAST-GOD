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

            // 3. Build Animation Clips
            AnimationClip idleClip = BuildIdleClip();
            AnimationClip walkClip = BuildWalkClip();
            AnimationClip jumpClip = BuildJumpClip();
            AnimationClip fallClip = BuildFallClip();
            AnimationClip laserClip = BuildLaserClip();

            // 4. Build Animator Controller
            BuildAnimatorController(idleClip, walkClip, jumpClip, fallClip, laserClip);

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
                ConfigureSprite($"Assets/Characters/Aeron/Sprites/Walk/aeron_walk_{i}.png");
            }

            // Idle sprites
            for (int i = 0; i < 6; i++)
            {
                ConfigureSprite($"Assets/Characters/Aeron/Sprites/Idle/aeron_idle_{i}.png");
            }

            // Jump & Fall sprites
            for (int i = 0; i < 3; i++)
            {
                ConfigureSprite($"Assets/Characters/Aeron/Sprites/Jump/aeron_jump_{i}.png");
            }
            ConfigureSprite("Assets/Characters/Aeron/Sprites/Jump/aeron_fall_0.png");

            // Laser sprites
            ConfigureSprite("Assets/Characters/Aeron/Sprites/Laser/aeron_laser_charge.png");
            ConfigureSprite("Assets/Characters/Aeron/Sprites/Laser/aeron_laser_fire.png");

            // Guard tactical sprites
            string[] guardSprites = new string[]
            {
                "Assets/Art/Act1/characters/guard_tactical_idle.png",
                "Assets/Art/Act1/characters/guard_tactical_aim.png",
                "Assets/Art/Act1/characters/guard_tactical_shoot.png",
                "Assets/Art/Act1/characters/guard_tactical_recoil.png",
                "Assets/Art/Act1/characters/guard_tactical_hurt.png",
                "Assets/Art/Act1/characters/guard_tactical_broken.png"
            };
            foreach (var gs in guardSprites)
            {
                ConfigureSprite(gs);
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
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            Sprite[] sprites = new Sprite[6];
            for (int i = 0; i < 6; i++)
            {
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Characters/Aeron/Sprites/Idle/aeron_idle_{i}.png");
            }

            SetSpriteCurve(clip, sprites, clip.frameRate);
            EditorUtility.SetDirty(clip);
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
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            Sprite[] sprites = new Sprite[8];
            for (int i = 0; i < 8; i++)
            {
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Characters/Aeron/Sprites/Walk/aeron_walk_{i}.png");
            }

            SetSpriteCurve(clip, sprites, clip.frameRate);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimationClip BuildJumpClip()
        {
            string clipPath = "Assets/Characters/Aeron/Animations/Aeron_Jump.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            clip.frameRate = 10f;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            Sprite[] sprites = new Sprite[3];
            for (int i = 0; i < 3; i++)
            {
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Characters/Aeron/Sprites/Jump/aeron_jump_{i}.png");
            }

            SetSpriteCurve(clip, sprites, clip.frameRate);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimationClip BuildFallClip()
        {
            string clipPath = "Assets/Characters/Aeron/Animations/Aeron_Fall.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            clip.frameRate = 8f;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            Sprite[] sprites = new Sprite[1];
            sprites[0] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Characters/Aeron/Sprites/Jump/aeron_fall_0.png");

            SetSpriteCurve(clip, sprites, clip.frameRate);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimationClip BuildLaserClip()
        {
            string clipPath = "Assets/Characters/Aeron/Animations/Aeron_Laser.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            clip.frameRate = 6f;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            Sprite[] sprites = new Sprite[2];
            sprites[0] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Characters/Aeron/Sprites/Laser/aeron_laser_charge.png");
            sprites[1] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Characters/Aeron/Sprites/Laser/aeron_laser_fire.png");

            SetSpriteCurve(clip, sprites, clip.frameRate);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static void SetSpriteCurve(AnimationClip clip, Sprite[] sprites, float frameRate)
        {
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
                    time = (float)i / frameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        }

        private static void BuildAnimatorController(AnimationClip idleClip, AnimationClip walkClip, AnimationClip jumpClip, AnimationClip fallClip, AnimationClip laserClip)
        {
            string controllerPath = "Assets/Characters/Aeron/Animator/Aeron.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            // Ensure parameters
            EnsureParameter(controller, "Speed", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "IsGrounded", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "VerticalVelocity", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "IsFiringBeam", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "JumpTrigger", AnimatorControllerParameterType.Trigger);

            var rootStateMachine = controller.layers[0].stateMachine;

            // Clear and rebuild states
            rootStateMachine.states = new ChildAnimatorState[0];

            var idleState = rootStateMachine.AddState("Idle");
            idleState.motion = idleClip;

            var walkState = rootStateMachine.AddState("Walk");
            walkState.motion = walkClip;

            var jumpState = rootStateMachine.AddState("Jump");
            jumpState.motion = jumpClip;

            var fallState = rootStateMachine.AddState("Fall");
            fallState.motion = fallClip;

            var laserState = rootStateMachine.AddState("LaserBeam");
            laserState.motion = laserClip;

            rootStateMachine.defaultState = idleState;

            // 1. Idle <-> Walk
            var idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.08f;
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

            var walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.08f;
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

            // 2. To Jump
            var idleToJump = idleState.AddTransition(jumpState);
            idleToJump.hasExitTime = false;
            idleToJump.duration = 0.05f;
            idleToJump.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");

            var walkToJump = walkState.AddTransition(jumpState);
            walkToJump.hasExitTime = false;
            walkToJump.duration = 0.05f;
            walkToJump.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");

            // 3. Jump -> Fall
            var jumpToFall = jumpState.AddTransition(fallState);
            jumpToFall.hasExitTime = false;
            jumpToFall.duration = 0.1f;
            jumpToFall.AddCondition(AnimatorConditionMode.Less, -0.1f, "VerticalVelocity");

            // 4. Fall -> Idle / Walk
            var fallToIdle = fallState.AddTransition(idleState);
            fallToIdle.hasExitTime = false;
            fallToIdle.duration = 0.05f;
            fallToIdle.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
            fallToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

            var fallToWalk = fallState.AddTransition(walkState);
            fallToWalk.hasExitTime = false;
            fallToWalk.duration = 0.05f;
            fallToWalk.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
            fallToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

            // 5. AnyState -> LaserBeam
            var anyToLaser = rootStateMachine.AddAnyStateTransition(laserState);
            anyToLaser.hasExitTime = false;
            anyToLaser.duration = 0.05f;
            anyToLaser.AddCondition(AnimatorConditionMode.If, 0, "IsFiringBeam");

            // 6. LaserBeam -> Idle / Fall
            var laserToIdle = laserState.AddTransition(idleState);
            laserToIdle.hasExitTime = false;
            laserToIdle.duration = 0.1f;
            laserToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsFiringBeam");
            laserToIdle.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");

            var laserToFall = laserState.AddTransition(fallState);
            laserToFall.hasExitTime = false;
            laserToFall.duration = 0.1f;
            laserToFall.AddCondition(AnimatorConditionMode.IfNot, 0, "IsFiringBeam");
            laserToFall.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");

            EditorUtility.SetDirty(controller);
            Debug.Log($"[BuildAeronHollowKnightAnimations] Built Animator Controller: {controllerPath}");
        }

        private static void EnsureParameter(AnimatorController controller, string paramName, AnimatorControllerParameterType type)
        {
            foreach (var p in controller.parameters)
            {
                if (p.name == paramName) return;
            }
            controller.AddParameter(paramName, type);
        }
    }
}
