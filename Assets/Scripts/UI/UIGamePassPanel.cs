using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.SceneManagement;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public class UIGamePassPanelData : UIPanelData
	{
	}
	public partial class UIGamePassPanel : UIPanel
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGamePassPanelData ?? new UIGamePassPanelData();
			//暂停游戏
			Time.timeScale = 0f;
			// please add init code here

			var titleText = transform.Find("Title")?.GetComponent<Text>();
			if (titleText) FontManager.Register(titleText);
			var backLabel = BtnBackToStart ? BtnBackToStart.GetComponentInChildren<Text>(true) : null;
			if (backLabel) FontManager.Register(backLabel);
			var difficultySummaryText = EnsureDifficultySummaryText();
			if (difficultySummaryText) FontManager.Register(difficultySummaryText);

			System.Action refreshUiText = () =>
			{
				if (!LocalizationManager.IsReady) return;
				if (titleText) titleText.text = LocalizationManager.T("ui.gamepass.title");
				if (backLabel) backLabel.text = LocalizationManager.T("ui.settings.return_main_menu");
				if (difficultySummaryText) difficultySummaryText.text = BuildDifficultySummaryText();
			};
			LocalizationManager.ReadyChanged.Register(() => refreshUiText()).UnRegisterWhenGameObjectDestroyed(gameObject);
			LocalizationManager.CurrentLanguage.Register(_ => refreshUiText()).UnRegisterWhenGameObjectDestroyed(gameObject);
			refreshUiText();

			//移除按空格键重玩功能
			// ActionKit.OnUpdate.Register(()=>
            // {
            //     if(Input.GetKeyDown(KeyCode.Space))
			// 	{
			// 		//重置数据
			// 		Global.ResetData();
			// 		//关闭当前面板
			// 		this.CloseSelf();
			// 		//重新加载场景
			// 		SceneManager.LoadScene("Game");
			// 	}
            // }).UnRegisterWhenGameObjectDestroyed(gameObject);

			BtnBackToStart.onClick.AddListener(() =>
            {
				//播放音效
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
				GameSettings.ClearActiveRunDifficulty();
                this.CloseSelf();
				SceneManager.LoadScene("GameStart");
            });

			//通关音效
			AudioKit.PlaySound("Retro Event Acute 11");
		}

		private Text EnsureDifficultySummaryText()
		{
			var existing = transform.Find("DifficultySummary")?.GetComponent<Text>();
			if (existing) return existing;

			var go = new GameObject("DifficultySummary", typeof(RectTransform), typeof(Text));
			var rt = go.GetComponent<RectTransform>();
			rt.SetParent(transform, false);
			rt.anchorMin = new Vector2(0.5f, 1f);
			rt.anchorMax = new Vector2(0.5f, 1f);
			rt.pivot = new Vector2(0.5f, 1f);
			rt.sizeDelta = new Vector2(900f, 56f);
			rt.anchoredPosition = new Vector2(0f, -140f);

			var text = go.GetComponent<Text>();
			var fallbackFont = GetBuiltinFallbackFont();
			if (fallbackFont) text.font = fallbackFont;
			text.fontSize = 24;
			text.alignment = TextAnchor.MiddleCenter;
			text.horizontalOverflow = HorizontalWrapMode.Wrap;
			text.verticalOverflow = VerticalWrapMode.Truncate;
			text.color = new Color(1f, 0.95f, 0.75f, 1f);
			text.raycastTarget = false;
			return text;
		}

		private static Font GetBuiltinFallbackFont()
		{
			try
			{
				return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			}
			catch
			{
				try
				{
					return Resources.GetBuiltinResource<Font>("Arial.ttf");
				}
				catch
				{
					return null;
				}
			}
		}

		private static string BuildDifficultySummaryText()
		{
			var difficulty = GameSettings.ActiveRunDifficulty;
			var profile = GameSettings.GetActiveRunProfile();
			var difficultyName = LocalizationManager.T(GameSettings.GetDifficultyLocalizationKey(difficulty));
			var label = LocalizationManager.Format("ui.result.difficulty_label", difficultyName);
			var enemyModifier = LocalizationManager.Format(
				"ui.result.enemy_strength_modifier",
				FormatSignedPercent(GameSettings.GetEnemyStrengthDeltaPercent(profile)));
			var rewardModifier = LocalizationManager.Format(
				"ui.result.reward_modifier",
				FormatSignedPercent(GameSettings.GetRewardDeltaPercent(profile)));
			return LocalizationManager.Format("ui.result.difficulty_summary", label, enemyModifier, rewardModifier);
		}

		private static string FormatSignedPercent(float percent)
		{
			var rounded = Mathf.RoundToInt(percent);
			if (rounded == 0) return "0%";
			return rounded > 0 ? $"+{rounded}%" : $"{rounded}%";
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
			//恢复游戏
			Time.timeScale = 1f;
        }
	}
}
