using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcadiaOfEchoes.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private Canvas mainCanvas;

        private readonly Stack<UIPanel> _panelStack = new();

        public UIPanel CurrentPanel => _panelStack.Count > 0 ? _panelStack.Peek() : null;

        public event Action<UIPanel> OnPanelOpened;
        public event Action<UIPanel> OnPanelClosed;

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

        public void OpenPanel(UIPanel panel)
        {
            if (_panelStack.Count > 0)
                _panelStack.Peek().OnDeactivate();

            _panelStack.Push(panel);
            panel.gameObject.SetActive(true);
            panel.OnActivate();
            OnPanelOpened?.Invoke(panel);
        }

        public void CloseCurrentPanel()
        {
            if (_panelStack.Count == 0) return;

            var panel = _panelStack.Pop();
            panel.OnDeactivate();
            panel.gameObject.SetActive(false);
            OnPanelClosed?.Invoke(panel);

            if (_panelStack.Count > 0)
                _panelStack.Peek().OnActivate();
        }

        public void CloseAllPanels()
        {
            while (_panelStack.Count > 0)
            {
                var panel = _panelStack.Pop();
                panel.OnDeactivate();
                panel.gameObject.SetActive(false);
            }
        }
    }

    public abstract class UIPanel : MonoBehaviour
    {
        public virtual void OnActivate() { }
        public virtual void OnDeactivate() { }
    }
}
