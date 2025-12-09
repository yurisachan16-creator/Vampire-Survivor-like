using UnityEngine;
using QFramework;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace VampireSurvivorLike
{
	public partial class RotateSword : ViewController
	{
        private List<Collider2D> _mSwords = new List<Collider2D>();
        void Start()
        {
            
            Sword.Hide();

            Global.RotateSwordCount.RegisterWithInitValue(count =>
            {
                var toAddCount = count - _mSwords.Count;

                for(var i = 0; i < toAddCount; i++)
                {
                    _mSwords.Add(Sword.InstantiateWithParent(this)
                    .Self(self =>
                    {
                        self.OnTriggerEnter2DEvent(collider=>
                        {
                            var hurtBox=collider.GetComponent<HurtBox>();
                            if (hurtBox)
                            {
                                if(hurtBox.Owner.CompareTag("Enemy"))
                                {
                                    hurtBox.Owner.GetComponent<Enemy>().Hurt(Global.RotateSwordDamage.Value);

                                    //有50%的概率对敌人进行击退
                                    if (Random.Range(0, 1.0f) < 0.5f)
                                    {
                                        collider.attachedRigidbody.velocity =
                                            collider.NormalizedDirection2DFrom(self) * 5 +
                                            collider.NormalizedDirection2DFrom(Player.Default) * 10;
                                    }
                                }
                            }

                        }).UnRegisterWhenGameObjectDestroyed(self);

                    })
                    .Show());
                                
                }
                
                UpdateCirclePos();

            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            Global.RotateSwordRange.Register((range) =>
            {
                UpdateCirclePos();
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            
        }

        void UpdateCirclePos()
        {
            var radius = Global.RotateSwordRange.Value;
            var durationDegrees = 360 / _mSwords.Count;

            for(var i = 0; i < _mSwords.Count; i++)
            {
                var circleLocalPos = new Vector2(Mathf.Cos(i * durationDegrees * Mathf.Deg2Rad) * radius
                                            , Mathf.Sin(i * durationDegrees * Mathf.Deg2Rad) * radius);

                _mSwords[i].LocalPosition(circleLocalPos.x, circleLocalPos.y)
                            .LocalEulerAnglesZ(durationDegrees * i - 90);
            }
          
        }

        void Update()
        {
            
			var degree = Time.frameCount * Global.RotateSwordSpeed.Value;

            this.LocalEulerAnglesZ(-degree);
			
        }
    }
}
