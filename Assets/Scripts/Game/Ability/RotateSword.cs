using UnityEngine;
using QFramework;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace VampireSurvivorLike
{
	public partial class RotateSword : ViewController
	{
        private const int SameTargetHitCooldownFrames = 6;
        private List<Collider2D> _mSwords = new List<Collider2D>();
        private readonly Dictionary<int, int> _lastHitFrameByEnemy = new Dictionary<int, int>(64);

        
        void Start()
        {
            
            Sword.Hide();

            void CreateSword()
            {
                _mSwords.Add(Sword.InstantiateWithParent(this)
                    .Self(self =>
                    {
                        CombatLayerSettings.ApplyPlayerAttackLayer(self.gameObject);
                        self.OnTriggerEnter2DEvent(collider=>
                        {
                            if (!collider.TryGetComponent<HitHurtBox>(out var hitHurtBox)) return;
                            if (!hitHurtBox.IsEnemyOwner) return;
                            if (!hitHurtBox.TryGetEnemy(out var enemy)) return;
                            if (!hitHurtBox.Owner) return;

                            var enemyId = hitHurtBox.Owner.GetInstanceID();
                            if (_lastHitFrameByEnemy.TryGetValue(enemyId, out var lastFrame) &&
                                Time.frameCount - lastFrame < SameTargetHitCooldownFrames)
                            {
                                return;
                            }

                            _lastHitFrameByEnemy[enemyId] = Time.frameCount;

                            var damageTimes = Global.SuperRotateSword.Value ? Random.Range(2, 4) : 1;
                            DamageSystem.CalculateDamage(Global.RotateSwordDamage.Value * damageTimes, enemy);

                            //有50%的概率对敌人进行击退
                            if (Random.Range(0, 1.0f) < 0.5f && Player.Default)
                            {
                                var knockbackDirection = collider.NormalizedDirection2DFrom(self);
                                var playerDirection = collider.NormalizedDirection2DFrom(Player.Default);
                                var combinedDirection = (knockbackDirection + playerDirection).normalized;
                                if (combinedDirection.sqrMagnitude <= 0.0001f)
                                {
                                    combinedDirection = knockbackDirection.sqrMagnitude > 0.0001f
                                        ? knockbackDirection
                                        : playerDirection;
                                }

                                enemy.ApplyExternalKnockback(combinedDirection, 6f, 0.14f);
                            }

                        }).UnRegisterWhenGameObjectDestroyed(self);

                    })
                    .Show());
            }

            void CreateSwords()
            {
                var toAddCount = Global.RotateSwordCount.Value + Global.AdditionalFlyThingCount.Value - _mSwords.Count;

                for(var i = 0; i < toAddCount; i++)
                {
                    CreateSword();
                                
                }
                
                UpdateCirclePos();
            }

            Global.RotateSwordCount.Or(Global.AdditionalFlyThingCount).Register(() =>
            {
                CreateSwords();

            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            Global.RotateSwordRange.Register((range) =>
            {
                UpdateCirclePos();
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            CreateSwords();
        }

        void UpdateCirclePos()
        {
            var radius = Global.RotateSwordRange.Value * Mathf.Max(1f, Global.AreaMultiplier.Value);
            var durationDegrees = 360 / _mSwords.Count;

            for(var i = 0; i < _mSwords.Count; i++)
            {
                var circleLocalPos = new Vector2(Mathf.Cos(i * durationDegrees * Mathf.Deg2Rad) * radius
                                            , Mathf.Sin(i * durationDegrees * Mathf.Deg2Rad) * radius);

                _mSwords[i].LocalPosition(circleLocalPos.x, circleLocalPos.y)
                            .LocalEulerAnglesZ(durationDegrees * i - 90);
            }
          
        }

        private float _rotationAngle;

        void Update()
        {
            var speed = Global.SuperRotateSword.Value 
                ? 10f * 60f 
                : Global.RotateSwordSpeed.Value * 60f;

            _rotationAngle += speed * Time.deltaTime;
            this.LocalEulerAnglesZ(-_rotationAngle);
			
        }
    }
}
