using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
	// Generate Id:f6c9070e-dc88-4100-8c15-9cd246c87fb8
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
		public UnityEngine.UI.Button BtnClose;
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
			BtnClose = null;
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
