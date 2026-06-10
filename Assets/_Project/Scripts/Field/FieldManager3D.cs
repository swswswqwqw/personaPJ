using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Amane.Core;

namespace Amane.Field
{
    /// <summary>
    /// 3Dフィールドのインタラクション管理。
    /// プレイヤーの近接判定、プロンプト表示、インタラクト処理。
    /// </summary>
    public sealed class FieldManager3D : MonoBehaviour
    {
        [SerializeField] private PlayerController3D _player;
        [SerializeField] private Text _promptText;
        [SerializeField] private Text _locationText;

        private readonly List<NPC3D> _npcs = new();
        private readonly List<LocationMarker3D> _locations = new();

        private object _currentTarget; // NPC3D or LocationMarker3D
        private string _currentTargetId;

        public event System.Action<string, LocationType> OnInteracted;

        public PlayerController3D Player => _player;

        public void RegisterNPC(NPC3D npc) => _npcs.Add(npc);
        public void RegisterLocation(LocationMarker3D loc) => _locations.Add(loc);

        private void Start()
        {
            if (_player != null)
                _player.OnInteract += OnPlayerInteract;
        }

        private void OnDestroy()
        {
            if (_player != null)
                _player.OnInteract -= OnPlayerInteract;
        }

        private void Update()
        {
            if (_player == null) return;
            var playerPos = _player.transform.position;

            // 最寄りのインタラクト対象を検出
            string nearestId = null;
            string nearestName = null;
            LocationType nearestType = LocationType.NPC;
            float nearestDist = float.MaxValue;

            foreach (var npc in _npcs)
            {
                if (npc == null) continue;
                float dist = Vector3.Distance(playerPos, npc.transform.position);
                if (dist < npc.InteractRadius && dist < nearestDist)
                {
                    nearestId = npc.Id;
                    nearestName = npc.DisplayName;
                    nearestType = npc.Type;
                    nearestDist = dist;
                }
            }

            foreach (var loc in _locations)
            {
                if (loc == null) continue;
                float dist = Vector3.Distance(playerPos, loc.transform.position);
                if (dist < loc.InteractRadius && dist < nearestDist)
                {
                    nearestId = loc.Id;
                    nearestName = loc.DisplayName;
                    nearestType = loc.Type;
                    nearestDist = dist;
                }
            }

            // プロンプト更新
            _currentTargetId = nearestId;
            if (_promptText != null)
            {
                if (nearestId != null)
                {
                    _promptText.gameObject.SetActive(true);
                    _promptText.text = nearestType switch
                    {
                        LocationType.NPC => $"Space: {nearestName}と話す",
                        LocationType.Dungeon => "Space: 未言界へ潜行する",
                        LocationType.Study => $"Space: {nearestName}で勉強する",
                        LocationType.Shop => $"Space: {nearestName}に入る",
                        LocationType.Job => $"Space: {nearestName}で働く",
                        LocationType.Meditate => $"Space: {nearestName}で瞑想する",
                        LocationType.Home => "Space: 帰宅する",
                        _ => $"Space: {nearestName}"
                    };
                }
                else
                {
                    _promptText.gameObject.SetActive(false);
                }
            }

            if (_locationText != null)
                _locationText.text = nearestName ?? "雨音市";
        }

        private void OnPlayerInteract()
        {
            if (_currentTargetId == null) return;

            // インタラクトSE
            var audio = Core.Audio.AudioManager.Instance;
            if (audio != null) audio.PlayInteract();

            // NPC/ロケーションのタイプを特定
            foreach (var npc in _npcs)
            {
                if (npc != null && npc.Id == _currentTargetId)
                {
                    OnInteracted?.Invoke(npc.Id, npc.Type);
                    return;
                }
            }
            foreach (var loc in _locations)
            {
                if (loc != null && loc.Id == _currentTargetId)
                {
                    OnInteracted?.Invoke(loc.Id, loc.Type);
                    return;
                }
            }
        }
    }
}
