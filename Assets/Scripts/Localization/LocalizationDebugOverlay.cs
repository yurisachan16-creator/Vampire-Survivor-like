using UnityEngine;
using System.Linq;

namespace VampireSurvivorLike
{
    public sealed class LocalizationDebugOverlay : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                LocalizationDebug.ToggleShowKeys();
            }
        }

        private void OnGUI()
        {
            var show = LocalizationDebug.ShowKeys.Value;
            var label = show ? "I18N:KEYS ON (F1)" : "I18N:KEYS OFF (F1)";
            GUI.Label(new Rect(10, 10, 260, 22), label);
            if (show)
            {
                GUI.Label(new Rect(10, 32, 360, 22), $"Missing: {LocalizationDebug.MissingKeys.Count}");
                var maxLines = 10;
                var i = 0;
                foreach (var key in LocalizationDebug.MissingKeys.OrderBy(x => x))
                {
                    if (i >= maxLines) break;
                    GUI.Label(new Rect(10, 54 + i * 18, 900, 18), key);
                    i++;
                }
            }
        }
    }
}
