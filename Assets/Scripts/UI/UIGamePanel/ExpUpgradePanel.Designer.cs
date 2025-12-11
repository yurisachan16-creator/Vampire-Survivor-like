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
		[SerializeField] public UnityEngine.UI.Image Icon;
		[SerializeField] public RectTransform UpgradeRoot;

		public void Clear()
		{
			BtnExpUpgradeItemPrefab = null;
			Icon = null;
			UpgradeRoot = null;
		}

		public override string ComponentName
		{
			get { return "ExpUpgradePanel";}
		}
	}
}
