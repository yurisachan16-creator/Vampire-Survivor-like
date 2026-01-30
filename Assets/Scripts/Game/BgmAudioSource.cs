using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
    /// <summary>
    /// 将场景中的 AudioSource 绑定到 AudioKit 的音乐音量设置
    /// 挂载到播放背景音乐的 GameObject 上（如 Main Camera）
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class BgmAudioSource : MonoBehaviour
    {
        private AudioSource _audioSource;
        private float _baseVolume;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            // 保存原始音量作为基准
            _baseVolume = _audioSource.volume;
        }

        private void Start()
        {
            // 绑定到 AudioKit 的音乐音量设置
            AudioKit.Settings.MusicVolume.RegisterWithInitValue(volume =>
            {
                _audioSource.volume = _baseVolume * volume;
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            // 绑定到音乐开关设置
            AudioKit.Settings.IsMusicOn.RegisterWithInitValue(isOn =>
            {
                _audioSource.mute = !isOn;
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }
    }
}
