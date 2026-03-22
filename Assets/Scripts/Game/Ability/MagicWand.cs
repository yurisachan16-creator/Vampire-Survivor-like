using System.Collections.Generic;
using UnityEngine;
using QFramework;
using QAssetBundle;
using UnityEngine.U2D;

namespace VampireSurvivorLike
{
    public class MagicWand : ViewController
    {
        private static readonly List<Transform> TargetsBuffer = new List<Transform>(256);
        private const string ProjectileSpriteName = "rpgItems_46";

        private const float TargetSearchRadius = 25f;
        private const float BulletSpeed = 14f;
        private const float BulletMaxDistanceFromPlayer = 26f;

        private float _currentSeconds;
        private GameObject _projectileTemplate;
        private GameObject _projectileSource;
        private ResLoader _resLoader;
        private SpriteAtlas _iconAtlas;

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
                if (!projectile)
                {
                    projectile = go.AddComponent<PooledMagicBullet>();
                    ObjectPoolSystem.RefreshPoolableCache(go);
                    projectile.OnSpawned();
                }

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
            EnsureAtlas();
            _projectileSource = ResolveProjectileSource();
            _projectileTemplate = CreateProjectileTemplate("MagicWand_BulletTemplate", _projectileSource, ProjectileSpriteName, 0.18f);
        }

        private void EnsureAtlas()
        {
            if (_iconAtlas) return;
            _resLoader ??= ResLoader.Allocate();
            _iconAtlas = _resLoader.LoadSync<SpriteAtlas>("icon");
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

        private GameObject CreateProjectileTemplate(string templateName, GameObject source, string spriteName, float colliderRadius)
        {
            var template = new GameObject(templateName);
            template.transform.SetParent(transform, false);
            template.SetActive(false);

            var spriteRenderer = template.AddComponent<SpriteRenderer>();
            var sourceRenderer = source ? source.GetComponentInChildren<SpriteRenderer>(true) : null;
            var projectileSprite = _iconAtlas ? _iconAtlas.GetSprite(spriteName) : null;
            if (projectileSprite || sourceRenderer)
            {
                spriteRenderer.sprite = projectileSprite ? projectileSprite : sourceRenderer.sprite;
                spriteRenderer.sortingLayerID = sourceRenderer ? sourceRenderer.sortingLayerID : spriteRenderer.sortingLayerID;
                spriteRenderer.sortingOrder = sourceRenderer ? sourceRenderer.sortingOrder : 0;
                spriteRenderer.color = Color.white;
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

            if (!template.GetComponent<PooledMagicBullet>())
            {
                template.AddComponent<PooledMagicBullet>();
            }

            return template;
        }

        private void OnDestroy()
        {
            if (_projectileTemplate)
            {
                Destroy(_projectileTemplate);
                _projectileTemplate = null;
            }

            if (_resLoader != null)
            {
                _resLoader.Recycle2Cache();
                _resLoader = null;
            }

            _iconAtlas = null;
        }
    }
}
