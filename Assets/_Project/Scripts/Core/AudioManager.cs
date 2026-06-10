using UnityEngine;
using System.Collections.Generic;

namespace Astra.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource seSource;
        [SerializeField] private float bgmFadeDuration = 1.0f;

        private Dictionary<string, AudioClip> _bgmCache = new();
        private Dictionary<string, AudioClip> _seCache = new();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayBGM(string key, float volume = 0.7f)
        {
            if (!_bgmCache.TryGetValue(key, out var clip))
            {
                clip = Resources.Load<AudioClip>($"Audio/BGM/{key}");
                if (clip == null) return;
                _bgmCache[key] = clip;
            }

            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.volume = volume;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            bgmSource.Stop();
        }

        public void PlaySE(string key, float volume = 1.0f)
        {
            if (!_seCache.TryGetValue(key, out var clip))
            {
                clip = Resources.Load<AudioClip>($"Audio/SE/{key}");
                if (clip == null) return;
                _seCache[key] = clip;
            }

            seSource.PlayOneShot(clip, volume);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
