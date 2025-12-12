/****************************************************************************
 * 2025.12 DESKTOP-JJUC8BO
 ****************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.Serialization;

namespace VampireSurvivorLike
{
	public partial class AchievementPanel
	{
		[SerializeField] public UnityEngine.UI.Button BtnClose;

		[FormerlySerializedAs("AchivementItemPrefab")]
		[SerializeField] public UnityEngine.UI.Button AchievementItemPrefab;

		[FormerlySerializedAs("AchivementItemRoot")]
		[SerializeField] public RectTransform AchievementItemRoot;

		public void Clear()
		{
			BtnClose = null;
			AchievementItemPrefab = null;
			AchievementItemRoot = null;
		}

		public override string ComponentName
		{
			get { return "AchievementPanel"; }
		}
	}
}
