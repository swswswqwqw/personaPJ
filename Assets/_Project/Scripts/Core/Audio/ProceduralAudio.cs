using UnityEngine;
using System;

namespace Amane.Core.Audio
{
    /// <summary>
    /// プロシージャル音声生成。外部ファイル不要で音を作る。
    /// </summary>
    public static class ProceduralAudio
    {
        private const int SampleRate = 44100;

        /// <summary>正弦波トーンのAudioClipを生成</summary>
        public static AudioClip CreateTone(string name, float frequency, float duration,
            float volume = 0.3f, WaveShape shape = WaveShape.Sine)
        {
            int samples = (int)(SampleRate * duration);
            var clip = AudioClip.Create(name, samples, 1, SampleRate, false);
            var data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float wave = GenerateWave(frequency, t, shape);
                // フェードイン/アウト（クリックノイズ防止）
                float env = Envelope(t, duration, 0.01f, 0.05f);
                data[i] = wave * volume * env;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>SE用の短い打撃音</summary>
        public static AudioClip CreateImpact(string name, float duration = 0.15f, float pitch = 200f)
        {
            int samples = (int)(SampleRate * duration);
            var clip = AudioClip.Create(name, samples, 1, SampleRate, false);
            var data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float noise = (UnityEngine.Random.value * 2f - 1f);
                float tone = Mathf.Sin(2f * Mathf.PI * pitch * t * (1f - t / duration));
                float env = Mathf.Exp(-t * 30f); // 急速減衰
                data[i] = (noise * 0.3f + tone * 0.7f) * env * 0.4f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>UI操作音（クリック・選択）</summary>
        public static AudioClip CreateUIClick(string name = "UIClick")
        {
            int samples = (int)(SampleRate * 0.05f);
            var clip = AudioClip.Create(name, samples, 1, SampleRate, false);
            var data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float wave = Mathf.Sin(2f * Mathf.PI * 1200f * t);
                float env = Mathf.Exp(-t * 80f);
                data[i] = wave * env * 0.2f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>弱点ヒット音（ガラスが割れるような）</summary>
        public static AudioClip CreateWeakHitSE()
        {
            float duration = 0.4f;
            int samples = (int)(SampleRate * duration);
            var clip = AudioClip.Create("WeakHit", samples, 1, SampleRate, false);
            var data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float noise = (UnityEngine.Random.value * 2f - 1f);
                float highTone = Mathf.Sin(2f * Mathf.PI * 3000f * t);
                float midTone = Mathf.Sin(2f * Mathf.PI * 800f * t);
                float env1 = Mathf.Exp(-t * 15f);
                float env2 = Mathf.Exp(-t * 8f);
                data[i] = (noise * 0.4f * env1 + highTone * 0.3f * env1 + midTone * 0.3f * env2) * 0.35f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>レベルアップ音（上昇アルペジオ）</summary>
        public static AudioClip CreateLevelUpSE()
        {
            float duration = 1.0f;
            int samples = (int)(SampleRate * duration);
            var clip = AudioClip.Create("LevelUp", samples, 1, SampleRate, false);
            var data = new float[samples];

            // C5 → E5 → G5 → C6 のアルペジオ
            float[] freqs = { 523.25f, 659.25f, 783.99f, 1046.50f };
            float noteLen = duration / freqs.Length;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                int noteIdx = Mathf.Min((int)(t / noteLen), freqs.Length - 1);
                float noteT = t - noteIdx * noteLen;
                float freq = freqs[noteIdx];
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f
                           + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.2f; // オーバートーン
                float env = Mathf.Exp(-noteT * 3f);
                float masterEnv = Envelope(t, duration, 0.01f, 0.2f);
                data[i] = wave * env * masterEnv * 0.3f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// BGM用のループ可能なアンビエント。
        /// ペルソナの雰囲気: ジャズ的な和音 + リバーブ感。
        /// </summary>
        public static AudioClip CreateAmbientBGM(string name, float duration, BGMStyle style)
        {
            int samples = (int)(SampleRate * duration);
            var clip = AudioClip.Create(name, samples, 1, SampleRate, false);
            var data = new float[samples];

            float[] chordFreqs;
            float tempo;
            float volume;

            switch (style)
            {
                case BGMStyle.Title:
                    // Am7 系: A3-C4-E4-G4 — 物悲しく美しい
                    chordFreqs = new[] { 220f, 261.63f, 329.63f, 392f };
                    tempo = 0.5f; volume = 0.12f;
                    break;
                case BGMStyle.Field:
                    // Dm9 系: D3-F3-A3-C4-E4 — 日常の哀愁
                    chordFreqs = new[] { 146.83f, 174.61f, 220f, 261.63f, 329.63f };
                    tempo = 0.7f; volume = 0.08f;
                    break;
                case BGMStyle.Battle:
                    // Em系: E2-B2-E3-G3 — 緊張感
                    chordFreqs = new[] { 82.41f, 123.47f, 164.81f, 196f };
                    tempo = 1.5f; volume = 0.10f;
                    break;
                default:
                    chordFreqs = new[] { 220f, 261.63f, 329.63f };
                    tempo = 0.5f; volume = 0.10f;
                    break;
            }

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float value = 0f;

                for (int c = 0; c < chordFreqs.Length; c++)
                {
                    float freq = chordFreqs[c];
                    // ゆっくりとした音量変動（呼吸するような）
                    float breathe = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * tempo * t + c * 0.7f);
                    float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.4f
                               + Mathf.Sin(2f * Mathf.PI * freq * 2.01f * t) * 0.2f  // 微妙にデチューン
                               + Mathf.Sin(2f * Mathf.PI * freq * 3f * t) * 0.1f;     // 倍音
                    value += wave * breathe / chordFreqs.Length;
                }

                // 戦闘BGMにリズム要素を追加
                if (style == BGMStyle.Battle)
                {
                    float beat = Mathf.Repeat(t * tempo * 4f, 1f);
                    float kick = beat < 0.1f ? Mathf.Sin(2f * Mathf.PI * 60f * beat * 10f) * (1f - beat * 10f) : 0f;
                    value += kick * 0.3f;
                }

                // クロスフェード用のループエンベロープ
                float loopEnv = 1f;
                float fadeTime = 0.5f;
                if (t < fadeTime) loopEnv = t / fadeTime;
                if (t > duration - fadeTime) loopEnv = (duration - t) / fadeTime;

                data[i] = Mathf.Clamp(value * volume * loopEnv, -1f, 1f);
            }

            clip.SetData(data, 0);
            return clip;
        }

        // ---- ヘルパー ----

        private static float GenerateWave(float freq, float t, WaveShape shape)
        {
            float phase = freq * t;
            return shape switch
            {
                WaveShape.Sine => Mathf.Sin(2f * Mathf.PI * phase),
                WaveShape.Square => Mathf.Sign(Mathf.Sin(2f * Mathf.PI * phase)),
                WaveShape.Triangle => 2f * Mathf.Abs(2f * (phase - Mathf.Floor(phase + 0.5f))) - 1f,
                WaveShape.Saw => 2f * (phase - Mathf.Floor(phase)) - 1f,
                _ => Mathf.Sin(2f * Mathf.PI * phase)
            };
        }

        private static float Envelope(float t, float duration, float attack, float release)
        {
            if (t < attack) return t / attack;
            if (t > duration - release) return (duration - t) / release;
            return 1f;
        }
    }

    public enum WaveShape { Sine, Square, Triangle, Saw }
    public enum BGMStyle { Title, Field, Battle }
}
