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
			if (fullscreenLabel) FontManager.Register(fullscreenLabel);

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
			if (languageText) FontManager.Register(languageText);

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

				if (titleText) titleText.text = LocalizationManager.T("ui.settings.title");

				if (screenText) screenText.text = LocalizationManager.T("ui.settings.screen_mode");

				if (fullscreenLabel)
				{
					fullscreenLabel.text = LocalizationManager.T(FullscreenToggle && FullscreenToggle.isOn ? "ui.settings.fullscreen" : "ui.settings.windowed");
				}

				if (musicLabel) musicLabel.text = LocalizationManager.T("ui.settings.music");
				if (soundLabel) soundLabel.text = LocalizationManager.T("ui.settings.sfx");

				if (backLabel) backLabel.text = LocalizationManager.T("ui.settings.back");
				if (returnMenuLabel) returnMenuLabel.text = LocalizationManager.T("ui.settings.return_main_menu");
				if (quitLabel) quitLabel.text = LocalizationManager.T("ui.settings.quit");

				if (languageToggle2)
				{
					var isEn = LocalizationManager.CurrentLanguage.Value == LanguageId.En;
					languageToggle2.SetIsOnWithoutNotify(isEn);
					if (languageText) languageText.text = LocalizationManager.T(isEn ? "ui.settings.lang_en" : "ui.settings.lang_zh");
				}

				if (languageLabel) languageLabel.text = LocalizationManager.T("ui.settings.language");
			};

			LocalizationManager.ReadyChanged.Register(() => refreshUiText()).UnRegisterWhenGameObjectDestroyed(gameObject);
			refreshUiText();

			if (languageToggle2)
			{
				languageToggle2.onValueChanged.RemoveAllListeners();
				languageToggle2.SetIsOnWithoutNotify(LocalizationManager.CurrentLanguage.Value == LanguageId.En);
				languageToggle2.onValueChanged.AddListener(isOn =>
				{
					AudioKit.PlaySound(Sfx.BUTTONCLICK);
					LocalizationManager.ChangeLanguage(isOn ? LanguageId.En : LanguageId.ZhHans);
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
			if (!LocalizationManager.IsReady) return;
			var fullscreenLabel = FullscreenToggle ? FullscreenToggle.GetComponentInChildren<Text>(true) : null;
			if (fullscreenLabel) fullscreenLabel.text = LocalizationManager.T(isFullscreen ? "ui.settings.fullscreen" : "ui.settings.windowed");
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
