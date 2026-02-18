using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    /// <summary>
    /// 音效节流器：防止同一音效在极短时间内重复播放导致 FMOD 资源耗尽。
    /// 在大量敌人同时死亡时（后期高密度刷怪），每帧可能触发数十次 AudioSource 创建，
    /// 这会耗尽 FMOD channel group 资源。本类通过两层限制来缓解：
    /// 1. 每帧全局播放总数上限
    /// 2. 同一音效的最短冷却间隔
    /// </summary>
    public static class SfxThrottle
    {
        /// <summary>同一音效两次播放之间的最短间隔（秒）</summary>
        private const float DefaultCooldown = 0.05f;

        /// <summary>每帧允许播放的最大音效总数</summary>
        public const int MaxSoundsPerFrame = 8;

        private static readonly Dictionary<string, float> _lastPlayTime = new Dictionary<string, float>(32);
        private static int _frameSoundCount;
        private static int _lastCountedFrame;

        /// <summary>
        /// 检查是否可以播放指定音效。通过返回 true 并记录时间。
        /// </summary>
        public static bool CanPlay(string key, float cooldown = DefaultCooldown)
        {
            // 每帧总数限制
            var frame = Time.frameCount;
            if (frame != _lastCountedFrame)
            {
                _lastCountedFrame = frame;
                _frameSoundCount = 0;
            }

            if (_frameSoundCount >= MaxSoundsPerFrame) return false;

            // 单音效冷却
            var now = Time.unscaledTime;
            if (_lastPlayTime.TryGetValue(key, out var last) && now - last < cooldown)
                return false;

            _lastPlayTime[key] = now;
            _frameSoundCount++;
            return true;
        }

        /// <summary>
        /// 重置所有节流状态（场景切换时调用）
        /// </summary>
        public static void Reset()
        {
            _lastPlayTime.Clear();
            _frameSoundCount = 0;
        }
    }
}
