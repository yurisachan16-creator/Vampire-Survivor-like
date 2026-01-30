/****************************************************************************
 * 2026.1 DESKTOP-JJUC8BO
 ****************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class AchievementPanel
	{
		[SerializeField] public UnityEngine.UI.Button AchievementItemPrefab;
		[SerializeField] public UnityEngine.UI.Button BtnClose;
		[SerializeField] public RectTransform AchievementItemRoot;

		public void Clear()
		{
			AchievementItemPrefab = null;
			BtnClose = null;
			AchievementItemRoot = null;
		}

		public override string ComponentName
		{
			get { return "AchievementPanel";}
		}
	}
}
