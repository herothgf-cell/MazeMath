using System;
using System.Collections.Generic;
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
        private Font font;
        private bool built;

        public string CurrentText => buffer != null ? buffer.Text : string.Empty;
        public int ButtonCount => buttons.Count;

        public void Configure(int maxLength, Action<string> submitHandler)
        {
            buffer = new NumericInputBuffer(maxLength);
            onSubmit = submitHandler;
            EnsureRuntimeUi();
            RefreshDisplay();
        }

        public void EnsureRuntimeUi()
        {
            if (built)
            {
                return;
            }

            built = true;
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var root = GetComponent<RectTransform>();
            root.sizeDelta = new Vector2(420f, 520f);

            var background = gameObject.GetComponent<Image>();
            if (background == null)
            {
                background = gameObject.AddComponent<Image>();
            }
            background.color = new Color(0.12f, 0.13f, 0.15f, 0.96f);

            displayText = CreateText(
                "Display",
                transform,
                string.Empty,
                42,
                TextAnchor.MiddleCenter);

            var displayRect = displayText.rectTransform;
            displayRect.anchorMin = new Vector2(0.08f, 0.82f);
            displayRect.anchorMax = new Vector2(0.92f, 0.97f);
            displayRect.offsetMin = Vector2.zero;
            displayRect.offsetMax = Vector2.zero;

            var gridObject = new GameObject(
                "KeyGrid",
                typeof(RectTransform),
                typeof(GridLayoutGroup));
            gridObject.transform.SetParent(transform, false);

            var gridRect = gridObject.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.06f, 0.05f);
            gridRect.anchorMax = new Vector2(0.94f, 0.78f);
            gridRect.offsetMin = Vector2.zero;
            gridRect.offsetMax = Vector2.zero;

            var grid = gridObject.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.spacing = new Vector2(10f, 10f);
            grid.padding = new RectOffset(4, 4, 4, 4);
            grid.cellSize = new Vector2(118f, 88f);
            grid.childAlignment = TextAnchor.MiddleCenter;

            for (var digit = 1; digit <= 9; digit++)
            {
                var captured = digit;
                CreateButton(gridObject.transform, digit.ToString(), () => PressDigit(captured));
            }

            CreateButton(gridObject.transform, "←", Backspace);
            CreateButton(gridObject.transform, "0", () => PressDigit(0));
            CreateButton(gridObject.transform, "확인", Submit);
        }

        public void PressDigit(int digit)
        {
            EnsureBuffer();
            buffer.AppendDigit(digit);
            RefreshDisplay();
        }

        public void Backspace()
        {
            EnsureBuffer();
            buffer.Backspace();
            RefreshDisplay();
        }

        public void Clear()
        {
            EnsureBuffer();
            buffer.Clear();
            RefreshDisplay();
        }

        public void Submit()
        {
            EnsureBuffer();
            if (string.IsNullOrEmpty(buffer.Text))
            {
                return;
            }

            onSubmit?.Invoke(buffer.Text);
        }

        private void EnsureBuffer()
        {
            if (buffer == null)
            {
                buffer = new NumericInputBuffer(3);
            }

            EnsureRuntimeUi();
        }

        private void RefreshDisplay()
        {
            if (displayText != null)
            {
                displayText.text = CurrentText;
            }
        }

        private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(
                "Button_" + label,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.30f, 0.32f, 0.35f, 1f);

            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            buttons.Add(button);

            var text = CreateText(
                "Label",
                buttonObject.transform,
                label,
                label == "확인" ? 26 : 34,
                TextAnchor.MiddleCenter);
            text.color = Color.white;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;

            return button;
        }

        private Text CreateText(
            string name,
            Transform parent,
            string value,
            int size,
            TextAnchor alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            var text = textObject.GetComponent<Text>();
            text.font = font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }
    }
}
