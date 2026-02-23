using QAssetBundle;
using QFramework;
using UnityEngine;
using UnityEngine.U2D;

namespace VampireSurvivorLike
{
    public class HolyWater : ViewController
    {
        private const string ZoneSpriteName = "rpgItems_42";
        private const float DropRadius = 2.4f;

        private float _currentSeconds;
        private GameObject _zoneTemplate;
        private ResLoader _resLoader;
        private SpriteAtlas _iconAtlas;

        private void Update()
        {
            if (!Global.HolyWaterUnlocked.Value) return;
            if (!Player.Default) return;

            EnsureTemplate();
            if (!_zoneTemplate) return;

            _currentSeconds += Time.deltaTime;
            var interval = GetEffectiveCooldown(Global.HolyWaterDuration.Value);
            if (_currentSeconds < interval) return;
            _currentSeconds = 0f;

            var superHolyWater = Global.SuperHolyWater.Value;
            var zoneCount = Mathf.Max(1, 1 + Global.AdditionalFlyThingCount.Value);
            var areaMultiplier = Mathf.Max(1f, Global.AreaMultiplier.Value);
            var radius = 1.3f * areaMultiplier * (superHolyWater ? 1.35f : 1f);
            var lifeTime = superHolyWater ? 3.2f : 2.2f;
            var tickInterval = Mathf.Max(0.12f, Global.HolyWaterTickInterval.Value * (superHolyWater ? 0.85f : 1f));
            var damage = Global.HolyWaterDamage.Value * (superHolyWater ? 1.65f : 1f);
            var slowMultiplier = Mathf.Clamp(Global.HolyWaterSlowMultiplier.Value * (superHolyWater ? 0.88f : 1f), 0.25f, 0.95f);
            var slowDuration = Mathf.Max(0.08f, Global.HolyWaterSlowDuration.Value + (superHolyWater ? 0.2f : 0f));

            if (SfxThrottle.CanPlay(Sfx.KNIFE))
            {
                AudioKit.PlaySound(Sfx.KNIFE);
            }

            for (var i = 0; i < zoneCount; i++)
            {
                var go = ObjectPoolSystem.Spawn(_zoneTemplate, null, true);
                if (!go) continue;

                var center = (Vector2)Player.Default.transform.position;
                var spawnPos = superHolyWater ? center : center + Random.insideUnitCircle * (DropRadius * areaMultiplier);
                go.transform.position = spawnPos;

                var zone = go.GetComponent<HolyWaterZone>();
                if (!zone) zone = go.AddComponent<HolyWaterZone>();
                zone.Configure(
                    damage,
                    tickInterval,
                    lifeTime,
                    radius,
                    slowMultiplier,
                    slowDuration,
                    superHolyWater,
                    superHolyWater);
            }
        }

        private static float GetEffectiveCooldown(float baseDuration)
        {
            var reduction = Mathf.Clamp(Global.CooldownReduction.Value, 0f, 0.75f);
            return Mathf.Max(0.12f, baseDuration * (1f - reduction));
        }

        private void EnsureTemplate()
        {
            if (_zoneTemplate) return;

            EnsureAtlas();
            _zoneTemplate = new GameObject("HolyWater_ZoneTemplate");
            _zoneTemplate.transform.SetParent(transform, false);
            _zoneTemplate.SetActive(false);

            var sr = _zoneTemplate.AddComponent<SpriteRenderer>();
            var sprite = _iconAtlas ? _iconAtlas.GetSprite(ZoneSpriteName) : null;
            if (sprite)
            {
                sr.sprite = sprite;
                sr.color = new Color(0.72f, 0.88f, 1f, 0.85f);
            }
            else
            {
                var fallback = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                sr.sprite = fallback;
                sr.color = new Color(0.72f, 0.88f, 1f, 0.5f);
                _zoneTemplate.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            }
            sr.sortingOrder = -1;

            var col = _zoneTemplate.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            _zoneTemplate.AddComponent<HolyWaterZone>();
        }

        private void EnsureAtlas()
        {
            if (_iconAtlas) return;
            _resLoader ??= ResLoader.Allocate();
            _iconAtlas = _resLoader.LoadSync<SpriteAtlas>("icon");
        }

        private void OnDestroy()
        {
            if (_zoneTemplate)
            {
                Destroy(_zoneTemplate);
                _zoneTemplate = null;
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
