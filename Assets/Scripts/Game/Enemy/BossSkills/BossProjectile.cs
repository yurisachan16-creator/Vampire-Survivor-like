using UnityEngine;

namespace VampireSurvivorLike
{
    /// <summary>
    /// Boss弹幕 - 用于范围攻击技能
    /// </summary>
    [DisallowMultipleComponent]
    public class BossProjectile : MonoBehaviour, ObjectPoolSystem.IPoolable
    {
        private const float DefaultArmingDelay = 0.08f;
        private const string TargetSortingLayer = "Instances";
        private const int TargetSortingOrder = 80;
        private const float DefaultVisualScale = 0.5f;
        private static Material s_trailMaterial;

        private float _damage = 1f;
        private float _lifetime = 3f;
        private float _timer = 0f;
        private float _armingDelay = DefaultArmingDelay;
        private bool _hasHit = false;
        private string _bossId = string.Empty;
        private string _damageSource = "BossProjectile";
        private Collider2D _hitCollider;
        private Rigidbody2D _rigidbody2D;
        private TrailRenderer _trail;
        private SpriteRenderer[] _renderers;
        private Vector3 _baseScale = Vector3.one * DefaultVisualScale;
        private bool _despawnQueued;

        private void Awake()
        {
            _hitCollider = GetComponentInChildren<Collider2D>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _trail = GetComponent<TrailRenderer>();
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            EnsureTrailRenderer();
        }
        
        public void Initialize(float damage, float lifetime)
        {
            Initialize(damage, lifetime, string.Empty, "BossProjectile");
        }

        public void Initialize(float damage, float lifetime, string bossId, string damageSource)
        {
            _damage = damage;
            _lifetime = lifetime;
            _timer = 0f;
            _hasHit = false;
            _despawnQueued = false;
            _armingDelay = DefaultArmingDelay;
            _bossId = bossId ?? string.Empty;
            _damageSource = string.IsNullOrEmpty(damageSource) ? "BossProjectile" : damageSource;
            if (_hitCollider) _hitCollider.enabled = true;
            if (_rigidbody2D) _rigidbody2D.simulated = true;
            ApplyVisualPreset();
            UpdatePulseVisual();
        }
        
        private void Update()
        {
            if (_despawnQueued) return;
            _timer += Time.deltaTime;
            UpdatePulseVisual();
            if (_timer >= _lifetime)
            {
                DespawnSelf();
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasHit || _timer < _armingDelay) return;

            // 检查是否击中玩家
            if (other.GetComponentInParent<Player>() != null)
            {
                _hasHit = true;
                if (_hitCollider) _hitCollider.enabled = false;
                if (_rigidbody2D) _rigidbody2D.velocity = Vector2.zero;

                var player = Player.Default;
                if (player)
                {
                    player.ApplyDamage(Mathf.Max(1, Mathf.CeilToInt(_damage)), _bossId, _damageSource);
                }
                
                // 命中后回收到对象池
                DespawnSelf();
            }
        }

        public void OnSpawned()
        {
            _timer = 0f;
            _hasHit = false;
            _despawnQueued = false;
            _armingDelay = DefaultArmingDelay;
            if (_hitCollider) _hitCollider.enabled = true;
            if (_rigidbody2D)
            {
                _rigidbody2D.simulated = true;
                _rigidbody2D.velocity = Vector2.zero;
                _rigidbody2D.angularVelocity = 0f;
            }

            EnsureTrailRenderer();
            if (_trail) _trail.Clear();
        }

        public void OnDespawned()
        {
            _hasHit = false;
            _despawnQueued = true;
            _timer = 0f;

            if (_hitCollider) _hitCollider.enabled = false;
            if (_rigidbody2D)
            {
                _rigidbody2D.velocity = Vector2.zero;
                _rigidbody2D.angularVelocity = 0f;
                _rigidbody2D.simulated = false;
            }

            if (_trail)
            {
                _trail.Clear();
            }
        }

        private void EnsureTrailRenderer()
        {
            if (!_trail)
            {
                _trail = gameObject.AddComponent<TrailRenderer>();
            }

            _trail.time = 0.18f;
            _trail.minVertexDistance = 0.02f;
            _trail.startWidth = 0.22f;
            _trail.endWidth = 0f;
            _trail.autodestruct = false;
            _trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _trail.receiveShadows = false;
            _trail.sortingLayerName = TargetSortingLayer;
            _trail.sortingOrder = TargetSortingOrder - 1;

            if (_trail.sharedMaterial == null)
            {
                if (!s_trailMaterial)
                {
                    s_trailMaterial = new Material(Shader.Find("Sprites/Default"));
                }

                _trail.sharedMaterial = s_trailMaterial;
            }
        }

        private void ApplyVisualPreset()
        {
            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            }

            var hotColor = ChooseBaseColorByBossType();
            for (var i = 0; i < _renderers.Length; i++)
            {
                var sr = _renderers[i];
                if (!sr) continue;
                sr.sortingLayerName = TargetSortingLayer;
                sr.sortingOrder = TargetSortingOrder;
                sr.color = hotColor;
            }

            _baseScale = Vector3.one * Mathf.Clamp(DefaultVisualScale, 0.45f, 0.55f);
            transform.localScale = _baseScale;

            if (_trail)
            {
                _trail.startColor = new Color(hotColor.r, hotColor.g, hotColor.b, 0.92f);
                _trail.endColor = new Color(hotColor.r, hotColor.g, hotColor.b, 0.04f);
            }
        }

        private Color ChooseBaseColorByBossType()
        {
            if (_bossId.IndexOf("Shooter", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new Color(1f, 0.42f, 0.1f, 0.98f);
            }

            return new Color(0.2f, 0.95f, 1f, 0.98f);
        }

        private void UpdatePulseVisual()
        {
            var basePulse = _timer < _armingDelay ? 0.16f : 0.08f;
            var pulse = 1f + basePulse * Mathf.Sin(_timer * 24f);
            transform.localScale = _baseScale * pulse;
        }

        private void DespawnSelf()
        {
            if (_despawnQueued) return;
            _despawnQueued = true;
            ObjectPoolSystem.Despawn(gameObject);
        }
    }
}
