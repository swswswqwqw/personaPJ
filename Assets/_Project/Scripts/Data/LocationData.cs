using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfArcadia.Data
{
    public enum LocationArea
    {
        School_Classroom,
        School_Hallway,
        School_Rooftop,
        School_Library,
        School_MusicRoom,
        School_Gate,
        School_Cafeteria,
        Town_Station,
        Town_ShoppingStreet,
        Town_CafeZankyo,
        Town_Shrine,
        Town_Lighthouse,
        Town_Beach,
        Town_Hospital,
        Home_Room,
        EchoRealm_Entrance,
        EchoRealm_Dungeon
    }

    [CreateAssetMenu(fileName = "NewLocation", menuName = "EchoesOfArcadia/Location Data")]
    public class LocationData : ScriptableObject
    {
        [Header("基本情報")]
        public LocationArea area;
        public string locationName;
        [TextArea(1, 3)] public string description;

        [Header("利用可能時間")]
        public bool availableMorning;
        public bool availableAfternoon = true;
        public bool availableEvening = true;
        public bool availableLateNight;

        [Header("天候制限")]
        public bool blockedByRain;
        public bool blockedByStorm;

        [Header("接続先")]
        public LocationArea[] connectedAreas;

        [Header("NPC配置")]
        public NPCPlacement[] npcs;

        [Header("アクティビティ")]
        public LocationActivity[] activities;
    }

    [Serializable]
    public class NPCPlacement
    {
        public CharacterData character;
        public TimeSystem.TimeOfDay[] availableTimes;
        public string dialogueFileOnInteract;
        public bool isBondTarget;
    }

    [Serializable]
    public class LocationActivity
    {
        public string activityName;
        [TextArea(1, 2)] public string description;
        public PersonalStat statToRaise;
        public int statPoints;
        public int actionPointCost = 1;
    }
}
