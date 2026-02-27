using UnityEngine;

namespace VampireSurvivorLike
{
    public static class RuntimeFontFallback
    {
        private static Font _cachedFont;

        public static Font Get()
        {
            if (_cachedFont) return _cachedFont;

            try
            {
                _cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_cachedFont) return _cachedFont;
            }
            catch
            {
            }

            try
            {
                _cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (_cachedFont) return _cachedFont;
            }
            catch
            {
            }

            return null;
        }
    }
}
