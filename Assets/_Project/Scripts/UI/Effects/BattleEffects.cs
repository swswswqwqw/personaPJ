using UnityEngine;
using UnityEngine.UI;
using Amane.Core.Tween;
using Amane.Battle;

namespace Amane.UI.Effects
{
    /// <summary>
    /// 戦闘演出マネージャー。ペルソナ5調査結果に基づく演出設計:
    /// - 弱点ヒット: ガラスにヒビ＋ピンクスプラッシュ（DESIGN.md演出1）
    /// - 総告白: コミック調フィニッシュカットイン（DESIGN.md演出2）
    /// - 言継ぎ: ハイタッチ＋ダメージブースト表示
    /// - ダメージポップ: 数字が飛び出して消える
    ///
    /// P5の設計原則: 「モーションがリズムを制御する」「1ドット1フレームの調整」
    /// </summary>
    public sealed class BattleEffects : MonoBehaviour
    {
        [Header("Screen Effects")]
        [SerializeField] private Image _screenFlash;
        [SerializeField] private Image _weakBanner;
        [SerializeField] private Text _weakText;
        [SerializeField] private Image _crackOverlay;

        [Header("All-Out Confession")]
        [SerializeField] private CanvasGroup _allOutGroup;
        [SerializeField] private Image _allOutBackground;
        [SerializeField] private Text _allOutTitle;
        [SerializeField] private Text _allOutQuote;
        [SerializeField] private Image _finisherPortrait;

        [Header("Kotsugi")]
        [SerializeField] private CanvasGroup _kotsugiGroup;
        [SerializeField] private Text _kotsugiText;
        [SerializeField] private Text _kotsugiBonus;

        [Header("Perfect Kotsugi — DESIGN.md 9-1")]
        [SerializeField] private CanvasGroup _perfectKotsugiGroup;
        [SerializeField] private Text _perfectKotsugiTitle;
        [SerializeField] private Text _perfectKotsugiSubtext;

        [Header("Reverse All-Out Calling — DESIGN.md 9-1")]
        [SerializeField] private CanvasGroup _reverseAllOutGroup;
        [SerializeField] private Text _reverseAllOutText;
        [SerializeField] private Image _reverseFlash;

        [Header("Dual Narrator — DESIGN.md 9-1")]
        [SerializeField] private CanvasGroup _dualNarratorGroup;
        [SerializeField] private Text _dualNarratorTitle;
        [SerializeField] private Text _dualNarratorSubtext;

        [Header("Damage Pop")]
        [SerializeField] private Text _damagePopTemplate;

        [Header("Colors — DESIGN.md カラーパレット")]
        private readonly Color _pinkAccent = new Color(1f, 0.243f, 0.541f);    // #FF3E8A 蛍光ピンク
        private readonly Color _deepBlue = new Color(0.106f, 0.165f, 0.29f);   // #1B2A4A 群青
        private readonly Color _readBlue = new Color(0.31f, 0.66f, 1f);        // #4FA8FF 既読ブルー
        private readonly Color _inkBlack = new Color(0.078f, 0.067f, 0.059f);  // #14110F 墨黒

        // ===== 弱点ヒット =====
        public void PlayWeakHit()
        {
            // SE
            Core.Audio.AudioManager.Instance?.PlayWeakHit();

            // 画面フラッシュ（白→透明 0.15s）
            if (_screenFlash != null)
            {
                _screenFlash.gameObject.SetActive(true);
                _screenFlash.color = Color.white;
                Tweener.Color(_screenFlash, Color.white, new Color(1, 1, 1, 0), 0.15f, Easing.OutQuad)
                    .SetOnComplete(() => _screenFlash.gameObject.SetActive(false));
            }

            // WEAK! バナー（左からスライドイン → 振動 → フェードアウト）
            if (_weakBanner != null && _weakText != null)
            {
                _weakBanner.gameObject.SetActive(true);
                _weakBanner.color = _pinkAccent;
                _weakText.text = "WEAK!";
                _weakText.color = Color.white;
                var rt = _weakBanner.rectTransform;
                Tweener.Move(rt, new Vector2(-500, 0), Vector2.zero, 0.12f, Easing.OutBack)
                    .SetOnComplete(() =>
                    {
                        Tweener.Shake(rt, 4f, 0.2f, 40f);
                        Tweener.Float(1f, 0f, 0.6f, t =>
                        {
                            if (_weakBanner != null)
                            {
                                var c = _weakBanner.color;
                                c.a = t;
                                _weakBanner.color = c;
                                _weakText.color = new Color(1, 1, 1, t);
                            }
                        }, Easing.InQuad).SetOnComplete(() => _weakBanner.gameObject.SetActive(false));
                    });
            }

            // ひび割れオーバーレイ（フェードイン→フェードアウト）
            if (_crackOverlay != null)
            {
                _crackOverlay.gameObject.SetActive(true);
                _crackOverlay.color = new Color(1, 1, 1, 0);
                Tweener.Float(0f, 0.7f, 0.08f, a =>
                {
                    if (_crackOverlay != null) _crackOverlay.color = new Color(1, 1, 1, a);
                }, Easing.OutQuad).SetOnComplete(() =>
                {
                    Tweener.Float(0.7f, 0f, 0.4f, a =>
                    {
                        if (_crackOverlay != null) _crackOverlay.color = new Color(1, 1, 1, a);
                    }, Easing.InQuad).SetOnComplete(() => _crackOverlay.gameObject.SetActive(false));
                });
            }
        }

