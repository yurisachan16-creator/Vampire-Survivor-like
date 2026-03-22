using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.SceneManagement;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public class UIGameOverPanelData : UIPanelData
	{
	}
	public partial class UIGameOverPanel : UIPanel
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGameOverPanelData ?? new UIGameOverPanelData();
			NormalizeFullscreenRoot();
			if (Application.isMobilePlatform && !GetComponent<SafeAreaFitter>())
			{
				gameObject.AddComponent<SafeAreaFitter>();
			}
			ApplySafeAreaNow();

			var titleText = transform.Find("Title")?.GetComponent<Text>();
			if (titleText) FontManager.Register(titleText);
			var backLabel = BtnBackToStart ? BtnBackToStart.GetComponentInChildren<Text>(true) : null;
			if (backLabel) FontManager.Register(backLabel);
			var difficultySummaryText = EnsureDifficultySummaryText();
			if (difficultySummaryText) FontManager.Register(difficultySummaryText);

			System.Action refreshUiText = () =>
			{
				if (!LocalizationManager.IsReady) return;
				var defaultTitle = LocalizationManager.T("ui.gameover.title");
				if (titleText) titleText.text = WitnessModeRuntime.GetResultTitle(false, defaultTitle);
				if (backLabel) backLabel.text = LocalizationManager.T("ui.settings.return_main_menu");
				if (difficultySummaryText) difficultySummaryText.text = BuildDifficultySummaryText();
				if (BtnBackToStart) BtnBackToStart.gameObject.SetActive(!WitnessModeRuntime.ShouldAutoReturnFromResult());
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
				//恢复时间缩放
				Time.timeScale = 1f;
				//重置游戏数据（保留金币）
				Global.ResetData();
				GameSettings.ClearActiveRunDifficulty();
				this.CloseSelf();
				SceneManager.LoadScene("GameStart");
			});

			if (UIKit.GetPanel<UIGameLocalLeaderboardPanel>() == null)
			{
				try
				{
					UIKit.OpenPanel<UIGameLocalLeaderboardPanel>(
						UILevel.PopUI,
						prefabName: UIGameLocalLeaderboardPanel.ResourcesPrefabPath);
				}
				catch (System.Exception e)
				{
					Debug.LogError($"[UIGameOverPanel] Open leaderboard failed: {e}");
				}
			}
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
			var summary = LocalizationManager.Format("ui.result.difficulty_summary", label, enemyModifier, rewardModifier);
			var witnessPrefix = WitnessModeRuntime.GetResultSummaryPrefix();
			return string.IsNullOrEmpty(witnessPrefix) ? summary : $"{witnessPrefix}\n{summary}";
		}

		private static string FormatSignedPercent(float percent)
		{
			var rounded = Mathf.RoundToInt(percent);
			if (rounded == 0) return "0%";
			return rounded > 0 ? $"+{rounded}%" : $"{rounded}%";
		}

        
        protected override void OnOpen(IUIData uiData = null)
		{
			NormalizeFullscreenRoot();
			ApplySafeAreaNow();
		}
		
		protected override void OnShow()
		{
			NormalizeFullscreenRoot();
			ApplySafeAreaNow();
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
		}

		private void NormalizeFullscreenRoot()
		{
			var root = transform as RectTransform;
			if (!root) return;

			root.anchorMin = Vector2.zero;
			root.anchorMax = Vector2.one;
			root.offsetMin = Vector2.zero;
			root.offsetMax = Vector2.zero;
			root.anchoredPosition3D = Vector3.zero;
			root.localScale = Vector3.one;
		}

		private void ApplySafeAreaNow()
		{
			if (!Application.isMobilePlatform) return;
			var fitter = GetComponent<SafeAreaFitter>();
			if (!fitter) fitter = gameObject.AddComponent<SafeAreaFitter>();
			fitter.ForceApply();
		}
	}
}
