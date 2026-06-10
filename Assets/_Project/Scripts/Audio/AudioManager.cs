using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ArcanaOfHollows.Core;

namespace ArcanaOfHollows.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource seSource;
        [SerializeField] private float bgmFadeDuration = 1.0f;

        [Header("BGM Clips")]
        [SerializeField] private AudioClip titleBGM;
        [SerializeField] private AudioClip fieldBGM;
        [SerializeField] private AudioClip battleBGM;
        [SerializeField] private AudioClip bossBGM;
        [SerializeField] private AudioClip socialBGM;
        [SerializeField] private AudioClip dungeonBGM;

        [Header("SE Clips")]
        [SerializeField] private AudioClip seConfirm;
        [SerializeField] private AudioClip seCancel;
        [SerializeField] private AudioClip seCursor;
        [SerializeField] private AudioClip seWeaknessHit;
        [SerializeField] private AudioClip seCritical;
        [SerializeField] private AudioClip seHeartStringUp;
        [SerializeField] private AudioClip seOneMore;

        private float bgmVolume = 0.7f;
        private float seVolume = 0.8f;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            EventBus.Subscribe<GamePhaseChangedEvent>(OnPhaseChanged);
            EventBus.Subscribe<Battle.OneMoreEvent>(OnOneMore);
            EventBus.Subscribe<Social.HeartStringRankUpEvent>(OnHeartStringRankUp);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GamePhaseChangedEvent>(OnPhaseChanged);
            EventBus.Unsubscribe<Battle.OneMoreEvent>(OnOneMore);
            EventBus.Unsubscribe<Social.HeartStringRankUpEvent>(OnHeartStringRankUp);
        }

        public async Task PlayBGM(AudioClip clip)
        {
            if (bgmSource == null || clip == null) return;
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            await FadeOutBGM();
            bgmSource.clip = clip;
            bgmSource.volume = 0f;
            bgmSource.loop = true;
            bgmSource.Play();
            await FadeInBGM();
        }

        public void PlaySE(AudioClip clip)
        {
            if (seSource == null || clip == null) return;
            seSource.PlayOneShot(clip, seVolume);
        }

        public void PlayConfirm() => PlaySE(seConfirm);
        public void PlayCancel() => PlaySE(seCancel);
        public void PlayCursor() => PlaySE(seCursor);
        public void PlayWeaknessHit() => PlaySE(seWeaknessHit);
        public void PlayCritical() => PlaySE(seCritical);

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            if (bgmSource != null)
                bgmSource.volume = bgmVolume;
        }

        public void SetSEVolume(float volume)
        {
            seVolume = Mathf.Clamp01(volume);
        }

        private async Task FadeOutBGM()
        {
            if (bgmSource == null || !bgmSource.isPlaying) return;

            float startVolume = bgmSource.volume;
            float elapsed = 0f;
            while (elapsed < bgmFadeDuration)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / bgmFadeDuration);
                await Task.Yield();
            }
            bgmSource.Stop();
        }

        private async Task FadeInBGM()
        {
            if (bgmSource == null) return;

            float elapsed = 0f;
            while (elapsed < bgmFadeDuration)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(0f, bgmVolume, elapsed / bgmFadeDuration);
                await Task.Yield();
            }
            bgmSource.volume = bgmVolume;
        }

        private void OnPhaseChanged(GamePhaseChangedEvent e)
        {
            var clip = e.NewPhase switch
            {
                GamePhase.Title => titleBGM,
                GamePhase.Field => fieldBGM,
                GamePhase.Battle => battleBGM,
                GamePhase.Social => socialBGM,
                GamePhase.Dungeon => dungeonBGM,
                _ => null
            };
            if (clip != null) _ = PlayBGM(clip);
        }

        private void OnOneMore(Battle.OneMoreEvent e) => PlaySE(seOneMore);
        private void OnHeartStringRankUp(Social.HeartStringRankUpEvent e) => PlaySE(seHeartStringUp);
    }
}
