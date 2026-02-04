using UnityEngine;
using QFramework;
using System.Collections.Generic;

namespace VampireSurvivorLike
{
    /// <summary>
    /// Boss类型枚举 - 每种类型对应不同的技能组合
    /// </summary>
    public enum BossType
    {
        /// <summary>
        /// 冲刺型Boss - 擅长高速冲刺攻击
        /// </summary>
        Dasher,
        
        /// <summary>
        /// 弹幕型Boss - 发射环形弹幕
        /// </summary>
        Shooter,
        
        /// <summary>
        /// 召唤型Boss - 召唤小怪援助
        /// </summary>
        Summoner,
        
        /// <summary>
        /// 狂战士型Boss - 旋转攻击 + 冲刺
        /// </summary>
        Berserker,
        
        /// <summary>
        /// 混合型Boss - 拥有多种技能
        /// </summary>
        Hybrid
    }
    
    /// <summary>
    /// MiniBoss - 可配置不同技能组合的Boss敌人
    /// </summary>
    public partial class EnemyMiniBoss : ViewController, IEnemy
    {
        [Header("Boss基础属性（可被EnemyStatsConfig覆盖）")]
        [Tooltip("Boss血量 - 建议设置为200+以增加挑战性")]
        public float Health = 200f;
        
        [Tooltip("移动速度")]
        public float MovementSpeed = 1.5f;
        
        [Tooltip("伤害倍率")]
        public float DamageMultiplier = 1f;
        
        [Tooltip("是否掉落宝箱（设为false则使用普通掉落率）")]
        public bool DropTreasureChest = true;
        
        [Tooltip("宝箱掉落概率（仅当DropTreasureChest为true时有效）")]
        [Range(0f, 1f)]
        public float TreasureChestDropRate = 0.3f;
        
        [Header("配置来源")]
        [Tooltip("是否从EnemyStatsConfig读取属性，如果为false则使用预制体上的值")]
        public bool UseStatsConfig = true;
        
        [Header("Boss类型与技能")]
        [Tooltip("Boss类型决定技能组合")]
        public BossType BossType = BossType.Dasher;
        
        [Header("技能预制体（根据Boss类型配置）")]
        [Tooltip("弹幕预制体 - 用于Shooter类型")]
        public GameObject BossProjectilePrefab;
        
        [Tooltip("小怪预制体 - 用于Summoner类型")]
        public GameObject MinionPrefab;
        
        // 状态机 - 简化为基础状态
        public enum States
        {
            Idle,           // 待机/追踪玩家
            ExecutingSkill  // 执行技能中
        }
        
        public FSM<States> FSM = new FSM<States>();
        
        // 技能列表
        private List<IBossSkill> _skills = new List<IBossSkill>();
        private IBossSkill _currentSkill;
        private float _initialHealth;
        
        void Start()
        {
            // 从配置中读取属性
            if (UseStatsConfig)
            {
                LoadStatsFromConfig();
            }
            
            EnemyGenerator.EnemyCount.Value++;
            _initialHealth = Health;
            
            // 根据Boss类型初始化技能
            InitializeSkills();
            
            // 设置状态机
            SetupFSM();
            
            FSM.StartState(States.Idle);
        }
        
        /// <summary>
        /// 从EnemyStatsConfig加载属性
        /// </summary>
        private void LoadStatsFromConfig()
        {
            var config = EnemyStatsConfig.Instance;
            if (config == null) return;
            
            var stats = config.GetStats(gameObject.name.Replace("(Clone)", "").Trim());
            if (stats == null) return;
            
            Health = stats.BaseHP;
            MovementSpeed = stats.BaseSpeed;
            DamageMultiplier = stats.BaseDamageMultiplier;
        }
        
