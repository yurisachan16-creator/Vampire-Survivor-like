using UnityEngine;
using UnityEngine.UI;
using QFramework;
using QAssetBundle;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

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
		private ResLoader _iconResLoader;
		
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGameSettingsPanelData ?? new UIGameSettingsPanelData();
			if (Application.isMobilePlatform && !GetComponent<SafeAreaFitter>()) gameObject.AddComponent<SafeAreaFitter>();
			
			// 获取 SettingsPanel 子对象的 RectTransform
			_settingsPanelRect = transform.Find("SettingsPanel")?.GetComponent<RectTransform>();
			if (Application.isMobilePlatform) ApplyMobileTouchTuning();

			// ===== 预加载图标资源并修复丢失 sprite =====
			_iconResLoader = ResLoader.Allocate();
			RepairMissingSpriteReferences();
			
			// ===== 显示设置（分辨率选择） =====
			// 隐藏旧的全屏 Toggle（保持 Designer 兼容，运行时不使用）
			if (FullscreenToggle)
			{
				FullscreenToggle.gameObject.SetActive(false);
			}

			// 查找分辨率 Dropdown（复用 DisplaySettings 容器中的 TMP_Dropdown）
			TMP_Dropdown resolutionDropdown = null;
			if (DisplaySettings)
			{
				resolutionDropdown = DisplaySettings.GetComponentInChildren<TMP_Dropdown>(true);
				if (resolutionDropdown)
				{
					if (resolutionDropdown.captionText) FontManager.Register(resolutionDropdown.captionText);
					if (resolutionDropdown.itemText) FontManager.Register(resolutionDropdown.itemText);
				}
			}

			Toggle debugHudToggle = null;
			Text debugHudLabel = null;
			GameObject debugHudRow = null;
			// 用于查找 template row 的辅助变量
			Transform settingsContent = transform.Find("SettingsPanel/Scroll View/Viewport/Content");
			// 尝试从 FullscreenToggle 获取模板行，如果不存在则从 DisplaySettings 获取
			GameObject templateRow = null;
			if (FullscreenToggle && FullscreenToggle.transform.parent && settingsContent
				&& FullscreenToggle.transform.parent.parent == settingsContent)
			{
				templateRow = FullscreenToggle.transform.parent.gameObject;
			}
			else if (DisplaySettings && DisplaySettings.transform.parent == settingsContent)
			{
				templateRow = DisplaySettings.gameObject;
			}

			if (Application.isMobilePlatform && Debug.isDebugBuild)
			{
				if (settingsContent && templateRow)
				{
					debugHudRow = Instantiate(templateRow, settingsContent, false);
					debugHudRow.name = "MobileDebugHudSetting";
					debugHudRow.transform.SetSiblingIndex(templateRow.transform.GetSiblingIndex() + 1);
					debugHudToggle = debugHudRow.GetComponentInChildren<Toggle>(true);
					if (debugHudToggle) debugHudToggle.SetIsOnWithoutNotify(GameSettings.EnableMobileDebugHud);

					debugHudLabel = FindNonSpecificLabel(debugHudRow, null);
					if (debugHudLabel) FontManager.Register(debugHudLabel);
				}
			}

			var languageToggle2 = LanguageToggle;
			GameObject fallbackLanguageRow = null;
			if (!languageToggle2)
			{
				if (settingsContent && templateRow)
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
				var isEn = LocalizationManager.CurrentLanguage.Value == LanguageId.En;
				string TL(string key, string fallbackZh, string fallbackEn)
				{
					if (LocalizationManager.IsReady) return LocalizationManager.T(key);
					return isEn ? fallbackEn : fallbackZh;
				}

				string GetLanguageDisplayName(LanguageId language)
				{
					if (language == LanguageId.ZhHans) return "简体中文";
					if (language == LanguageId.ZhHant) return "繁體中文";
					if (language == LanguageId.En) return "English";
					if (language.ToString() == "ja") return "日本語";
					if (language.ToString() == "ko") return "한국어";
					if (language.ToString() == "fr") return "Français";
					if (language.ToString() == "de") return "Deutsch";
					if (language.ToString() == "es") return "Español";
					return language.ToString();
				}

				if (titleText) titleText.text = TL("ui.settings.title", "设置", "Settings");

				// 分辨率标签
				if (screenText) screenText.text = TL("ui.settings.resolution", "分辨率", "Resolution");

				// 分辨率 Dropdown 选项
				if (resolutionDropdown)
				{
					PopulateResolutionDropdown(resolutionDropdown);
				}

				if (musicLabel) musicLabel.text = TL("ui.settings.music", "音乐", "Music");
				if (soundLabel) soundLabel.text = TL("ui.settings.sfx", "音效", "SFX");

				if (backLabel) backLabel.text = TL("ui.settings.back", "返回", "Back");
				if (returnMenuLabel) returnMenuLabel.text = TL("ui.settings.return_main_menu", "主菜单", "Main Menu");
				if (quitLabel) quitLabel.text = TL("ui.settings.quit", "退出", "Quit");

				if (languageToggle2 && !languageDropdown)
				{
					var isEnToggle = LocalizationManager.CurrentLanguage.Value == LanguageId.En;
					languageToggle2.SetIsOnWithoutNotify(isEnToggle);
					var langLabel = GetLanguageDisplayName(isEnToggle ? LanguageId.En : LanguageId.ZhHans);
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
						options.Add(new TMP_Dropdown.OptionData(GetLanguageDisplayName(lang)));
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

				if (languageLabel) languageLabel.text = TL("ui.settings.language", "语言", "Language");

				if (debugHudLabel)
				{
					debugHudLabel.text = LocalizationManager.CurrentLanguage.Value == LanguageId.En ? "Debug HUD" : "调试HUD";
				}
			};

			_refreshUiText = refreshUiText;

			LocalizationManager.ReadyChanged.Register(() => refreshUiText()).UnRegisterWhenGameObjectDestroyed(gameObject);
			LocalizationManager.CurrentLanguage.Register(_ => refreshUiText()).UnRegisterWhenGameObjectDestroyed(gameObject);

			refreshUiText();

			// ===== 分辨率 Dropdown 事件 =====
			if (resolutionDropdown)
			{
				resolutionDropdown.onValueChanged.RemoveAllListeners();
				resolutionDropdown.onValueChanged.AddListener(index =>
				{
					AudioKit.PlaySound(Sfx.BUTTONCLICK);
					GameSettings.ApplyResolution(index);
					// 分辨率变化后刷新 UI 布局
					StartCoroutine(RefreshLayoutAfterResolutionChange());
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

		/// <summary>
		/// 填充分辨率 Dropdown 选项
		/// </summary>
		private void PopulateResolutionDropdown(TMP_Dropdown dropdown)
		{
			var resolutions = GameSettings.GetAvailableResolutions();
			var recommendedIdx = GameSettings.GetRecommendedIndex();
			var options = dropdown.options;
			options.Clear();

			for (var i = 0; i < resolutions.Count; i++)
			{
				var res = resolutions[i];
				string text;
				if (res.IsAutoDetect)
				{
					text = LocalizationManager.IsReady
						? LocalizationManager.T("ui.settings.auto_detect")
						: (LocalizationManager.CurrentLanguage.Value == LanguageId.En ? "Auto Detect" : "自动检测");
				}
				else
				{
					text = res.GetDisplayText(i == recommendedIdx);
				}
				options.Add(new TMP_Dropdown.OptionData(text));
			}

			var savedIndex = GameSettings.ResolutionIndex;
			if (savedIndex < 0 || savedIndex >= options.Count) savedIndex = 0;
			dropdown.SetValueWithoutNotify(savedIndex);
			dropdown.RefreshShownValue();
		}

		/// <summary>
		/// 分辨率变更后刷新 UI 布局
		/// </summary>
		private IEnumerator RefreshLayoutAfterResolutionChange()
		{
			yield return new WaitForEndOfFrame();
			Canvas.ForceUpdateCanvases();
			if (_settingsPanelRect)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(_settingsPanelRect);
			}
			// 移动端重新应用触控优化
			if (Application.isMobilePlatform) ApplyMobileTouchTuning();
		}

		/// <summary>
		/// 查找非特定用途的 Text 标签（用于动态创建的行）
		/// </summary>
		private Text FindNonSpecificLabel(GameObject row, Text excludeLabel)
		{
			var texts = row.GetComponentsInChildren<Text>(true);
			for (var i = 0; i < texts.Length; i++)
			{
				if (!texts[i]) continue;
				if (excludeLabel && texts[i] == excludeLabel) continue;
				return texts[i];
			}
			return null;
		}

		/// <summary>
		/// 修复 Prefab 中可能丢失的 Sprite 引用
		/// 当从 AssetBundle 加载时，Unity 内置 Sprite 可能不会被包含
		/// </summary>
		private void RepairMissingSpriteReferences()
		{
			var images = GetComponentsInChildren<Image>(true);
			var missingCount = 0;
			for (var i = 0; i < images.Length; i++)
			{
				if (!images[i]) continue;
				// 检查 Image 是否应该有 sprite 但实际丢失
				// 排除纯色背景 Image（sprite 为 null 且 color.a > 0 是正常的纯色背景）
				if (images[i].sprite == null && images[i].type != Image.Type.Filled)
				{
					// 如果是 Toggle 的 Checkmark 或按钮背景，需要修复
					var go = images[i].gameObject;
					if (go.name == "Checkmark" || go.name == "Background" || go.name == "Handle")
					{
						// 创建一个简单的白色 sprite 作为替代
						images[i].sprite = CreateFallbackSprite();
						missingCount++;
					}
				}
			}
			if (missingCount > 0)
			{
				Debug.Log($"[UIGameSettingsPanel] Repaired {missingCount} missing sprite references");
			}
		}

		/// <summary>
		/// 创建一个纯白 fallback sprite（用于修复 AssetBundle 中丢失的内置 sprite）
		/// </summary>
		private static Sprite _fallbackSprite;
		private static Sprite CreateFallbackSprite()
		{
			if (_fallbackSprite != null) return _fallbackSprite;
			var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
			var pixels = new Color32[16];
			for (var i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
			tex.SetPixels32(pixels);
			tex.Apply();
			_fallbackSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
			_fallbackSprite.name = "FallbackSprite";
			return _fallbackSprite;
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
		// 释放图标资源加载器
		if (_iconResLoader != null)
		{
			_iconResLoader.Recycle2Cache();
			_iconResLoader = null;
		}
	}
}
}
