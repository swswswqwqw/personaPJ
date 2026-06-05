using UnityEngine;
using ArcadiaOfEchoes.Core;
using ArcadiaOfEchoes.Time;

namespace ArcadiaOfEchoes.Echo
{
    public enum EchoFloorType
    {
        Normal,
        Boss,
        SafeRoom,
        Treasure
    }

    public class EchoRealmManager : MonoBehaviour
    {
        public static EchoRealmManager Instance { get; private set; }

        public int CurrentFloor { get; private set; }
        public int MaxReachedFloor { get; private set; }
        public bool IsInEchoRealm { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void EnterEchoRealm(int startFloor = -1)
        {
            IsInEchoRealm = true;
            CurrentFloor = startFloor >= 0 ? startFloor : MaxReachedFloor;

            EventBus.Publish(new EchoRealmEnteredEvent(CurrentFloor));

            TimeManager.Instance?.ConsumeActionPoint();
        }

        public void AdvanceFloor()
        {
            CurrentFloor++;
            if (CurrentFloor > MaxReachedFloor)
                MaxReachedFloor = CurrentFloor;

            EventBus.Publish(new EchoFloorChangedEvent(CurrentFloor));
        }

        public void ExitEchoRealm()
        {
            IsInEchoRealm = false;
            EventBus.Publish(new EchoRealmExitedEvent(CurrentFloor));
        }

        public void ReturnToSafeRoom()
        {
            EventBus.Publish(new ReturnToSafeRoomEvent(CurrentFloor));
        }
    }

    public readonly struct EchoRealmEnteredEvent
    {
        public readonly int Floor;
        public EchoRealmEnteredEvent(int floor) { Floor = floor; }
    }

    public readonly struct EchoFloorChangedEvent
    {
        public readonly int NewFloor;
        public EchoFloorChangedEvent(int floor) { NewFloor = floor; }
    }

    public readonly struct EchoRealmExitedEvent
    {
        public readonly int LastFloor;
        public EchoRealmExitedEvent(int floor) { LastFloor = floor; }
    }

    public readonly struct ReturnToSafeRoomEvent
    {
        public readonly int Floor;
        public ReturnToSafeRoomEvent(int floor) { Floor = floor; }
    }
}
