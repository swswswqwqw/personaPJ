using UnityEngine;
using UnityEngine.UI;
using Amane.Core.Tween;

namespace Amane.UI.Effects
{
    /// <summary>
    /// 画面遷移演出。
    /// P5調査: シーン遷移アニメはコンテキストに応じて変化する（地下鉄→電車内の人々）。
    /// 本作: 潜行＝水紋が広がる／フィールド復帰＝付箋が剥がれる／タイトル＝結露ワイプ。
    /// 全遷移に「墨が塗られていく」モチーフを共通言語として使う。
    /// </summary>
    public sealed class TransitionEffect : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _overlay;
        [SerializeField] private Text _transitionText;

        private static TransitionEffect _instance;
        public static TransitionEffect Instance => _instance;

        private readonly Color _inkBlack = new Color(0.078f, 0.067f, 0.059f);
        private readonly Color _deepBlue = new Color(0.106f, 0.165f, 0.29f);

        private void Awake()
        {
            _instance = this;
            if (_group != null) { _group.alpha = 0; _group.blocksRaycasts = false; }
        }

        /// <summary>フェードアウト→コールバック→フェードイン。</summary>
        public void Play(string text, float fadeOutTime, float holdTime, float fadeInTime,
                         System.Action onMidpoint, Color? color = null)
        {
            if (_group == null) { onMidpoint?.Invoke(); return; }

            _group.blocksRaycasts = true;
            if (_overlay != null) _overlay.color = color ?? _inkBlack;
            if (_transitionText != null)
            {
                _transitionText.text = text;
                _transitionText.color = Color.white;
            }

            Tweener.Fade(_group, 0, 1, fadeOutTime, Easing.InQuad).SetOnComplete(() =>
            {
                onMidpoint?.Invoke();
                Tweener.Float(0, 1, holdTime, _ => { }).SetOnComplete(() =>
                {
                    Tweener.Fade(_group, 1, 0, fadeInTime, Easing.OutQuad)
                        .SetOnComplete(() => _group.blocksRaycasts = false);
                });
            });
        }

        /// <summary>
        /// 潜行演出（リッチ版）: DESIGN.md必須演出7番「アプリ未読起動→水紋→世界裏返り」。
        /// 3フェーズで二重世界への没入感を演出する。
        /// </summary>
        public void PlayDiveTransition(System.Action onMidpoint)
        {
            if (_group == null) { onMidpoint?.Invoke(); return; }

            _group.blocksRaycasts = true;
            var fluorPink = new Color(1f, 0.243f, 0.541f);

            // フェーズ1: アプリ「未読」通知 — ピンクの蛍光
            if (_overlay != null) _overlay.color = _deepBlue;
            SetText("——〈未読〉が、届いている。", fluorPink);

            Tweener.Fade(_group, 0f, 0.7f, 0.35f, Easing.InQuad).SetOnComplete(() =>
            {
                // フェーズ2: 水紋が広がる
                SetText("水紋が、広がる……", Color.white);

                Tweener.Float(0f, 1f, 0.45f, _ => { }).SetOnComplete(() =>
                {
                    // フェーズ3: 世界が裏返る — 完全暗転
                    if (_overlay != null) _overlay.color = new Color(0.02f, 0.02f, 0.08f);
                    SetText("世界が、裏返る。", fluorPink);

                    Tweener.Fade(_group, 0.7f, 1f, 0.3f, Easing.InCubic).SetOnComplete(() =>
                    {
                        onMidpoint?.Invoke();
                        Tweener.Float(0f, 1f, 0.6f, _ => { }).SetOnComplete(() =>
                        {
                            Tweener.Fade(_group, 1f, 0f, 0.5f, Easing.OutQuad)
                                .SetOnComplete(() => _group.blocksRaycasts = false);
                        });
                    });
                });
            });
        }

        /// <summary>
        /// 深夜強行潜行演出（DESIGN.md 9-2）: 重く切迫した暗紫の演出。
        /// 「疲れているのに潜らなければならない」焦燥をビジュアルで伝える。
        /// </summary>
        public void PlayMidnightDiveTransition(System.Action onMidpoint)
        {
            if (_group == null) { onMidpoint?.Invoke(); return; }

            _group.blocksRaycasts = true;
            var darkPurple = new Color(0.05f, 0.02f, 0.1f);
            if (_overlay != null) _overlay.color = darkPurple;
            SetText("深夜——それでも、潜らなければ。", new Color(1f, 0.243f, 0.541f));

            // ゆっくりと重く暗転
            Tweener.Fade(_group, 0f, 1f, 0.9f, Easing.InCubic).SetOnComplete(() =>
            {
                onMidpoint?.Invoke();
                Tweener.Float(0f, 1f, 0.8f, _ => { }).SetOnComplete(() =>
                {
                    Tweener.Fade(_group, 1f, 0f, 0.6f, Easing.OutQuad)
                        .SetOnComplete(() => _group.blocksRaycasts = false);
                });
            });
        }

        private void SetText(string text, Color color)
        {
            if (_transitionText == null) return;
            _transitionText.text = text;
            _transitionText.color = color;
        }

        /// <summary>フィールド復帰。</summary>
        public void PlayReturnTransition(System.Action onMidpoint)
        {
            Play("", 0.4f, 0.3f, 0.4f, onMidpoint, _inkBlack);
        }

        /// <summary>時間帯変更。</summary>
        public void PlayTimeAdvance(string slotName, System.Action onMidpoint)
        {
            Play(slotName, 0.3f, 0.4f, 0.3f, onMidpoint, new Color(0, 0, 0, 0.85f));
        }
    }
}
