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
		public static EasyEvent FlashScreen = new EasyEvent(); //屏幕闪烁事件
		public static EasyEvent OpenTreasureChestPanel = new EasyEvent(); //打开宝箱面板事件
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGamePanelData ?? new UIGamePanelData();
			// please add init code here

			

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
				//更新经验值显示，使用ExpValue的fillAmount来显示经验值进度
				ExpValue.fillAmount = exp / (float)Global.ExpToNextLevel();
                // ExpText.text = "经验值:(" + exp + "/" + Global.ExpToNextLevel() + ")";
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

			///等级提升处理
			Global.Level.RegisterWithInitValue((level) =>
			{
				LevelText.text = "等级:" + level;
				
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			//默认升级界面隐藏
			ExpUpgradePanel.Hide();
			///等级提升处理（只在等级变化时触发，不在初始化时触发）
			Global.Level.Register((level) =>
			{
				//暂停游戏
				Time.timeScale = 0f;
				
				//显示升级面板
				ExpUpgradePanel.Show();
				//升级音效
				AudioKit.PlaySound("LevelUp");
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			///经验值转等级处理
			Global.Exp.RegisterWithInitValue((exp) =>
			{
                while (exp >= Global.ExpToNextLevel())
                {
                    Global.Exp.Value -= Global.ExpToNextLevel();
					Global.Level.Value += 1;
					exp = Global.Exp.Value;
                }
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			FlashScreen.Register(()=>
			{
				//屏幕闪烁效果
				ActionKit
				.Sequence()
				.Lerp(0,0.5f,0.1f
					,alpha=>ScreenColor.ColorAlpha(alpha))
				.Lerp(0.5f,0f,0.1f
					,alpha=>ScreenColor.ColorAlpha(alpha))
				.Start(this);
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

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

			OpenTreasureChestPanel.Register(()=>
			{
				Time.timeScale = 0f;
				TreasureChestPanel.Show();
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
