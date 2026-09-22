using System;
using System.Linq;
using MazeMath.Maze;
using MazeMath.Maze.Data;
using MazeMath.Maze.Generation;

namespace MazeMath.Maze.Runtime
{
    public sealed class MazeRuntimeService : IMazeService
    {
        private readonly MazeGenerator generator;
        private readonly MazeValidator validator;
        private readonly IMazeChapterProvider chapterProvider;

        public MazeGraph CurrentGraph { get; private set; }
        public MazeRunState CurrentState { get; private set; }

        public MazeRuntimeService(
            MazeGenerator generator,
            MazeValidator validator,
            IMazeChapterProvider chapterProvider)
        {
            this.generator = generator ?? throw new ArgumentNullException(nameof(generator));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.chapterProvider = chapterProvider ?? throw new ArgumentNullException(nameof(chapterProvider));
        }

        public MazeGraph StartChapter(string chapterId, int runSeed)
        {
            var chapter = chapterProvider.Get(chapterId);
            if (chapter == null)
            {
                throw new InvalidOperationException("Unknown maze chapter: " + chapterId);
            }

            var graph = generator.Generate(chapter, runSeed);
            var validation = validator.Validate(graph, chapter);
            if (validation.HasCriticalErrors)
            {
                throw new InvalidOperationException(
                    "Generated maze is invalid: " + string.Join(",", validation.Errors));
            }

            var start = graph.Nodes.Values.Single(node => node.Type == RoomType.Start);

            CurrentGraph = graph;
            CurrentState = new MazeRunState
            {
                chapterId = chapterId,
                runSeed = runSeed,
                currentNodeId = start.NodeId,
                currentFloor = start.Floor
            };

            AddUnique(CurrentState.visitedNodeIds, start.NodeId);
            AddUnique(CurrentState.seenNodeIds, start.NodeId);
            return CurrentGraph;
        }

        public void EnterNode(string nodeId)
        {
            EnsureRunStarted();

            if (!CurrentGraph.Nodes.TryGetValue(nodeId, out var node))
            {
                throw new InvalidOperationException("Unknown maze node: " + nodeId);
            }

            CurrentState.currentNodeId = nodeId;
            CurrentState.currentFloor = node.Floor;
            AddUnique(CurrentState.visitedNodeIds, nodeId);
            AddUnique(CurrentState.seenNodeIds, nodeId);
        }

        public void SolveGate(string gateId)
        {
            EnsureRunStarted();

            if (string.IsNullOrWhiteSpace(gateId))
            {
                throw new ArgumentException("Gate id must not be empty.", nameof(gateId));
            }

            AddUnique(CurrentState.solvedGateIds, gateId);
        }

        public bool CanTraverse(string edgeId)
        {
            EnsureRunStarted();

            if (!CurrentGraph.Edges.TryGetValue(edgeId, out var edge))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(edge.GateId))
            {
                return true;
            }

            return CurrentState.solvedGateIds.Contains(edge.GateId);
        }

        private void EnsureRunStarted()
        {
            if (CurrentGraph == null || CurrentState == null)
            {
                throw new InvalidOperationException("Maze run has not started.");
            }
        }

        private static void AddUnique(System.Collections.Generic.List<string> list, string value)
        {
            if (!list.Contains(value))
            {
                list.Add(value);
            }
        }
    }
}
