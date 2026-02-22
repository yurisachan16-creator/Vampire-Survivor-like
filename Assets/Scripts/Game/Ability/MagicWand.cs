using System.Collections.Generic;
using UnityEngine;
using QFramework;
using QAssetBundle;

namespace VampireSurvivorLike
{
    public class MagicWand : ViewController
    {
        private static readonly List<Transform> TargetsBuffer = new List<Transform>(256);

        private const float TargetSearchRadius = 25f;
        private const float BulletSpeed = 14f;
        private const float BulletMaxDistanceFromPlayer = 26f;

        private float _currentSeconds;
        private GameObject _projectileTemplate;
        private GameObject _projectileSource;

        public void SetProjectileSource(GameObject source)
        {
            _projectileSource = source;
        }

        private void Update()
        {
            if (!Global.MagicWandUnlocked.Value) return;
            if (!Player.Default) return;

            EnsureTemplate();
            if (!_projectileTemplate) return;

            _currentSeconds += Time.deltaTime;
            var interval = GetEffectiveCooldown(Global.MagicWandDuration.Value);
            if (_currentSeconds < interval) return;
            _currentSeconds = 0f;

            var superWand = Global.SuperMagicWand.Value;
            var targetCount = Mathf.Max(1, Global.MagicWandCount.Value + Global.AdditionalFlyThingCount.Value);
            var searchRadius = TargetSearchRadius * Mathf.Max(1f, Global.AreaMultiplier.Value);
            EnemySpatialIndex.GetNearestTargets(Player.Default.transform.position, searchRadius, targetCount, TargetsBuffer);
            if (TargetsBuffer.Count == 0) return;

            if (SfxThrottle.CanPlay(Sfx.KNIFE))
            {
                AudioKit.PlaySound(Sfx.KNIFE);
            }

            foreach (var targetTransform in TargetsBuffer)
            {
                if (!targetTransform) continue;

                var direction = ((Vector2)targetTransform.position - (Vector2)Player.Default.transform.position).normalized;
                var go = ObjectPoolSystem.Spawn(_projectileTemplate, null, true);
                if (!go) continue;

                go.transform.position = this.Position();
                go.transform.localScale = Vector3.one * (superWand ? 1.1f : 0.9f);

                var projectile = go.GetComponent<PooledMagicBullet>();
                if (!projectile) projectile = go.AddComponent<PooledMagicBullet>();

                var damage = Global.MagicWandDamage.Value * (superWand ? 1.8f : 1f);
                var maxHits = superWand ? 4 : 1;
                var knockback = superWand ? 10f : 6f;
                var maxDistance = BulletMaxDistanceFromPlayer * Mathf.Max(1f, Global.AreaMultiplier.Value);
                var speed = BulletSpeed * (superWand ? 1.15f : 1f);
                projectile.Configure(direction, speed, damage, superWand, maxHits, maxDistance, knockback);
            }
        }

        private static float GetEffectiveCooldown(float baseDuration)
        {
            var reduction = Mathf.Clamp(Global.CooldownReduction.Value, 0f, 0.75f);
            return Mathf.Max(0.08f, baseDuration * (1f - reduction));
        }

        private void EnsureTemplate()
        {
            if (_projectileTemplate) return;
            _projectileSource = ResolveProjectileSource();
            _projectileTemplate = CreateProjectileTemplate("MagicWand_BulletTemplate", _projectileSource, 0.18f);
        }

        private GameObject ResolveProjectileSource()
        {
            if (_projectileSource) return _projectileSource;

            var abilityController = GetComponent<AbilityController>();
            if (abilityController != null)
            {
                if (abilityController.SimpleKnife && abilityController.SimpleKnife.Knife)
                {
                    return abilityController.SimpleKnife.Knife.gameObject;
                }

                if (abilityController.SimpleAxe && abilityController.SimpleAxe.Axe)
                {
                    return abilityController.SimpleAxe.Axe.gameObject;
                }
            }

            return null;
        }

        private GameObject CreateProjectileTemplate(string templateName, GameObject source, float colliderRadius)
        {
            var template = new GameObject(templateName);
            template.transform.SetParent(transform, false);
            template.SetActive(false);

            var spriteRenderer = template.AddComponent<SpriteRenderer>();
            var sourceRenderer = source ? source.GetComponentInChildren<SpriteRenderer>(true) : null;
            if (sourceRenderer)
            {
                spriteRenderer.sprite = sourceRenderer.sprite;
                spriteRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
                spriteRenderer.sortingOrder = sourceRenderer.sortingOrder;
                spriteRenderer.color = sourceRenderer.color;
            }
            else
            {
                var fallback = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                spriteRenderer.sprite = fallback;
                template.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
            }

            var rb = template.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var circle = template.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = colliderRadius;

            return template;
        }

        private void OnDestroy()
        {
            if (_projectileTemplate)
            {
                Destroy(_projectileTemplate);
                _projectileTemplate = null;
            }
        }
    }
}