        // ===== クリティカルヒット =====
        public void PlayCriticalHit()
        {
            Core.Audio.AudioManager.Instance?.PlayImpact();

            if (_screenFlash != null)
            {
                _screenFlash.gameObject.SetActive(true);
                _screenFlash.color = new Color(1f, 0.9f, 0.3f);
                Tweener.Color(_screenFlash, new Color(1f, 0.9f, 0.3f, 1f),
                    new Color(1f, 0.9f, 0.3f, 0f), 0.2f, Easing.OutQuad)
                    .SetOnComplete(() => _screenFlash.gameObject.SetActive(false));
            }

            if (_weakBanner != null && _weakText != null)
            {
                _weakBanner.gameObject.SetActive(true);
                _weakBanner.color = new Color(1f, 0.8f, 0.1f);
                _weakText.text = "CRITICAL!";
                _weakText.color = _inkBlack;
                var rt = _weakBanner.rectTransform;
                Tweener.Scale(rt, Vector3.one * 2f, Vector3.one, 0.15f, Easing.OutElastic)
                    .SetOnComplete(() =>
                    {
                        Tweener.Float(1f, 0f, 0.5f, t =>
                        {
                            if (_weakBanner != null)
                            {
                                var c = _weakBanner.color; c.a = t; _weakBanner.color = c;
                                _weakText.color = new Color(_inkBlack.r, _inkBlack.g, _inkBlack.b, t);
                            }
                        }).SetOnComplete(() => _weakBanner.gameObject.SetActive(false));
                    });
            }
        }

        // ===== DOWN! =====
        public void PlayDown()
        {
            if (_weakBanner != null && _weakText != null)
            {
                _weakBanner.gameObject.SetActive(true);
                _weakBanner.color = _readBlue;
                _weakText.text = "DOWN!";
                _weakText.color = Color.white;
                var rt = _weakBanner.rectTransform;
                Tweener.Scale(rt, Vector3.one * 0.3f, Vector3.one, 0.2f, Easing.OutBounce)
                    .SetOnComplete(() =>
                    {
                        Tweener.Float(1f, 0f, 0.8f, t =>
                        {
                            if (_weakBanner != null)
                            {
                                var c = _weakBanner.color; c.a = t; _weakBanner.color = c;
                                _weakText.color = new Color(1, 1, 1, t);
                            }
                        }).SetOnComplete(() => _weakBanner.gameObject.SetActive(false));
                    });
            }
        }

        // ===== ダメージポップ =====
        public void PlayDamageNumber(int damage, HitType type, Vector2 position)
        {
            if (_damagePopTemplate == null) return;
            var pop = Instantiate(_damagePopTemplate, _damagePopTemplate.transform.parent);
            pop.gameObject.SetActive(true);
            pop.text = damage.ToString();
            pop.color = type switch
            {
                HitType.Weak => _pinkAccent,
                HitType.Critical => new Color(1f, 0.8f, 0.1f),
                HitType.Resist => new Color(0.5f, 0.5f, 0.5f),
                _ => Color.white
            };
            pop.fontSize = type == HitType.Weak || type == HitType.Critical ? 32 : 24;

            var rt = pop.rectTransform;
            rt.anchoredPosition = position;

            Tweener.Move(rt, position, position + new Vector2(0, 60), 0.6f, Easing.OutQuad);
            Tweener.Float(1f, 0f, 0.6f, a => { if (pop != null) pop.color = new Color(pop.color.r, pop.color.g, pop.color.b, a); },
                Easing.InQuad).SetOnComplete(() => { if (pop != null) Destroy(pop.gameObject); });
            Tweener.Scale(rt, Vector3.one * 1.5f, Vector3.one, 0.15f, Easing.OutElastic);
        }