        /// <summary>
        /// 根据BossType初始化技能组合
        /// </summary>
        private void InitializeSkills()
        {
            _skills.Clear();
            
            switch (BossType)
            {
                case BossType.Dasher:
                    // 冲刺型：强化冲刺 + 快速冲刺
                    _skills.Add(new DashSkill(cooldown: 4f, warningDuration: 1.2f, dashSpeedMultiplier: 15f, triggerDistance: 18f));
                    _skills.Add(new DashSkill(cooldown: 2f, warningDuration: 0.5f, dashSpeedMultiplier: 20f, triggerDistance: 10f));
                    break;
                    
                case BossType.Shooter:
                    // 弹幕型：多波弹幕
                    _skills.Add(new AreaAttackSkill(cooldown: 5f, projectileCount: 8, waveCount: 2, triggerDistance: 15f));
                    _skills.Add(new AreaAttackSkill(cooldown: 8f, projectileCount: 12, waveCount: 3, triggerDistance: 20f));
                    break;
                    
                case BossType.Summoner:
                    // 召唤型：召唤 + 防御性冲刺
                    _skills.Add(new SummonSkill(cooldown: 10f, summonCount: 4, triggerHPPercent: 0.8f));
                    _skills.Add(new SummonSkill(cooldown: 15f, summonCount: 6, triggerHPPercent: 0.4f));
                    _skills.Add(new DashSkill(cooldown: 6f, warningDuration: 1f, dashSpeedMultiplier: 10f, triggerDistance: 8f));
                    break;
                    
                case BossType.Berserker:
                    // 狂战士型：旋转攻击 + 冲刺
                    _skills.Add(new SpinAttackSkill(cooldown: 5f, spinDuration: 2.5f, triggerDistance: 10f));
                    _skills.Add(new DashSkill(cooldown: 4f, warningDuration: 0.8f, dashSpeedMultiplier: 12f, triggerDistance: 15f));
                    break;
                    
                case BossType.Hybrid:
                    // 混合型：所有技能
                    _skills.Add(new DashSkill(cooldown: 5f, warningDuration: 1.5f, dashSpeedMultiplier: 12f, triggerDistance: 15f));
                    _skills.Add(new AreaAttackSkill(cooldown: 8f, projectileCount: 6, waveCount: 2, triggerDistance: 12f));
                    _skills.Add(new SummonSkill(cooldown: 12f, summonCount: 3, triggerHPPercent: 0.5f));
                    _skills.Add(new SpinAttackSkill(cooldown: 7f, spinDuration: 2f, triggerDistance: 8f));
                    break;
            }
            
            // 初始化所有技能
            foreach (var skill in _skills)
            {
                skill.Initialize(this);
            }
        }
        
        private void SetupFSM()
        {
            FSM.State(States.Idle)
                .OnFixedUpdate(() =>
                {
                    if (Player.Default)
                    {
                        // 追踪玩家
                        var direction = (Player.Default.transform.position - transform.position).normalized;
                        SelfRigidbody2D.velocity = direction * MovementSpeed;
                    }
                    else
                    {
                        SelfRigidbody2D.velocity = Vector2.zero;
                    }
                })
                .OnUpdate(() =>
                {
                    // 尝试触发技能
                    TryTriggerSkill();
                });
            
            FSM.State(States.ExecutingSkill)
                .OnEnter(() =>
                {
                    SelfRigidbody2D.velocity = Vector2.zero;
                })
                .OnUpdate(() =>
                {
                    if (_currentSkill != null)
                    {
                        _currentSkill.OnUpdate();
                        
                        // 技能执行完毕，返回待机状态
                        if (!_currentSkill.IsExecuting)
                        {
                            _currentSkill = null;
                            FSM.ChangeState(States.Idle);
                        }
                    }
                    else
                    {
                        FSM.ChangeState(States.Idle);
                    }
                })
                .OnFixedUpdate(() =>
                {
                    _currentSkill?.OnFixedUpdate();
                });
        }
        
