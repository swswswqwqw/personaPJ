using System.Collections.Generic;
using UnityEngine;

namespace ArcanaOfHollows.Core
{
    public class StoryFlagManager : MonoBehaviour
    {
        public static StoryFlagManager Instance { get; private set; }

        private HashSet<string> flags = new();

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

        public void SetFlag(string flagId)
        {
            if (flags.Add(flagId))
                EventBus.Publish(new StoryFlagSetEvent(flagId));
        }

        public bool HasFlag(string flagId) => flags.Contains(flagId);

        public void RemoveFlag(string flagId) => flags.Remove(flagId);

        public List<string> GetAllFlags() => new(flags);

        public void LoadFlags(string[] savedFlags)
        {
            flags.Clear();
            if (savedFlags != null)
                foreach (var flag in savedFlags)
                    flags.Add(flag);
        }
    }

    public readonly struct StoryFlagSetEvent
    {
        public readonly string FlagId;
        public StoryFlagSetEvent(string flagId) { FlagId = flagId; }
    }
}
