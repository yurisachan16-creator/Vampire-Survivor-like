using System.Collections.Generic;
using QAssetBundle;
using QFramework;
using UnityEngine;
using UnityEngine.U2D;

namespace VampireSurvivorLike
{
    public class HolyWater : ViewController
    {
        private static readonly List<Transform> TargetsBuffer = new List<Transform>(256);
        private const string ProjectileSpriteName = "rpgItems_42";
        private const int ZoneSortingOrder = 30;
        private const int ProjectileSortingOrder = 33;
        private const int ZoneTextureSize = 64;
        private const float TargetSearchRadius = 24f;
        private const float ProjectileSpeed = 10.8f;
        private const float ProjectileMaxDistance = 18f;
        private const float ProjectileHomingStrength = 0.88f;
        private const string ZoneShaderResourcePath = "Shaders/HolyWaterZonePulse";
        private static Sprite sZoneSprite;
        private static Texture2D sZoneTexture;
        private static bool sFallbackWarningLogged;

        private float _currentSeconds;
        private GameObject _projectileTemplate;
        private GameObject _zoneTemplate;
        private Material _zoneMaterial;
        private ResLoader _resLoader;
        private SpriteAtlas _iconAtlas;

        private void Update()
        {
            if (!Global.HolyWaterUnlocked.Value) return;
            if (!Player.Default) return;

            EnsureTemplates();
            if (!_zoneTemplate || !_projectileTemplate) return;

            _currentSeconds += Time.deltaTime;
            var interval = GetEffectiveCooldown(Global.HolyWaterDuration.Value);
            if (_currentSeconds < interval) return;
            _currentSeconds = 0f;

            var superHolyWater = Global.SuperHolyWater.Value;
            var zoneCount = Mathf.Max(1, 1 + Global.AdditionalFlyThingCount.Value);
            var areaMultiplier = Mathf.Max(1f, Global.AreaMultiplier.Value);
            var radius = 1.65f * areaMultiplier * (superHolyWater ? 1.35f : 1f);
            var lifeTime = superHolyWater ? 3.2f : 2.6f;
            var tickInterval = Mathf.Max(0.12f, Global.HolyWaterTickInterval.Value * (superHolyWater ? 0.85f : 1f));
            var damage = Global.HolyWaterDamage.Value * (superHolyWater ? 1.75f : 1f);
            var slowMultiplier = Mathf.Clamp(Global.HolyWaterSlowMultiplier.Value * (superHolyWater ? 0.87f : 1f), 0.25f, 0.95f);
            var slowDuration = Mathf.Max(0.08f, Global.HolyWaterSlowDuration.Value + (superHolyWater ? 0.2f : 0f));

            var searchRadius = TargetSearchRadius * areaMultiplier;
            EnemySpatialIndex.GetNearestTargets(Player.Default.transform.position, searchRadius, zoneCount, TargetsBuffer);
            if (TargetsBuffer.Count == 0) return;

            if (SfxThrottle.CanPlay(Sfx.KNIFE))
            {
                AudioKit.PlaySound(Sfx.KNIFE);
            }

            for (var i = 0; i < zoneCount; i++)
            {
                var target = TargetsBuffer[i % TargetsBuffer.Count];
                if (!target) continue;

                var go = ObjectPoolSystem.Spawn(_projectileTemplate, null, true);
                if (!go) continue;

                var center = (Vector2)Player.Default.transform.position;
                var dir = ((Vector2)target.position - center).normalized;
                if (dir.sqrMagnitude <= 0.0001f) dir = Random.insideUnitCircle.normalized;
                if (dir.sqrMagnitude <= 0.0001f) dir = Vector2.right;
                var spawnPos = center + dir * 0.3f;
                go.transform.position = spawnPos;
                go.transform.localScale = Vector3.one * (superHolyWater ? 1.1f : 0.95f);

                var projectile = go.GetComponent<PooledHolyWaterProjectile>();
                if (!projectile) projectile = go.AddComponent<PooledHolyWaterProjectile>();
                projectile.Configure(
                    dir,
                    ProjectileSpeed * (superHolyWater ? 1.12f : 1f),
                    ProjectileMaxDistance * areaMultiplier * (superHolyWater ? 1.08f : 1f),
                    Mathf.Clamp01(ProjectileHomingStrength + (superHolyWater ? 0.08f : 0f)),
                    _zoneTemplate,
                    damage,
                    tickInterval,
                    lifeTime,
                    radius,
                    slowMultiplier,
                    slowDuration,
                    superHolyWater);
            }
        }

