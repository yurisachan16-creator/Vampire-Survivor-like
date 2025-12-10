/****************************************************************************
 * 2025.12 DESKTOP-JJUC8BO
 ****************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class TreasureChestPanel
	{
		[SerializeField] public UnityEngine.UI.Button BtnSure;
		[SerializeField] public UnityEngine.UI.Text Content;

		public void Clear()
		{
			BtnSure = null;
			Content = null;
		}

		public override string ComponentName
		{
			get { return "TreasureChestPanel";}
		}
	}
}
