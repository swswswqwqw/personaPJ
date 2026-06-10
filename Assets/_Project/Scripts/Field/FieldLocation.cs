using UnityEngine;
using UnityEngine.UI;

namespace Amane.Field
{
    /// <summary>
    /// フィールド上のインタラクト可能な場所/NPC。
    /// プレイヤーが近づくとラベルが表示され、Spaceで操作可能。
    /// </summary>
    public sealed class FieldLocation : MonoBehaviour
    {
        [SerializeField] private RectTransform _rect;
        [SerializeField] private Image _icon;
        [SerializeField] private Text _label;
        [SerializeField] private Text _promptText; // 「Spaceで話す」等
        [SerializeField] private float _interactRadius = 50f;

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public LocationType Type { get; set; }
        public Vector2 Position => _rect != null ? _rect.anchoredPosition : Vector2.zero;
        public float InteractRadius => _interactRadius;

        public event System.Action<FieldLocation> OnInteracted;

        private bool _playerNearby;

        public void SetPlayerNearby(bool nearby)
        {
            if (_playerNearby == nearby) return;
            _playerNearby = nearby;

            if (_promptText != null)
            {
                _promptText.gameObject.SetActive(nearby);
                if (nearby)
                {
                    // プロンプト表示アニメ
                    _promptText.text = Type switch
                    {
                        LocationType.NPC => "Space: 話す",
                        LocationType.Dungeon => "Space: 潜行する",
                        LocationType.Shop => "Space: 入る",
                        LocationType.Study => "Space: 勉強する",
                        LocationType.Home => "Space: 帰宅する",
                        _ => "Space"
                    };
                }
            }

            // 近接時のハイライト
            if (_icon != null)
            {
                var c = _icon.color;
                _icon.color = nearby ? new Color(c.r * 1.3f, c.g * 1.3f, c.b * 1.3f, 1f) : c;
            }
        }

        public void TriggerInteract()
        {
            OnInteracted?.Invoke(this);
        }
    }

    public enum LocationType
    {
        NPC,
        Dungeon,
        Shop,
        Study,
        Home,
        Meditate,
        Job
    }
}
