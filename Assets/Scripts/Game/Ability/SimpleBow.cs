using System.Collections.Generic;
using UnityEngine;
using QFramework;
using QAssetBundle;
using UnityEngine.U2D;

namespace VampireSurvivorLike
{
    public class SimpleBow : ViewController
    {
        private static readonly List<Transform> TargetsBuffer = new List<Transform>(256);
        private static readonly float[] SuperArrowSpreadAngles = { -12f, 0f, 12f };
        private const string ProjectileSpriteName = "rpgItems_53";

        private const float TargetSearchRadius = 30f;
        private const float ArrowSpeed = 18f;
        private const float ArrowDistance = 30f;

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
            if (!Global.SimpleBowUnlocked.Value) return;
            if (!Player.Default) return;

            EnsureTemplate();
            if (!_projectileTemplate) return;

            _currentSeconds += Time.deltaTime;
            var interval = GetEffectiveCooldown(Global.SimpleBowDuration.Value);
            if (_currentSeconds < interval) return;
            _currentSeconds = 0f;

            var superBow = Global.SuperBow.Value;
            var shotCount = Mathf.Max(1, Global.SimpleBowCount.Value + Global.AdditionalFlyThingCount.Value);
            var searchRadius = TargetSearchRadius * Mathf.Max(1f, Global.AreaMultiplier.Value);
            EnemySpatialIndex.GetNearestTargets(Player.Default.transform.position, searchRadius, shotCount, TargetsBuffer);
            if (TargetsBuffer.Count == 0) return;

            if (SfxThrottle.CanPlay(Sfx.KNIFE))
            {
                AudioKit.PlaySound(Sfx.KNIFE);
            }

            var baseDamage = Global.SimpleBowDamage.Value * (superBow ? 1.5f : 1f);
            var basePierce = Mathf.Max(1, Global.SimpleBowPierce.Value + (superBow ? 2 : 0));

            for (var i = 0; i < shotCount; i++)
            {
                var target = TargetsBuffer[i % TargetsBuffer.Count];
                if (!target) continue;

                var baseDirection = ((Vector2)target.position - (Vector2)Player.Default.transform.position).normalized;
                if (!superBow)
                {
                    SpawnArrow(baseDirection, baseDamage, basePierce, 0f, false);
                    continue;
                }

                for (var s = 0; s < SuperArrowSpreadAngles.Length; s++)
                {
                    var spreadDirection = Quaternion.Euler(0f, 0f, SuperArrowSpreadAngles[s]) * baseDirection;
                    SpawnArrow(spreadDirection, baseDamage, basePierce, 0.22f, true);
                }
            }
        }

        private void SpawnArrow(Vector2 direction, float damage, int maxHits, float homingStrength, bool superArrow)
        {
            var go = ObjectPoolSystem.Spawn(_projectileTemplate, null, true);
            if (!go) return;

            go.transform.position = this.Position();
            go.transform.localScale = Vector3.one * (superArrow ? 1.15f : 0.95f);

            var projectile = go.GetComponent<PooledArrowProjectile>();
            if (!projectile) projectile = go.AddComponent<PooledArrowProjectile>();

            var distance = ArrowDistance * Mathf.Max(1f, Global.AreaMultiplier.Value) * (superArrow ? 1.2f : 1f);
            projectile.Configure(direction, ArrowSpeed, damage, maxHits, distance, homingStrength);
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
            _projectileTemplate = CreateProjectileTemplate("SimpleBow_ArrowTemplate", _projectileSource, ProjectileSpriteName, 0.12f, new Vector3(0.45f, 0.9f, 1f));
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

        private GameObject CreateProjectileTemplate(string templateName, GameObject source, string spriteName, float colliderRadius, Vector3 fallbackScale)
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
                template.transform.localScale = fallbackScale;
            }
            else
            {
                var fallback = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                spriteRenderer.sprite = fallback;
                template.transform.localScale = fallbackScale * 0.35f;
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
