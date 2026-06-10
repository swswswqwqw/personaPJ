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

        /// <summary>潜行演出: 水紋＋深い藍。</summary>
        public void PlayDiveTransition(System.Action onMidpoint)
        {
            Play("未言界へ潜行する……", 0.6f, 0.8f, 0.5f, onMidpoint, _deepBlue);
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
