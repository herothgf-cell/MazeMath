using System;
using System.Collections.Generic;
using MazeMath.Puzzles.Types;
using UnityEngine;
using UnityEngine.UI;

namespace MazeMath.UI.Puzzles
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class EnvironmentPuzzleDemoView : MonoBehaviour
    {
        private enum DemoMode
        {
            Weight,
            Sokoban,
            Laser
        }

        private readonly List<GameObject> weightControls = new List<GameObject>();
        private readonly List<GameObject> sokobanControls = new List<GameObject>();
        private readonly List<GameObject> laserControls = new List<GameObject>();

        private Action<string> onSolved;
        private Text titleText;
        private Text statusText;
        private WeightBridgePuzzle weightPuzzle;
        private SokobanPuzzle sokobanPuzzle;
        private LaserMirrorPuzzle laserPuzzle;
        private DemoMode mode;
        private bool built;
        private bool completionNotified;

        public bool IsActiveSolved
        {
            get
            {
                switch (mode)
                {
                    case DemoMode.Weight: return weightPuzzle != null && weightPuzzle.IsSolved();
                    case DemoMode.Sokoban: return sokobanPuzzle != null && sokobanPuzzle.IsSolved();
                    case DemoMode.Laser: return laserPuzzle != null && laserPuzzle.IsSolved();
                    default: return false;
                }
            }
        }

        public void Configure(Action<string> solvedHandler)
        {
            onSolved = solvedHandler;
            EnsureRuntimeUi();
            SelectWeightPuzzle();
        }

        public void EnsureRuntimeUi()
        {
            if (built)
                return;

            built = true;
            var root = GetComponent<RectTransform>();
            root.sizeDelta = new Vector2(620f, 720f);

            var background = GetComponent<Image>();
            if (background == null)
                background = gameObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.10f, 0.12f, 0.98f);

            titleText = CreateText(transform, "Title", 34, TextAnchor.MiddleCenter);
            SetRect(titleText.rectTransform, 0.05f, 0.86f, 0.95f, 0.97f);

            statusText = CreateText(transform, "Status", 24, TextAnchor.MiddleCenter);
            SetRect(statusText.rectTransform, 0.08f, 0.48f, 0.92f, 0.84f);

            var modeRow = CreateHorizontalArea("Modes", transform, 0.06f, 0.37f, 0.94f, 0.46f);
            AddButton(modeRow, "무게", SelectWeightPuzzle);
            AddButton(modeRow, "소코반", SelectSokobanPuzzle);
            AddButton(modeRow, "레이저", SelectLaserPuzzle);

            var actionArea = CreateHorizontalArea("Actions", transform, 0.06f, 0.20f, 0.94f, 0.34f);

            weightControls.Add(AddButton(actionArea, "3kg 올리기", AddWeight3).gameObject);
            weightControls.Add(AddButton(actionArea, "5kg 올리기", AddWeight5).gameObject);

            sokobanControls.Add(AddButton(actionArea, "←", MoveLeft).gameObject);
            sokobanControls.Add(AddButton(actionArea, "↑", MoveUp).gameObject);
            sokobanControls.Add(AddButton(actionArea, "↓", MoveDown).gameObject);
            sokobanControls.Add(AddButton(actionArea, "→", MoveRight).gameObject);

            laserControls.Add(AddButton(actionArea, "거울 회전", RotateLaserMirror).gameObject);

            var resetButton = AddButton(transform, "다시 시작", ResetActivePuzzle);
            var resetRect = resetButton.GetComponent<RectTransform>();
            SetRect(resetRect, 0.30f, 0.05f, 0.70f, 0.14f);
        }

        public void SelectWeightPuzzle()
        {
            EnsureRuntimeUi();
            mode = DemoMode.Weight;
            weightPuzzle = new WeightBridgePuzzle("demo.weight", 8);
            weightPuzzle.StartPuzzle();
            completionNotified = false;
            ShowControls(weightControls);
            RefreshStatus();
        }

        public void SelectSokobanPuzzle()
        {
            EnsureRuntimeUi();
            mode = DemoMode.Sokoban;
            sokobanPuzzle = new SokobanPuzzle(
                "demo.sokoban",
                4,
                3,
                new HashSet<GridPosition>(),
                new HashSet<GridPosition> { new GridPosition(2, 1) },
                new HashSet<GridPosition> { new GridPosition(3, 1) },
                new GridPosition(1, 1));
            sokobanPuzzle.StartPuzzle();
            completionNotified = false;
            ShowControls(sokobanControls);
            RefreshStatus();
        }

        public void SelectLaserPuzzle()
        {
            EnsureRuntimeUi();
            mode = DemoMode.Laser;
            laserPuzzle = new LaserMirrorPuzzle(
                "demo.laser",
                5,
                5,
                new GridPosition(0, 1),
                GridDirection.Right,
                new GridPosition(2, 3),
                new HashSet<GridPosition>(),
                new Dictionary<GridPosition, MirrorOrientation>
                {
                    { new GridPosition(2, 1), MirrorOrientation.Backslash }
                });
            laserPuzzle.StartPuzzle();
            completionNotified = false;
            ShowControls(laserControls);
            RefreshStatus();
        }

        public void AddWeight3()
        {
            if (mode != DemoMode.Weight || weightPuzzle == null)
                return;

            weightPuzzle.PlaceWeight("weight-3", 3);
            RefreshStatus();
            NotifyIfSolved("demo.weight");
        }

        public void AddWeight5()
        {
            if (mode != DemoMode.Weight || weightPuzzle == null)
                return;

            weightPuzzle.PlaceWeight("weight-5", 5);
            RefreshStatus();
            NotifyIfSolved("demo.weight");
        }

        public void MoveLeft() => MoveSokoban(GridDirection.Left);
        public void MoveRight() => MoveSokoban(GridDirection.Right);
        public void MoveUp() => MoveSokoban(GridDirection.Up);
        public void MoveDown() => MoveSokoban(GridDirection.Down);

        public void RotateLaserMirror()
        {
            if (mode != DemoMode.Laser || laserPuzzle == null)
                return;

            laserPuzzle.RotateMirror(new GridPosition(2, 1));
            RefreshStatus();
            NotifyIfSolved("demo.laser");
        }

        public void ResetActivePuzzle()
        {
            switch (mode)
            {
                case DemoMode.Weight:
                    SelectWeightPuzzle();
                    break;
                case DemoMode.Sokoban:
                    SelectSokobanPuzzle();
                    break;
                case DemoMode.Laser:
                    SelectLaserPuzzle();
                    break;
            }
        }

        private void MoveSokoban(GridDirection direction)
        {
            if (mode != DemoMode.Sokoban || sokobanPuzzle == null)
                return;

            sokobanPuzzle.Move(direction);
            RefreshStatus();
            NotifyIfSolved("demo.sokoban");
        }

        private void NotifyIfSolved(string puzzleId)
        {
            if (!IsActiveSolved || completionNotified)
                return;

            completionNotified = true;
            onSolved?.Invoke(puzzleId);
        }

        private void RefreshStatus()
        {
            if (titleText == null || statusText == null)
                return;

            switch (mode)
            {
                case DemoMode.Weight:
                    titleText.text = "무게 다리";
                    statusText.text =
                        "목표 무게 8kg\n" +
                        $"현재 무게: {(weightPuzzle != null ? weightPuzzle.CurrentWeight : 0)}kg\n\n" +
                        "3kg 상자와 5kg 상자를 조합해 다리를 내려보세요.";
                    break;

                case DemoMode.Sokoban:
                    titleText.text = "소코반 Lite";
                    statusText.text =
                        BuildSokobanBoard() +
                        "\n상자를 오른쪽 목표 칸까지 밀어보세요.";
                    break;

                case DemoMode.Laser:
                    titleText.text = "레이저 거울";
                    statusText.text =
                        "발사기 → 거울 → 목표 센서\n\n" +
                        (laserPuzzle != null && laserPuzzle.IsSolved()
                            ? "광선이 목표에 도달했습니다!"
                            : "거울을 90° 회전해 광선을 위쪽 목표로 보내보세요.");
                    break;
            }

            if (IsActiveSolved)
                statusText.text += "\n\n✓ 해결 완료";
        }

        private string BuildSokobanBoard()
        {
            if (sokobanPuzzle == null)
                return string.Empty;

            var lines = new List<string>();
            for (var y = 2; y >= 0; y--)
            {
                var line = string.Empty;
                for (var x = 0; x < 4; x++)
                {
                    var p = new GridPosition(x, y);
                    if (p == sokobanPuzzle.Player)
                        line += "P ";
                    else if (Contains(sokobanPuzzle.Boxes, p))
                        line += "B ";
                    else if (p == new GridPosition(3, 1))
                        line += "G ";
                    else
                        line += "· ";
                }
                lines.Add(line);
            }

            return string.Join("\n", lines);
        }

        private static bool Contains(
            IReadOnlyCollection<GridPosition> positions,
            GridPosition target)
        {
            foreach (var position in positions)
            {
                if (position == target)
                    return true;
            }
            return false;
        }

        private void ShowControls(List<GameObject> active)
        {
            SetControlList(weightControls, active == weightControls);
            SetControlList(sokobanControls, active == sokobanControls);
            SetControlList(laserControls, active == laserControls);
        }

        private static void SetControlList(List<GameObject> controls, bool visible)
        {
            foreach (var control in controls)
            {
                if (control != null)
                    control.SetActive(visible);
            }
        }

        private static Transform CreateHorizontalArea(
            string name,
            Transform parent,
            float minX,
            float minY,
            float maxX,
            float maxY)
        {
            var go = new GameObject(
                name,
                typeof(RectTransform),
                typeof(HorizontalLayoutGroup));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            SetRect(rect, minX, minY, maxX, maxY);

            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            return go.transform;
        }

        private static Button AddButton(
            Transform parent,
            string label,
            UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(
                "Button_" + label,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            go.GetComponent<Image>().color = new Color(0.28f, 0.31f, 0.34f, 1f);
            go.GetComponent<LayoutElement>().preferredHeight = 68f;

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(action);

            var text = CreateText(go.transform, "Label", 20, TextAnchor.MiddleCenter);
            text.text = label;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private static Text CreateText(
            Transform parent,
            string name,
            int fontSize,
            TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = MazeMath.UI.RuntimeFontProvider.Get();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void SetRect(
            RectTransform rect,
            float minX,
            float minY,
            float maxX,
            float maxY)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
