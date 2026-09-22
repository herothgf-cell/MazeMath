using System;
using System.Collections.Generic;
using System.Linq;
using MazeMath.Maze;
using UnityEngine;

namespace MazeMath.Demo
{
    public sealed class MazeWorldDebugRenderer : MonoBehaviour
    {
        private readonly Dictionary<string, Transform> nodeViews =
            new Dictionary<string, Transform>();
        private Sprite squareSprite;
        private Transform playerMarker;

        public void Render(MazeGraph graph)
        {
            Clear();
            if (graph == null) return;

            squareSprite = CreateSquareSprite();

            var critical = graph.Nodes.Values
                .Where(node => node.IsCriticalPath)
                .OrderBy(node => CriticalIndex(node.NodeId))
                .ToList();

            for (var i = 0; i < critical.Count; i++)
            {
                CreateNode(critical[i], new Vector3((i - critical.Count / 2f) * 2.5f, 0f, 0f));
            }

            foreach (var node in graph.Nodes.Values.Where(node => !node.IsCriticalPath))
            {
                var edge = graph.Edges.Values.FirstOrDefault(e =>
                    e.FromNodeId == node.NodeId || e.ToNodeId == node.NodeId);

                var parentId = edge == null
                    ? null
                    : edge.FromNodeId == node.NodeId ? edge.ToNodeId : edge.FromNodeId;

                var parentPosition = parentId != null && nodeViews.TryGetValue(parentId, out var parent)
                    ? parent.position
                    : Vector3.zero;

                var offset = (nodeViews.Count % 2 == 0 ? 1f : -1f) * 2.2f;
                CreateNode(node, parentPosition + new Vector3(0f, offset, 0f));
            }

            foreach (var edge in graph.Edges.Values)
            {
                if (nodeViews.TryGetValue(edge.FromNodeId, out var from) &&
                    nodeViews.TryGetValue(edge.ToNodeId, out var to))
                {
                    CreateEdge(edge, from.position, to.position);
                }
            }

            var player = new GameObject("PlayerMarker");
            player.transform.SetParent(transform, false);
            var renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = squareSprite;
            renderer.color = new Color(1f, 0.84f, 0.15f);
            renderer.sortingOrder = 10;
            player.transform.localScale = Vector3.one * 0.45f;
            playerMarker = player.transform;
        }

        public void MovePlayer(string nodeId)
        {
            if (playerMarker == null || !nodeViews.TryGetValue(nodeId, out var node))
                return;

            playerMarker.position = node.position + new Vector3(0f, 0.62f, 0f);
        }

        private void CreateNode(MazeNode node, Vector3 position)
        {
            var go = new GameObject("Room_" + node.NodeId);
            go.transform.SetParent(transform, false);
            go.transform.position = position;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = squareSprite;
            renderer.color = ColorFor(node.Type);
            go.transform.localScale = new Vector3(1.6f, 1.0f, 1f);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, -0.85f, -0.1f);
            var label = labelGo.AddComponent<TextMesh>();
            label.text = ShortLabel(node.Type);
            label.fontSize = 40;
            label.characterSize = 0.08f;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = Color.white;

            nodeViews[node.NodeId] = go.transform;
        }

        private void CreateEdge(MazeEdge edge, Vector3 from, Vector3 to)
        {
            var go = new GameObject("Edge_" + edge.EdgeId);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            line.startWidth = 0.08f;
            line.endWidth = 0.08f;
            line.useWorldSpace = true;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = edge.Type == EdgeType.EquipmentLocked ? Color.red : Color.gray;
            line.endColor = line.startColor;
            line.sortingOrder = -1;
        }

        private void Clear()
        {
            nodeViews.Clear();
            playerMarker = null;

            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        private static int CriticalIndex(string id)
        {
            if (id == "start") return 0;
            if (id == "boss") return 999;
            if (id != null && id.StartsWith("critical-") &&
                int.TryParse(id.Substring("critical-".Length), out var value))
                return value;
            return 500;
        }

        private static Color ColorFor(RoomType type)
        {
            switch (type)
            {
                case RoomType.Start: return new Color(0.30f, 0.75f, 0.42f);
                case RoomType.Boss: return new Color(0.82f, 0.24f, 0.20f);
                case RoomType.Question: return new Color(0.20f, 0.55f, 0.90f);
                case RoomType.Puzzle: return new Color(0.65f, 0.38f, 0.85f);
                case RoomType.Workshop: return new Color(0.85f, 0.58f, 0.18f);
                case RoomType.Reward: return new Color(0.25f, 0.75f, 0.74f);
                default: return new Color(0.42f, 0.45f, 0.48f);
            }
        }

        private static string ShortLabel(RoomType type)
        {
            switch (type)
            {
                case RoomType.Start: return "START";
                case RoomType.Boss: return "BOSS";
                case RoomType.Question: return "Q";
                case RoomType.Puzzle: return "PUZ";
                case RoomType.Workshop: return "WORK";
                case RoomType.Reward: return "BOX";
                default: return type.ToString().ToUpperInvariant();
            }
        }

        private static Sprite CreateSquareSprite()
        {
            var texture = new Texture2D(1, 1);
            texture.name = "MazeMathRuntimeWhite";
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
        }
    }
}
