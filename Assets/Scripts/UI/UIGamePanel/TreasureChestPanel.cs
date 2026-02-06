/****************************************************************************
 * 2025.12 DESKTOP-JJUC8BO
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using System.Linq;
using UnityEngine.U2D;

namespace VampireSurvivorLike
{
	public partial class TreasureChestPanel : UIElement,IController
	{
        private ResLoader _mResLoader;
        private ExpUpgradeItem _currentItem;
        private string _currentKey;
        private object[] _currentArgs;
		private void Awake()
        {
            LocalizationManager.PreloadTable("game");
            LocalizationManager.PreloadTable("upgrade");
			_mResLoader = ResLoader.Allocate();
            BtnSure.onClick.AddListener(()=>
            {
                Time.timeScale = 1f;
				this.Hide();
            });

            LocalizationManager.CurrentLanguage.Register(_ =>
            {
                if (gameObject.activeInHierarchy) RefreshContent();
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnEnable()
        {
            //超级武器的匹配逻辑类似与游戏《星穹铁道》的活动“银河球棒侠传说”
            //其中一个武器的等级达到7级后，并且另一个配对的武器解锁后，可以进行合成升级
            //1.判断是否有匹配的 没合成的
            //2.判断是否有没升级完成的
            var expUpgradeSystem = this.GetSystem<ExpUpgradeSystem>();

            var matchedPairedItems = expUpgradeSystem.Items.Where(item =>
            {
                if (item.CurrentLevel.Value >= 7 && item.PairedName.IsNotNullAndEmpty())
                {
					if (!expUpgradeSystem.Pairs.TryGetValue(item.Key, out var pairedItemKey)) return false;
					if (!expUpgradeSystem.Dictionary.TryGetValue(pairedItemKey, out var pairedItem) || pairedItem == null) return false;
					// 使用当前武器的Key来检查是否已经合成过
					if (!expUpgradeSystem.PairedProperties.TryGetValue(item.Key, out var currentUnlockedProperty) || currentUnlockedProperty == null) return false;

					var pairedItemStartUpgrade = pairedItem.CurrentLevel.Value > 0;
					var alreadyUnlocked = currentUnlockedProperty.Value;  // 检查当前武器的超级版本是否已解锁

					// 配对武器已开始升级 且 当前武器的超级版本尚未解锁
					return pairedItemStartUpgrade && !alreadyUnlocked;
                }

                return false;
            });

            if(matchedPairedItems.Any())
            {
                var item = matchedPairedItems.ToList().GetRandomItem();
                _currentItem = item;
                _currentKey = "game.treasure.paired_content";
                _currentArgs = new object[] { item.PairedName, item.PairedDescription };
                RefreshContent();

                //如果是超级武器，直接升级到满级
                while(!item.UpgradeFinish)
                {
                    item.Upgrade();
                }

				var atlas = _mResLoader.LoadSync<SpriteAtlas>("icon");
				if (atlas) Icon.sprite = atlas.GetSprite(item.PairedIconName);
                Icon.Show();

				if (expUpgradeSystem.PairedProperties.TryGetValue(item.Key, out var prop) && prop != null)
				{
					prop.Value = true;
				}
            }
            else
            {
                var upgradeItems = expUpgradeSystem.Items.Where(item=>item.CurrentLevel.Value > 0 &&!item.UpgradeFinish).ToList();

                if (upgradeItems.Any())
                {
                    var item = upgradeItems.GetRandomItem();
                    _currentItem = item;
                    _currentKey = null;
                    _currentArgs = null;
                    RefreshContent();

					var atlas = _mResLoader.LoadSync<SpriteAtlas>("icon");
					if (atlas) Icon.sprite = atlas.GetSprite(item.IconName);
                     Icon.Show();

                    item.Upgrade();
                }
                else
                {
                    if(Global.HP.Value < Global.MaxHP.Value)
                    {
                        if (UnityEngine.Random.Range(0, 1.0f) < 0.2f)
                        {
                            _currentItem = null;
                            _currentKey = "game.treasure.heal_1";
                            _currentArgs = null;
                            RefreshContent();
                            AudioKit.PlaySound("Retro Event Acute 08");
                            Global.HP.Value += 1;
                            Icon.Hide();
                            return;
                        }
                        
                    }

                    _currentItem = null;
                    _currentKey = "game.treasure.add_coin_50";
                    _currentArgs = null;
                    RefreshContent();
                    Global.Coin.Value += 50;
                    Icon.Hide();
                }
            }

            //TODO:

			
        }

        protected override void OnBeforeDestroy()
        {
			if (_mResLoader != null)
			{
				_mResLoader.Recycle2Cache();
				_mResLoader = null;
			}
        }

        public IArchitecture GetArchitecture()
        {
            return Global.Interface;
        }

        private void RefreshContent()
        {
            if (!Content) return;

            if (_currentItem != null && string.IsNullOrWhiteSpace(_currentKey))
            {
                Content.text = _currentItem.Description;
                return;
            }

            if (!string.IsNullOrWhiteSpace(_currentKey))
            {
                Content.text = _currentArgs != null && _currentArgs.Length > 0
                    ? LocalizationManager.Format(_currentKey, _currentArgs)
                    : LocalizationManager.T(_currentKey);
                return;
            }

            Content.text = string.Empty;
        }
    }
}
