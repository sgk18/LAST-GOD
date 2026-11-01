using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LastGod.Act1;

namespace LastGod.Combat
{
    public class ArmLaserBeam : MonoBehaviour
    {
        [Header("Beam Properties")]
        public float maxRange = 16f;
        public float baseWidth = 0.45f;
        public float damagePerSecond = 75f;
        public LayerMask hitLayers;

        [Header("Visual Elements")]
        public LineRenderer coreLine;
        public LineRenderer auraLine;
        public GameObject impactSparksPrefab;

        private AeronController owner;
        private float direction = 1f;
        private float duration = 0.5f;
        private float timer = 0f;
        private float tickTimer = 0f;
        private HashSet<Collider2D> hitsThisTick = new HashSet<Collider2D>();

        public static ArmLaserBeam Spawn(GameObject prefab, AeronController owner, Vector2 origin, float dir, float duration)
        {
            GameObject obj;
            if (prefab != null)
            {
                obj = Instantiate(prefab, origin, Quaternion.identity);
            }
            else
            {
                obj = new GameObject("ArmLaserBeam");
                obj.transform.position = origin;
            }

            var beam = obj.GetComponent<ArmLaserBeam>();
            if (beam == null) beam = obj.AddComponent<ArmLaserBeam>();
            beam.Setup(owner, origin, dir, duration);
            return beam;
        }

        public void Setup(AeronController ownerRef, Vector2 origin, float dir, float dur)
        {
            owner = ownerRef;
            direction = Mathf.Sign(dir);
            duration = dur;
            transform.position = origin;

            EnsureLines();
            UpdateVisualProperties();
        }

        private void EnsureLines()
        {
            if (coreLine == null)
            {
                var coreObj = new GameObject("CoreLine");
                coreObj.transform.parent = transform;
                coreObj.transform.localPosition = Vector3.zero;
                coreLine = coreObj.AddComponent<LineRenderer>();
                coreLine.material = new Material(Shader.Find("Sprites/Default"));
                coreLine.startColor = new Color(0.85f, 1f, 1f, 0.95f);
                coreLine.endColor = new Color(0.2f, 0.9f, 0.95f, 0.9f);
                coreLine.sortingOrder = 10;
                coreLine.useWorldSpace = true;
                coreLine.positionCount = 2;
            }

            if (auraLine == null)
            {
                var auraObj = new GameObject("AuraLine");
                auraObj.transform.parent = transform;
                auraObj.transform.localPosition = Vector3.zero;
                auraLine = auraObj.AddComponent<LineRenderer>();
                auraLine.material = new Material(Shader.Find("Sprites/Default"));
                auraLine.startColor = new Color(0f, 0.85f, 0.95f, 0.45f);
                auraLine.endColor = new Color(0f, 0.5f, 0.8f, 0.15f);
                auraLine.sortingOrder = 9;
                auraLine.useWorldSpace = true;
                auraLine.positionCount = 2;
            }
        }

        private void UpdateVisualProperties()
        {
            float humanityRatio = owner != null ? Mathf.Clamp01(owner.humanity / 100f) : 1f;
            // The lower the humanity, the thicker and more chaotic the divine beam
            float widthScale = Mathf.Lerp(1.7f, 0.9f, humanityRatio);

            if (coreLine != null)
            {
                coreLine.startWidth = baseWidth * widthScale;
                coreLine.endWidth = baseWidth * widthScale * 0.85f;
            }

            if (auraLine != null)
            {
                auraLine.startWidth = baseWidth * widthScale * 2.8f;
                auraLine.endWidth = baseWidth * widthScale * 2.2f;
            }

            // Screen shake scaled by divine overcharge
            float shakeIntensity = Mathf.Lerp(0.35f, 0.15f, humanityRatio);
            CameraFollow2D.Instance?.Shake(duration, shakeIntensity);
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= duration)
            {
                Destroy(gameObject);
                return;
            }

            // Keep beam origin anchored to Aeron's attack/arm point
            if (owner != null && owner.attackPoint != null)
            {
                transform.position = owner.attackPoint.position;
            }

            Vector2 origin = transform.position;
            Vector2 endPoint = origin + Vector2.right * (direction * maxRange);

            // Raycast piercing through targets
            float humanityRatio = owner != null ? Mathf.Clamp01(owner.humanity / 100f) : 1f;
            float radius = Mathf.Lerp(0.8f, 0.45f, humanityRatio);

            RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, radius, Vector2.right * direction, maxRange);

            tickTimer += Time.deltaTime;
            bool doDamageTick = (tickTimer >= 0.08f);
            if (doDamageTick)
            {
                hitsThisTick.Clear();
                tickTimer = 0f;
            }

            foreach (var hit in hits)
            {
                if (hit.collider == null || hit.collider.gameObject == owner?.gameObject) continue;

                // 1. Evaporate oncoming bullets
                var bullet = hit.collider.GetComponent<Bullet>();
                if (bullet != null && !bullet.IsReflected)
                {
                    bullet.DisintegrateByLaser();
                    owner?.TriggerEcho(); // Reward player with Echo energy!
                }

                // 2. Damage guards
                if (doDamageTick && !hitsThisTick.Contains(hit.collider))
                {
                    hitsThisTick.Add(hit.collider);
                    var guard = hit.collider.GetComponent<GuardAI>();
                    if (guard != null && guard.CurrentState != GuardAI.AIState.Dead)
                    {
                        float tickDmg = damagePerSecond * 0.08f * Mathf.Lerp(1.5f, 1.0f, humanityRatio);
                        guard.TakeDamage(tickDmg, direction);

                        // If guard gets critically low, break them immediately for easy absorption
                        if (guard.currentHealth <= guard.maxHealth * 0.35f && guard.CurrentState != GuardAI.AIState.Broken)
                        {
                            guard.ForceBrokenState();
                        }
                    }
                }
            }

            // Subtle pulsing jitter for high-energy crackle
            float jitter = Random.Range(-0.04f, 0.04f);
            Vector3 startPos = origin;
            Vector3 finalEndPos = new Vector3(endPoint.x, endPoint.y + jitter, 0f);

            if (coreLine != null)
            {
                coreLine.SetPosition(0, startPos);
                coreLine.SetPosition(1, finalEndPos);
            }

            if (auraLine != null)
            {
                auraLine.SetPosition(0, startPos);
                auraLine.SetPosition(1, finalEndPos);
            }
        }
    }
}

