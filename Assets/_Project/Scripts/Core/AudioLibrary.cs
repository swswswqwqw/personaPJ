using System;
using UnityEngine;

namespace EchoesOfArcadia.Core
{
    [CreateAssetMenu(fileName = "AudioLibrary", menuName = "EchoesOfArcadia/Audio Library")]
    public class AudioLibrary : ScriptableObject
    {
        [Header("BGM")]
        [SerializeField] private BGMEntry[] bgmEntries;

        [Header("SFX")]
        [SerializeField] private SFXEntry[] sfxEntries;

        public AudioClip GetBGM(BGMTrack track)
        {
            if (bgmEntries == null) return null;
            foreach (var entry in bgmEntries)
            {
                if (entry.track == track) return entry.clip;
            }
            return null;
        }

        public AudioClip GetSFX(SFXType type)
        {
            if (sfxEntries == null) return null;
            foreach (var entry in sfxEntries)
            {
                if (entry.type == type) return entry.clip;
            }
            return null;
        }

        [Serializable]
        public struct BGMEntry
        {
            public BGMTrack track;
            public AudioClip clip;
        }

        [Serializable]
        public struct SFXEntry
        {
            public SFXType type;
            public AudioClip clip;
        }
    }
}
