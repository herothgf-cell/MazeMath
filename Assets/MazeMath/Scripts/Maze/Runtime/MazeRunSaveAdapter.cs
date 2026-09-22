using System;
using MazeMath.Core.Save;

namespace MazeMath.Maze.Runtime
{
    public sealed class MazeRunSaveAdapter
    {
        public const string DefaultSaveKey = "maze.current-run";

        private readonly SaveService saveService;
        private readonly string saveKey;

        public MazeRunSaveAdapter(
            SaveService saveService,
            string saveKey = DefaultSaveKey)
        {
            this.saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            this.saveKey = string.IsNullOrWhiteSpace(saveKey)
                ? DefaultSaveKey
                : saveKey;
        }

        public void Save(MazeRunState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            saveService.Save(saveKey, state);
        }

        public bool TryLoad(out MazeRunState state)
        {
            return saveService.TryLoad(saveKey, out state);
        }

        public void Delete()
        {
            saveService.Delete(saveKey);
        }
    }
}
