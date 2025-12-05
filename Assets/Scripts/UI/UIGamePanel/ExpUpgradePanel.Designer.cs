/****************************************************************************
 * 2025.12 DESKTOP-JJUC8BO
 ****************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class ExpUpgradePanel
	{
		[SerializeField] public UnityEngine.UI.Button BtnExpUpgradeItemPrefab;
		[SerializeField] public RectTransform UpgradeRoot;
		[SerializeField] public UnityEngine.UI.Button BtnUpgrade;
		[SerializeField] public UnityEngine.UI.Button BtnSimpleDurationUpgrade;

		public void Clear()
		{
			BtnExpUpgradeItemPrefab = null;
			UpgradeRoot = null;
			BtnUpgrade = null;
			BtnSimpleDurationUpgrade = null;
		}

		public override string ComponentName
		{
			get { return "ExpUpgradePanel";}
		}
	}
}
