using System.IO;
using UnityEngine;

namespace EchoesOfArcadia.Dialogue
{
    public static class DialogueDataLoader
    {
        public static DialogueJsonData LoadFromStreamingAssets(string fileName)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Dialogue", fileName);
            if (!File.Exists(path))
            {
                Debug.LogError($"Dialogue file not found: {path}");
                return null;
            }

            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<DialogueJsonData>(json);
        }

        public static DialogueData ConvertToScriptableObject(DialogueJsonData jsonData)
        {
            var data = ScriptableObject.CreateInstance<DialogueData>();
            data.dialogueId = jsonData.dialogueId;
            data.contextDescription = jsonData.contextDescription;
            data.lines = new System.Collections.Generic.List<DialogueLine>();

            foreach (var line in jsonData.lines)
            {
                var dialogueLine = new DialogueLine
                {
                    speakerName = line.speakerName,
                    expression = line.expression,
                    text = line.text,
                    nextLineId = line.nextLineId
                };

                if (line.choices != null && line.choices.Length > 0)
                {
                    dialogueLine.choices = new System.Collections.Generic.List<DialogueChoice>();
                    foreach (var choice in line.choices)
                    {
                        dialogueLine.choices.Add(new DialogueChoice
                        {
                            text = choice.text,
                            nextLineId = choice.nextLineId,
                            bondPoints = choice.bondPoints
                        });
                    }
                }

                data.lines.Add(dialogueLine);
            }

            return data;
        }
    }

    [System.Serializable]
    public class DialogueJsonData
    {
        public string dialogueId;
        public string contextDescription;
        public string relatedArcana;
        public int requiredBondRank;
        public DialogueJsonLine[] lines;
    }

    [System.Serializable]
    public class DialogueJsonLine
    {
        public string speakerName;
        public string expression;
        public string text;
        public string nextLineId;
        public DialogueJsonChoice[] choices;
    }

    [System.Serializable]
    public class DialogueJsonChoice
    {
        public string text;
        public string nextLineId;
        public int bondPoints;
    }
}
