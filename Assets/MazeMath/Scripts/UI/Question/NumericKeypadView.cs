using System;
using System.Collections.Generic;
using MazeMath.Adventure;
using MazeMath.Questions.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace MazeMath.UI.Question
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class NumericKeypadView : MonoBehaviour
    {
        private readonly List<Button> buttons = new List<Button>();
        private NumericInputBuffer buffer;
        private Action<string> onSubmit;
        private Text displayText;
        private RectTransform grid;
        private AdventureTheme theme;
        private bool ownsTheme, built, acceptsInput = true;
        private Vector2 lastGridSize;
        public string CurrentText => buffer != null ? buffer.Text : string.Empty;
        public int ButtonCount => buttons.Count;

        public void UseTheme(AdventureTheme value)
        {
            if (built) return;
            theme = value;
        }

        public void Configure(int maxLength, Action<string> submitHandler)
        {
            buffer = new NumericInputBuffer(maxLength);
            onSubmit = submitHandler;
            EnsureRuntimeUi();
            SetInteractable(true);
            RefreshDisplay();
        }

        public void EnsureRuntimeUi()
        {
            if (built) return;
            if (theme == null) { theme = new AdventureTheme(); ownsTheme = true; }
            built = true;
            var root = GetComponent<RectTransform>();
            if (root.parent == null) root.sizeDelta = new Vector2(310f, 350f);
            var inputCard = theme.Box(transform, "InputCard", .02f, .82f, .98f, .99f);
            inputCard.GetComponent<Image>().color = theme.BlockStone;
            displayText = theme.Label(inputCard, string.Empty, 28, TextAnchor.MiddleCenter, .04f, .05f, .96f, .95f);
            grid = theme.Rect(transform, "KeyGrid", .02f, .02f, .98f, .78f);
            for (int i = 0; i < 12; i++)
            {
                int index = i;
                string label = i < 9 ? (i + 1).ToString() : i == 9 ? "←" : i == 10 ? "0" : theme.T("확인", "OK");
                var button = theme.Button(grid, label, () => PressKey(index), 0, 0, 1, 1, i == 11);
                var text = button.GetComponentInChildren<Text>();
                text.fontSize = i == 11 ? 18 : 22;
                text.resizeTextMaxSize = text.fontSize;
                buttons.Add(button);
            }
            Reflow();
        }

        private void LateUpdate()
        {
            if (grid != null && lastGridSize != grid.rect.size) Reflow();
        }

        public void Reflow()
        {
            if (grid == null) return;
            lastGridSize = grid.rect.size;
            float gapX = Mathf.Min(.035f, 8f / Mathf.Max(1f, grid.rect.width));
            float gapY = Mathf.Min(.04f, 8f / Mathf.Max(1f, grid.rect.height));
            for (int i = 0; i < buttons.Count; i++)
            {
                int row = i / 3, col = i % 3;
                var r = buttons[i].GetComponent<RectTransform>();
                r.anchorMin = new Vector2(col / 3f + gapX / 2, 1f - (row + 1) / 4f + gapY / 2);
                r.anchorMax = new Vector2((col + 1) / 3f - gapX / 2, 1f - row / 4f - gapY / 2);
                r.offsetMin = r.offsetMax = Vector2.zero;
            }
        }

        private void PressKey(int index)
        {
            if (index < 9) PressDigit(index + 1);
            else if (index == 9) Backspace();
            else if (index == 10) PressDigit(0);
            else Submit();
        }
        public void PressDigit(int digit)
        {
            if (!acceptsInput) return;
            EnsureBuffer(); buffer.AppendDigit(digit); RefreshDisplay();
        }
        public void Backspace()
        {
            if (!acceptsInput) return;
            EnsureBuffer(); buffer.Backspace(); RefreshDisplay();
        }
        public void Clear() { EnsureBuffer(); buffer.Clear(); RefreshDisplay(); }
        public void Submit()
        {
            if (!acceptsInput) return;
            EnsureBuffer();
            if (buffer.Text.Length > 0) onSubmit?.Invoke(buffer.Text);
        }
        public void SetInteractable(bool value)
        {
            acceptsInput = value;
            foreach (var button in buttons) if (button != null) button.interactable = value;
        }
        private void EnsureBuffer()
        {
            if (buffer == null) buffer = new NumericInputBuffer(3);
            EnsureRuntimeUi();
        }
        private void RefreshDisplay() { if (displayText != null) displayText.text = CurrentText; }
        private void OnDestroy() { onSubmit = null; if (ownsTheme) theme?.Dispose(); }
    }
}
