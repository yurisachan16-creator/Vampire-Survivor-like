using UnityEngine;
using QFramework;
using System.Collections;

namespace VampireSurvivorLike
{
    public class LemonBuff : PowerUp
    {
        private CircleCollider2D _collider;
        private static Coroutine _activeRoutine;
        private static MonoBehaviour _routineHost;

        private void OnEnable()
        {
            PowerUpRegistry.ActiveLemonBuffCount++;
            var sr = GetComponent<SpriteRenderer>();
            LootGuideSystem.Current?.Register(this, LootGuideKind.LemonBuff, sr ? sr.sprite : null);
            LootGuideSystem.Current?.TryPlayDropFeedback(transform.position, LootGuideKind.LemonBuff);
        }

        private void OnDisable()
        {
            PowerUpRegistry.ActiveLemonBuffCount--;
            LootGuideSystem.Current?.Unregister(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<CollectableAera>(out _)) return;
            if (Global.IsGameOver.Value) return;

            FlyingToPalyer = true;
        }

        protected override void Execute()
        {
            if (Global.IsGameOver.Value)
            {
                this.DestroyGameObjGracefully();
                return;
            }

            ApplyOrRefreshBuff();
            ShowBuffFloatingText();
            AudioKit.PlaySound("Retro Event Acute 08");
            this.DestroyGameObjGracefully();
        }

        private static void ApplyOrRefreshBuff()
        {
            Global.LemonDamageBuffBonus.Value = Config.LemonBuffDamageBonus;

            var host = PowerUpManager.Default as MonoBehaviour;
            if (!host) return;

            if (_activeRoutine != null && _routineHost)
            {
                _routineHost.StopCoroutine(_activeRoutine);
            }

            _routineHost = host;
            _activeRoutine = host.StartCoroutine(BuffCountdown());
        }

        private static IEnumerator BuffCountdown()
        {
            yield return new WaitForSeconds(Config.LemonBuffDurationSeconds);
            Global.LemonDamageBuffBonus.Value = 0f;
            _activeRoutine = null;
            _routineHost = null;
        }

        private static void ShowBuffFloatingText()
        {
            if (!Player.Default) return;

            var percent = Mathf.RoundToInt(Config.LemonBuffDamageBonus * 100f);
            var text = LocalizationManager.Format("game.buff.lemon_damage_boost", percent);
            var pos = Player.Default.transform.position + Vector3.up * 1.1f;
            FloatingTextController.Play(pos, text);
        }

        protected override Collider2D Collider2D => _collider ? _collider : (_collider = GetComponent<CircleCollider2D>());
    }
}
