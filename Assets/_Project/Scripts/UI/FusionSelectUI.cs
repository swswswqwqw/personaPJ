using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amane.Battle;
using Amane.Core;

namespace Amane.UI
{
    // 語り手融合UI — 八雲老人の古書店「言ノ葉」で使用する。
    // 3ステップ: 一体目選択 → 二体目選択 → 融合確認
    // テーマ根拠: 「言葉は組み合わせると新しい意味になる。語り手も同じじゃ。」（八雲）
    public sealed class FusionSelectUI : MonoBehaviour
    {
        private enum Step { SelectFirst, SelectSecond, Confirm }

        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _stepLabel;        // "一体目を選べ" etc.
        [SerializeField] private Text _previewText;      // 融合結果プレビュー
        [SerializeField] private Text _costText;         // 費用表示
        [SerializeField] private Button[] _narratorButtons; // 最大6
        [SerializeField] private Text[]   _narratorLabels;
        [SerializeField] private Button   _confirmButton;
        [SerializeField] private Button   _cancelButton;

        public event Action<Narrator> OnFusionComplete;
        public event Action           OnCancelled;

        private NarratorInventory _inventory;
        private Step _step;
        private Narrator _selected1;
        private Narrator _selected2;
        private List<Narrator> _displayedNarrators = new();

        public void Show(NarratorInventory inventory)
        {
            _inventory = inventory;
            _selected1 = null;
            _selected2 = null;
            _step = Step.SelectFirst;

            if (_panel != null) _panel.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void Awake()
        {
            for (int i = 0; i < _narratorButtons?.Length; i++)
            {
                int idx = i;
                _narratorButtons[idx]?.onClick.AddListener(() => OnNarratorSelected(idx));
            }
            _confirmButton?.onClick.AddListener(OnConfirm);
            _cancelButton?.onClick.AddListener(OnCancel);
        }

        private void Refresh()
        {
            if (_inventory == null) return;

            bool isConfirm = (_step == Step.Confirm);
            SetVisible(_confirmButton, isConfirm);
            SetVisible(_previewText,   isConfirm);

            if (_stepLabel != null)
                _stepLabel.text = _step switch
                {
                    Step.SelectFirst  => "── 一体目の語り手を選べ ──",
                    Step.SelectSecond => $"「{_selected1?.DisplayName}」と融合させる語り手は？",
                    Step.Confirm      => "── 融合を行うか？ ──",
                    _                 => ""
                };

            if (isConfirm)
            {
                var mergedElem = _inventory.PreviewElement(_selected1, _selected2);
                if (_previewText != null)
                    _previewText.text = $"融合後: {_selected1.DisplayName}×{_selected2.DisplayName}\n属性: {mergedElem}";
                if (_costText != null)
                    _costText.text = $"費用: {_inventory.FusionCost} 円";

                // 確認画面ではナレーターボタンを非表示
                foreach (var btn in _narratorButtons)
                    SetVisible(btn, false);
                return;
            }

            // ナレーターボタンを更新
            _displayedNarrators.Clear();
            foreach (var n in _inventory.Owned)
            {
                if (_step == Step.SelectSecond && n == _selected1) continue;
                _displayedNarrators.Add(n);
            }

            for (int i = 0; i < _narratorButtons.Length; i++)
            {
                bool show = (i < _displayedNarrators.Count);
                SetVisible(_narratorButtons[i], show);
                if (show)
                {
                    if (_narratorLabels != null && i < _narratorLabels.Length && _narratorLabels[i] != null)
                        _narratorLabels[i].text = $"{_displayedNarrators[i].DisplayName}\n[{_displayedNarrators[i].PrimaryElement}]";
                }
            }

            if (_costText != null)
                _costText.text = $"融合費用: {_inventory.FusionCost} 円";
        }

        private void OnNarratorSelected(int idx)
        {
            if (idx >= _displayedNarrators.Count) return;

            if (_step == Step.SelectFirst)
            {
                _selected1 = _displayedNarrators[idx];
                _step = Step.SelectSecond;
                Refresh();
            }
            else if (_step == Step.SelectSecond)
            {
                _selected2 = _displayedNarrators[idx];
                _step = Step.Confirm;
                Refresh();
            }
        }

        private void OnConfirm()
        {
            if (_selected1 == null || _selected2 == null || _inventory == null) return;

            var inv = InventoryManager.Instance;
            if (inv != null && !inv.SpendMoney(_inventory.FusionCost))
            {
                if (_costText != null)
                    _costText.text = "所持金が足りない";
                return;
            }

            var result = _inventory.Fuse(_selected1, _selected2);
            Hide();
            OnFusionComplete?.Invoke(result);
        }

        private void OnCancel()
        {
            Hide();
            OnCancelled?.Invoke();
        }

        private static void SetVisible(Component c, bool v) { if (c != null) c.gameObject.SetActive(v); }
    }
}
