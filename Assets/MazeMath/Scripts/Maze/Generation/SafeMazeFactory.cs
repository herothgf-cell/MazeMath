using MazeMath.Maze.Data;

namespace MazeMath.Maze.Generation
{
    public sealed class SafeMazeFactory
    {
        public MazeGraph CreateChapter1SafeLayout()
        {
            var graph = new MazeGraph("c00-start", "c06-boss");

            graph.AddNode(new MazeNode("c00-start", "start-room", 1, RoomType.Start, true));
            graph.AddNode(new MazeNode("c01-corridor", "corridor-room", 1, RoomType.Corridor, true));
            graph.AddNode(new MazeNode("c02-question", "question-room", 1, RoomType.Question, true));
            graph.AddNode(new MazeNode("c03-junction", "junction-room", 1, RoomType.Junction, true));
            graph.AddNode(new MazeNode("c04-puzzle", "puzzle-room", 1, RoomType.Puzzle, true));
            graph.AddNode(new MazeNode("c05-checkpoint", "checkpoint-room", 1, RoomType.Checkpoint, true));
            graph.AddNode(new MazeNode("c06-boss", "boss-room", 1, RoomType.Boss, true));
            graph.AddNode(new MazeNode("o00-reward", "reward-room", 1, RoomType.Reward, false));

            graph.AddEdge(new MazeEdge("e00", "c00-start", "c01-corridor", EdgeType.Open, null, true));
            graph.AddEdge(new MazeEdge("e01", "c01-corridor", "c02-question", EdgeType.Open, null, true));
            graph.AddEdge(new MazeEdge("e02", "c02-question", "c03-junction", EdgeType.Open, null, true));
            graph.AddEdge(new MazeEdge("e03", "c03-junction", "c04-puzzle", EdgeType.Open, null, true));
            graph.AddEdge(new MazeEdge("e04", "c04-puzzle", "c05-checkpoint", EdgeType.Open, null, true));
            graph.AddEdge(new MazeEdge("e05", "c05-checkpoint", "c06-boss", EdgeType.Open, null, true));
            graph.AddEdge(new MazeEdge("oe00", "c03-junction", "o00-reward", EdgeType.Open, null, true));

            return graph;
        }
    }
}
