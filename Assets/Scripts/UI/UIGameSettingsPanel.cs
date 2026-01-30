using UnityEngine;
using UnityEngine.UI;
using QFramework;
using QAssetBundle;
using UnityEngine.SceneManagement;

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
			
			// WebGL 平台隐藏退出按钮
			#if UNITY_WEBGL && !UNITY_EDITOR
			BtnQuit.gameObject.SetActive(false);
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
			// 强制重置 SettingsPanel 位置到屏幕中央
			if (_settingsPanelRect != null)
			{
				_settingsPanelRect.anchoredPosition = Vector2.zero;
			}
			
			// 如果是从游戏中打开，暂停游戏
			if (mData.IsFromGame)
			{
				_previousTimeScale = Time.timeScale;
				Time.timeScale = 0f;
			}
		}
		
		protected override void OnShow()
		{
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