        private static float GetEffectiveCooldown(float baseDuration)
        {
            var reduction = Mathf.Clamp(Global.CooldownReduction.Value, 0f, 0.75f);
            return Mathf.Max(0.12f, baseDuration * (1f - reduction));
        }

        private void EnsureTemplates()
        {
            EnsureZoneTemplate();
            EnsureProjectileTemplate();
        }

        private void EnsureZoneTemplate()
        {
            if (_zoneTemplate) return;

            _zoneTemplate = new GameObject("HolyWater_ZoneTemplate");
            _zoneTemplate.transform.SetParent(transform, false);
            _zoneTemplate.SetActive(false);

            var sr = _zoneTemplate.AddComponent<SpriteRenderer>();
            sr.sprite = GetZoneSprite();
            sr.color = new Color(1f, 1f, 1f, Config.HolyWaterVfxGuideAlpha * 0.75f);
            sr.sortingOrder = ZoneSortingOrder;
            ApplyZoneVisualMaterial(sr);

            var col = _zoneTemplate.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            _zoneTemplate.AddComponent<HolyWaterZone>();
        }

        private void EnsureProjectileTemplate()
        {
            if (_projectileTemplate) return;
            EnsureAtlas();

            _projectileTemplate = new GameObject("HolyWater_ProjectileTemplate");
            _projectileTemplate.transform.SetParent(transform, false);
            _projectileTemplate.SetActive(false);

            var sr = _projectileTemplate.AddComponent<SpriteRenderer>();
            var projectileSprite = _iconAtlas ? _iconAtlas.GetSprite(ProjectileSpriteName) : null;
            if (projectileSprite)
            {
                sr.sprite = projectileSprite;
                sr.color = new Color(0.66f, 0.92f, 1f, 0.95f);
            }
            else
            {
                var fallback = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                sr.sprite = fallback;
                sr.color = new Color(0.58f, 0.86f, 1f, 0.9f);
                _projectileTemplate.transform.localScale = new Vector3(0.24f, 0.24f, 1f);
            }
            sr.sortingOrder = ProjectileSortingOrder;

            var rb = _projectileTemplate.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = _projectileTemplate.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.14f;

            _projectileTemplate.AddComponent<PooledHolyWaterProjectile>();
        }

        private void EnsureAtlas()
        {
            if (_iconAtlas) return;
            _resLoader ??= ResLoader.Allocate();
            _iconAtlas = _resLoader.LoadSync<SpriteAtlas>("icon");
        }

        private void ApplyZoneVisualMaterial(SpriteRenderer sr)
        {
            if (!sr) return;

            if (_zoneMaterial == null)
            {
                var shader = Resources.Load<Shader>(ZoneShaderResourcePath);
                if (shader == null)
                {
                    shader = Shader.Find("VSL/HolyWaterZonePulse");
                }
                if (shader == null)
                {
                    shader = Shader.Find("Sprites/Default");
                    if (shader != null && !sFallbackWarningLogged)
                    {
                        Debug.LogWarning("[HolyWater] Shader fallback to Sprites/Default. HolyWaterZonePulse not found.");
                        sFallbackWarningLogged = true;
                    }
                }
                if (shader != null)
                {
                    _zoneMaterial = new Material(shader);
                }
                else if (!sFallbackWarningLogged)
                {
                    Debug.LogWarning("[HolyWater] Shader missing. Zone will render with SpriteRenderer default material.");
                    sFallbackWarningLogged = true;
                }
            }

            if (_zoneMaterial != null)
            {
                sr.sharedMaterial = _zoneMaterial;
            }
        }

        private static Sprite GetZoneSprite()
        {
            if (sZoneSprite != null) return sZoneSprite;

            if (sZoneTexture == null)
            {
                sZoneTexture = new Texture2D(ZoneTextureSize, ZoneTextureSize, TextureFormat.RGBA32, false, true)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
                var pixels = new Color32[ZoneTextureSize * ZoneTextureSize];
                for (var i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32(255, 255, 255, 255);
                }
                sZoneTexture.SetPixels32(pixels);
                sZoneTexture.Apply(false, true);
            }

            sZoneSprite = Sprite.Create(
                sZoneTexture,
                new Rect(0f, 0f, ZoneTextureSize, ZoneTextureSize),
                new Vector2(0.5f, 0.5f),
                ZoneTextureSize);
            return sZoneSprite;
        }

        private void OnDestroy()
        {
            if (_projectileTemplate)
            {
                Destroy(_projectileTemplate);
                _projectileTemplate = null;
            }

            if (_zoneTemplate)
            {
                Destroy(_zoneTemplate);
                _zoneTemplate = null;
            }

            if (_zoneMaterial != null)
            {
                Destroy(_zoneMaterial);
                _zoneMaterial = null;
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
