using UnityEngine;
using UnityEngine.UI;
using QFramework;
using QAssetBundle;
using UnityEngine.SceneManagement;
using System.Collections;

namespace VampireSurvivorLike
{
	public class UIGameSettingsPanelData : UIPanelData
	{
		/// <summary>
		/// 是否是从游戏中打开（需要暂停游戏）
		/// </summary>
		public bool IsFromGame = false;
	}
	
	public partial class UIGameSettingsPanel : UIPanel
	{
		private float _previousTimeScale = 1f;
		private RectTransform _settingsPanelRect;
		
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGameSettingsPanelData ?? new UIGameSettingsPanelData();
			
			// 获取 SettingsPanel 子对象的 RectTransform
			_settingsPanelRect = transform.Find("SettingsPanel")?.GetComponent<RectTransform>();
			
			// ===== 显示设置 =====
			// 初始化全屏 Toggle
			FullscreenToggle.isOn = GameSettings.IsFullscreen;
			FullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
			var fullscreenLabel = FullscreenToggle.GetComponentInChildren<Text>(true);
			if (fullscreenLabel) fullscreenLabel.text = LocalizationManager.T("ui.settings.fullscreen");
			if (fullscreenLabel) FontManager.Register(fullscreenLabel);
			LocalizationManager.CurrentLanguage.Register(_ =>
			{
				if (fullscreenLabel) fullscreenLabel.text = LocalizationManager.T("ui.settings.fullscreen");
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			var languageToggle2 = LanguageToggle;
			if (languageToggle2)
			{
				var languageTextTransform = LanguageSettings ? LanguageSettings.transform.Find("LanguageText") : null;
				var languageText = languageTextTransform ? languageTextTransform.GetComponent<Text>() : null;
				if (languageText) FontManager.Register(languageText);

				languageToggle2.onValueChanged.RemoveAllListeners();
				languageToggle2.SetIsOnWithoutNotify(LocalizationManager.CurrentLanguage.Value == LanguageId.En);
				languageToggle2.onValueChanged.AddListener(isOn =>
				{
					AudioKit.PlaySound(Sfx.BUTTONCLICK);
					LocalizationManager.ChangeLanguage(isOn ? LanguageId.En : LanguageId.ZhHans);
				});

				System.Action refreshLanguageUi = () =>
				{
					if (!languageToggle2) return;
					var isEn = LocalizationManager.CurrentLanguage.Value == LanguageId.En;
					languageToggle2.SetIsOnWithoutNotify(isEn);
					if (languageText) languageText.text = LocalizationManager.T(isEn ? "ui.settings.lang_en" : "ui.settings.lang_zh");
				};

				refreshLanguageUi();
				LocalizationManager.CurrentLanguage.Register(_ => refreshLanguageUi()).UnRegisterWhenGameObjectDestroyed(gameObject);
			}
			else
			{
				var settingsContent = transform.Find("SettingsPanel/Scroll View/Viewport/Content");
				var templateRow = FullscreenToggle.transform.parent ? FullscreenToggle.transform.parent.gameObject : FullscreenToggle.gameObject;
				if (settingsContent && templateRow && templateRow.transform.parent == settingsContent)
				{
					var lootGuideRow = Instantiate(templateRow, settingsContent, false);
					lootGuideRow.name = "LootGuideSetting";
					lootGuideRow.transform.SetSiblingIndex(templateRow.transform.GetSiblingIndex() + 1);

					var toggle = lootGuideRow.GetComponentInChildren<Toggle>(true);
					if (toggle)
					{
						toggle.gameObject.name = "LootGuideToggle";
						toggle.onValueChanged.RemoveAllListeners();
						toggle.isOn = GameSettings.EnableLootGuide;
						toggle.onValueChanged.AddListener(isOn =>
						{
							AudioKit.PlaySound(Sfx.BUTTONCLICK);
							GameSettings.EnableLootGuide = isOn;
						});
					}

					var texts = lootGuideRow.GetComponentsInChildren<Text>(true);
					for (var i = 0; i < texts.Length; i++)
					{
						if (texts[i] && (texts[i].text == "全屏" || texts[i].text.Contains("全屏")))
						{
							texts[i].text = LocalizationManager.T("ui.settings.loot_guide");
							FontManager.Register(texts[i]);
							break;
						}
					}
					LocalizationManager.CurrentLanguage.Register(_ =>
					{
						var innerTexts = lootGuideRow.GetComponentsInChildren<Text>(true);
						for (var j = 0; j < innerTexts.Length; j++)
						{
							if (innerTexts[j] && (innerTexts[j].text == "全屏" || innerTexts[j].text == "道具引导" || innerTexts[j].text == "Loot Guide"))
							{
								innerTexts[j].text = LocalizationManager.T("ui.settings.loot_guide");
								break;
							}
						}
					}).UnRegisterWhenGameObjectDestroyed(lootGuideRow);

					var languageRow = Instantiate(templateRow, settingsContent, false);
					languageRow.name = "LanguageSetting";
					languageRow.transform.SetSiblingIndex(lootGuideRow.transform.GetSiblingIndex() + 1);

					var languageToggle = languageRow.GetComponentInChildren<Toggle>(true);
					if (languageToggle)
					{
						languageToggle.gameObject.name = "LanguageToggle";
						languageToggle.onValueChanged.RemoveAllListeners();
						languageToggle.SetIsOnWithoutNotify(LocalizationManager.CurrentLanguage.Value == LanguageId.En);
						languageToggle.onValueChanged.AddListener(isOn =>
						{
							AudioKit.PlaySound(Sfx.BUTTONCLICK);
							LocalizationManager.ChangeLanguage(isOn ? LanguageId.En : LanguageId.ZhHans);
						});
					}

					var languageTexts = languageRow.GetComponentsInChildren<Text>(true);
					for (var i = 0; i < languageTexts.Length; i++)
					{
						if (languageTexts[i] && (languageTexts[i].text == "全屏" || languageTexts[i].text.Contains("全屏")))
						{
							languageTexts[i].text = LocalizationManager.T("ui.settings.language");
							FontManager.Register(languageTexts[i]);
							break;
						}
					}

					LocalizationManager.CurrentLanguage.Register(_ =>
					{
						if (languageToggle) languageToggle.SetIsOnWithoutNotify(LocalizationManager.CurrentLanguage.Value == LanguageId.En);
						var innerTexts = languageRow.GetComponentsInChildren<Text>(true);
						for (var j = 0; j < innerTexts.Length; j++)
						{
							if (innerTexts[j] && (innerTexts[j].text == "全屏" || innerTexts[j].text == "语言" || innerTexts[j].text == "Language"))
							{
								innerTexts[j].text = LocalizationManager.T("ui.settings.language");
								break;
							}
						}
					}).UnRegisterWhenGameObjectDestroyed(languageRow);
				}
			}
			
			// ===== 音频设置 =====
			// 音乐音量滑块
			AudioKit.Settings.MusicVolume.RegisterWithInitValue(v => 
			{
				MusicVolumeSlider.value = v;
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			
			MusicVolumeSlider.onValueChanged.AddListener(v => 
			{
				AudioKit.Settings.MusicVolume.Value = v;
			});
			
			// 音效音量滑块
			AudioKit.Settings.SoundVolume.RegisterWithInitValue(v => 
			{
				SoundVolumeSlider.value = v;
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			
			SoundVolumeSlider.onValueChanged.AddListener(v => 
			{
				AudioKit.Settings.SoundVolume.Value = v;
			});
			
			// ===== 按钮事件 =====
			// 返回按钮
			BtnClose.onClick.AddListener(() =>
			{
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
				CloseSelf();
			});

			//返回主菜单按钮
			BtnReturnToMainMenu.onClick.AddListener(() =>
			{
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
				//停止所有正在播放的音效
				AudioKit.StopAllSound();
				//关闭所有面板
				UIKit.CloseAllPanel();
				//加载主菜单场景（会清理 Game 场景的所有游戏对象）
				SceneManager.LoadScene("GameStart");
				//恢复时间流逝
				Time.timeScale = 1f;
			});
			var returnMenuLabel = BtnReturnToMainMenu.GetComponentInChildren<Text>(true);
			if (returnMenuLabel) returnMenuLabel.text = LocalizationManager.T("ui.settings.return_main_menu");
			if (returnMenuLabel) FontManager.Register(returnMenuLabel);
			LocalizationManager.CurrentLanguage.Register(_ =>
			{
				if (returnMenuLabel) returnMenuLabel.text = LocalizationManager.T("ui.settings.return_main_menu");
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			
			// 退出按钮
			BtnQuit.onClick.AddListener(() =>
			{
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
				GameSettings.QuitGame();
			});
			var quitLabel = BtnQuit.GetComponentInChildren<Text>(true);
			if (quitLabel) quitLabel.text = LocalizationManager.T("ui.settings.quit");
			if (quitLabel) FontManager.Register(quitLabel);
			LocalizationManager.CurrentLanguage.Register(_ =>
			{
				if (quitLabel) quitLabel.text = LocalizationManager.T("ui.settings.quit");
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			
			// 平台特定的按钮显示逻辑
			#if UNITY_WEBGL && !UNITY_EDITOR
			// WebGL 实际运行时：隐藏退出按钮（WebGL不支持退出），显示返回主界面按钮
			BtnQuit.gameObject.SetActive(false);
			BtnReturnToMainMenu.gameObject.SetActive(true);
			#else
			// 编辑器和其他平台：显示退出按钮，也显示返回主界面按钮
			BtnQuit.gameObject.SetActive(true);
			BtnReturnToMainMenu.gameObject.SetActive(true);
			#endif
		}
		
		private void Update()
		{
			// ESC 键关闭设置面板
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
				CloseSelf();
			}
		}
		
		private void OnFullscreenChanged(bool isFullscreen)
		{
			AudioKit.PlaySound(Sfx.BUTTONCLICK);
			GameSettings.ApplyFullscreen(isFullscreen);
		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
		// 立即重置位置
		ResetPanelPosition();
		
		// 延迟一帧再次重置（确保 WebGL 布局完成后位置正确）
		StartCoroutine(ResetPositionEndOfFrame());
		
		// 如果是从游戏中打开，暂停游戏
		if (mData.IsFromGame)
		{
			_previousTimeScale = Time.timeScale;
			Time.timeScale = 0f;
		}
	}
	
	private IEnumerator ResetPositionEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		ResetPanelPosition();
	}
	
	private void ResetPanelPosition()
	{
		// 强制更新 Canvas 布局
		Canvas.ForceUpdateCanvases();
		
		// 强制重置 SettingsPanel 位置到屏幕中央
		if (_settingsPanelRect != null)
		{
			_settingsPanelRect.anchoredPosition = Vector2.zero;
			// 强制刷新布局
			LayoutRebuilder.ForceRebuildLayoutImmediate(_settingsPanelRect);
		}
	}
	
	protected override void OnShow()
	{
		// OnShow 时再次确保位置正确
		ResetPanelPosition();
	}
	
	protected override void OnHide()
	{
	}
		
	protected override void OnClose()
	{
		// 如果是从游戏中打开，恢复游戏
		if (mData.IsFromGame)
		{
			Time.timeScale = _previousTimeScale;
		}
	}
}
}
