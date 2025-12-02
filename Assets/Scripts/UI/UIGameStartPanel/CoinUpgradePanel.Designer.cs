/****************************************************************************
 * 2025.12 DESKTOP-JJUC8BO
 ****************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class CoinUpgradePanel
	{
		[SerializeField] public UnityEngine.UI.Button BtnCoinPercentUpgrade;
		[SerializeField] public UnityEngine.UI.Button BtnExpPercentUpgrade;
		[SerializeField] public UnityEngine.UI.Button BtnPlayerMaxHpUpgrade;
		[SerializeField] public UnityEngine.UI.Button BtnClose;
		[SerializeField] public UnityEngine.UI.Text CoinText;
		[SerializeField] public RectTransform CoinUpgradeItemRoot;
		[SerializeField] public UnityEngine.UI.Button CoinUpgradeItemPrefab;

		public void Clear()
		{
			BtnCoinPercentUpgrade = null;
			BtnExpPercentUpgrade = null;
			BtnPlayerMaxHpUpgrade = null;
			BtnClose = null;
			CoinText = null;
			CoinUpgradeItemRoot = null;
			CoinUpgradeItemPrefab = null;
		}

		public override string ComponentName
		{
			get { return "CoinUpgradePanel";}
		}
	}
}
