using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Amane.Battle;
using Amane.Core.Tween;

namespace Amane.UI.Effects
{
    /// <summary>
    /// 戦闘勝利後のEXP獲得＋レベルアップ演出。
    /// ペルソナ5風: EXPバーが溜まっていき、レベルアップ時にフラッシュ。
    /// </summary>
    public sealed class LevelUpEffect : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Text _expGainedText;      // "+50 EXP"
        [SerializeField] private Text _levelText;           // "Lv.5"
        [SerializeField] private Image _expBar;             // 経験値バー（fillAmount）
        [SerializeField] private Text _levelUpBanner;       // "LEVEL UP!" テキスト
        [SerializeField] private Image _flashOverlay;       // レベルアップ時のフラッシュ

        private Action _onComplete;

        /// <summary>
        /// EXP獲得演出を再生。
        /// </summary>
        public void Play(int expGained, int newLevel, float expProgress,
            List<LevelUpResult> levelUps, Action onComplete = null)
        {
            _onComplete = onComplete;
            gameObject.SetActive(true);
            StartCoroutine(PlaySequence(expGained, newLevel, expProgress, levelUps));
        }

        private IEnumerator PlaySequence(int expGained, int newLevel, float expProgress,
            List<LevelUpResult> levelUps)
        {
            // 初期化
            if (_group != null) _group.alpha = 0;
            if (_levelUpBanner != null) _levelUpBanner.gameObject.SetActive(false);
            if (_flashOverlay != null)
            {
                _flashOverlay.gameObject.SetActive(false);
                _flashOverlay.color = new Color(1, 1, 1, 0);
            }
            if (_expBar != null) _expBar.fillAmount = 0;

            // フェードイン
            if (_group != null)
                Tweener.Float(0, 1, 0.3f, v => { if (_group != null) _group.alpha = v; });

            yield return new WaitForSeconds(0.3f);

            // EXP数値表示
            if (_expGainedText != null)
                _expGainedText.text = $"+{expGained} EXP";

            // レベル表示
            int displayLevel = levelUps.Count > 0 ? levelUps[0].NewLevel - 1 : newLevel;
            if (_levelText != null)
                _levelText.text = $"Lv.{displayLevel}";

            yield return new WaitForSeconds(0.2f);

            // EXPバーアニメーション
            if (levelUps.Count > 0)
            {
                // レベルアップがある場合: バーを満タンにしてフラッシュ
                foreach (var lu in levelUps)
                {
                    // バーを満タンに
                    float startFill = _expBar != null ? _expBar.fillAmount : 0;
                    Tweener.Float(startFill, 1f, 0.5f, v =>
                    {
                        if (_expBar != null) _expBar.fillAmount = v;
                    }, Easing.OutQuad);

                    yield return new WaitForSeconds(0.5f);

                    // レベルアップフラッシュ
                    PlayLevelUpFlash(lu.NewLevel);

                    // SE
                    var audio = Core.Audio.AudioManager.Instance;
                    if (audio != null) audio.PlayLevelUp();

                    yield return new WaitForSeconds(0.8f);

                    // バーをリセット
                    if (_expBar != null) _expBar.fillAmount = 0;
                }

                // 最終的なバー位置
                if (_expBar != null)
                    Tweener.Float(0, expProgress, 0.3f, v =>
                    {
                        if (_expBar != null) _expBar.fillAmount = v;
                    });
            }
            else
            {
                // レベルアップなし: 現在位置までバーを伸ばす
                Tweener.Float(0, expProgress, 0.8f, v =>
                {
                    if (_expBar != null) _expBar.fillAmount = v;
                }, Easing.OutQuad);
            }

            yield return new WaitForSeconds(1.5f);

            // フェードアウト
            if (_group != null)
                Tweener.Float(1, 0, 0.5f, v => { if (_group != null) _group.alpha = v; });

            yield return new WaitForSeconds(0.5f);

            gameObject.SetActive(false);
            _onComplete?.Invoke();
        }

        private void PlayLevelUpFlash(int newLevel)
        {
            // レベルテキスト更新
            if (_levelText != null)
                _levelText.text = $"Lv.{newLevel}";

            // "LEVEL UP!" バナー
            if (_levelUpBanner != null)
            {
                _levelUpBanner.gameObject.SetActive(true);
                _levelUpBanner.text = "LEVEL UP!";
                var rt = _levelUpBanner.GetComponent<RectTransform>();
                if (rt != null)
                    Tweener.Scale(rt, Vector3.one * 1.5f, Vector3.one, 0.3f, Easing.OutBack);
            }

            // フラッシュ
            if (_flashOverlay != null)
            {
                _flashOverlay.gameObject.SetActive(true);
                Tweener.Float(0.6f, 0f, 0.4f, v =>
                {
                    if (_flashOverlay != null)
                        _flashOverlay.color = new Color(1f, 0.243f, 0.541f, v);
                });
            }
        }
    }
}
