using System.Collections.Generic;
using UnityEngine;
using AriaOfEchoes.Core;

namespace AriaOfEchoes.Audio
{
    [CreateAssetMenu(fileName = "BGMData", menuName = "AriaOfEchoes/BGMData")]
    public class BGMData : ScriptableObject
    {
        public string bgmId;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.8f;
        public bool loop = true;
        public float fadeInDuration = 1f;
    }

    [CreateAssetMenu(fileName = "SEData", menuName = "AriaOfEchoes/SEData")]
    public class SEData : ScriptableObject
    {
        public string seId;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] AudioSource bgmSource;
        [SerializeField] AudioSource seSource;
        [SerializeField] List<BGMData> bgmLibrary = new();
        [SerializeField] List<SEData> seLibrary = new();

        Dictionary<string, BGMData> bgmMap = new();
        Dictionary<string, SEData> seMap = new();

        string currentBGMId;
        float masterVolume = 1f;
        float bgmVolume = 0.8f;
        float seVolume = 1f;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var bgm in bgmLibrary)
                bgmMap[bgm.bgmId] = bgm;
            foreach (var se in seLibrary)
                seMap[se.seId] = se;
        }

        public void PlayBGM(string bgmId)
        {
            if (currentBGMId == bgmId && bgmSource.isPlaying) return;
            if (!bgmMap.TryGetValue(bgmId, out var data)) return;

            currentBGMId = bgmId;
            bgmSource.clip = data.clip;
            bgmSource.volume = data.volume * bgmVolume * masterVolume;
            bgmSource.loop = data.loop;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            bgmSource.Stop();
            currentBGMId = null;
        }

        public void PlaySE(string seId)
        {
            if (!seMap.TryGetValue(seId, out var data)) return;
            seSource.PlayOneShot(data.clip, data.volume * seVolume * masterVolume);
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateBGMVolume();
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            UpdateBGMVolume();
        }

        public void SetSEVolume(float volume)
        {
            seVolume = Mathf.Clamp01(volume);
        }

        void UpdateBGMVolume()
        {
            if (bgmMap.TryGetValue(currentBGMId ?? "", out var data))
                bgmSource.volume = data.volume * bgmVolume * masterVolume;
        }
    }
}
