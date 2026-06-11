using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amane.Core;
using Amane.Echo;
using Amane.UI.Effects;

namespace Amane.UI
{
    public sealed class DungeonController : MonoBehaviour
    {
        [SerializeField] private Text _floorText;
        [SerializeField] private Text _roomDescText;
        [SerializeField] private Text _messageText;
        [SerializeField] private Button[] _moveButtons;
        [SerializeField] private Text[] _moveLabels;
        [SerializeField] private Button _stairsButton;
        [SerializeField] private Button _retreatButton;

        private DungeonExplorer _explorer;

        private void OnEnable()
        {
            _explorer = DungeonExplorer.Instance;
            if (_explorer != null)
            {
                _explorer.OnRoomEntered += HandleRoomEntered;
                _explorer.OnFloorGenerated += HandleFloorGenerated;
            }

            if (_retreatButton != null)
                _retreatButton.onClick.AddListener(OnRetreatClicked);

            if (_stairsButton != null)
                _stairsButton.onClick.AddListener(OnStairsClicked);

            // バトル勝利から戻ってきた場合、部屋を突破済みにする
            if (DungeonBattleContext.BattleWon)
            {
                if (_explorer?.CurrentRoom != null)
                    _explorer.CurrentRoom.cleared = true;
                DungeonBattleContext.BattleWon = false;
                SetMessage("澱を浄化した。先へ進もう……。");
            }

            RefreshUI();
        }

        private void OnDisable()
        {
            if (_explorer != null)
            {
                _explorer.OnRoomEntered -= HandleRoomEntered;
                _explorer.OnFloorGenerated -= HandleFloorGenerated;
            }

            if (_retreatButton != null)
                _retreatButton.onClick.RemoveListener(OnRetreatClicked);

            if (_stairsButton != null)
                _stairsButton.onClick.RemoveListener(OnStairsClicked);
        }

        private void HandleFloorGenerated(DungeonFloor floor)
        {
            if (_floorText != null)
                _floorText.text = $"未言界 {floor.floorName}";
            RefreshMoveButtons();
        }

        private void HandleRoomEntered(DungeonRoom room)
        {
            if (_roomDescText != null)
                _roomDescText.text = room.description;
            RefreshMoveButtons();
            ProcessRoomEffect(room);
        }

        private void ProcessRoomEffect(DungeonRoom room)
        {
            switch (room.type)
            {
                case RoomType.Battle:
                case RoomType.MiniBoss:
                case RoomType.Boss:
                    if (!room.cleared)
                    {
                        string msg = room.type == RoomType.Boss
                            ? "ボス……「言伝の間」だ。強大な澱の気配が渦巻いている！"
                            : room.type == RoomType.MiniBoss
                                ? "中ボス……巨大な澱の塊が行く手を塞いでいる！"
                                : "澱の怪物の気配を感じる……！";
                        SetMessage(msg);
                        TriggerBattle(room);
                        return;
                    }
                    SetMessage("この部屋の澱はすでに浄化されている。");
                    break;

                case RoomType.Treasure:
                    if (!room.cleared)
                    {
                        room.cleared = true;
                        SetMessage("光る封筒を見つけた。言葉の欠片を手に入れた！");
                    }
                    break;

                case RoomType.Trap:
                    if (!room.cleared)
                    {
                        room.cleared = true;
                        SetMessage($"罠にかかった！ {room.trapDamage}の言葉の刃が刺さる……！");
                    }
                    break;

                case RoomType.RestPoint:
                    SetMessage("穏やかな残響が漂う安息の間。少し休んで体力を回復した。");
                    break;

                case RoomType.Stairs:
                    SetMessage("次の層へ続く道が見える。進む準備ができたら「次の層へ」を押せ。");
                    break;

                default:
                    SetMessage("静かな部屋。言えなかった言葉の残響だけが、微かに漂う……。");
                    break;
            }

            RefreshMoveButtons();
        }

        private void TriggerBattle(DungeonRoom room)
        {
            DungeonBattleContext.IsInDungeon = true;
            DungeonBattleContext.BattleWon = false;
            DungeonBattleContext.CurrentRoomType = room.type;

            var transition = TransitionEffect.Instance;
            if (transition != null)
                transition.PlayDiveTransition(() => GameManager.Instance?.Machine.ChangeTo<BattleState>());
            else
                GameManager.Instance?.Machine.ChangeTo<BattleState>();
        }

        private void RefreshUI()
        {
            if (_explorer == null) return;

            var floor = _explorer.CurrentFloor;
            if (_floorText != null)
                _floorText.text = floor != null ? $"未言界 {floor.floorName}" : "未言界";

            var room = _explorer.CurrentRoom;
            if (_roomDescText != null)
                _roomDescText.text = room?.description ?? "……この場所に、言葉の残響が漂っている。";

            RefreshMoveButtons();
        }

        private void RefreshMoveButtons()
        {
            if (_explorer == null || _moveButtons == null) return;

            bool isStairs = _explorer.CurrentRoom?.type == RoomType.Stairs;
            if (_stairsButton != null)
                _stairsButton.gameObject.SetActive(isStairs);

            var connected = _explorer.GetConnectedRooms();
            for (int i = 0; i < _moveButtons.Length; i++)
            {
                bool hasRoom = i < connected.Count;
                _moveButtons[i].gameObject.SetActive(hasRoom);

                if (!hasRoom) continue;

                string desc = connected[i].description ?? "";
                if (desc.Length > 14) desc = desc.Substring(0, 14) + "…";
                if (_moveLabels != null && i < _moveLabels.Length && _moveLabels[i] != null)
                    _moveLabels[i].text = $"→ {desc}";

                int capturedId = connected[i].roomId;
                _moveButtons[i].onClick.RemoveAllListeners();
                _moveButtons[i].onClick.AddListener(() => _explorer.EnterRoom(capturedId));
            }
        }

        private void SetMessage(string msg)
        {
            if (_messageText != null)
                _messageText.text = msg;
            Debug.Log($"[Dungeon] {msg}");
        }

        private void OnRetreatClicked()
        {
            DungeonBattleContext.Reset();
            MigenkaiManager.Instance?.Retreat();
            var transition = TransitionEffect.Instance;
            if (transition != null)
                transition.PlayReturnTransition(() => GameManager.Instance?.ReturnToField());
            else
                GameManager.Instance?.ReturnToField();
        }

        private void OnStairsClicked()
        {
            _explorer?.UseStairs();
        }
    }
}
