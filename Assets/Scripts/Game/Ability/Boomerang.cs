using System.Collections.Generic;
using QAssetBundle;
using QFramework;
using UnityEngine;
using UnityEngine.U2D;

namespace VampireSurvivorLike
{
    public class Boomerang : ViewController
    {
        private static readonly List<Transform> TargetsBuffer = new List<Transform>(256);
        private const string ProjectileSpriteName = "rpgItems_49";
        private const float TargetSearchRadius = 24f;
        private const float ProjectileSpeed = 13f;
        private const float OutboundDistance = 8f;

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
            if (!Global.BoomerangUnlocked.Value) return;
            if (!Player.Default) return;

            EnsureTemplate();
            if (!_projectileTemplate) return;

            _currentSeconds += Time.deltaTime;
            var interval = GetEffectiveCooldown(Global.BoomerangDuration.Value);
            if (_currentSeconds < interval) return;
            _currentSeconds = 0f;

            var superBoomerang = Global.SuperBoomerang.Value;
            var shotCount = Mathf.Max(1, Global.BoomerangCount.Value + Global.AdditionalFlyThingCount.Value);
            var searchRadius = TargetSearchRadius * Mathf.Max(1f, Global.AreaMultiplier.Value);
            EnemySpatialIndex.GetNearestTargets(Player.Default.transform.position, searchRadius, shotCount, TargetsBuffer);

            if (SfxThrottle.CanPlay(Sfx.KNIFE))
            {
                AudioKit.PlaySound(Sfx.KNIFE);
            }

            var damage = Global.BoomerangDamage.Value * (superBoomerang ? 1.5f : 1f);
            var maxHits = Mathf.Max(1, Global.BoomerangMaxHits.Value + (superBoomerang ? 3 : 0));
            var returnCount = Mathf.Max(1, Global.BoomerangReturnCount.Value + (superBoomerang ? 2 : 0));
            var maxDistance = OutboundDistance * Mathf.Max(1f, Global.AreaMultiplier.Value) * (superBoomerang ? 1.2f : 1f);

            for (var i = 0; i < shotCount; i++)
            {
                Vector2 direction;
                if (TargetsBuffer.Count > 0 && TargetsBuffer[i % TargetsBuffer.Count])
                {
                    direction = ((Vector2)TargetsBuffer[i % TargetsBuffer.Count].position - (Vector2)Player.Default.transform.position).normalized;
                }
                else
                {
                    direction = Random.insideUnitCircle.normalized;
                    if (direction.sqrMagnitude <= 0.001f) direction = Vector2.right;
                }

                var go = ObjectPoolSystem.Spawn(_projectileTemplate, null, true);
                if (!go) continue;

                go.transform.position = this.Position();
                go.transform.localScale = Vector3.one * (superBoomerang ? 1.25f : 1f);

                var projectile = go.GetComponent<PooledBoomerangProjectile>();
                if (!projectile) projectile = go.AddComponent<PooledBoomerangProjectile>();
                projectile.Configure(direction, ProjectileSpeed, maxDistance, damage, maxHits, returnCount, superBoomerang);
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
            _projectileTemplate = CreateProjectileTemplate("Boomerang_ProjectileTemplate", _projectileSource, ProjectileSpriteName, 0.16f);
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
                template.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            }
            else
            {
                var fallback = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                spriteRenderer.sprite = fallback;
                template.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
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

            if (_resLoader != null)
            {
                _resLoader.Recycle2Cache();
                _resLoader = null;
            }

            _iconAtlas = null;
        }
    }
}
