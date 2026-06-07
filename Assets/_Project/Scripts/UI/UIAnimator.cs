using UnityEngine;
using DG.Tweening;

namespace EchoesOfArcadia.UI
{
    public static class UIAnimator
    {
        private const float DefaultFadeDuration = 0.25f;
        private const float DefaultSlideDuration = 0.3f;
        private const float DefaultPunchDuration = 0.4f;

        public static Tween FadeIn(CanvasGroup group, float duration = DefaultFadeDuration)
        {
            if (group == null) return null;
            group.interactable = true;
            group.blocksRaycasts = true;
            return group.DOFade(1f, duration).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        public static Tween FadeOut(CanvasGroup group, float duration = DefaultFadeDuration)
        {
            if (group == null) return null;
            return group.DOFade(0f, duration).SetEase(Ease.InQuad).SetUpdate(true)
                .OnComplete(() =>
                {
                    group.interactable = false;
                    group.blocksRaycasts = false;
                });
        }

        public static Sequence SlideInFromLeft(CanvasGroup group, RectTransform rect, float duration = DefaultSlideDuration)
        {
            if (group == null || rect == null) return null;
            var seq = DOTween.Sequence().SetUpdate(true);
            float targetX = rect.anchoredPosition.x;
            rect.anchoredPosition = new Vector2(targetX - 120f, rect.anchoredPosition.y);
            group.alpha = 0f;
            group.interactable = true;
            group.blocksRaycasts = true;
            seq.Append(rect.DOAnchorPosX(targetX, duration).SetEase(Ease.OutCubic));
            seq.Join(group.DOFade(1f, duration * 0.7f));
            return seq;
        }

        public static Sequence SlideInFromRight(CanvasGroup group, RectTransform rect, float duration = DefaultSlideDuration)
        {
            if (group == null || rect == null) return null;
            var seq = DOTween.Sequence().SetUpdate(true);
            float targetX = rect.anchoredPosition.x;
            rect.anchoredPosition = new Vector2(targetX + 120f, rect.anchoredPosition.y);
            group.alpha = 0f;
            group.interactable = true;
            group.blocksRaycasts = true;
            seq.Append(rect.DOAnchorPosX(targetX, duration).SetEase(Ease.OutCubic));
            seq.Join(group.DOFade(1f, duration * 0.7f));
            return seq;
        }

        public static Sequence SlideInFromBottom(CanvasGroup group, RectTransform rect, float duration = DefaultSlideDuration)
        {
            if (group == null || rect == null) return null;
            var seq = DOTween.Sequence().SetUpdate(true);
            float targetY = rect.anchoredPosition.y;
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, targetY - 80f);
            group.alpha = 0f;
            group.interactable = true;
            group.blocksRaycasts = true;
            seq.Append(rect.DOAnchorPosY(targetY, duration).SetEase(Ease.OutCubic));
            seq.Join(group.DOFade(1f, duration * 0.7f));
            return seq;
        }

        public static Sequence SlideOutToLeft(CanvasGroup group, RectTransform rect, float duration = DefaultSlideDuration)
        {
            if (group == null || rect == null) return null;
            var seq = DOTween.Sequence().SetUpdate(true);
            float startX = rect.anchoredPosition.x;
            seq.Append(rect.DOAnchorPosX(startX - 120f, duration).SetEase(Ease.InCubic));
            seq.Join(group.DOFade(0f, duration * 0.7f));
            seq.OnComplete(() =>
            {
                group.interactable = false;
                group.blocksRaycasts = false;
                rect.anchoredPosition = new Vector2(startX, rect.anchoredPosition.y);
            });
            return seq;
        }

        public static void PunchScale(RectTransform rect, float punch = 0.15f, float duration = DefaultPunchDuration)
        {
            if (rect == null) return;
            rect.localScale = Vector3.one;
            rect.DOPunchScale(Vector3.one * punch, duration, 6, 0.5f).SetUpdate(true);
        }

        public static Tween ScaleIn(RectTransform rect, float duration = 0.3f)
        {
            if (rect == null) return null;
            rect.localScale = Vector3.one * 0.7f;
            return rect.DOScale(1f, duration).SetEase(Ease.OutBack).SetUpdate(true);
        }

        public static Sequence PopIn(CanvasGroup group, RectTransform rect, float duration = 0.35f)
        {
            if (group == null || rect == null) return null;
            var seq = DOTween.Sequence().SetUpdate(true);
            group.alpha = 0f;
            rect.localScale = Vector3.one * 0.5f;
            group.interactable = true;
            group.blocksRaycasts = true;
            seq.Append(group.DOFade(1f, duration * 0.6f));
            seq.Join(rect.DOScale(1f, duration).SetEase(Ease.OutBack));
            return seq;
        }

        public static Sequence PopOut(CanvasGroup group, RectTransform rect, float duration = 0.2f)
        {
            if (group == null || rect == null) return null;
            var seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(group.DOFade(0f, duration));
            seq.Join(rect.DOScale(0.8f, duration).SetEase(Ease.InBack));
            seq.OnComplete(() =>
            {
                group.interactable = false;
                group.blocksRaycasts = false;
                rect.localScale = Vector3.one;
            });
            return seq;
        }

        public static Tween FlashColor(UnityEngine.UI.Image image, Color flashColor, float duration = 0.15f)
        {
            if (image == null) return null;
            Color original = image.color;
            image.color = flashColor;
            return image.DOColor(original, duration).SetUpdate(true);
        }

        public static Tween ShakePosition(RectTransform rect, float strength = 8f, float duration = 0.3f)
        {
            if (rect == null) return null;
            return rect.DOShakeAnchorPos(duration, strength, 20, 90, false, true).SetUpdate(true);
        }

        public static void SetVisible(CanvasGroup group, bool visible)
        {
            if (group == null) return;
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
