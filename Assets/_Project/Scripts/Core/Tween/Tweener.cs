using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amane.Core.Tween
{
    /// <summary>
    /// 軽量トゥイーンエンジン。DOTween非依存で演出の基盤を担う。
    /// ペルソナ5調査結果: 「モーションがリズムを制御する」—
    /// 全UIアニメーションをこのエンジンで駆動し、1フレーム単位の調整を可能にする。
    /// </summary>
    public sealed class Tweener : MonoBehaviour
    {
        private static Tweener _instance;
        private readonly List<TweenHandle> _active = new();
        private readonly List<TweenHandle> _toAdd = new();
        private readonly List<TweenHandle> _toRemove = new();

        public static Tweener Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("[Tweener]");
                    _instance = obj.AddComponent<Tweener>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        private void Update()
        {
            if (_toAdd.Count > 0) { _active.AddRange(_toAdd); _toAdd.Clear(); }

            float dt = UnityEngine.Time.deltaTime;
            foreach (var tw in _active)
            {
                tw.Elapsed += dt;
                float t = Mathf.Clamp01(tw.Elapsed / tw.Duration);
                float eased = tw.Ease(t);
                tw.Update(eased);
                if (t >= 1f)
                {
                    tw.OnComplete?.Invoke();
                    _toRemove.Add(tw);
                }
            }

            if (_toRemove.Count > 0)
            {
                foreach (var r in _toRemove) _active.Remove(r);
                _toRemove.Clear();
            }
        }

        // ---- Static API ----

        public static TweenHandle Float(float from, float to, float duration, Action<float> onUpdate,
                                        Func<float, float> ease = null)
        {
            var h = new TweenHandle(duration, ease ?? Easing.OutQuad,
                t => onUpdate(Mathf.LerpUnclamped(from, to, t)));
            Instance._toAdd.Add(h);
            return h;
        }

        public static TweenHandle Move(RectTransform rt, Vector2 from, Vector2 to, float duration,
                                       Func<float, float> ease = null)
        {
            return Float(0, 1, duration,
                t => { if (rt != null) rt.anchoredPosition = Vector2.LerpUnclamped(from, to, t); },
                ease);
        }

        public static TweenHandle Scale(Transform tr, Vector3 from, Vector3 to, float duration,
                                        Func<float, float> ease = null)
        {
            return Float(0, 1, duration,
                t => { if (tr != null) tr.localScale = Vector3.LerpUnclamped(from, to, t); },
                ease);
        }

        public static TweenHandle Fade(CanvasGroup cg, float from, float to, float duration,
                                       Func<float, float> ease = null)
        {
            return Float(from, to, duration,
                v => { if (cg != null) cg.alpha = v; },
                ease);
        }

        public static TweenHandle Color(UnityEngine.UI.Graphic g, UnityEngine.Color from,
                                        UnityEngine.Color to, float duration, Func<float, float> ease = null)
        {
            return Float(0, 1, duration,
                t => { if (g != null) g.color = UnityEngine.Color.LerpUnclamped(from, to, t); },
                ease);
        }

        public static TweenHandle Shake(Transform tr, float intensity, float duration, float frequency = 30f)
        {
            var original = tr != null ? tr.localPosition : Vector3.zero;
            return Float(0, 1, duration,
                t =>
                {
                    if (tr == null) return;
                    float decay = 1f - t;
                    float x = Mathf.PerlinNoise(UnityEngine.Time.time * frequency, 0f) * 2f - 1f;
                    float y = Mathf.PerlinNoise(0f, UnityEngine.Time.time * frequency) * 2f - 1f;
                    tr.localPosition = original + new Vector3(x, y, 0) * intensity * decay;
                },
                Easing.Linear).SetOnComplete(() => { if (tr != null) tr.localPosition = original; });
        }

        public static TweenHandle Punch(Transform tr, Vector3 punch, float duration)
        {
            var original = tr != null ? tr.localScale : Vector3.one;
            return Float(0, 1, duration,
                t =>
                {
                    if (tr == null) return;
                    float wave = Mathf.Sin(t * Mathf.PI) * (1f - t);
                    tr.localScale = original + punch * wave;
                },
                Easing.Linear).SetOnComplete(() => { if (tr != null) tr.localScale = original; });
        }

        public static TweenHandle Sequence(params (float delay, Action action)[] steps)
        {
            float totalDuration = 0;
            foreach (var s in steps) totalDuration += s.delay;
            float accumulated = 0;
            float[] thresholds = new float[steps.Length];
            for (int i = 0; i < steps.Length; i++)
            {
                accumulated += steps[i].delay;
                thresholds[i] = accumulated / totalDuration;
            }
            int current = 0;
            return Float(0, 1, totalDuration,
                t =>
                {
                    while (current < steps.Length && t >= thresholds[current])
                    {
                        steps[current].action?.Invoke();
                        current++;
                    }
                }, Easing.Linear);
        }

        public static void KillAll()
        {
            Instance._active.Clear();
            Instance._toAdd.Clear();
        }
    }

    public sealed class TweenHandle
    {
        public float Duration;
        public float Elapsed;
        public Func<float, float> Ease;
        public Action<float> Update;
        public Action OnComplete;

        public TweenHandle(float duration, Func<float, float> ease, Action<float> update)
        {
            Duration = duration;
            Ease = ease ?? Easing.Linear;
            Update = update;
        }

        public TweenHandle SetOnComplete(Action action) { OnComplete = action; return this; }
    }

    public static class Easing
    {
        public static readonly Func<float, float> Linear = t => t;
        public static readonly Func<float, float> InQuad = t => t * t;
        public static readonly Func<float, float> OutQuad = t => t * (2 - t);
        public static readonly Func<float, float> InOutQuad = t => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
        public static readonly Func<float, float> OutBack = t => { float s = 1.70158f; return --t * t * ((s + 1) * t + s) + 1; };
        public static readonly Func<float, float> OutElastic = t =>
        {
            if (t <= 0 || t >= 1) return t;
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - 0.075f) * (2 * Mathf.PI) / 0.3f) + 1;
        };
        public static readonly Func<float, float> OutBounce = t =>
        {
            if (t < 1 / 2.75f) return 7.5625f * t * t;
            if (t < 2 / 2.75f) { t -= 1.5f / 2.75f; return 7.5625f * t * t + 0.75f; }
            if (t < 2.5f / 2.75f) { t -= 2.25f / 2.75f; return 7.5625f * t * t + 0.9375f; }
            t -= 2.625f / 2.75f; return 7.5625f * t * t + 0.984375f;
        };
    }
}
