using UnityEngine;
using UnityEngine.UI;
using QFramework;
using QAssetBundle;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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
		private System.Action _refreshUiText;
		
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGameSettingsPanelData ?? new UIGameSettingsPanelData();
			if (Application.isMobilePlatform && !GetComponent<SafeAreaFitter>()) gameObject.AddComponent<SafeAreaFitter>();
			
			// 获取 SettingsPanel 子对象的 RectTransform
			_settingsPanelRect = transform.Find("SettingsPanel")?.GetComponent<RectTransform>();
			if (Application.isMobilePlatform) ApplyMobileTouchTuning();
			
			// ===== 显示设置 =====
			// 初始化全屏 Toggle
			FullscreenToggle.isOn = GameSettings.IsFullscreen;
			FullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
			var fullscreenLabel = FullscreenToggle.GetComponentInChildren<Text>(true);
			if (fullscreenLabel) FontManager.Register(fullscreenLabel);

			TMP_Dropdown screenModeDropdown = null;
			if (DisplaySettings)
			{
				screenModeDropdown = DisplaySettings.GetComponentInChildren<TMP_Dropdown>(true);
				if (screenModeDropdown)
				{
					if (screenModeDropdown.captionText) FontManager.Register(screenModeDropdown.captionText);
					if (screenModeDropdown.itemText) FontManager.Register(screenModeDropdown.itemText);
				}
			}

			Toggle debugHudToggle = null;
			Text debugHudLabel = null;
			GameObject debugHudRow = null;
			if (Application.isMobilePlatform && Debug.isDebugBuild)
			{
				var settingsContent = transform.Find("SettingsPanel/Scroll View/Viewport/Content");
				var templateRow = FullscreenToggle.transform.parent ? FullscreenToggle.transform.parent.gameObject : FullscreenToggle.gameObject;
				if (settingsContent && templateRow && templateRow.transform.parent == settingsContent)
				{
					debugHudRow = Instantiate(templateRow, settingsContent, false);
					debugHudRow.name = "MobileDebugHudSetting";
					debugHudRow.transform.SetSiblingIndex(templateRow.transform.GetSiblingIndex() + 1);
					debugHudToggle = debugHudRow.GetComponentInChildren<Toggle>(true);
					if (debugHudToggle) debugHudToggle.SetIsOnWithoutNotify(GameSettings.EnableMobileDebugHud);

					var texts = debugHudRow.GetComponentsInChildren<Text>(true);
					for (var i = 0; i < texts.Length; i++)
					{
						if (!texts[i]) continue;
						if (fullscreenLabel && texts[i] == fullscreenLabel) continue;
						debugHudLabel = texts[i];
						break;
					}
					if (debugHudLabel) FontManager.Register(debugHudLabel);
				}
			}

			var languageToggle2 = LanguageToggle;
			GameObject fallbackLanguageRow = null;
			if (!languageToggle2)
			{
				var settingsContent = transform.Find("SettingsPanel/Scroll View/Viewport/Content");
				var templateRow = FullscreenToggle.transform.parent ? FullscreenToggle.transform.parent.gameObject : FullscreenToggle.gameObject;
				if (settingsContent && templateRow && templateRow.transform.parent == settingsContent)
				{
					fallbackLanguageRow = Instantiate(templateRow, settingsContent, false);
					fallbackLanguageRow.name = "LanguageSetting";
					fallbackLanguageRow.transform.SetSiblingIndex(templateRow.transform.GetSiblingIndex() + 1);
					languageToggle2 = fallbackLanguageRow.GetComponentInChildren<Toggle>(true);
				}
			}
			var titleText = transform.Find("SettingsPanel/Scroll View/Viewport/Content/Title")?.GetComponent<Text>();
			if (titleText) FontManager.Register(titleText);

			var screenText = DisplaySettings ? DisplaySettings.transform.Find("ScreenText")?.GetComponent<Text>() : null;
			if (screenText) FontManager.Register(screenText);

			var musicLabel = AudioSettings ? AudioSettings.transform.Find("MusicVolumeText")?.GetComponent<Text>() : null;
			if (musicLabel) FontManager.Register(musicLabel);

			var soundLabel = AudioSettings ? AudioSettings.transform.Find("SoundVolumeText")?.GetComponent<Text>() : null;
			if (soundLabel) FontManager.Register(soundLabel);

			var backLabel = BtnClose ? BtnClose.GetComponentInChildren<Text>(true) : null;
			if (backLabel) FontManager.Register(backLabel);

			var returnMenuLabel = BtnReturnToMainMenu ? BtnReturnToMainMenu.GetComponentInChildren<Text>(true) : null;
			if (returnMenuLabel) FontManager.Register(returnMenuLabel);

			var quitLabel = BtnQuit ? BtnQuit.GetComponentInChildren<Text>(true) : null;
			if (quitLabel) FontManager.Register(quitLabel);

			var languageTextTransform = LanguageSettings ? LanguageSettings.transform.Find("LanguageText") : null;
			var languageText = languageTextTransform ? languageTextTransform.GetComponent<Text>() : null;
			var languageTmpText = languageTextTransform ? languageTextTransform.GetComponent<TMP_Text>() : null;
			if (languageText) FontManager.Register(languageText);
			if (languageTmpText) FontManager.Register(languageTmpText);

			TMP_Dropdown languageDropdown = null;
			var languageDropdownIds = new List<LanguageId>();
			if (LanguageSettings)
			{
				languageDropdown = LanguageSettings.GetComponentInChildren<TMP_Dropdown>(true);
			}
			if (!languageDropdown && fallbackLanguageRow)
			{
				languageDropdown = fallbackLanguageRow.GetComponentInChildren<TMP_Dropdown>(true);
			}
			if (languageDropdown)
			{
				if (languageDropdown.captionText) FontManager.Register(languageDropdown.captionText);
				if (languageDropdown.itemText) FontManager.Register(languageDropdown.itemText);
			}

			var languageLabel = (Text)null;
			if (LanguageSettings)
			{
				var candidates = LanguageSettings.GetComponentsInChildren<Text>(true);
				for (var i = 0; i < candidates.Length; i++)
				{
					if (!candidates[i]) continue;
					if (candidates[i].gameObject.name == "LanguageText") continue;
					languageLabel = candidates[i];
					break;
				}
				if (languageLabel) FontManager.Register(languageLabel);
			}
			else if (fallbackLanguageRow)
			{
				var candidates = fallbackLanguageRow.GetComponentsInChildren<Text>(true);
				for (var i = 0; i < candidates.Length; i++)
				{
					if (!candidates[i]) continue;
					if (candidates[i].text == "全屏" || candidates[i].text == "语言" || candidates[i].text == "Language")
					{
						languageLabel = candidates[i];
						break;
					}
				}
				if (languageLabel) FontManager.Register(languageLabel);
			}

			System.Action refreshUiText = () =>
			{
				if (!LocalizationManager.IsReady) return;

				if (FullscreenToggle) FullscreenToggle.SetIsOnWithoutNotify(GameSettings.IsFullscreen);

				if (titleText) titleText.text = LocalizationManager.T("ui.settings.title");

				if (screenText) screenText.text = LocalizationManager.T("ui.settings.screen_mode");

				if (fullscreenLabel)
				{
					fullscreenLabel.text = LocalizationManager.T(GameSettings.IsFullscreen ? "ui.settings.fullscreen" : "ui.settings.windowed");
				}

				if (screenModeDropdown)
				{
					var options = screenModeDropdown.options;
					options.Clear();
					options.Add(new TMP_Dropdown.OptionData(LocalizationManager.T("ui.settings.windowed")));
					options.Add(new TMP_Dropdown.OptionData(LocalizationManager.T("ui.settings.fullscreen")));

					var selectedIndex = GameSettings.IsFullscreen ? 1 : 0;
					screenModeDropdown.SetValueWithoutNotify(selectedIndex);
					screenModeDropdown.RefreshShownValue();
				}

				if (musicLabel) musicLabel.text = LocalizationManager.T("ui.settings.music");
				if (soundLabel) soundLabel.text = LocalizationManager.T("ui.settings.sfx");

				if (backLabel) backLabel.text = LocalizationManager.T("ui.settings.back");
				if (returnMenuLabel) returnMenuLabel.text = LocalizationManager.T("ui.settings.return_main_menu");
				if (quitLabel) quitLabel.text = LocalizationManager.T("ui.settings.quit");

				if (languageToggle2 && !languageDropdown)
				{
					var isEn = LocalizationManager.CurrentLanguage.Value == LanguageId.En;
					languageToggle2.SetIsOnWithoutNotify(isEn);
					var langLabel = LocalizationManager.T(isEn ? "ui.settings.lang_en" : "ui.settings.lang_zh");
					if (languageText) languageText.text = langLabel;
					if (languageTmpText) languageTmpText.text = langLabel;
				}

				if (languageDropdown)
				{
					languageDropdownIds.Clear();
					var supported = LocalizationManager.Settings.SupportedLanguages ?? new List<LanguageId> { LanguageId.ZhHans, LanguageId.En };
					for (var i = 0; i < supported.Count; i++)
					{
						languageDropdownIds.Add(supported[i]);
					}

					var options = languageDropdown.options;
					options.Clear();
					for (var i = 0; i < languageDropdownIds.Count; i++)
					{
						var lang = languageDropdownIds[i];
						var labelKey = lang == LanguageId.En ? "ui.settings.lang_en" : (lang == LanguageId.ZhHans ? "ui.settings.lang_zh" : string.Empty);
						var label = string.IsNullOrEmpty(labelKey) ? lang.ToString() : LocalizationManager.T(labelKey);
						options.Add(new TMP_Dropdown.OptionData(label));
					}

					var selectedIndex = 0;
					for (var i = 0; i < languageDropdownIds.Count; i++)
					{
						if (languageDropdownIds[i] == LocalizationManager.CurrentLanguage.Value)
						{
							selectedIndex = i;
							break;
						}
					}
					languageDropdown.SetValueWithoutNotify(selectedIndex);
					languageDropdown.RefreshShownValue();

					if (selectedIndex >= 0 && selectedIndex < options.Count)
					{
						var label = options[selectedIndex].text;
						if (languageText) languageText.text = label;
						if (languageTmpText) languageTmpText.text = label;
					}
				}

				if (languageLabel) languageLabel.text = LocalizationManager.T("ui.settings.language");

				if (debugHudLabel)
				{
					debugHudLabel.text = LocalizationManager.CurrentLanguage.Value == LanguageId.En ? "Debug HUD" : "调试HUD";
				}
			};

			_refreshUiText = refreshUiText;

			LocalizationManager.ReadyChanged.Register(() => refreshUiText()).UnRegisterWhenGameObjectDestroyed(gameObject);
			refreshUiText();

			if (screenModeDropdown)
			{
				screenModeDropdown.onValueChanged.RemoveAllListeners();
				screenModeDropdown.onValueChanged.AddListener(index =>
				{
					AudioKit.PlaySound(Sfx.BUTTONCLICK);
					var fullscreen = index == 1;
					if (GameSettings.IsFullscreen == fullscreen) return;
					GameSettings.ApplyFullscreen(fullscreen);
					refreshUiText();
				});
			}

			if (languageDropdown)
			{
				languageDropdown.onValueChanged.RemoveAllListeners();
				languageDropdown.onValueChanged.AddListener(index =>
				{
					AudioKit.PlaySound(Sfx.BUTTONCLICK);
					if (index < 0 || index >= languageDropdownIds.Count) return;
					LocalizationManager.ChangeLanguage(languageDropdownIds[index]);
				});
			}
			else if (languageToggle2)
			{
				languageToggle2.onValueChanged.RemoveAllListeners();
				languageToggle2.SetIsOnWithoutNotify(LocalizationManager.CurrentLanguage.Value == LanguageId.En);
				languageToggle2.onValueChanged.AddListener(isOn =>
				{
					AudioKit.PlaySound(Sfx.BUTTONCLICK);
					LocalizationManager.ChangeLanguage(isOn ? LanguageId.En : LanguageId.ZhHans);
				});
			}

			if (debugHudToggle)
			{
				debugHudToggle.onValueChanged.RemoveAllListeners();
				debugHudToggle.SetIsOnWithoutNotify(GameSettings.EnableMobileDebugHud);
				debugHudToggle.onValueChanged.AddListener(isOn =>
				{
					AudioKit.PlaySound(Sfx.BUTTONCLICK);
					GameSettings.EnableMobileDebugHud = isOn;
				});
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
			
			// 退出按钮
			BtnQuit.onClick.AddListener(() =>
			{
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
				GameSettings.QuitGame();
			});
			
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

		private void ApplyMobileTouchTuning()
		{
			var settingsPanel = _settingsPanelRect ? _settingsPanelRect : (transform as RectTransform);
			if (!settingsPanel) return;

			var selectables = settingsPanel.GetComponentsInChildren<Selectable>(true);
			for (var i = 0; i < selectables.Length; i++)
			{
				var s = selectables[i];
				if (!s) continue;

				var g = s.targetGraphic;
				if (g) g.raycastPadding = new Vector4(24f, 24f, 24f, 24f);

				if (s is Slider slider)
				{
					if (slider.fillRect)
					{
						var fillImg = slider.fillRect.GetComponent<Image>();
						if (fillImg) fillImg.raycastPadding = new Vector4(24f, 24f, 24f, 24f);
					}

					if (slider.handleRect)
					{
						var hImg = slider.handleRect.GetComponent<Image>();
						if (hImg) hImg.raycastPadding = new Vector4(24f, 24f, 24f, 24f);
					}
				}
			}
		}
		
		private void Update()
		{
			if (PlatformInput.GetBackDown())
			{
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
				CloseSelf();
			}
		}
		
		private void OnFullscreenChanged(bool isFullscreen)
		{
			AudioKit.PlaySound(Sfx.BUTTONCLICK);
			GameSettings.ApplyFullscreen(isFullscreen);
			_refreshUiText?.Invoke();
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
