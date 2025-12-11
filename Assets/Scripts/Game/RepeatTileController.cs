using UnityEngine;
using QFramework;
using UnityEngine.Tilemaps;

namespace VampireSurvivorLike
{
	/// <summary>
	/// 挂载到Grid下，用于重复铺设Tilemap以实现无缝地图滚动
	/// 具体表现为：在玩家移动时，根据玩家位置动态调整Tilemap的位置，
	/// 始终保持玩家在中心Tilemap所在区域，从而实现无缝滚动效果
	/// </summary>
	public partial class RepeatTileController : ViewController
	{
		private Tilemap _mUp;
		private Tilemap _mDown;
		private Tilemap _mLeft;
		private Tilemap _mRight;
		private Tilemap _mUpLeft;
		private Tilemap _mUpRight;
		private Tilemap _mDownLeft;
		private Tilemap _mDownRight;

		private int AreaX = 0;
		private int AreaY = 0;

		private void CreateTileMaps()
        {
            _mUp=Ground.InstantiateWithParent(transform);
			_mDown=Ground.InstantiateWithParent(transform);
			_mLeft=Ground.InstantiateWithParent(transform);
			_mRight=Ground.InstantiateWithParent(transform);
			_mUpLeft=Ground.InstantiateWithParent(transform);
			_mUpRight=Ground.InstantiateWithParent(transform);
			_mDownLeft=Ground.InstantiateWithParent(transform);
			_mDownRight=Ground.InstantiateWithParent(transform);
        }

		private void UpdatePositions()
        {
            _mUp.Position(new Vector3(AreaX * Ground.size.x,(AreaY + 1) * Ground.size.y));
			_mDown.Position(new Vector3(AreaX * Ground.size.x,(AreaY - 1) * Ground.size.y));
			_mLeft.Position(new Vector3((AreaX - 1) * Ground.size.x,(AreaY + 0) * Ground.size.y));
			_mRight.Position(new Vector3((AreaX + 1) * Ground.size.x,(AreaY + 0) * Ground.size.y));
			_mUpLeft.Position(new Vector3((AreaX - 1) * Ground.size.x,(AreaY + 1) * Ground.size.y));
			_mUpRight.Position(new Vector3((AreaX + 1) * Ground.size.x,(AreaY + 1) * Ground.size.y));
			_mDownLeft.Position(new Vector3((AreaX - 1) * Ground.size.x,(AreaY - 1) * Ground.size.y));
			_mDownRight.Position(new Vector3((AreaX + 1) * Ground.size.x,(AreaY - 1) * Ground.size.y));
			Ground.Position(new Vector3(AreaX * Ground.size.x, AreaY * Ground.size.y));
        }

        void Start()
        {
            CreateTileMaps();
			UpdatePositions();
        }

		private void Update()
        {
            if (Player.Default && Time.frameCount % 60 == 0)
            {
                var cellPos=Ground.layoutGrid.WorldToCell(Player.Default.Position());
				var newAreaX = Mathf.FloorToInt((float)cellPos.x / Ground.size.x);
				var newAreaY = Mathf.FloorToInt((float)cellPos.y / Ground.size.y);
				
				if(newAreaX != AreaX || newAreaY != AreaY)
				{
					AreaX = newAreaX;
					AreaY = newAreaY;
					UpdatePositions();
				}
            }
        }
    }
}
