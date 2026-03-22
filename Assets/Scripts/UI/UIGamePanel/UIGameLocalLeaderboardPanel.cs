using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using QFramework;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public class UIGameLocalLeaderboardPanelData : UIPanelData
	{
	}

	public partial class UIGameLocalLeaderboardPanel : UIPanel
	{
		public const string ResourcesPrefabPath = "Resources/uigamelocalleaderboardpanel";

		private readonly List<GameObject> _spawnedRows = new List<GameObject>(32);
		private RectTransform _contentRoot;
		private Text _titleText;
		private Text _btnCloseText;
		private Text _btnReturnText;
		private Text _btnClearText;
		private bool _subscribedLeaderboardEvent;

		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGameLocalLeaderboardPanelData ?? new UIGameLocalLeaderboardPanelData();
			NormalizeFullscreenRoot();
			if (Application.isMobilePlatform && !GetComponent<SafeAreaFitter>())
			{
				gameObject.AddComponent<SafeAreaFitter>();
			}
			ApplySafeAreaNow();

			_contentRoot = FindChildRecursive(transform, "Content") as RectTransform;
			_titleText = FindChildRecursive(transform, "Title")?.GetComponent<Text>();
			_btnCloseText = BtnClose ? BtnClose.GetComponentInChildren<Text>(true) : null;
			_btnReturnText = BtnReturnToMainMenu ? BtnReturnToMainMenu.GetComponentInChildren<Text>(true) : null;
			_btnClearText = BtnQuit ? BtnQuit.GetComponentInChildren<Text>(true) : null;

			RegisterFont(_titleText);
			RegisterFont(_btnCloseText);
			RegisterFont(_btnReturnText);
			RegisterFont(_btnClearText);

			if (BtnClose)
			{
				BtnClose.onClick.RemoveAllListeners();
				BtnClose.onClick.AddListener(() =>
				{
					AudioKit.PlaySound(Sfx.BUTTONCLICK);
					CloseSelf();
				});
			}

			if (BtnReturnToMainMenu)
			{
				BtnReturnToMainMenu.onClick.RemoveAllListeners();
				BtnReturnToMainMenu.onClick.AddListener(() =>
				{
					AudioKit.PlaySound(Sfx.BUTTONCLICK);
					var scene = SceneManager.GetActiveScene().name;
					if (scene == "GameStart")
					{
						CloseSelf();
						return;
					}

					UIKit.CloseAllPanel();
					GameSettings.ClearActiveRunDifficulty();
					SceneManager.LoadScene("GameStart");
					Time.timeScale = 1f;
				});
			}

			if (BtnQuit)
			{
				BtnQuit.onClick.RemoveAllListeners();
				BtnQuit.onClick.AddListener(() =>
				{
					AudioKit.PlaySound(Sfx.BUTTONCLICK);
					LeaderboardSystem.ClearAll();
					RefreshRows();
				});
			}

			LocalizationManager.ReadyChanged.Register(RefreshTexts).UnRegisterWhenGameObjectDestroyed(gameObject);
			LocalizationManager.CurrentLanguage.Register(_ => RefreshTexts()).UnRegisterWhenGameObjectDestroyed(gameObject);
			RefreshTexts();

			if (!_subscribedLeaderboardEvent)
			{
				_subscribedLeaderboardEvent = true;
				LeaderboardSystem.OnLeaderboardChanged += RefreshRows;
			}

			RefreshRows();
		}

		protected override void OnOpen(IUIData uiData = null)
		{
			NormalizeFullscreenRoot();
			ApplySafeAreaNow();
			RefreshTexts();
			RefreshRows();
		}

		protected override void OnShow()
		{
			NormalizeFullscreenRoot();
			ApplySafeAreaNow();
		}

		protected override void OnClose()
		{
			if (_subscribedLeaderboardEvent)
			{
				_subscribedLeaderboardEvent = false;
				LeaderboardSystem.OnLeaderboardChanged -= RefreshRows;
			}

			ClearRows();
		}

		private void RefreshTexts()
		{
			if (_titleText)
			{
				_titleText.text = TryGetText("ui.leaderboard.title", "排行榜");
			}

			if (_btnCloseText)
			{
				_btnCloseText.text = TryGetText("ui.settings.back", "返回");
			}

			if (_btnReturnText)
			{
				_btnReturnText.text = TryGetText("ui.settings.return_main_menu", "主菜单");
			}

			if (_btnClearText)
			{
				_btnClearText.text = TryGetText("ui.leaderboard.clear", "清空记录");
			}
		}

		private void RefreshRows()
		{
			ClearRows();
			if (!RankingTemplate || !_contentRoot) return;

			var entries = LeaderboardSystem.GetTopEntries();
			if (entries == null || entries.Count == 0)
			{
				var row = CreateRow();
				FillRow(row, 0, null);
				return;
			}

			for (var i = 0; i < entries.Count; i++)
			{
				var row = CreateRow();
				FillRow(row, i + 1, entries[i]);
			}
		}

		private GameObject CreateRow()
		{
			var row = Instantiate(RankingTemplate.gameObject, _contentRoot, false);
			row.name = $"RankingRow_{_spawnedRows.Count + 1}";
			row.SetActive(true);
			_spawnedRows.Add(row);
			return row;
		}

		private void FillRow(GameObject row, int rank, LeaderboardSystem.Entry entry)
		{
			if (!row) return;

			var rankText = FindChildRecursive(row.transform, "RankingText")?.GetComponent<Text>();
			var scoreText = FindChildRecursive(row.transform, "ScoreText")?.GetComponent<Text>();
			var waveText = FindChildRecursive(row.transform, "WaveText")?.GetComponent<Text>();
			var levelText = FindChildRecursive(row.transform, "LevelText")?.GetComponent<Text>();
			var coinText = FindChildRecursive(row.transform, "CoinText")?.GetComponent<Text>();
			var killText = FindChildRecursive(row.transform, "KillCountText")?.GetComponent<Text>();
			var deathText = FindChildRecursive(row.transform, "DeathReasonText")?.GetComponent<Text>();

			RegisterFont(rankText);
			RegisterFont(scoreText);
			RegisterFont(waveText);
			RegisterFont(levelText);
			RegisterFont(coinText);
			RegisterFont(killText);
			RegisterFont(deathText);

			if (entry == null)
			{
				if (rankText) rankText.text = "-";
				if (scoreText) scoreText.text = "-";
				if (waveText) waveText.text = "-";
				if (levelText) levelText.text = "-";
				if (coinText) coinText.text = "-";
				if (killText) killText.text = "-";
				if (deathText) deathText.text = TryGetText("ui.leaderboard.empty", "暂无记录");
				return;
			}

			if (rankText) rankText.text = rank.ToString();
			if (scoreText) scoreText.text = entry.Score.ToString();
			if (waveText) waveText.text = entry.WaveMinute.ToString();
			if (levelText) levelText.text = entry.Level.ToString();
			if (coinText) coinText.text = entry.Coins.ToString();
			if (killText) killText.text = entry.KillCount.ToString();
			if (deathText) deathText.text = string.IsNullOrWhiteSpace(entry.DeathReason) ? "未知" : entry.DeathReason;
		}

		private void ClearRows()
		{
			for (var i = 0; i < _spawnedRows.Count; i++)
			{
				if (_spawnedRows[i])
				{
					Destroy(_spawnedRows[i]);
				}
			}

			_spawnedRows.Clear();
		}

		private static string TryGetText(string key, string fallback)
		{
			if (LocalizationManager.TryGet(key, out var value))
			{
				return value;
			}

			return fallback;
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

		private static void RegisterFont(Text text)
		{
			if (text) FontManager.Register(text);
		}

		private static Transform FindChildRecursive(Transform root, string targetName)
		{
			if (!root) return null;
			if (root.name == targetName) return root;

			for (var i = 0; i < root.childCount; i++)
			{
				var result = FindChildRecursive(root.GetChild(i), targetName);
				if (result) return result;
			}

			return null;
		}
	}
}
