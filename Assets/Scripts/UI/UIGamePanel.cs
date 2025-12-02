using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
	public class UIGamePanelData : UIPanelData
	{
	}
	public partial class UIGamePanel : UIPanel
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGamePanelData ?? new UIGamePanelData();
			// please add init code here

			Global.HP.RegisterWithInitValue((hp) =>
			{
				HPText.text = "生命值:" + Global.HP.Value + "/"+Global.MaxHP.Value;
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			Global.MaxHP.RegisterWithInitValue((hp) =>
			{
				HPText.text = "生命值:" + Global.MaxHP.Value + "/"+Global.MaxHP.Value;
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			EnemyGenerator.EnemyCount.RegisterWithInitValue(EnemyCount =>
			{
				EnemyCountText.text = "敌人数量:" + EnemyCount;
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			///时间变化处理
			Global.CurrentSeconds.RegisterWithInitValue((currentSeconds) =>
			{
				if(Time.frameCount %30 == 0)
				{
					var secondsInt = Mathf.FloorToInt(currentSeconds);
					var seconds = secondsInt % 60;
					var minutes = secondsInt / 60;
					TimeText.text = "时间:" + $"{minutes:00}:{seconds:00}";
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			///经验值变化处理
			Global.Exp.RegisterWithInitValue((exp) =>
            {
                ExpText.text = "经验值:(" + exp + "/" + Global.ExpToNextLevel() + ")";
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

			///等级提升处理
			Global.Level.RegisterWithInitValue((level) =>
			{
				LevelText.text = "等级:" + level;
				
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			///等级提升处理
			Global.Level.RegisterWithInitValue((level) =>
			{
				//暂停游戏
				Time.timeScale = 0f;
				
				//显示升级按钮
				UpgradeRoot.Show();
				//升级音效
				AudioKit.PlaySound("LevelUp");
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			///经验值转等级处理
			Global.Exp.RegisterWithInitValue((exp) =>
			{
                if (exp >= Global.ExpToNextLevel())
                {
                    Global.Exp.Value -= Global.ExpToNextLevel();
					Global.Level.Value += 1;
                }
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			//UpgradeRoot.Hide();

			//简单攻击伤害升级按钮点击事件
			BtnUpgrade.onClick.AddListener(()=>
			{
				//恢复游戏
				Time.timeScale = 1f;
				//提升简单攻击伤害
				Global.SimpleAbilityDamage.Value *= 1.5f;
				//隐藏升级按钮
				UpgradeRoot.Hide();
				//TODO:播放升级音效
				AudioKit.PlaySound("");
			});

			//简单攻击间隔时间升级按钮点击事件
			BtnSimpleDurationUpgrade.onClick.AddListener(()=>
			{
				//恢复游戏
				Time.timeScale = 1f;
				//缩短简单攻击间隔时间
				Global.SimpleAbilityDuration.Value *= 0.8f;
				//隐藏升级按钮
				UpgradeRoot.Hide();
				//TODO:播放升级音效
				AudioKit.PlaySound("");
			});

			var enemyGenerator=FindObjectOfType<EnemyGenerator>();
			///游戏时间流逝处理
			ActionKit.OnUpdate.Register(()=>
			{
				Global.CurrentSeconds.Value += Time.deltaTime;
				//敌人全部死亡，通关
				if(enemyGenerator.IsLastWave && EnemyGenerator.EnemyCount.Value == 0 && enemyGenerator.CurrentWave==null)
				{
					//关闭当前面板
					this.CloseSelf();
					//打开通关面板
					UIKit.OpenPanel<UIGamePassPanel>();
				}

			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			

			
			Global.Coin.RegisterWithInitValue((coin) =>
			{
				

				CoinText.text = "金币:" + coin;

			}).UnRegisterWhenGameObjectDestroyed(gameObject);
		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
		}
		
		protected override void OnShow()
		{
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
		}
	}
}
