using UnityEngine;
using TMPro;
using DG.Tweening;
using EchoesOfArcadia.Data;

namespace EchoesOfArcadia.UI
{
    public class DamagePopupUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private CanvasGroup canvasGroup;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void Show(int damage, ElementType element, bool isWeak, bool isCritical,
            bool isHealed, bool isMiss)
        {
            if (damageText == null) return;

            if (isMiss)
            {
                damageText.text = "MISS";
                damageText.color = new Color(0.5f, 0.5f, 0.5f);
                AnimateAndDestroy(0.6f, 30f);
                return;
            }

            if (isHealed)
            {
                damageText.text = $"+{damage}";
                damageText.color = new Color(0.3f, 1f, 0.5f);
                AnimateAndDestroy(0.8f, 40f);
                return;
            }

            damageText.text = damage.ToString();
            damageText.color = UIColors.GetElementColor(element);

            string suffix = "";
            if (isWeak) suffix = "\n<size=60%>WEAK!</size>";
            if (isCritical) suffix = "\n<size=60%>CRITICAL!</size>";
            damageText.text += suffix;

            float scale = 1f;
            if (isWeak) scale = 1.3f;
            if (isCritical) scale = 1.5f;

            float rise = isWeak || isCritical ? 60f : 40f;
            AnimateAndDestroy(scale, rise);
        }

        private void AnimateAndDestroy(float targetScale, float riseHeight)
        {
            if (canvasGroup == null || rectTransform == null)
            {
                Destroy(gameObject, 1f);
                return;
            }

            canvasGroup.alpha = 0f;
            rectTransform.localScale = Vector3.one * 0.3f;

            var seq = DOTween.Sequence();
            seq.Append(canvasGroup.DOFade(1f, 0.1f));
            seq.Join(rectTransform.DOScale(targetScale, 0.2f).SetEase(Ease.OutBack));
            seq.Join(rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + riseHeight, 0.3f)
                .SetEase(Ease.OutQuad));

            seq.AppendInterval(0.4f);

            seq.Append(canvasGroup.DOFade(0f, 0.3f));
            seq.Join(rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + riseHeight + 20f, 0.3f));

            seq.OnComplete(() => Destroy(gameObject));
        }
    }
}
