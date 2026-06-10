using UnityEngine;
using UnityEngine.UI;
using Amane.Core.Tween;

namespace Amane.Battle
{
    /// <summary>
    /// アクションコマンド — 攻撃時にタイミング入力でダメージ倍率UP。
    /// ペーパーマリオ / マリオRPG 風のタイミングシステム。
    ///
    /// 仕組み:
    /// - ゲージが左→右に動く
    /// - 赤ゾーン（中央付近）でSpaceを押すと「EXCELLENT!」(x1.5)
    /// - 黄ゾーンなら「GOOD!」(x1.2)
    /// - ミスなら通常ダメージ(x1.0)
    /// </summary>
    public sealed class ActionCommand : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _gaugeBar;
        [SerializeField] private Image _gaugeFill;
        [SerializeField] private Image _cursor;
        [SerializeField] private Image _excellentZone;
        [SerializeField] private Image _goodZone;
        [SerializeField] private Text _resultText;

        private float _timer;
        private float _duration = 1.2f;
        private float _cursorProgress;
        private bool _isActive;
        private bool _hasPressed;
        private System.Action<float> _onComplete;

        // ゾーン定義（0-1の範囲で）
        private const float ExcellentMin = 0.40f;
        private const float ExcellentMax = 0.55f;
        private const float GoodMin = 0.28f;
        private const float GoodMax = 0.68f;

        private readonly Color _excellentColor = new Color(1f, 0.243f, 0.541f, 0.6f);
        private readonly Color _goodColor = new Color(1f, 0.7f, 0.2f, 0.4f);
        private readonly Color _cursorColor = Color.white;

        /// <summary>アクションコマンドを開始。完了時にダメージ倍率(1.0/1.2/1.5)をコールバック。</summary>
        public void StartCommand(System.Action<float> onComplete)
        {
            if (_group == null) { onComplete?.Invoke(1f); return; }

            _onComplete = onComplete;
            _timer = 0;
            _cursorProgress = 0;
            _hasPressed = false;
            _isActive = true;

            _group.gameObject.SetActive(true);
            _group.alpha = 1;
            _group.blocksRaycasts = true;

            if (_resultText != null) _resultText.text = "";

            // ゾーンの色設定
            if (_excellentZone != null) _excellentZone.color = _excellentColor;
            if (_goodZone != null) _goodZone.color = _goodColor;
            if (_cursor != null) _cursor.color = _cursorColor;

            // カーソルを左端に
            UpdateCursorPosition(0);
        }

        private void Update()
        {
            if (!_isActive) return;

            _timer += UnityEngine.Time.deltaTime;
            _cursorProgress = _timer / _duration;

            // カーソル移動
            UpdateCursorPosition(_cursorProgress);

            // 入力チェック
            if (!_hasPressed && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            {
                _hasPressed = true;
                EvaluatePress(_cursorProgress);
                return;
            }

            // タイムアウト
            if (_cursorProgress >= 1f)
            {
                _isActive = false;
                ShowResult("MISS", Color.gray, 1.0f);
            }
        }

        private void UpdateCursorPosition(float progress)
        {
            if (_cursor == null || _gaugeBar == null) return;
            float width = _gaugeBar.rect.width;
            float x = Mathf.Lerp(-width / 2, width / 2, Mathf.Clamp01(progress));
            _cursor.rectTransform.anchoredPosition = new Vector2(x, 0);
        }

        private void EvaluatePress(float progress)
        {
            _isActive = false;

            if (progress >= ExcellentMin && progress <= ExcellentMax)
            {
                // EXCELLENT!
                ShowResult("EXCELLENT!", new Color(1f, 0.243f, 0.541f), 1.5f);
                // 画面フラッシュ
                if (_cursor != null)
                    Tweener.Scale(_cursor.transform, Vector3.one * 2f, Vector3.one, 0.2f, Easing.OutElastic);
            }
            else if (progress >= GoodMin && progress <= GoodMax)
            {
                // GOOD!
                ShowResult("GOOD!", new Color(1f, 0.7f, 0.2f), 1.2f);
            }
            else
            {
                // MISS
                ShowResult("MISS", Color.gray, 1.0f);
            }
        }

        private void ShowResult(string text, Color color, float multiplier)
        {
            if (_resultText != null)
            {
                _resultText.text = text;
                _resultText.color = color;
                Tweener.Scale(_resultText.transform, Vector3.one * 1.5f, Vector3.one, 0.3f, Easing.OutElastic);
            }

            // 少し待ってから完了
            Tweener.Float(0, 1, 0.6f, _ => { }).SetOnComplete(() =>
            {
                if (_group != null)
                {
                    _group.gameObject.SetActive(false);
                    _group.blocksRaycasts = false;
                }
                _onComplete?.Invoke(multiplier);
                _onComplete = null;
            });
        }

        public void Hide()
        {
            _isActive = false;
            if (_group != null)
            {
                _group.gameObject.SetActive(false);
                _group.blocksRaycasts = false;
            }
        }
    }
}
