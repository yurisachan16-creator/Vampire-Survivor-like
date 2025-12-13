using UnityEngine;
using QFramework;
using System.Collections;

namespace VampireSurvivorLike
{
	public partial class TestMaxPowerUpCount : ViewController
	{
		private int _mPowerUpCount = 0;
		IEnumerator Start()
		{
			var PowerUpManager = FindObjectOfType<PowerUpManager>();

			PowerUpManager.GetAllExp.Instantiate()
				.Position(this.Position())
				.Show();

			for(int i = 0; i < 1000; i++)
			{
				gameObject.Position(Random.Range(3,20) * RandomUtility.Choose(-1,1),
					 Random.Range(3,20) * RandomUtility.Choose(-1,1));
				Global.GeneratePowerUp(gameObject,false);
				_mPowerUpCount++;

				gameObject.Position(Random.Range(3,20) * RandomUtility.Choose(-1,1),
					 Random.Range(3,20) * RandomUtility.Choose(-1,1));
				Global.GeneratePowerUp(gameObject,false);
				_mPowerUpCount++;

				gameObject.Position(Random.Range(3,20) * RandomUtility.Choose(-1,1),
					 Random.Range(3,20) * RandomUtility.Choose(-1,1));
				Global.GeneratePowerUp(gameObject,false);
				_mPowerUpCount++;

				gameObject.Position(Random.Range(3,20) * RandomUtility.Choose(-1,1),
					 Random.Range(3,20) * RandomUtility.Choose(-1,1));
				Global.GeneratePowerUp(gameObject,false);
				_mPowerUpCount++;

				gameObject.Position(Random.Range(3,20) * RandomUtility.Choose(-1,1),
					 Random.Range(3,20) * RandomUtility.Choose(-1,1));
				Global.GeneratePowerUp(gameObject,false);
				_mPowerUpCount++;

				gameObject.Position(Random.Range(3,20) * RandomUtility.Choose(-1,1),
					 Random.Range(3,20) * RandomUtility.Choose(-1,1));
				Global.GeneratePowerUp(gameObject,false);
				_mPowerUpCount++;

				gameObject.Position(Random.Range(3,20) * RandomUtility.Choose(-1,1),
					 Random.Range(3,20) * RandomUtility.Choose(-1,1));
				Global.GeneratePowerUp(gameObject,false);
				_mPowerUpCount++;

				gameObject.Position(Random.Range(3,20) * RandomUtility.Choose(-1,1),
					 Random.Range(3,20) * RandomUtility.Choose(-1,1));
				Global.GeneratePowerUp(gameObject,false);
				_mPowerUpCount++;

				gameObject.Position(Random.Range(3,20) * RandomUtility.Choose(-1,1),
					 Random.Range(3,20) * RandomUtility.Choose(-1,1));
				Global.GeneratePowerUp(gameObject,false);
				_mPowerUpCount++;

				gameObject.Position(Random.Range(3,20) * RandomUtility.Choose(-1,1),
					 Random.Range(3,20) * RandomUtility.Choose(-1,1));
				Global.GeneratePowerUp(gameObject,false);
				_mPowerUpCount++;
				yield return new WaitForEndOfFrame();
			}
		}

		private void OnGUI()
		{
			var cached = GUI.matrix;
			IMGUIHelper.SetDesignResolution(960,540);
			GUILayout.Space(10);
			GUILayout.Label(_mPowerUpCount.ToString());

			GUI.matrix = cached;
		}
	}
}
