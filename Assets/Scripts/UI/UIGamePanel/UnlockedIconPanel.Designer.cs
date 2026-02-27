/****************************************************************************
 * 2026.2 DESKTOP-JJUC8BO
 ****************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class UnlockedIconPanel
	{
		[SerializeField] public UnityEngine.UI.Image UnlockedIconPrefab;
		[SerializeField] public RectTransform UnlockedIconRoot;

		public void Clear()
		{
			UnlockedIconPrefab = null;
			UnlockedIconRoot = null;
		}

		public override string ComponentName
		{
			get { return "UnlockedIconPanel";}
		}
	}
}
