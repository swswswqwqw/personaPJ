using UnityEngine;
using System.Collections.Generic;

namespace Amane.Core.Audio
{
    /// <summary>
    /// ゲーム全体の音響管理。BGM/SE を制御する。
    /// 外部ファイル不要：ProceduralAudio でランタイム生成。
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _bgmSource;
        private AudioSource _seSource;
        private AudioSource _ambientSource;

        // キャッシュ
        private readonly Dictionary<string, AudioClip> _seCache = new();
        private readonly Dictionary<BGMStyle, AudioClip> _bgmCache = new();

        private BGMStyle? _currentBGM;
        private float _bgmVolume = 0.5f;
        private float _seVolume = 0.7f;
        private float _fadeSpeed = 1.5f;
        private float _targetBGMVolume;
        private bool _fading;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // AudioSource生成
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
            _bgmSource.volume = 0;

            _seSource = gameObject.AddComponent<AudioSource>();
            _seSource.loop = false;
            _seSource.playOnAwake = false;

            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _ambientSource.playOnAwake = false;
            _ambientSource.volume = 0.05f;

            PreloadSE();
            PreloadBGM();
            StartAmbientRain();
        }

        private void Update()
        {
            // BGMフェードイン/アウト
            if (_fading)
            {
                _bgmSource.volume = Mathf.MoveTowards(
                    _bgmSource.volume, _targetBGMVolume,
                    _fadeSpeed * UnityEngine.Time.deltaTime);

                if (Mathf.Approximately(_bgmSource.volume, _targetBGMVolume))
                    _fading = false;
            }
        }

        // =============================================
        //  BGM
        // =============================================
        public void PlayBGM(BGMStyle style)
        {
            if (_currentBGM == style && _bgmSource.isPlaying) return;

            _currentBGM = style;

            if (!_bgmCache.TryGetValue(style, out var clip))
            {
                clip = ProceduralAudio.CreateAmbientBGM($"BGM_{style}", 16f, style);
                _bgmCache[style] = clip;
            }

            // クロスフェード
            _targetBGMVolume = 0f;
            _fading = true;

            // 少し待ってから新BGMを開始（簡易クロスフェード）
            StartCoroutine(SwitchBGMCoroutine(clip));
        }

        private System.Collections.IEnumerator SwitchBGMCoroutine(AudioClip newClip)
        {
            // フェードアウト
            _targetBGMVolume = 0f;
            _fading = true;

            while (_bgmSource.volume > 0.01f)
                yield return null;

            _bgmSource.clip = newClip;
            _bgmSource.Play();

            // フェードイン
            _targetBGMVolume = _bgmVolume;
            _fading = true;
        }

        public void StopBGM()
        {
            _currentBGM = null;
            _targetBGMVolume = 0f;
            _fading = true;
        }

        // =============================================
        //  SE
        // =============================================
        public void PlaySE(string seName)
        {
            if (_seCache.TryGetValue(seName, out var clip))
            {
                _seSource.PlayOneShot(clip, _seVolume);
            }
        }

        public void PlayUIClick() => PlaySE("ui_click");
        public void PlayWeakHit() => PlaySE("weak_hit");
        public void PlayLevelUp() => PlaySE("level_up");
        public void PlayImpact() => PlaySE("impact");
        public void PlayInteract() => PlaySE("interact");

        // =============================================
        //  環境音（雨）— 雨音市の雰囲気
        // =============================================
        private void StartAmbientRain()
        {
            // 雨音＝低周波ノイズをループ
            float duration = 4f;
            int samples = (int)(44100 * duration);
            var clip = AudioClip.Create("Rain", samples, 1, 44100, false);
            var data = new float[samples];

            // ブラウンノイズ（低周波フィルタ済みホワイトノイズ）
            float last = 0;
            for (int i = 0; i < samples; i++)
            {
                float white = Random.value * 2f - 1f;
                last = (last + 0.02f * white) / 1.02f; // ローパスフィルタ
                float t = (float)i / 44100;
                // 雨粒のランダムなタップ音を混ぜる
                float tap = Random.value < 0.001f ? Random.value * 0.3f : 0f;
                float env = 1f;
                float fadeTime = 0.5f;
                if (t < fadeTime) env = t / fadeTime;
                if (t > duration - fadeTime) env = (duration - t) / fadeTime;
                data[i] = (last * 3f + tap) * env * 0.08f;
            }

            clip.SetData(data, 0);
            _ambientSource.clip = clip;
            _ambientSource.Play();
        }

        public void SetAmbientVolume(float vol)
        {
            if (_ambientSource != null)
                _ambientSource.volume = Mathf.Clamp01(vol);
        }

        // =============================================
        //  プリロード
        // =============================================
        private void PreloadSE()
        {
            _seCache["ui_click"] = ProceduralAudio.CreateUIClick();
            _seCache["weak_hit"] = ProceduralAudio.CreateWeakHitSE();
            _seCache["level_up"] = ProceduralAudio.CreateLevelUpSE();
            _seCache["impact"] = ProceduralAudio.CreateImpact("Impact", 0.15f, 200f);
            _seCache["interact"] = ProceduralAudio.CreateTone("Interact", 880f, 0.1f, 0.15f);
        }

        private void PreloadBGM()
        {
            // BGMは使用時に遅延生成（メモリ節約）
            // 必要ならここでプリロード
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
