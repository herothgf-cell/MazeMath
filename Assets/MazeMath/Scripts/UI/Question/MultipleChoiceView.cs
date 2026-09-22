using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MazeMath.UI.Question
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class MultipleChoiceView : MonoBehaviour
    {
        private readonly List<Button> buttons = new List<Button>();
        private Font font;

        public int ButtonCount => buttons.Count;

        public void ShowChoices(int[] choices, Action<int> onSelected)
        {
            ClearButtons();
            if (choices == null) return;

            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var layout = GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = gameObject.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 12f;
                layout.padding = new RectOffset(8, 8, 8, 8);
                layout.childControlHeight = true;
                layout.childForceExpandHeight = false;
            }

            for (var i = 0; i < choices.Length; i++)
            {
                var index = i;
                var buttonObject = new GameObject(
                    "Choice_" + i,
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(Button),
                    typeof(LayoutElement));
                buttonObject.transform.SetParent(transform, false);

                var image = buttonObject.GetComponent<Image>();
                image.color = new Color(0.28f, 0.30f, 0.34f, 1f);

                var layoutElement = buttonObject.GetComponent<LayoutElement>();
                layoutElement.preferredHeight = 72f;

                var button = buttonObject.GetComponent<Button>();
                button.onClick.AddListener(() => onSelected?.Invoke(index));
                buttons.Add(button);

                var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
                labelObject.transform.SetParent(buttonObject.transform, false);
                var labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;

                var label = labelObject.GetComponent<Text>();
                label.font = font;
                label.fontSize = 32;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = Color.white;
                label.text = choices[i].ToString();
            }
        }

        public void ClearButtons()
        {
            for (var i = 0; i < buttons.Count; i++)
            {
                if (buttons[i] != null)
                {
                    Destroy(buttons[i].gameObject);
                }
            }
            buttons.Clear();
        }
    }
}
