using MazeMath.Maze;
using MazeMath.Maze.Data;
using NUnit.Framework;
using UnityEngine;

namespace MazeMath.Tests.Maze
{
    public sealed class MazeDefinitionTests
    {
        [Test]
        public void ValidateAuthoring_RejectsZeroFloors()
        {
            var chapter = ScriptableObject.CreateInstance<MazeChapterDefinition>();
            chapter.floorCount = 0;
            chapter.difficulty = ScriptableObject.CreateInstance<MazeDifficultyProfile>();

            var errors = chapter.ValidateAuthoring();

            CollectionAssert.Contains(errors, "FloorCount must be at least 1.");
        }

        [Test]
        public void ValidateAuthoring_RejectsReversedRoomRange()
        {
            var chapter = ScriptableObject.CreateInstance<MazeChapterDefinition>();
            chapter.floorCount = 1;
            chapter.requiredRoomCountRange = new Vector2Int(8, 6);
            chapter.optionalRoomCountRange = new Vector2Int(0, 0);
            chapter.difficulty = ScriptableObject.CreateInstance<MazeDifficultyProfile>();

            var errors = chapter.ValidateAuthoring();

            CollectionAssert.Contains(errors, "Required room range is invalid.");
        }

        [Test]
        public void ValidateAuthoring_RequiresStartAndBossTemplates()
        {
            var chapter = ScriptableObject.CreateInstance<MazeChapterDefinition>();
            chapter.floorCount = 1;
            chapter.requiredRoomCountRange = new Vector2Int(3, 3);
            chapter.optionalRoomCountRange = new Vector2Int(0, 0);
            chapter.difficulty = ScriptableObject.CreateInstance<MazeDifficultyProfile>();

            var errors = chapter.ValidateAuthoring();

            CollectionAssert.Contains(errors, "A Start room template is required.");
            CollectionAssert.Contains(errors, "A Boss room template is required.");
        }
    }
}
