using UnityEngine;
using UnityEngine.UI;
using Amane.Core.Tween;

namespace Amane.UI.Effects
{
    /// <summary>
    /// 絆ランクアップ演出。DESIGN.md演出3:
    /// 「封筒が"既読"になる — 想いが届いた瞬間の温度。」
    ///
    /// 演出フロー:
    /// 1. 画面が暗転し、封筒アイコンが中央に浮かぶ
    /// 2. 封筒が開封される（回転＋スケール）
    /// 3. 中から蛍光ピンクの光が溢れる
    /// 4. 「言伝 Rank X」テキストがタイプライター表示
    /// 5. キャラ名と絆ランクが表示
    /// 6. 画面がフェードアウト
    /// </summary>
    public sealed class BondRankUpEffect : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _background;
        [SerializeField] private Image _envelope;
        [SerializeField] private Image _glowEffect;
        [SerializeField] private Text _bondTitle;
        [SerializeField] private Text _rankText;
        [SerializeField] private Text _characterName;

        private readonly Color _pinkAccent = new Color(1f, 0.243f, 0.541f);
        private readonly Color _deepBlue = new Color(0.106f, 0.165f, 0.29f);

        public void Play(string charName, int newRank, System.Action onComplete)
        {
            if (_group == null) { onComplete?.Invoke(); return; }
            _group.gameObject.SetActive(true);
            _group.alpha = 0;
            _group.blocksRaycasts = true;

            if (_background != null) _background.color = new Color(0, 0, 0, 0.9f);
            if (_bondTitle != null) { _bondTitle.text = ""; _bondTitle.color = Color.white; }
            if (_rankText != null) { _rankText.text = ""; _rankText.color = _pinkAccent; }
            if (_characterName != null) { _characterName.text = ""; _characterName.color = Color.white; }
            if (_glowEffect != null) { _glowEffect.color = new Color(_pinkAccent.r, _pinkAccent.g, _pinkAccent.b, 0); }

            // Phase 1: 暗転（0.4s）
            Tweener.Fade(_group, 0, 1, 0.4f, Easing.OutQuad).SetOnComplete(() =>
            {
                // Phase 2: 封筒が現れる（0.3s）
                if (_envelope != null)
                {
                    _envelope.color = new Color(0.93f, 0.91f, 0.85f);
                    Tweener.Scale(_envelope.transform, Vector3.zero, Vector3.one, 0.3f, Easing.OutBack);
                }

                Tweener.Float(0, 1, 0.5f, _ => { }).SetOnComplete(() =>
                {
                    // Phase 3: 開封＋光
                    if (_envelope != null)
                        Tweener.Scale(_envelope.transform, Vector3.one, new Vector3(1.2f, 0.8f, 1), 0.2f, Easing.OutQuad);

                    if (_glowEffect != null)
                    {
                        Tweener.Float(0, 0.8f, 0.3f, a =>
                        {
                            if (_glowEffect != null)
                                _glowEffect.color = new Color(_pinkAccent.r, _pinkAccent.g, _pinkAccent.b, a);
                        }, Easing.OutQuad);
                    }

                    Tweener.Float(0, 1, 0.4f, _ => { }).SetOnComplete(() =>
                    {
                        // Phase 4: テキスト
                        if (_bondTitle != null) _bondTitle.text = "言伝";
                        if (_rankText != null) _rankText.text = $"Rank {newRank}";
                        if (_characterName != null) _characterName.text = charName;

                        if (_rankText != null)
                            Tweener.Scale(_rankText.transform, Vector3.one * 2f, Vector3.one, 0.3f, Easing.OutElastic);

                        // Phase 5: 待機→フェードアウト
                        Tweener.Float(0, 1, 1.5f, _ => { }).SetOnComplete(() =>
                        {
                            Tweener.Fade(_group, 1, 0, 0.5f, Easing.InQuad).SetOnComplete(() =>
                            {
                                _group.gameObject.SetActive(false);
                                _group.blocksRaycasts = false;
                                onComplete?.Invoke();
                            });
                        });
                    });
                });
            });
        }
    }
}
