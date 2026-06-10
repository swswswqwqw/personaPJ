using System.Collections.Generic;
using UnityEngine;

namespace ArcadiaOfEchoes.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource seSource;
        [SerializeField] private float bgmFadeDuration = 1.0f;

        [Header("BGM Library")]
        [SerializeField] private List<BGMEntry> bgmLibrary;

        [Header("SE Library")]
        [SerializeField] private List<SEEntry> seLibrary;

        private readonly Dictionary<string, AudioClip> _bgmCache = new();
        private readonly Dictionary<string, AudioClip> _seCache = new();
        private float _bgmVolume = 0.7f;
        private float _seVolume = 1.0f;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CacheAudioClips();
        }

        public void PlayBGM(string bgmId, bool fade = true)
        {
            if (!_bgmCache.TryGetValue(bgmId, out var clip)) return;
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            if (fade && bgmSource.isPlaying)
            {
                StartCoroutine(CrossFadeBGM(clip));
            }
            else
            {
                bgmSource.clip = clip;
                bgmSource.volume = _bgmVolume;
                bgmSource.loop = true;
                bgmSource.Play();
            }
        }

        public void StopBGM(bool fade = true)
        {
            if (fade)
                StartCoroutine(FadeOutBGM());
            else
                bgmSource.Stop();
        }

        public void PlaySE(string seId)
        {
            if (_seCache.TryGetValue(seId, out var clip))
                seSource.PlayOneShot(clip, _seVolume);
        }

        public void SetBGMVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            if (bgmSource != null)
                bgmSource.volume = _bgmVolume;
        }

        public void SetSEVolume(float volume)
        {
            _seVolume = Mathf.Clamp01(volume);
        }

        private void CacheAudioClips()
        {
            if (bgmLibrary != null)
                foreach (var entry in bgmLibrary)
                    if (entry.Clip != null)
                        _bgmCache[entry.Id] = entry.Clip;

            if (seLibrary != null)
                foreach (var entry in seLibrary)
                    if (entry.Clip != null)
                        _seCache[entry.Id] = entry.Clip;
        }

        private System.Collections.IEnumerator CrossFadeBGM(AudioClip newClip)
        {
            float elapsed = 0f;
            float startVolume = bgmSource.volume;

            while (elapsed < bgmFadeDuration * 0.5f)
            {
                elapsed += UnityEngine.Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (bgmFadeDuration * 0.5f));
                yield return null;
            }

            bgmSource.clip = newClip;
            bgmSource.Play();
            elapsed = 0f;

            while (elapsed < bgmFadeDuration * 0.5f)
            {
                elapsed += UnityEngine.Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(0f, _bgmVolume, elapsed / (bgmFadeDuration * 0.5f));
                yield return null;
            }

            bgmSource.volume = _bgmVolume;
        }

        private System.Collections.IEnumerator FadeOutBGM()
        {
            float elapsed = 0f;
            float startVolume = bgmSource.volume;

            while (elapsed < bgmFadeDuration)
            {
                elapsed += UnityEngine.Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / bgmFadeDuration);
                yield return null;
            }

            bgmSource.Stop();
            bgmSource.volume = _bgmVolume;
        }
    }

    [System.Serializable]
    public class BGMEntry
    {
        public string Id;
        public string DisplayName;
        public AudioClip Clip;
    }

    [System.Serializable]
    public class SEEntry
    {
        public string Id;
        public AudioClip Clip;
    }
}
