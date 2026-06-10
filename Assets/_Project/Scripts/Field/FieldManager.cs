using System;
using System.Collections.Generic;
using UnityEngine;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Time;

namespace EchoesOfArcadia.Field
{
    public enum Location
    {
        Home,
        School,
        SchoolRooftop,
        SchoolLibrary,
        SchoolArtRoom,
        SchoolPool,
        ShoppingDistrict,
        Cafe,
        Shrine,
        Beach,
        Harbor,
        Observatory,
        Library,
        ConvenienceStore
    }

    [Serializable]
    public class LocationData
    {
        public Location location;
        public string locationName;
        public string description;
        public bool isAvailable;
        public TimePeriod[] availablePeriods;
        public List<string> availableCharacters;
        public List<FieldAction> availableActions;
    }

    [Serializable]
    public class FieldAction
    {
        public string actionName;
        public string description;
        public FieldActionType actionType;
        public string targetCharacterId;
        public string dialogueScriptId;
        public Data.SocialStat statBonus;
        public int statBonusAmount;
        public int moneyCost;
        public string requiredStat;
        public int requiredStatLevel;
    }

    public enum FieldActionType
    {
        Resonance,
        Study,
        PartTimeJob,
        Explore,
        Shop,
        Rest,
        SpecialEvent,
        EnterDungeon
    }

    public class FieldManager : MonoBehaviour
    {
        public static FieldManager Instance { get; private set; }

        [SerializeField] private List<LocationData> locations = new();

        private Location _currentLocation;

        public Location CurrentLocation => _currentLocation;

        public event Action<Location> OnLocationChanged;
        public event Action<FieldAction> OnActionExecuted;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void MoveToLocation(Location location)
        {
            var data = GetLocationData(location);
            if (data == null || !data.isAvailable) return;

            var period = TimeManager.Instance?.CurrentPeriod ?? TimePeriod.AfterSchool;
            bool periodAllowed = false;
            foreach (var p in data.availablePeriods)
            {
                if (p == period) { periodAllowed = true; break; }
            }
            if (!periodAllowed) return;

            _currentLocation = location;
            OnLocationChanged?.Invoke(location);
            GameEventBus.Publish(new LocationChangedEvent(location, data.locationName));
        }

        public void ExecuteAction(FieldAction action)
        {
            if (TimeManager.Instance?.RemainingActions <= 0) return;

            OnActionExecuted?.Invoke(action);
            GameEventBus.Publish(new FieldActionEvent(action));

            TimeManager.Instance?.ConsumeAction();
        }

        public List<LocationData> GetAvailableLocations()
        {
            var period = TimeManager.Instance?.CurrentPeriod ?? TimePeriod.AfterSchool;
            var result = new List<LocationData>();

            foreach (var loc in locations)
            {
                if (!loc.isAvailable) continue;
                foreach (var p in loc.availablePeriods)
                {
                    if (p == period)
                    {
                        result.Add(loc);
                        break;
                    }
                }
            }
            return result;
        }

        public LocationData GetLocationData(Location location)
        {
            foreach (var loc in locations)
            {
                if (loc.location == location) return loc;
            }
            return null;
        }
    }

    public readonly struct LocationChangedEvent
    {
        public readonly Location Location;
        public readonly string LocationName;
        public LocationChangedEvent(Location loc, string name)
        {
            Location = loc;
            LocationName = name;
        }
    }

    public readonly struct FieldActionEvent
    {
        public readonly FieldAction Action;
        public FieldActionEvent(FieldAction action) => Action = action;
    }
}
