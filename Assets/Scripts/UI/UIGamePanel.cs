using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
			LocalizationManager.PreloadTable("game");
			if (!FindObjectOfType<EventSystem>())
			{
				new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
			}

			var tipTransform = transform.Find("Tip");
			if (tipTransform)
			{
				var tooltipView = tipTransform.GetComponent<UITooltipView>();
				if (!tooltipView) tooltipView = tipTransform.gameObject.AddComponent<UITooltipView>();
				tooltipView.Hide();
			}

			var overlayGo = new GameObject("LootGuideOverlay", typeof(RectTransform), typeof(CanvasGroup));
			var overlayRt = (RectTransform)overlayGo.transform;
			overlayRt.SetParent(transform, false);
			overlayRt.anchorMin = Vector2.zero;
			overlayRt.anchorMax = Vector2.one;
			overlayRt.offsetMin = Vector2.zero;
			overlayRt.offsetMax = Vector2.zero;
			overlayRt.pivot = new Vector2(0.5f, 0.5f);
			var cg = overlayGo.GetComponent<CanvasGroup>();
			cg.blocksRaycasts = false;
			cg.interactable = false;

			var lootGuideSystem = gameObject.GetComponent<LootGuideSystem>() ?? gameObject.AddComponent<LootGuideSystem>();
			var canvas = GetComponentInParent<Canvas>();
			lootGuideSystem.Initialize(overlayRt, canvas, Camera.main, Player.Default ? Player.Default.transform : null);
			

			EnemyGenerator.EnemyCount.RegisterWithInitValue(EnemyCount =>
			{
				EnemyCountText.text = LocalizationManager.Format("game.ui.enemy_count", EnemyCount);
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			// 波次信息显示
			EnemyGenerator.CurrentWaveIndex.RegisterWithInitValue(_ =>
			{
				EnemyWaveCountText.text = LocalizationManager.Format("game.ui.wave", EnemyGenerator.CurrentWaveIndex.Value, EnemyGenerator.TotalWaveCount.Value);
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			EnemyGenerator.TotalWaveCount.RegisterWithInitValue(_ =>
			{
				EnemyWaveCountText.text = LocalizationManager.Format("game.ui.wave", EnemyGenerator.CurrentWaveIndex.Value, EnemyGenerator.TotalWaveCount.Value);
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			// 波次剩余时间显示
			EnemyGenerator.WaveRemainingTime.RegisterWithInitValue(_ =>
			{
				if (string.IsNullOrEmpty(EnemyGenerator.CurrentWaveName.Value))
				{
					EnemyCountNextTimeText.text = LocalizationManager.T("game.ui.wait_next_wave");
				}
				else
				{
					var remaining = Mathf.CeilToInt(EnemyGenerator.WaveRemainingTime.Value);
					EnemyCountNextTimeText.text = LocalizationManager.Format("game.ui.wave_remaining", EnemyGenerator.CurrentWaveName.Value, remaining);
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			EnemyGenerator.CurrentWaveName.RegisterWithInitValue(_ =>
			{
				if (string.IsNullOrEmpty(EnemyGenerator.CurrentWaveName.Value))
				{
					EnemyCountNextTimeText.text = LocalizationManager.T("game.ui.wait_next_wave");
				}
				else
				{
					var remaining = Mathf.CeilToInt(EnemyGenerator.WaveRemainingTime.Value);
					EnemyCountNextTimeText.text = LocalizationManager.Format("game.ui.wave_remaining", EnemyGenerator.CurrentWaveName.Value, remaining);
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			///时间变化处理
			Global.CurrentSeconds.RegisterWithInitValue((currentSeconds) =>
			{
				if(Time.frameCount %30 == 0)
				{
					var secondsInt = Mathf.FloorToInt(currentSeconds);
					var seconds = secondsInt % 60;
					var minutes = secondsInt / 60;
					TimeText.text = LocalizationManager.Format("game.ui.time", $"{minutes:00}:{seconds:00}");
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
				LevelText.text = LocalizationManager.Format("game.ui.level", level);
				
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			//默认升级界面隐藏
			ExpUpgradePanel.Hide();
			///等级提升处理：监听升级系统事件，只有在有可选项时才显示面板
			ExpUpgradeSystem.OnUpgradePanelShouldShow.Register((hasItems) =>
			{
				if (hasItems)
				{
					//暂停游戏
					Time.timeScale = 0f;
					//显示升级面板
					ExpUpgradePanel.Show();
					//升级音效
					AudioKit.PlaySound("LevelUp");
				}
				else
				{
					//没有可升级项目，给予补偿奖励
					Global.Coin.Value += 100;
					AudioKit.PlaySound("Retro Event Acute 08");
				}
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
				if(enemyGenerator.IsAllWavesFinished && EnemyGenerator.EnemyCount.Value == 0)
				{
					//关闭当前面板
					this.CloseSelf();
					//打开通关面板
					UIKit.OpenPanel<UIGamePassPanel>();
				}

			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			

			
			Global.Coin.RegisterWithInitValue((coin) =>
			{
				

				CoinText.text = LocalizationManager.Format("game.ui.coin", coin);

			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			LocalizationManager.CurrentLanguage.Register(_ =>
			{
				EnemyCountText.text = LocalizationManager.Format("game.ui.enemy_count", EnemyGenerator.EnemyCount.Value);
				EnemyWaveCountText.text = LocalizationManager.Format("game.ui.wave", EnemyGenerator.CurrentWaveIndex.Value, EnemyGenerator.TotalWaveCount.Value);
				if (string.IsNullOrEmpty(EnemyGenerator.CurrentWaveName.Value))
				{
					EnemyCountNextTimeText.text = LocalizationManager.T("game.ui.wait_next_wave");
				}
				else
				{
					var remaining = Mathf.CeilToInt(EnemyGenerator.WaveRemainingTime.Value);
					EnemyCountNextTimeText.text = LocalizationManager.Format("game.ui.wave_remaining", EnemyGenerator.CurrentWaveName.Value, remaining);
				}

				var secondsInt = Mathf.FloorToInt(Global.CurrentSeconds.Value);
				var seconds = secondsInt % 60;
				var minutes = secondsInt / 60;
				TimeText.text = LocalizationManager.Format("game.ui.time", $"{minutes:00}:{seconds:00}");

				LevelText.text = LocalizationManager.Format("game.ui.level", Global.Level.Value);
				CoinText.text = LocalizationManager.Format("game.ui.coin", Global.Coin.Value);
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
