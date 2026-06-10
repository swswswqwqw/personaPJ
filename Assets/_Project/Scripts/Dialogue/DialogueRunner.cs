using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amane.Dialogue
{
    public sealed class DialogueRunner
    {
        private DialogueData _current;
        private int _lineIndex;
        private bool _waitingForChoice;

        public DialogueLine CurrentLine =>
            _current != null && _lineIndex < _current.lines.Count
                ? _current.lines[_lineIndex]
                : null;

        public bool IsRunning => _current != null;
        public bool IsWaitingForChoice => _waitingForChoice;
        public DialogueChoice PendingChoice { get; private set; }

        public event Action<DialogueLine> OnLineShown;
        public event Action<DialogueChoice> OnChoicePresented;
        public event Action<DialogueData> OnDialogueEnd;

        public void Start(DialogueData data)
        {
            if (data == null || data.lines.Count == 0) return;
            _current = data;
            _lineIndex = 0;
            _waitingForChoice = false;
            PendingChoice = null;
            ShowCurrentLine();
        }

        public void Advance()
        {
            if (_current == null || _waitingForChoice) return;
            _lineIndex++;
            if (_lineIndex >= _current.lines.Count)
            {
                End();
                return;
            }
            CheckForChoice();
            ShowCurrentLine();
        }

        public int SelectOption(int optionIndex)
        {
            if (!_waitingForChoice || PendingChoice == null) return 0;
            var options = PendingChoice.options;
            if (optionIndex < 0 || optionIndex >= options.Count) return 0;

            int bondBonus = options[optionIndex].bondBonus;
            _waitingForChoice = false;
            PendingChoice = null;

            _lineIndex++;
            if (_lineIndex >= _current.lines.Count)
            {
                End();
                return bondBonus;
            }
            ShowCurrentLine();
            return bondBonus;
        }

        public static DialogueData LoadFromJson(string json)
        {
            return JsonUtility.FromJson<DialogueData>(json);
        }

        public static DialogueData LoadFromStreamingAssets(string fileName)
        {
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, "Dialogue", fileName);
            if (!System.IO.File.Exists(path)) return null;
            string json = System.IO.File.ReadAllText(path);
            return LoadFromJson(json);
        }

        private void ShowCurrentLine()
        {
            var line = CurrentLine;
            if (line != null) OnLineShown?.Invoke(line);
        }

        private void CheckForChoice()
        {
            if (_current.choices == null) return;
            foreach (var choice in _current.choices)
            {
                if (choice.afterLineIndex == _lineIndex - 1)
                {
                    _waitingForChoice = true;
                    PendingChoice = choice;
                    OnChoicePresented?.Invoke(choice);
                    return;
                }
            }
        }

        private void End()
        {
            var finished = _current;
            _current = null;
            _lineIndex = 0;
            _waitingForChoice = false;
            PendingChoice = null;
            OnDialogueEnd?.Invoke(finished);
        }
    }
}
