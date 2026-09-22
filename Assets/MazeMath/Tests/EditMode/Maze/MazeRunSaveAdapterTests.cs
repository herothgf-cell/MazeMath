using System.Collections.Generic;
using MazeMath.Core.Save;
using MazeMath.Maze.Runtime;
using NUnit.Framework;

namespace MazeMath.Tests.Maze
{
    public sealed class MazeRunSaveAdapterTests
    {
        private sealed class MemorySaveStore : ISaveStore
        {
            private readonly Dictionary<string, string> values = new Dictionary<string, string>();

            public void Save(string key, string json) => values[key] = json;
            public bool TryLoad(string key, out string json) => values.TryGetValue(key, out json);
            public void Delete(string key) => values.Remove(key);
        }

        [Test]
        public void SaveThenLoad_RestoresMazeRunState()
        {
            var service = new SaveService(new MemorySaveStore());
            var adapter = new MazeRunSaveAdapter(service);
            var state = new MazeRunState
            {
                chapterId = "chapter-01",
                runSeed = 44,
                currentNodeId = "critical-03",
                currentFloor = 1,
                activeObjectiveId = "objective-blue-door",
                playerSpawnId = "spawn-b"
            };
            state.visitedNodeIds.Add("start");
            state.visitedNodeIds.Add("critical-03");
            state.seenNodeIds.Add("optional-00");
            state.clearedNodeIds.Add("critical-01");
            state.solvedGateIds.Add("gate-blue");

            adapter.Save(state);

            Assert.IsTrue(adapter.TryLoad(out var loaded));
            Assert.AreEqual(state.chapterId, loaded.chapterId);
            Assert.AreEqual(state.runSeed, loaded.runSeed);
            Assert.AreEqual(state.currentNodeId, loaded.currentNodeId);
            Assert.AreEqual(state.currentFloor, loaded.currentFloor);
            CollectionAssert.AreEquivalent(state.visitedNodeIds, loaded.visitedNodeIds);
            CollectionAssert.AreEquivalent(state.seenNodeIds, loaded.seenNodeIds);
            CollectionAssert.AreEquivalent(state.clearedNodeIds, loaded.clearedNodeIds);
            CollectionAssert.AreEquivalent(state.solvedGateIds, loaded.solvedGateIds);
        }
    }
}
