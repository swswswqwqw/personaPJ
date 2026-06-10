using System.IO;
using UnityEngine;

namespace ArcanaOfHollows.Dialogue
{
    public static class DialogueLoader
    {
        public static DialogueSequence LoadFromStreamingAssets(string fileName)
        {
            var path = Path.Combine(Application.streamingAssetsPath, "Dialogue", fileName);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Dialogue file not found: {path}");
                return null;
            }

            var json = File.ReadAllText(path);
            return JsonUtility.FromJson<DialogueSequence>(json);
        }

        public static DialogueSequence LoadFromJson(string json)
        {
            return JsonUtility.FromJson<DialogueSequence>(json);
        }
    }
}
