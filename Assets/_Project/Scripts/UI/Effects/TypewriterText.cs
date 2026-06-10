using UnityEngine;
using UnityEngine.UI;
using Amane.Core.Tween;

namespace Amane.UI.Effects
{
    /// <summary>
    /// タイプライター式テキスト表示。
    /// DESIGN.md演出9: 重要台詞前の「間」演出を preSilence で制御。
    /// P5調査: 台詞テキストに「切り抜き文字」モチーフ。
    /// 本作: 通常台詞はタイプライター、重要台詞は手書き風フェードイン。
    /// </summary>
    public sealed class TypewriterText : MonoBehaviour
    {
        [SerializeField] private Text _text;
        [SerializeField] private float _charInterval = 0.03f;
        [SerializeField] private float _punctuationPause = 0.12f;

        private string _fullText;
        private int _revealed;
        private float _timer;
        private float _currentPause;
        private bool _isRevealing;
        private System.Action _onComplete;

        public bool IsRevealing => _isRevealing;
        public bool IsComplete => !_isRevealing && _revealed >= (_fullText?.Length ?? 0);

        public void Show(string text, float preSilence = 0f, System.Action onComplete = null)
        {
            _fullText = text ?? "";
            _revealed = 0;
            _timer = 0;
            _onComplete = onComplete;

            if (_text != null) _text.text = "";

            if (preSilence > 0)
            {
                _isRevealing = false;
                Tweener.Float(0, 1, preSilence, _ => { }).SetOnComplete(() =>
                {
                    _isRevealing = true;
                });
            }
            else
            {
                _isRevealing = true;
            }
        }

        public void SkipToEnd()
        {
            if (_fullText == null) return;
            _revealed = _fullText.Length;
            _isRevealing = false;
            if (_text != null) _text.text = _fullText;
            _onComplete?.Invoke();
            _onComplete = null;
        }

        private void Update()
        {
            if (!_isRevealing || _fullText == null) return;

            _timer += UnityEngine.Time.deltaTime;
            float interval = _currentPause > 0 ? _currentPause : _charInterval;

            while (_timer >= interval && _revealed < _fullText.Length)
            {
                _timer -= interval;
                _revealed++;
                if (_text != null) _text.text = _fullText[.._revealed];

                _currentPause = 0;
                if (_revealed < _fullText.Length)
                {
                    char c = _fullText[_revealed - 1];
                    if (c == '。' || c == '！' || c == '？' || c == '…' || c == '、' || c == '.' || c == '!' || c == '?')
                        _currentPause = _punctuationPause;
                    if (c == '—' || c == '─')
                        _currentPause = _punctuationPause * 2;
                }
            }

            if (_revealed >= _fullText.Length)
            {
                _isRevealing = false;
                _onComplete?.Invoke();
                _onComplete = null;
            }
        }
    }
}
