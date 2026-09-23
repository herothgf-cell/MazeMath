using System;

namespace MazeMath.Adventure
{
    /// <summary>Pure entry policy shared by runtime redirection and the core regression suite.</summary>
    public static class AdventureStartupPolicy
    {
        public const string UiRevision = "Cozy UI 3.1";

        public static bool ShouldRedirect(string sceneName, bool legacyDemoPresent, bool currentAdventurePresent)
        {
            if (currentAdventurePresent) return false;
            return legacyDemoPresent || string.Equals(sceneName, "Bootstrap", StringComparison.Ordinal);
        }
    }
}
