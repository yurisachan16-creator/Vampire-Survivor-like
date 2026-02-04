/****************************************************************************
 * 2026.2 DESKTOP-JJUC8BO
 ****************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class AchievementController
	{
		[SerializeField] public UnityEngine.UI.Image AchievementItem;
		[SerializeField] public UnityEngine.UI.Text Description;
		[SerializeField] public UnityEngine.UI.Text Title;
		[SerializeField] public UnityEngine.UI.Image Icon;

		public void Clear()
		{
			AchievementItem = null;
			Description = null;
			Title = null;
			Icon = null;
		}

		public override string ComponentName
		{
			get { return "AchievementController";}
		}
	}
}
