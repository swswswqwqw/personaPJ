using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Echo;

namespace EchoesOfArcadia.UI
{
    public class EchoRealmUIController : MonoBehaviour
    {
        [Header("Floor Info")]
        [SerializeField] private CanvasGroup floorInfoGroup;
        [SerializeField] private RectTransform floorInfoRect;
        [SerializeField] private TextMeshProUGUI floorNameText;
        [SerializeField] private TextMeshProUGUI roomCountText;

        [Header("Room Display")]
        [SerializeField] private CanvasGroup roomGroup;
        [SerializeField] private RectTransform roomRect;
        [SerializeField] private TextMeshProUGUI roomTypeText;
        [SerializeField] private TextMeshProUGUI roomDescriptionText;
        [SerializeField] private Image roomIcon;

        [Header("Navigation")]
        [SerializeField] private CanvasGroup navGroup;
        [SerializeField] private RectTransform navRect;
        [SerializeField] private Transform routeButtonContainer;
        [SerializeField] private GameObject routeButtonPrefab;
        [SerializeField] private Button useStairsButton;
        [SerializeField] private Button retreatButton;

        [Header("Minimap")]
        [SerializeField] private CanvasGroup minimapGroup;
        [SerializeField] private Transform minimapNodeContainer;
        [SerializeField] private GameObject minimapNodePrefab;

        [Header("Floor Transition")]
        [SerializeField] private CanvasGroup floorTransitionOverlay;
        [SerializeField] private RectTransform floorTransitionRect;
        [SerializeField] private TextMeshProUGUI floorTransitionText;

        private readonly List<GameObject> spawnedRouteButtons = new();
        private readonly List<GameObject> spawnedMinimapNodes = new();

        private void OnEnable()
        {
            if (DungeonExplorer.Instance == null) return;
            DungeonExplorer.Instance.OnFloorGenerated += OnFloorGenerated;
            DungeonExplorer.Instance.OnRoomEntered += OnRoomEntered;
            DungeonExplorer.Instance.OnRoomCleared += OnRoomCleared;
            DungeonExplorer.Instance.OnFloorCompleted += OnFloorCompleted;
        }

        private void OnDisable()
        {
            if (DungeonExplorer.Instance == null) return;
            DungeonExplorer.Instance.OnFloorGenerated -= OnFloorGenerated;
            DungeonExplorer.Instance.OnRoomEntered -= OnRoomEntered;
            DungeonExplorer.Instance.OnRoomCleared -= OnRoomCleared;
            DungeonExplorer.Instance.OnFloorCompleted -= OnFloorCompleted;
        }

        private void Start()
        {
            UIAnimator.SetVisible(floorInfoGroup, false);
            UIAnimator.SetVisible(roomGroup, false);
            UIAnimator.SetVisible(navGroup, false);
            UIAnimator.SetVisible(minimapGroup, false);
            UIAnimator.SetVisible(floorTransitionOverlay, false);

            if (useStairsButton != null)
                useStairsButton.onClick.AddListener(OnUseStairsClicked);
            if (retreatButton != null)
                retreatButton.onClick.AddListener(OnRetreatClicked);
        }

        private void OnFloorGenerated(DungeonFloor floor)
        {
            PlayFloorTransition(floor);
        }

        private void OnRoomEntered(DungeonRoom room)
        {
            UpdateRoomDisplay(room);
            UpdateNavigation(room);
            UpdateMinimap();
        }

        private void OnRoomCleared(DungeonRoom room)
        {
            UpdateRoomDisplay(room);
            UpdateNavigation(room);
            UpdateMinimap();
        }

        private void OnFloorCompleted(int floorNumber)
        {
            AudioManager.Instance?.PlaySFX(SFXType.UI_Confirm);
        }