        private void TryTriggerSkill()
        {
            if (!Player.Default) return;
            
            float distanceToPlayer = Vector2.Distance(transform.position, Player.Default.transform.position);
            float hpPercent = Health / _initialHealth;
            
            // 遍历技能，找到可以触发的技能
            foreach (var skill in _skills)
            {
                if (!skill.IsReady) continue;
                
                bool shouldTrigger = false;
                
                // 根据技能类型检查触发条件
                if (skill is DashSkill dashSkill)
                {
                    shouldTrigger = distanceToPlayer <= dashSkill.TriggerDistance;
                }
                else if (skill is AreaAttackSkill areaSkill)
                {
                    shouldTrigger = distanceToPlayer <= areaSkill.TriggerDistance;
                }
                else if (skill is SummonSkill summonSkill)
                {
                    shouldTrigger = summonSkill.ShouldTrigger();
                }
                else if (skill is SpinAttackSkill spinSkill)
                {
                    shouldTrigger = distanceToPlayer <= spinSkill.TriggerDistance;
                }
                
                if (shouldTrigger && skill.TryExecute())
                {
                    _currentSkill = skill;
                    FSM.ChangeState(States.ExecutingSkill);
                    return;
                }
            }
        }
        
        void Update()
        {
            FSM.Update();
            
            // 更新所有技能冷却
            foreach (var skill in _skills)
            {
                if (skill != _currentSkill)
                {
                    skill.OnUpdate();
                }
            }
            
            // 死亡检查
            if (Health <= 0)
            {
                OnDeath();
            }
        }
        
        void FixedUpdate()
        {
            FSM.FixedUpdate();
        }
        
        private void OnDeath()
        {
            // 根据配置决定掉落
            if (DropTreasureChest)
            {
                // 概率掉落宝箱
                if (Random.value <= TreasureChestDropRate)
                {
                    Global.GeneratePowerUp(gameObject, true);
                }
                else
                {
                    // 掉落普通道具
                    Global.GeneratePowerUpWithRates(gameObject, false, 0.8f, 0.5f, 0.2f, 0.1f);
                }
            }
            else
            {
                // 使用普通掉落率（较高概率）
                Global.GeneratePowerUpWithRates(gameObject, false, 0.8f, 0.5f, 0.2f, 0.1f);
            }
            
            AudioKit.PlaySound("BossDie");
            this.DestroyGameObjGracefully();
        }
        
        void OnDestroy()
        {
            EnemyGenerator.EnemyCount.Value--;
        }
        
        #region IEnemy Implementation
        
        private bool _isIgnoreHurt = false;
        
        public void Hurt(float value, bool force = false, bool critical = false)
        {
            if (_isIgnoreHurt && !force) return;
            
            _isIgnoreHurt = true;
            
            // 显示伤害数字
            FloatingTextController.Play(transform.position + Vector3.up * 0.5f, value.ToString("0"), critical);
            
            Sprite.color = Color.red;
            AudioKit.PlaySound("Hit");
            
            // 延时后扣血并恢复
            ActionKit.Delay(0.2f, () =>
            {
                this.Health -= value;
                this.Sprite.color = Color.white;
                _isIgnoreHurt = false;
            }).Start(this);
        }
        
        public void SetSpeedScale(float speedScale)
        {
            MovementSpeed *= speedScale;
        }
        
        public void SetHPScale(float hpScale)
        {
            Health *= hpScale;
            _initialHealth = Health;
        }
        
        public void SetDamageScale(float damageScale)
        {
            DamageMultiplier *= damageScale;
        }
        
        public void SetBaseSpeed(float baseSpeed)
        {
            MovementSpeed = baseSpeed;
        }
        
        public void SetDropRates(float expRate, float coinRate, float hpRate, float bombRate)
        {
            // Boss使用自己的掉落逻辑
        }
        
        public void SetTreasureChest(bool isTreasureChest)
        {
            DropTreasureChest = isTreasureChest;
        }
        
        #endregion
    }
}
