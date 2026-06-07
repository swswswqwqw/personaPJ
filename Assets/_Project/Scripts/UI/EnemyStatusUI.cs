using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EchoesOfArcadia.Battle;

namespace EchoesOfArcadia.UI
{
    public class EnemyStatusUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Slider hpBar;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private GameObject downIndicator;
        [SerializeField] private CanvasGroup canvasGroup;

        private BattleUnit boundUnit;

        public void SetData(BattleUnit unit)
        {
            boundUnit = unit;
            gameObject.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Refresh()
        {
            if (boundUnit == null) return;

            if (nameText != null) nameText.text = boundUnit.Name;

            if (hpBar != null)
            {
                hpBar.maxValue = boundUnit.MaxHP;
                hpBar.value = boundUnit.CurrentHP;
            }

            if (downIndicator != null)
                downIndicator.SetActive(boundUnit.IsDown);

            UpdateColor();
        }

        private void UpdateColor()
        {
            if (hpFillImage == null || boundUnit == null) return;

            if (!boundUnit.IsAlive)
                hpFillImage.color = Color.gray;
            else if (boundUnit.IsDown)
                hpFillImage.color = UIColors.Amber;
            else
                hpFillImage.color = UIColors.Crimson;
        }
    }
}
