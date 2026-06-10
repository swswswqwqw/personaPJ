using System;
using System.Collections.Generic;
using UnityEngine;
using ArcadiaOfEchoes.Core;

namespace ArcadiaOfEchoes.Field
{
    public class FieldManager : MonoBehaviour
    {
        public static FieldManager Instance { get; private set; }

        [SerializeField] private List<LocationData> locations;

        public LocationData CurrentLocation { get; private set; }
        public event Action<LocationData> OnLocationChanged;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public List<LocationData> GetAvailableLocations(TimeSlot timeSlot)
        {
            return locations.FindAll(loc =>
                loc.AvailableTimeSlots.Contains(timeSlot) && loc.IsUnlocked);
        }

        public void MoveToLocation(LocationData location)
        {
            CurrentLocation = location;
            OnLocationChanged?.Invoke(location);
            // TODO: Trigger scene transition or map update
        }
    }

    [Serializable]
    public class LocationData
    {
        public string LocationId;
        public string LocationName;
        [TextArea] public string Description;
        public LocationType Type;
        public List<TimeSlot> AvailableTimeSlots;
        public bool IsUnlocked = true;
        public string UnlockCondition;
        public List<string> AvailableActivities;
    }

    public enum LocationType
    {
        School,
        Residential,
        Commercial,
        EchoRealm,
        Special
    }
}
