using System.Linq;
using MazeMath.Maze.Data;

namespace MazeMath.Maze.Generation
{
    public sealed class MazeValidator
    {
        public MazeValidationResult Validate(MazeGraph graph, MazeChapterDefinition chapter)
        {
            var result = new MazeValidationResult();

            if (graph == null || chapter == null)
            {
                result.Add(MazeValidationError.MissingRequiredRoom);
                return result;
            }

            var start = graph.Nodes.Values.FirstOrDefault(node => node.Type == RoomType.Start);
            var boss = graph.Nodes.Values.FirstOrDefault(node => node.Type == RoomType.Boss);

            if (start == null || boss == null)
            {
                result.Add(MazeValidationError.MissingRequiredRoom);
                return result;
            }

            if (graph.FindPath(start.NodeId, boss.NodeId).Count == 0)
            {
                result.Add(MazeValidationError.NoPathToBoss);
            }

            ValidateFloors(graph, chapter, result);
            ValidateOptionalReachability(graph, start.NodeId, result);
            ValidateConsecutiveLearningRooms(graph, start.NodeId, boss.NodeId, result);

            return result;
        }

        private static void ValidateFloors(
            MazeGraph graph,
            MazeChapterDefinition chapter,
            MazeValidationResult result)
        {
            foreach (var node in graph.Nodes.Values)
            {
                if (node.Floor < 0 || node.Floor >= chapter.floorCount)
                {
                    result.Add(MazeValidationError.InvalidFloorTransition);
                    break;
                }
            }

            foreach (var edge in graph.Edges.Values)
            {
                var from = graph.Nodes[edge.FromNodeId];
                var to = graph.Nodes[edge.ToNodeId];
                if (from.Floor == to.Floor)
                {
                    continue;
                }

                if (edge.Type != EdgeType.Ladder && edge.Type != EdgeType.Elevator)
                {
                    result.Add(MazeValidationError.InvalidFloorTransition);
                    break;
                }
            }
        }

        private static void ValidateOptionalReachability(
            MazeGraph graph,
            string startNodeId,
            MazeValidationResult result)
        {
            foreach (var node in graph.Nodes.Values.Where(node => !node.IsCriticalPath))
            {
                if (graph.FindPath(startNodeId, node.NodeId).Count == 0)
                {
                    result.Add(MazeValidationError.UnreachableOptionalRoom);
                    return;
                }
            }
        }

        private static void ValidateConsecutiveLearningRooms(
            MazeGraph graph,
            string startNodeId,
            string bossNodeId,
            MazeValidationResult result)
        {
            var path = graph.FindPath(startNodeId, bossNodeId);
            if (path.Count < 2)
            {
                return;
            }

            for (var i = 1; i < path.Count; i++)
            {
                var previous = graph.Nodes[path[i - 1]].Type;
                var current = graph.Nodes[path[i]].Type;

                var repeatedQuestion = previous == RoomType.Question && current == RoomType.Question;
                var repeatedPuzzle = previous == RoomType.Puzzle && current == RoomType.Puzzle;

                if (repeatedQuestion || repeatedPuzzle)
                {
                    result.Add(MazeValidationError.ConsecutiveSamePuzzleType);
                    return;
                }
            }
        }
    }
}
