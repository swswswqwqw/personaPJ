using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EchoesOfArcadia.Battle;

namespace EchoesOfArcadia.UI
{
    public class PartyMemberStatusUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI spText;
        [SerializeField] private Slider hpBar;
        [SerializeField] private Slider spBar;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private Image spFillImage;
        [SerializeField] private Image turnIndicator;
        [SerializeField] private CanvasGroup canvasGroup;

        private BattleUnit boundUnit;

        public void SetData(BattleUnit unit)
        {
            boundUnit = unit;
            gameObject.SetActive(true);
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            Refresh();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetTurnActive(bool isActive)
        {
            if (turnIndicator != null)
                turnIndicator.color = isActive ? UIColors.Cyan : Color.clear;
        }

        public void Refresh()
        {
            if (boundUnit == null) return;

            if (nameText != null) nameText.text = boundUnit.Name;

            if (hpText != null) hpText.text = $"{boundUnit.CurrentHP}/{boundUnit.MaxHP}";
            if (spText != null) spText.text = $"{boundUnit.CurrentSP}/{boundUnit.MaxSP}";

            if (hpBar != null)
            {
                hpBar.maxValue = boundUnit.MaxHP;
                hpBar.value = boundUnit.CurrentHP;
            }

            if (spBar != null)
            {
                spBar.maxValue = boundUnit.MaxSP;
                spBar.value = boundUnit.CurrentSP;
            }

            UpdateHPBarColor();
        }

        private void UpdateHPBarColor()
        {
            if (hpFillImage == null || boundUnit == null) return;

            float ratio = (float)boundUnit.CurrentHP / boundUnit.MaxHP;
            if (ratio <= 0.25f)
                hpFillImage.color = UIColors.Crimson;
            else if (ratio <= 0.5f)
                hpFillImage.color = UIColors.Amber;
            else
                hpFillImage.color = UIColors.Cyan;
        }
    }
}
