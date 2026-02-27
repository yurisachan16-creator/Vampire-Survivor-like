using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class HolyWaterZone : MonoBehaviour, ObjectPoolSystem.IPoolable
    {
        private static readonly List<Transform> TargetsBuffer = new List<Transform>(256);
        private static readonly int RadiusId = Shader.PropertyToID("_Radius");
        private static readonly int FeatherId = Shader.PropertyToID("_Feather");
        private static readonly int RingThicknessId = Shader.PropertyToID("_RingThickness");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int InnerColorId = Shader.PropertyToID("_InnerColor");
        private static readonly int OuterColorId = Shader.PropertyToID("_OuterColor");
        private static readonly int RingColorId = Shader.PropertyToID("_RingColor");
        private static readonly int NoiseScaleId = Shader.PropertyToID("_NoiseScale");
        private static readonly int SwirlSpeedId = Shader.PropertyToID("_SwirlSpeed");
        private static readonly int GuideAlphaId = Shader.PropertyToID("_GuideAlpha");
        private static readonly int Life01Id = Shader.PropertyToID("_Life01");
        private static readonly int TickFlashId = Shader.PropertyToID("_TickFlash");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");
        private static Material sWaterParticleMaterial;
        private static Texture2D sWaterParticleTexture;

        private float _damagePerTick;
        private float _tickInterval;
        private float _lifeTime;
        private float _radius;
        private float _slowMultiplier;
        private float _slowDuration;
        private bool _followPlayer;
        private bool _superMode;
        private float _tickTimer;
        private float _lifeTimer;
        private SpriteRenderer _spriteRenderer;
        private MaterialPropertyBlock _mpb;
        private float _pulseTime;
        private float _life01;
        private float _tickFlash;
        private float _seed;
        private ParticleSystem _breathParticle;
        private ParticleSystem _breathCoreParticle;
        private ParticleSystemRenderer _breathRenderer;
        private ParticleSystemRenderer _breathCoreRenderer;

        public void Configure(
            float damagePerTick,
            float tickInterval,
            float lifeTime,
            float radius,
            float slowMultiplier,
            float slowDuration,
            bool followPlayer,
            bool superMode)
        {
            _damagePerTick = Mathf.Max(1f, damagePerTick);
            _tickInterval = Mathf.Max(0.12f, tickInterval);
            _lifeTime = Mathf.Max(_tickInterval, lifeTime);
            _radius = Mathf.Max(0.5f, radius);
            _slowMultiplier = Mathf.Clamp(slowMultiplier, 0.25f, 1f);
            _slowDuration = Mathf.Max(0.05f, slowDuration);
            _followPlayer = followPlayer;
            _superMode = superMode;
            _tickTimer = 0f;
            _lifeTimer = 0f;
            _pulseTime = 0f;
            _life01 = 0f;
            _tickFlash = 0f;
            _seed = Random.Range(0f, 1024f);

            transform.localScale = Vector3.one * (_radius * 1.65f);
            EnsureBreathVfx();
            ConfigureBreathVfx();
            ApplyVisualGuide();
            ApplyTickDamage();
        }

        private void Update()
        {
            if (_followPlayer && Player.Default)
            {
                transform.position = Player.Default.transform.position;
            }

            _lifeTimer += Time.deltaTime;
            _pulseTime += Time.deltaTime;
            _life01 = Mathf.Clamp01(_lifeTimer / Mathf.Max(0.01f, _lifeTime));
            _tickFlash = Mathf.Max(0f, _tickFlash - Time.deltaTime * 2.8f);
            ApplyVisualGuide();
            UpdateBreathVfx();
            if (_lifeTimer >= _lifeTime)
            {
                ObjectPoolSystem.Despawn(gameObject);
                return;
            }

            _tickTimer += Time.deltaTime;
            if (_tickTimer < _tickInterval) return;

            _tickTimer -= _tickInterval;
            ApplyTickDamage();
        }

        private void ApplyTickDamage()
        {
            _tickFlash = 1f;
            EnemySpatialIndex.GetNearestTargets(transform.position, _radius, 160, TargetsBuffer);
            if (TargetsBuffer.Count == 0) return;

            var center = (Vector2)transform.position;
            var radiusSqr = _radius * _radius;
            for (var i = 0; i < TargetsBuffer.Count; i++)
            {
                var target = TargetsBuffer[i];
                if (!target) continue;
                if (((Vector2)target.position - center).sqrMagnitude > radiusSqr) continue;

                var enemy = target.GetComponent<IEnemy>();
                if (enemy == null) continue;

                DamageSystem.CalculateDamage(_damagePerTick, enemy, maxNormalDamage: _superMode ? 2 : 1, criticalDamageTimes: _superMode ? 4f : 3f);
                enemy.ApplySlow(_slowMultiplier, _slowDuration);
            }
        }

        private void ApplyVisualGuide()
        {
            if (!_spriteRenderer) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (!_spriteRenderer) return;

            _mpb ??= new MaterialPropertyBlock();
            _spriteRenderer.GetPropertyBlock(_mpb);
            var pulse = Mathf.Sin(_pulseTime * (_superMode ? 2.6f : 2.1f)) * 0.02f;
            _mpb.SetFloat(RadiusId, 0.45f + pulse);
            _mpb.SetFloat(FeatherId, _superMode ? 0.2f : 0.17f);
            _mpb.SetFloat(RingThicknessId, (_superMode ? 0.1f : 0.08f) * (1f + _tickFlash * 0.16f));
            _mpb.SetFloat(IntensityId, (_superMode ? 1.05f : 0.82f) + _tickFlash * 0.22f);
            _mpb.SetColor(InnerColorId, _superMode ? Config.HolyWaterVfxInnerColor * 1.1f : Config.HolyWaterVfxInnerColor);
            _mpb.SetColor(OuterColorId, _superMode ? Config.HolyWaterVfxOuterColor * 1.05f : Config.HolyWaterVfxOuterColor);
            _mpb.SetColor(RingColorId, Config.HolyWaterVfxRingColor);
            _mpb.SetFloat(NoiseScaleId, Config.HolyWaterVfxNoiseScale * (_superMode ? 1.08f : 1f));
            _mpb.SetFloat(SwirlSpeedId, Config.HolyWaterVfxSwirlSpeed * (_superMode ? 1.16f : 1f));
            _mpb.SetFloat(GuideAlphaId, Config.HolyWaterVfxGuideAlpha);
            _mpb.SetFloat(Life01Id, _life01);
            _mpb.SetFloat(TickFlashId, _tickFlash);
            _mpb.SetFloat(SeedId, _seed);
            _spriteRenderer.SetPropertyBlock(_mpb);
        }

        public void OnSpawned()
        {
            _damagePerTick = 1f;
            _tickInterval = 0.5f;
            _lifeTime = 2f;
            _radius = 1f;
            _slowMultiplier = 1f;
            _slowDuration = 0.1f;
            _followPlayer = false;
            _superMode = false;
            _tickTimer = 0f;
            _lifeTimer = 0f;
            _pulseTime = 0f;
            _life01 = 0f;
            _tickFlash = 0f;
            _seed = 0f;
            transform.localScale = Vector3.one;
            if (!_spriteRenderer) _spriteRenderer = GetComponent<SpriteRenderer>();
            _mpb ??= new MaterialPropertyBlock();
            if (_breathParticle)
            {
                _breathParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (_breathCoreParticle)
            {
                _breathCoreParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public void OnDespawned()
        {
            _tickTimer = 0f;
            _lifeTimer = 0f;
            _followPlayer = false;
            if (_breathParticle)
            {
                _breathParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (_breathCoreParticle)
            {
                _breathCoreParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void EnsureBreathVfx()
        {
            if (_breathParticle && _breathCoreParticle) return;

            var cloudGo = new GameObject("HolyWater_BreathCloud");
            cloudGo.transform.SetParent(transform, false);
            cloudGo.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            _breathParticle = cloudGo.AddComponent<ParticleSystem>();
            _breathRenderer = cloudGo.GetComponent<ParticleSystemRenderer>();
            SetupBreathParticle(_breathParticle, _breathRenderer);

            var coreGo = new GameObject("HolyWater_BreathCore");
            coreGo.transform.SetParent(transform, false);
            coreGo.transform.localPosition = new Vector3(0f, 0f, -0.03f);
            _breathCoreParticle = coreGo.AddComponent<ParticleSystem>();
            _breathCoreRenderer = coreGo.GetComponent<ParticleSystemRenderer>();
            SetupCoreBreathParticle(_breathCoreParticle, _breathCoreRenderer);
        }

        private void ConfigureBreathVfx()
        {
            if (!_breathParticle || !_breathCoreParticle) return;

            var radiusScale = Mathf.Clamp(_radius / 1.65f, 0.8f, 2.2f);

            var main = _breathParticle.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.62f, _superMode ? 1.35f : 1.15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.09f * radiusScale, (_superMode ? 0.36f : 0.28f) * radiusScale);
            var emission = _breathParticle.emission;
            emission.rateOverTime = Config.HolyWaterBreathEmissionRate * (_superMode ? 1.35f : 1f);

            var coreMain = _breathCoreParticle.main;
            coreMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, _superMode ? 1.08f : 0.95f);
            coreMain.startSize = new ParticleSystem.MinMaxCurve(0.05f * radiusScale, (_superMode ? 0.24f : 0.19f) * radiusScale);
            var coreEmission = _breathCoreParticle.emission;
            coreEmission.rateOverTime = Config.HolyWaterBreathCoreEmissionRate * (_superMode ? 1.4f : 1f);

            _breathParticle.Play(true);
            _breathCoreParticle.Play(true);
            UpdateBreathVfx();
        }

        private void UpdateBreathVfx()
        {
            if (!_breathParticle || !_breathCoreParticle) return;

            var guideOrder = _spriteRenderer ? _spriteRenderer.sortingOrder : 30;
            if (_breathRenderer) _breathRenderer.sortingOrder = guideOrder + 1;
            if (_breathCoreRenderer) _breathCoreRenderer.sortingOrder = guideOrder + 2;
            if (_spriteRenderer && _breathRenderer)
            {
                _breathRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
            }
            if (_spriteRenderer && _breathCoreRenderer)
            {
                _breathCoreRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
            }

            var endFade = 1f - Mathf.SmoothStep(0.78f, 1f, _life01);
            var flashBoost = 1f + _tickFlash * 0.14f;
            var emission = _breathParticle.emission;
            emission.rateOverTime = Config.HolyWaterBreathEmissionRate * (_superMode ? 1.35f : 1f) * endFade * flashBoost;
            var coreEmission = _breathCoreParticle.emission;
            coreEmission.rateOverTime = Config.HolyWaterBreathCoreEmissionRate * (_superMode ? 1.4f : 1f) * endFade * flashBoost;
        }

        private static void SetupBreathParticle(ParticleSystem ps, ParticleSystemRenderer renderer)
        {
            if (!ps || !renderer) return;

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Shape;
            main.maxParticles = 240;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.62f, 1.15f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f, 0.08f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.09f, 0.28f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(0.54f, 0.88f, 1f, 0.45f);
            main.gravityModifier = 0f;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = Config.HolyWaterBreathEmissionRate;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.42f;
            shape.arc = 360f;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.07f, 0.07f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.separateAxes = true;
            noise.strengthX = 0.08f;
            noise.strengthY = 0.04f;
            noise.strengthZ = 0.08f;
            noise.frequency = 0.35f;
            noise.scrollSpeed = 0.18f;
            noise.octaveCount = 2;
            noise.quality = ParticleSystemNoiseQuality.Low;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            var sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.34f),
                new Keyframe(0.45f, 0.82f),
                new Keyframe(1f, 1.03f));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.72f, 0.94f, 1f), 0f),
                    new GradientColorKey(new Color(0.4f, 0.76f, 0.98f), 0.58f),
                    new GradientColorKey(new Color(0.16f, 0.45f, 0.82f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.38f, 0.26f),
                    new GradientAlphaKey(0.12f, 0.84f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortMode = ParticleSystemSortMode.OldestInFront;
            renderer.sharedMaterial = GetWaterParticleMaterial();
        }

        private static void SetupCoreBreathParticle(ParticleSystem ps, ParticleSystemRenderer renderer)
        {
            if (!ps || !renderer) return;

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Shape;
            main.maxParticles = 90;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.95f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.005f, 0.06f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.19f);
            main.startColor = new Color(0.82f, 0.97f, 1f, 0.36f);

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = Config.HolyWaterBreathCoreEmissionRate;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            var sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.24f),
                new Keyframe(0.45f, 0.68f),
                new Keyframe(1f, 0.92f));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.9f, 0.98f, 1f), 0f),
                    new GradientColorKey(new Color(0.58f, 0.9f, 1f), 0.6f),
                    new GradientColorKey(new Color(0.24f, 0.62f, 0.94f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.22f, 0.24f),
                    new GradientAlphaKey(0.08f, 0.82f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortMode = ParticleSystemSortMode.OldestInFront;
            renderer.sharedMaterial = GetWaterParticleMaterial();
        }

        private static Material GetWaterParticleMaterial()
        {
            if (sWaterParticleMaterial) return sWaterParticleMaterial;

            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (!shader) shader = Shader.Find("Particles/Standard Unlit");
            if (!shader) shader = Shader.Find("Sprites/Default");
            if (!shader) return null;

            sWaterParticleMaterial = new Material(shader);
            var tex = GetWaterParticleTexture();
            if (tex != null)
            {
                if (sWaterParticleMaterial.HasProperty("_BaseMap"))
                {
                    sWaterParticleMaterial.SetTexture("_BaseMap", tex);
                }
                if (sWaterParticleMaterial.HasProperty("_MainTex"))
                {
                    sWaterParticleMaterial.SetTexture("_MainTex", tex);
                }
            }

            if (sWaterParticleMaterial.HasProperty("_Surface"))
            {
                sWaterParticleMaterial.SetFloat("_Surface", 1f);
            }
            if (sWaterParticleMaterial.HasProperty("_Blend"))
            {
                sWaterParticleMaterial.SetFloat("_Blend", 0f);
            }

            return sWaterParticleMaterial;
        }

        private static Texture2D GetWaterParticleTexture()
        {
            if (sWaterParticleTexture) return sWaterParticleTexture;

            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var maxDist = center.magnitude;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var p = new Vector2(x, y);
                    var d = Vector2.Distance(p, center) / maxDist;
                    var a = Mathf.Clamp01(1f - d * 1.55f);
                    a = Mathf.Pow(a, 1.9f);
                    var idx = y * size + x;
                    pixels[idx] = new Color(1f, 1f, 1f, a);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            sWaterParticleTexture = tex;
            return sWaterParticleTexture;
        }
    }
}
