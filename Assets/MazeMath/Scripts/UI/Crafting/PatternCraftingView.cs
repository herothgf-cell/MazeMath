using System;
using System.Collections.Generic;
using MazeMath.Crafting;
using UnityEngine;
using UnityEngine.UI;

namespace MazeMath.UI.Crafting
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class PatternCraftingView : MonoBehaviour
    {
        private readonly string[] grid = new string[9];
        private readonly List<Button> slotButtons = new List<Button>();
        private readonly List<Text> slotLabels = new List<Text>();
        private IReadOnlyList<string> selectableItems = Array.Empty<string>();
        private ICraftingService craftingService;
        private string recipeId;
        private Action<CraftResult> resultHandler;
        private bool built;

        public IReadOnlyList<string> Grid => grid;

        public void Configure(
            ICraftingService service,
            string targetRecipeId,
            IReadOnlyList<string> availableItemIds,
            Action<CraftResult> onResult)
        {
            craftingService = service;
            recipeId = targetRecipeId;
            selectableItems = availableItemIds ?? Array.Empty<string>();
            resultHandler = onResult;
            EnsureRuntimeUi();
            ClearGrid();
        }

        public void EnsureRuntimeUi()
        {
            if (built) return;
            built = true;

            var root = GetComponent<RectTransform>();
            root.sizeDelta = new Vector2(520f, 640f);

            var image = GetComponent<Image>();
            if (image == null) image = gameObject.AddComponent<Image>();
            image.color = new Color(0.13f, 0.13f, 0.14f, 0.97f);

            var gridObject = new GameObject("CraftGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObject.transform.SetParent(transform, false);
            var gridRect = gridObject.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.08f, 0.22f);
            gridRect.anchorMax = new Vector2(0.92f, 0.92f);
            gridRect.offsetMin = Vector2.zero;
            gridRect.offsetMax = Vector2.zero;

            var layout = gridObject.GetComponent<GridLayoutGroup>();
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;
            layout.cellSize = new Vector2(130f, 130f);
            layout.spacing = new Vector2(10f, 10f);
            layout.childAlignment = TextAnchor.MiddleCenter;

            for (var i = 0; i < 9; i++)
            {
                CreateSlot(gridObject.transform, i);
            }

            var craftButton = CreateButton(transform, "조합하기", Craft);
            var craftRect = craftButton.GetComponent<RectTransform>();
            craftRect.anchorMin = new Vector2(0.20f, 0.05f);
            craftRect.anchorMax = new Vector2(0.80f, 0.16f);
            craftRect.offsetMin = Vector2.zero;
            craftRect.offsetMax = Vector2.zero;
        }

        public void SetSlot(int index, string itemId)
        {
            if (index < 0 || index >= 9) throw new ArgumentOutOfRangeException(nameof(index));
            grid[index] = string.IsNullOrWhiteSpace(itemId) ? null : itemId;
            RefreshSlot(index);
        }

        public void ClearGrid()
        {
            for (var i = 0; i < grid.Length; i++)
            {
                grid[i] = null;
                RefreshSlot(i);
            }
        }

        public void Craft()
        {
            if (craftingService == null || string.IsNullOrWhiteSpace(recipeId))
                return;

            resultHandler?.Invoke(craftingService.CraftPattern(recipeId, grid));
        }

        private void CycleSlot(int index)
        {
            if (selectableItems.Count == 0)
            {
                SetSlot(index, null);
                return;
            }

            var current = grid[index];
            if (string.IsNullOrEmpty(current))
            {
                SetSlot(index, selectableItems[0]);
                return;
            }

            var found = -1;
            for (var i = 0; i < selectableItems.Count; i++)
            {
                if (selectableItems[i] == current)
                {
                    found = i;
                    break;
                }
            }

            var next = found + 1;
            SetSlot(index, next >= selectableItems.Count ? null : selectableItems[next]);
        }

        private void CreateSlot(Transform parent, int index)
        {
            var button = CreateButton(parent, string.Empty, () => CycleSlot(index));
            slotButtons.Add(button);
            slotLabels.Add(button.GetComponentInChildren<Text>());
        }

        private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.32f, 0.30f, 0.26f, 1f);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(action);

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(go.transform, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 22;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;
            return button;
        }

        private void RefreshSlot(int index)
        {
            if (index < slotLabels.Count && slotLabels[index] != null)
                slotLabels[index].text = string.IsNullOrEmpty(grid[index]) ? "□" : grid[index];
        }
    }
}