        // ===== 総告白（ALL-OUT CONFESSION） =====
        public void PlayAllOutConfession(string finisherName, string quote)
        {
            if (_allOutGroup == null) return;
            _allOutGroup.gameObject.SetActive(true);
            _allOutGroup.alpha = 0;

            if (_allOutTitle != null) { _allOutTitle.text = "ALL-OUT CONFESSION"; _allOutTitle.color = _pinkAccent; }
            if (_allOutQuote != null) { _allOutQuote.text = ""; _allOutQuote.color = Color.white; }
            if (_allOutBackground != null) _allOutBackground.color = _inkBlack;

            // Phase 1: 背景フェードイン（0.3s）
            Tweener.Fade(_allOutGroup, 0, 1, 0.3f, Easing.OutQuad).SetOnComplete(() =>
            {
                // Phase 2: タイトルスライドイン（0.2s）
                if (_allOutTitle != null)
                {
                    var rt = _allOutTitle.rectTransform;
                    Tweener.Move(rt, new Vector2(600, 0), Vector2.zero, 0.2f, Easing.OutBack);
                    Tweener.Scale(rt, Vector3.one * 0.5f, Vector3.one, 0.2f, Easing.OutBack)
                        .SetOnComplete(() =>
                        {
                            // Phase 3: 引用テキストをタイプライター表示（1.5s）
                            TypewriterReveal(_allOutQuote, $"「{quote}」", 0.04f, () =>
                            {
                                // Phase 4: フラッシュ→フェードアウト（1s待ち→0.5s）
                                Tweener.Float(0, 1, 1.0f, _ => { }, Easing.Linear).SetOnComplete(() =>
                                {
                                    if (_screenFlash != null)
                                    {
                                        _screenFlash.gameObject.SetActive(true);
                                        _screenFlash.color = _pinkAccent;
                                        Tweener.Color(_screenFlash, _pinkAccent, new Color(_pinkAccent.r, _pinkAccent.g, _pinkAccent.b, 0), 0.3f)
                                            .SetOnComplete(() => _screenFlash.gameObject.SetActive(false));
                                    }
                                    Tweener.Fade(_allOutGroup, 1, 0, 0.5f, Easing.InQuad)
                                        .SetOnComplete(() => _allOutGroup.gameObject.SetActive(false));
                                });
                            });
                        });
                }
            });
        }

        // ===== 言継ぎ（KOTSUGI） =====
        public void PlayKotsugi(string fromName, string toName, float bonus)
        {
            if (_kotsugiGroup == null) return;
            _kotsugiGroup.gameObject.SetActive(true);
            _kotsugiGroup.alpha = 0;

            if (_kotsugiText != null)
            {
                _kotsugiText.text = $"{fromName} → {toName}";
                _kotsugiText.color = Color.white;
            }
            if (_kotsugiBonus != null)
            {
                _kotsugiBonus.text = $"ダメージ +{bonus:P0}";
                _kotsugiBonus.color = new Color(1f, 0.7f, 0.2f);
            }

            Tweener.Fade(_kotsugiGroup, 0, 1, 0.15f, Easing.OutQuad);
            if (_kotsugiText != null)
                Tweener.Move(_kotsugiText.rectTransform, new Vector2(-300, 0), Vector2.zero, 0.2f, Easing.OutBack);
            if (_kotsugiBonus != null)
                Tweener.Move(_kotsugiBonus.rectTransform, new Vector2(300, 0), Vector2.zero, 0.25f, Easing.OutBack);

            Tweener.Float(0, 1, 1.2f, _ => { }, Easing.Linear).SetOnComplete(() =>
            {
                Tweener.Fade(_kotsugiGroup, 1, 0, 0.3f, Easing.InQuad)
                    .SetOnComplete(() => _kotsugiGroup.gameObject.SetActive(false));
            });
        }