        private void PlayFloorTransition(DungeonFloor floor)
        {
            if (floorTransitionOverlay == null) return;

            if (floorTransitionText != null)
                floorTransitionText.text = $"{floor.floorName}\n{floor.floorNumber}階層";

            var seq = DOTween.Sequence();
            seq.Append(UIAnimator.FadeIn(floorTransitionOverlay, 0.4f));
            if (floorTransitionRect != null)
            {
                floorTransitionRect.localScale = Vector3.one * 1.2f;
                seq.Join(floorTransitionRect.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
            }
            seq.AppendInterval(1.2f);
            seq.Append(UIAnimator.FadeOut(floorTransitionOverlay, 0.4f));
            seq.OnComplete(() =>
            {
                ShowExplorationUI(floor);
            });
        }

        private void ShowExplorationUI(DungeonFloor floor)
        {
            if (floorNameText != null)
                floorNameText.text = floor.floorName;

            UIAnimator.SlideInFromLeft(floorInfoGroup, floorInfoRect, 0.3f);
            UIAnimator.FadeIn(minimapGroup, 0.3f);
        }

        private void UpdateRoomDisplay(DungeonRoom room)
        {
            if (roomTypeText != null)
                roomTypeText.text = GetRoomTypeName(room.type);
            if (roomDescriptionText != null)
                roomDescriptionText.text = room.description;

            if (roomCountText != null)
            {
                var floor = DungeonExplorer.Instance?.CurrentFloor;
                if (floor != null)
                {
                    int explored = 0;
                    foreach (var r in floor.rooms) { if (r.explored) explored++; }
                    roomCountText.text = $"{explored} / {floor.rooms.Count}";
                }
            }

            if (useStairsButton != null)
                useStairsButton.gameObject.SetActive(room.type == RoomType.Stairs && room.cleared);

            UIAnimator.SlideInFromRight(roomGroup, roomRect, 0.25f);
        }

        private void UpdateNavigation(DungeonRoom room)
        {
            foreach (var btn in spawnedRouteButtons) Destroy(btn);
            spawnedRouteButtons.Clear();

            if (routeButtonContainer == null || routeButtonPrefab == null) return;

            var connected = DungeonExplorer.Instance?.GetConnectedRooms();
            if (connected == null) return;

            for (int i = 0; i < connected.Count; i++)
            {
                var connectedRoom = connected[i];
                var obj = Instantiate(routeButtonPrefab, routeButtonContainer);
                spawnedRouteButtons.Add(obj);

                var text = obj.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    string status = connectedRoom.explored
                        ? (connectedRoom.cleared ? "済" : "！")
                        : "？";
                    text.text = $"Room {connectedRoom.roomId + 1} [{status}]";
                }

                var btn = obj.GetComponent<Button>();
                int index = i;
                if (btn != null)
                {
                    btn.onClick.AddListener(() =>
                    {
                        AudioManager.Instance?.PlaySFX(SFXType.UI_Select);
                        DungeonExplorer.Instance?.MoveToConnectedRoom(index);
                    });
                }
            }

            UIAnimator.SlideInFromBottom(navGroup, navRect, 0.25f);
        }

        private void UpdateMinimap()
        {
            foreach (var node in spawnedMinimapNodes) Destroy(node);
            spawnedMinimapNodes.Clear();

            var floor = DungeonExplorer.Instance?.CurrentFloor;
            if (floor == null || minimapNodeContainer == null || minimapNodePrefab == null) return;

            foreach (var room in floor.rooms)
            {
                if (!room.explored) continue;

                var node = Instantiate(minimapNodePrefab, minimapNodeContainer);
                spawnedMinimapNodes.Add(node);

                var img = node.GetComponent<Image>();
                if (img != null)
                {
                    bool isCurrent = room == DungeonExplorer.Instance.CurrentRoom;
                    img.color = isCurrent
                        ? new Color(0.2f, 0.8f, 1f)
                        : (room.cleared ? new Color(0.5f, 0.5f, 0.5f) : Color.white);
                }
            }
        }

        private void OnUseStairsClicked()
        {
            AudioManager.Instance?.PlaySFX(SFXType.UI_Confirm);
            UIAnimator.FadeOut(roomGroup, 0.2f);
            UIAnimator.FadeOut(navGroup, 0.2f);
            UIAnimator.FadeOut(minimapGroup, 0.2f);
            UIAnimator.FadeOut(floorInfoGroup, 0.2f);
            DOVirtual.DelayedCall(0.3f, () =>
            {
                DungeonExplorer.Instance?.UseStairs();
            });
        }

        private void OnRetreatClicked()
        {
            AudioManager.Instance?.PlaySFX(SFXType.UI_Cancel);
            UIAnimator.FadeOut(roomGroup, 0.2f);
            UIAnimator.FadeOut(navGroup, 0.2f);
            UIAnimator.FadeOut(minimapGroup, 0.2f);
            UIAnimator.FadeOut(floorInfoGroup, 0.2f);
            DOVirtual.DelayedCall(0.3f, () =>
            {
                EchoRealmManager.Instance?.RetreatFromDungeon();
            });
        }

        private static string GetRoomTypeName(RoomType type) => type switch
        {
            RoomType.Empty => "空き部屋",
            RoomType.Battle => "戦闘",
            RoomType.Treasure => "宝箱",
            RoomType.Trap => "罠",
            RoomType.RestPoint => "休息の間",
            RoomType.Event => "イベント",
            RoomType.MiniBoss => "中ボス",
            RoomType.Boss => "ボス",
            RoomType.Stairs => "階段",
            RoomType.Entrance => "入口",
            _ => ""
        };
    }
}
