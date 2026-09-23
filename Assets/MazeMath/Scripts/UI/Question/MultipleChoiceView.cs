using System;
using System.Collections.Generic;
using MazeMath.Adventure;
using UnityEngine;
using UnityEngine.UI;

namespace MazeMath.UI.Question
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class MultipleChoiceView : MonoBehaviour
    {
        private readonly List<Button> buttons = new List<Button>();
        private AdventureTheme theme;
        private bool ownsTheme;
        public int ButtonCount => buttons.Count;
        public void UseTheme(AdventureTheme value)
        {
            if (theme != null) return;
            theme = value;
        }
        public void ShowChoices(int[] values, Action<int> onSelected)
        {
            ClearButtons();
            if (values == null || values.Length == 0) return;
            if (theme == null) { theme = new AdventureTheme(); ownsTheme = true; }
            float row = 1f / values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                int index = i;
                var button = theme.Button(transform, values[i].ToString(), () => onSelected?.Invoke(index),
                    .04f, 1f - (i + 1) * row + .012f, .96f, 1f - i * row - .012f);
                var label = button.GetComponentInChildren<Text>();
                label.fontSize = 22;
                label.resizeTextMaxSize = 22;
                buttons.Add(button);
            }
        }
        public void SetInteractable(bool value)
        {
            foreach (var button in buttons) if (button != null) button.interactable = value;
        }
        public void ClearButtons()
        {
            foreach (var button in buttons)
                if (button != null) { button.gameObject.SetActive(false); Destroy(button.gameObject); }
            buttons.Clear();
        }
        private void OnDestroy() { if (ownsTheme) theme?.Dispose(); }
    }
}
