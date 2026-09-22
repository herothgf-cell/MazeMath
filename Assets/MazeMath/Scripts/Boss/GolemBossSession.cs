using System;
using System.Collections.Generic;

namespace MazeMath.Boss
{
    public sealed class GolemBossSession
    {
        private readonly HashSet<string> completedPhaseIds = new HashSet<string>();

        public int MaxShields { get; }
        public int ShieldsRemaining { get; private set; }
        public bool CanFinish => ShieldsRemaining == 0;
        public bool IsCleared { get; private set; }

        public GolemBossSession(int shieldCount)
        {
            if (shieldCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(shieldCount));

            MaxShields = shieldCount;
            ShieldsRemaining = shieldCount;
        }

        public bool CompletePhase(string phaseId)
        {
            if (IsCleared ||
                ShieldsRemaining <= 0 ||
                string.IsNullOrWhiteSpace(phaseId) ||
                !completedPhaseIds.Add(phaseId))
            {
                return false;
            }

            ShieldsRemaining--;
            return true;
        }

        public bool Finish()
        {
            if (!CanFinish || IsCleared)
                return false;

            IsCleared = true;
            return true;
        }

        public bool HasCompletedPhase(string phaseId)
        {
            return completedPhaseIds.Contains(phaseId);
        }
    }
}