        // ===== Null/反射 =====
        public void PlayNull()
        {
            if (_weakBanner != null && _weakText != null)
            {
                _weakBanner.gameObject.SetActive(true);
                _weakBanner.color = new Color(0.3f, 0.3f, 0.3f);
                _weakText.text = "NULLIFIED";
                _weakText.color = Color.white;
                Tweener.Float(1f, 0f, 1f, t =>
                {
                    if (_weakBanner != null) { var c = _weakBanner.color; c.a = t; _weakBanner.color = c; _weakText.color = new Color(1, 1, 1, t); }
                }).SetOnComplete(() => _weakBanner.gameObject.SetActive(false));
            }
        }

        // ===== パーフェクト言継ぎ（PERFECT KOTSUGI）DESIGN.md 9-1 =====
        // 4人全員がリレーを繋いだ時の達成演出。「想いは、ひとりじゃ届かない」
        public void PlayPerfectKotsugi()
        {
            Core.Audio.AudioManager.Instance?.PlayLevelUp();

            if (_perfectKotsugiGroup == null) return;
            _perfectKotsugiGroup.gameObject.SetActive(true);
            _perfectKotsugiGroup.alpha = 0;

            if (_perfectKotsugiTitle != null)
            {
                _perfectKotsugiTitle.text = "PERFECT KOTSUGI";
                _perfectKotsugiTitle.color = _pinkAccent;
            }
            if (_perfectKotsugiSubtext != null)
            {
                _perfectKotsugiSubtext.text = "";
                _perfectKotsugiSubtext.color = Color.white;
            }

            // フェードイン
            Tweener.Fade(_perfectKotsugiGroup, 0, 1, 0.2f, Easing.OutQuad).SetOnComplete(() =>
            {
                // タイトルをスケールイン
                if (_perfectKotsugiTitle != null)
                    Tweener.Scale(_perfectKotsugiTitle.rectTransform, Vector3.one * 0.5f, Vector3.one, 0.25f, Easing.OutElastic);

                // 「想いは、ひとりじゃ届かない」をタイプライター表示
                TypewriterReveal(_perfectKotsugiSubtext, "——想いは、ひとりじゃ届かない。", 0.05f, () =>
                {
                    // 1s待ってフェードアウト
                    Tweener.Float(0, 1, 1.0f, _ => { }, Easing.Linear).SetOnComplete(() =>
                    {
                        Tweener.Fade(_perfectKotsugiGroup, 1, 0, 0.4f, Easing.InQuad)
                            .SetOnComplete(() => _perfectKotsugiGroup.gameObject.SetActive(false));
                    });
                });
            });
        }

        // ===== 逆総告白（REVERSE ALL-OUT CALLING）DESIGN.md 9-1 =====
        // 味方全員DOWN → 敵の連続行動前の警告演出。プレイヤーの危機感を最大化する。
        public void PlayReverseAllOutCalling(string enemyName)
        {
            Core.Audio.AudioManager.Instance?.PlayImpact();

            // 赤いフラッシュ（3回点滅）
            if (_reverseFlash != null)
            {
                _reverseFlash.gameObject.SetActive(true);
                var dangerRed = new Color(0.8f, 0.1f, 0.1f, 0.7f);

                Tweener.Float(0, 1, 0.08f, t =>
                {
                    if (_reverseFlash != null) _reverseFlash.color = new Color(dangerRed.r, dangerRed.g, dangerRed.b, t * 0.7f);
                }, Easing.OutQuad).SetOnComplete(() =>
                {
                    Tweener.Float(0.7f, 0, 0.12f, t =>
                    {
                        if (_reverseFlash != null) _reverseFlash.color = new Color(dangerRed.r, dangerRed.g, dangerRed.b, t);
                    }).SetOnComplete(() =>
                    {
                        Tweener.Float(0, 0.7f, 0.08f, t =>
                        {
                            if (_reverseFlash != null) _reverseFlash.color = new Color(dangerRed.r, dangerRed.g, dangerRed.b, t);
                        }).SetOnComplete(() =>
                        {
                            Tweener.Float(0.7f, 0, 0.2f, t =>
                            {
                                if (_reverseFlash != null) _reverseFlash.color = new Color(dangerRed.r, dangerRed.g, dangerRed.b, t);
                            }).SetOnComplete(() => _reverseFlash.gameObject.SetActive(false));
                        });
                    });
                });
            }

            if (_reverseAllOutGroup == null) return;
            _reverseAllOutGroup.gameObject.SetActive(true);
            _reverseAllOutGroup.alpha = 0;

            if (_reverseAllOutText != null)
            {
                _reverseAllOutText.text = $"── {enemyName} の逆襲 ──";
                _reverseAllOutText.color = new Color(1f, 0.3f, 0.3f);
            }

            // ぐらつくように表示して消える
            var rt = _reverseAllOutGroup.GetComponent<RectTransform>() ?? _reverseAllOutText?.rectTransform;
            if (rt != null) Tweener.Shake(rt, 6f, 0.3f, 30f);

            Tweener.Fade(_reverseAllOutGroup, 0, 1, 0.15f, Easing.OutQuad);
            Tweener.Float(0, 1, 1.5f, _ => { }, Easing.Linear).SetOnComplete(() =>
            {
                Tweener.Fade(_reverseAllOutGroup, 1, 0, 0.3f, Easing.InQuad)
                    .SetOnComplete(() => _reverseAllOutGroup.gameObject.SetActive(false));
            });
        }

