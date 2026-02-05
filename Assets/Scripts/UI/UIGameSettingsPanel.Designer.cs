using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
	// Generate Id:55d62c4c-54f3-4c08-a855-a1cdae06314f
	public partial class UIGameSettingsPanel
	{
		public const string Name = "UIGameSettingsPanel";
		
		[SerializeField]
		public UnityEngine.UI.Image DisplaySettings;
		[SerializeField]
		public UnityEngine.UI.Toggle FullscreenToggle;
		[SerializeField]
		public UnityEngine.UI.Image AudioSettings;
		[SerializeField]
		public UnityEngine.UI.Slider MusicVolumeSlider;
		[SerializeField]
		public UnityEngine.UI.Slider SoundVolumeSlider;
		[SerializeField]
		public UnityEngine.UI.Image LanguageSettings;
		[SerializeField]
		public UnityEngine.UI.Toggle LanguageToggle;
		[SerializeField]
		public UnityEngine.UI.Button BtnClose;
		[SerializeField]
		public UnityEngine.UI.Button BtnReturnToMainMenu;
		[SerializeField]
		public UnityEngine.UI.Button BtnQuit;
		
		private UIGameSettingsPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			DisplaySettings = null;
			FullscreenToggle = null;
			AudioSettings = null;
			MusicVolumeSlider = null;
			SoundVolumeSlider = null;
			LanguageSettings = null;
			LanguageToggle = null;
			BtnClose = null;
			BtnReturnToMainMenu = null;
			BtnQuit = null;
			
			mData = null;
		}
		
		public UIGameSettingsPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		UIGameSettingsPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UIGameSettingsPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
