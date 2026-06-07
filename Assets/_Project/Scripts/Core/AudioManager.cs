using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace EchoesOfArcadia.Core
{
    public enum BGMTrack
    {
        None,
        Title,
        Field_Day,
        Field_Evening,
        Field_Night,
        Battle_Normal,
        Battle_Boss,
        EchoRealm,
        Dialogue_Calm,
        Dialogue_Tension,
        BondRankUp,
        Victory,
        Defeat,
        Prologue
    }

    public enum SFXType
    {
        UI_Select,
        UI_Cancel,
        UI_Confirm,
        UI_Open,
        UI_Close,
        Battle_Attack,
        Battle_WeakHit,
        Battle_CriticalHit,
        Battle_Miss,
        Battle_Heal,
        Battle_Guard,
        Battle_Resonance,
        Battle_FullResonance,
        Battle_Absorb,
        Battle_Reflect,
        Battle_Null,
        Dialogue_Next,
        Dialogue_Choice,
        Bond_PointGain,
        Bond_RankUp,
        Stat_RankUp,
        Calendar_DateChange,
        Save,
        Load
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource bgmSubSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;

        [Header("Audio Library")]
        [SerializeField] private AudioLibrary audioLibrary;

        [Header("Settings")]
        [SerializeField] private float bgmCrossfadeDuration = 1.5f;
        [SerializeField] private float defaultBGMVolume = 0.6f;
        [SerializeField] private float defaultSFXVolume = 0.8f;

        private BGMTrack currentTrack = BGMTrack.None;
        private bool isTransitioning;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (bgmSource != null)
            {
                bgmSource.loop = true;
                bgmSource.volume = defaultBGMVolume;
            }
            if (bgmSubSource != null)
            {
                bgmSubSource.loop = true;
                bgmSubSource.volume = 0f;
            }
        }

        public void PlayBGM(BGMTrack track)
        {
            if (track == currentTrack || isTransitioning) return;
            if (audioLibrary == null) return;

            var clip = audioLibrary.GetBGM(track);
            if (clip == null) return;

            if (currentTrack == BGMTrack.None || bgmSource == null || !bgmSource.isPlaying)
            {
                if (bgmSource != null)
                {
                    bgmSource.clip = clip;
                    bgmSource.volume = 0f;
                    bgmSource.Play();
                    bgmSource.DOFade(defaultBGMVolume, bgmCrossfadeDuration);
                }
                currentTrack = track;
                return;
            }

            CrossfadeBGM(clip, track);
        }

        private void CrossfadeBGM(AudioClip newClip, BGMTrack newTrack)
        {
            if (bgmSubSource == null || bgmSource == null) return;
            isTransitioning = true;

            bgmSubSource.clip = newClip;
            bgmSubSource.volume = 0f;
            bgmSubSource.Play();

            var seq = DOTween.Sequence();
            seq.Append(bgmSource.DOFade(0f, bgmCrossfadeDuration));
            seq.Join(bgmSubSource.DOFade(defaultBGMVolume, bgmCrossfadeDuration));
            seq.OnComplete(() =>
            {
                bgmSource.Stop();
                var temp = bgmSource;
                bgmSource = bgmSubSource;
                bgmSubSource = temp;
                currentTrack = newTrack;
                isTransitioning = false;
            });
        }

        public void StopBGM(float fadeOutDuration = 1f)
        {
            if (bgmSource == null || !bgmSource.isPlaying) return;
            bgmSource.DOFade(0f, fadeOutDuration).OnComplete(() =>
            {
                bgmSource.Stop();
                currentTrack = BGMTrack.None;
            });
        }

        public void PlaySFX(SFXType type)
        {
            if (audioLibrary == null) return;
            var clip = audioLibrary.GetSFX(type);
            if (clip == null) return;

            var source = IsUISound(type) ? uiSource : sfxSource;
            if (source != null)
                source.PlayOneShot(clip, defaultSFXVolume);
        }

        public void SetBGMVolume(float volume)
        {
            defaultBGMVolume = Mathf.Clamp01(volume);
            if (bgmSource != null && bgmSource.isPlaying)
                bgmSource.volume = defaultBGMVolume;
        }

        public void SetSFXVolume(float volume)
        {
            defaultSFXVolume = Mathf.Clamp01(volume);
        }

        private static bool IsUISound(SFXType type)
        {
            return type is SFXType.UI_Select or SFXType.UI_Cancel or SFXType.UI_Confirm
                or SFXType.UI_Open or SFXType.UI_Close or SFXType.Dialogue_Next
                or SFXType.Dialogue_Choice;
        }
    }
}