        // ===== デュアルナレーター発動演出（DESIGN.md 9-1）=====
        // 2体の語り手が同時に発動する瞬間の「声の重なり」演出。
        // 属性シナジーなら蛍光ピンク×群青の波形重なり、対立なら紫×赤の干渉表現。
        public void PlayDualNarratorActivated(string primaryName, string secondaryName, bool isSynergy)
        {
            Core.Audio.AudioManager.Instance?.PlayLevelUp();

            // 画面フラッシュ（シナジー: ピンク、対立: 紫）
            if (_screenFlash != null)
            {
                _screenFlash.gameObject.SetActive(true);
                var flashColor = isSynergy
                    ? new Color(1f, 0.243f, 0.541f, 0.6f)  // シナジー: 蛍光ピンク
                    : new Color(0.6f, 0.1f, 0.8f, 0.6f);   // 対立: 紫（声が干渉する）
                _screenFlash.color = flashColor;
                Tweener.Color(_screenFlash, flashColor, new Color(flashColor.r, flashColor.g, flashColor.b, 0f),
                    0.4f, Easing.OutQuad).SetOnComplete(() => _screenFlash.gameObject.SetActive(false));
            }

            if (_dualNarratorGroup == null) return;
            _dualNarratorGroup.gameObject.SetActive(true);
            _dualNarratorGroup.alpha = 0;

            if (_dualNarratorTitle != null)
            {
                _dualNarratorTitle.text = "DUAL NARRATOR";
                _dualNarratorTitle.color = isSynergy ? _pinkAccent : new Color(0.6f, 0.1f, 0.8f);
            }
            if (_dualNarratorSubtext != null)
            {
                var synText = isSynergy ? "——2つの声が、共鳴する。" : "——相反する声が、ぶつかり合う。";
                _dualNarratorSubtext.text = $"{primaryName}  ×  {secondaryName}\n{synText}";
                _dualNarratorSubtext.color = Color.white;
            }

            Tweener.Fade(_dualNarratorGroup, 0, 1, 0.2f, Easing.OutQuad).SetOnComplete(() =>
            {
                if (_dualNarratorTitle != null)
                    Tweener.Scale(_dualNarratorTitle.rectTransform, Vector3.one * 0.6f, Vector3.one, 0.2f, Easing.OutBack);

                Tweener.Float(0, 1, 1.2f, _ => { }, Easing.Linear).SetOnComplete(() =>
                {
                    Tweener.Fade(_dualNarratorGroup, 1, 0, 0.3f, Easing.InQuad)
                        .SetOnComplete(() => _dualNarratorGroup.gameObject.SetActive(false));
                });
            });
        }

        // ===== 汎用タイプライター =====
        private void TypewriterReveal(Text textComp, string fullText, float charInterval, System.Action onComplete)
        {
            if (textComp == null) { onComplete?.Invoke(); return; }
            textComp.text = "";
            int charCount = fullText.Length;
            float totalTime = charCount * charInterval;
            Tweener.Float(0, charCount, totalTime, t =>
            {
                int show = Mathf.Min((int)t, charCount);
                if (textComp != null) textComp.text = fullText[..show];
            }, Easing.Linear).SetOnComplete(() =>
            {
                if (textComp != null) textComp.text = fullText;
                onComplete?.Invoke();
            });
        }
    }
}
