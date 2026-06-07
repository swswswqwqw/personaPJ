using System;
using System.Collections.Generic;
using UnityEngine;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Data;
using EchoesOfArcadia.TimeSystem;

namespace EchoesOfArcadia.Field
{
    public class FieldManager : MonoBehaviour
    {
        public static FieldManager Instance { get; private set; }

        [SerializeField] private LocationData[] allLocations;
        [SerializeField] private LocationData startingLocation;

        public LocationData CurrentLocation { get; private set; }
        public List<NPCPlacement> AvailableNPCs { get; private set; } = new();

        public event Action<LocationData> OnLocationChanged;
        public event Action<List<NPCPlacement>> OnNPCsRefreshed;

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

        private void Start()
        {
            if (startingLocation != null)
                MoveToLocation(startingLocation);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<TimeAdvancedEvent>(OnTimeAdvanced);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<TimeAdvancedEvent>(OnTimeAdvanced);
        }

        public void MoveToLocation(LocationData location)
        {
            if (!IsLocationAvailable(location))
            {
                Debug.LogWarning($"Location {location.locationName} is not available now.");
                return;
            }

            CurrentLocation = location;
            RefreshAvailableNPCs();
            OnLocationChanged?.Invoke(location);
            GameEventBus.Publish(new LocationChangedEvent(location.area, location.locationName));
        }

        public void MoveToArea(LocationArea area)
        {
            var location = FindLocation(area);
            if (location != null)
                MoveToLocation(location);
        }

        public List<LocationData> GetConnectedLocations()
        {
            var result = new List<LocationData>();
            if (CurrentLocation?.connectedAreas == null) return result;

            foreach (var area in CurrentLocation.connectedAreas)
            {
                var loc = FindLocation(area);
                if (loc != null && IsLocationAvailable(loc))
                    result.Add(loc);
            }

            return result;
        }

        public List<LocationActivity> GetAvailableActivities()
        {
            if (CurrentLocation?.activities == null) return new List<LocationActivity>();

            var result = new List<LocationActivity>();
            foreach (var activity in CurrentLocation.activities)
            {
                if (TimeManager.Instance != null && TimeManager.Instance.RemainingActionPoints >= activity.actionPointCost)
                    result.Add(activity);
            }

            return result;
        }

        public void PerformActivity(LocationActivity activity)
        {
            if (TimeManager.Instance == null) return;
            if (!TimeManager.Instance.SpendActionPoint()) return;

            PlayerStats.Instance?.AddPoints(activity.statToRaise, activity.statPoints);
            GameEventBus.Publish(new ActivityPerformedEvent(activity.activityName, activity.statToRaise, activity.statPoints));

            TimeManager.Instance.AdvanceTimeOfDay();
        }

        public void InteractWithNPC(NPCPlacement npc)
        {
            if (npc.isBondTarget)
            {
                if (TimeManager.Instance != null && !TimeManager.Instance.SpendActionPoint())
                    return;
            }

            if (!string.IsNullOrEmpty(npc.dialogueFileOnInteract))
            {
                var jsonData = Dialogue.DialogueDataLoader.LoadFromStreamingAssets(npc.dialogueFileOnInteract);
                if (jsonData != null)
                {
                    var dialogueData = Dialogue.DialogueDataLoader.ConvertToScriptableObject(jsonData);
                    Dialogue.DialogueSystem.Instance?.StartDialogue(dialogueData);
                }
            }
        }

        private bool IsLocationAvailable(LocationData location)
        {
            if (TimeManager.Instance == null) return true;

            var time = TimeManager.Instance.CurrentTimeOfDay;
            bool timeOk = time switch
            {
                TimeOfDay.Morning => location.availableMorning,
                TimeOfDay.Afternoon => location.availableAfternoon,
                TimeOfDay.Evening => location.availableEvening,
                TimeOfDay.LateNight => location.availableLateNight,
                _ => true
            };

            if (!timeOk) return false;

            var weather = TimeManager.Instance.CurrentWeather;
            if (location.blockedByRain && weather == Weather.Rainy) return false;
            if (location.blockedByStorm && weather == Weather.Stormy) return false;

            return true;
        }

        private void RefreshAvailableNPCs()
        {
            AvailableNPCs.Clear();
            if (CurrentLocation?.npcs == null) return;

            var currentTime = TimeManager.Instance?.CurrentTimeOfDay ?? TimeOfDay.Afternoon;

            foreach (var npc in CurrentLocation.npcs)
            {
                bool isAvailable = false;
                if (npc.availableTimes != null)
                {
                    foreach (var t in npc.availableTimes)
                    {
                        if (t == currentTime) { isAvailable = true; break; }
                    }
                }
                else
                {
                    isAvailable = true;
                }

                if (isAvailable)
                    AvailableNPCs.Add(npc);
            }

            OnNPCsRefreshed?.Invoke(AvailableNPCs);
        }

        private void OnTimeAdvanced(TimeAdvancedEvent e)
        {
            RefreshAvailableNPCs();
        }

        private LocationData FindLocation(LocationArea area)
        {
            if (allLocations == null) return null;
            foreach (var loc in allLocations)
            {
                if (loc != null && loc.area == area) return loc;
            }
            return null;
        }
    }

    public readonly struct LocationChangedEvent
    {
        public readonly LocationArea Area;
        public readonly string LocationName;
        public LocationChangedEvent(LocationArea area, string name)
        {
            Area = area;
            LocationName = name;
        }
    }

    public readonly struct ActivityPerformedEvent
    {
        public readonly string ActivityName;
        public readonly PersonalStat Stat;
        public readonly int Points;
        public ActivityPerformedEvent(string name, PersonalStat stat, int points)
        {
            ActivityName = name;
            Stat = stat;
            Points = points;
        }
    }
}
