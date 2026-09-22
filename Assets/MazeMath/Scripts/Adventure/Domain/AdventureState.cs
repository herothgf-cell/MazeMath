using System;
using System.Collections.Generic;
using System.Linq;

namespace MazeMath.Adventure
{
    // Plain fields deliberately match both JsonUtility and the headless test serializer.
    [Serializable]
    public sealed class AdventureState
    {
        public int schema = 2;
        public int seed;
        public float x = 6, y, checkpointX = 6, checkpointY;
        public int health = 5, shield = 1, xp, questionIndex;
        public int[] materials = new int[5]; // scrap, iron, gear, crystal, core
        public int[] pending = new int[5];
        public bool[] owned = new bool[5]; // mining, wrench, boots, shield, sensor
        public bool[] equipped = new bool[5];
        public int[] enchants = { -1, -1, -1, -1, -1 };
        public bool[] unlockedEnchants = new bool[10];
        public bool[] visited = new bool[15];
        public List<string> flags = new List<string>();
        public AdventureQuestion question;
        public bool Has(string flag) { return flags.Contains(flag); }

        public static AdventureState NewRun(int seed)
        {
            var s = new AdventureState { seed = seed };
            s.materials[0] = 1; s.materials[1] = 1; s.visited[0] = true;
            return s;
        }

        public bool IsValid()
        {
            if (schema != 2 || !Finite(x) || !Finite(y) || !Finite(checkpointX) || !Finite(checkpointY)) return false;
            if (x < .4f || x > 59.6f || y < -4 || y > 24 || checkpointX < .4f || checkpointX > 59.6f || checkpointY < 0 || checkpointY > 20) return false;
            if (health < 1 || health > 5 || shield < 0 || shield > 1 || xp < 0 || xp > 99999 || questionIndex < 0 || questionIndex > 100000) return false;
            if (materials == null || materials.Length != 5 || pending == null || pending.Length != 5 || owned == null || owned.Length != 5 || equipped == null || equipped.Length != 5 || enchants == null || enchants.Length != 5 || unlockedEnchants == null || unlockedEnchants.Length != 10 || visited == null || visited.Length != 15) return false;
            if (flags == null || flags.Count > 256 || flags.Any(v => string.IsNullOrWhiteSpace(v) || v.Length > 80) || flags.Distinct().Count() != flags.Count) return false;
            for (int i = 0; i < 5; i++)
            {
                if (materials[i] < 0 || materials[i] > 99 || pending[i] < 0 || pending[i] > 9999 || (equipped[i] && !owned[i])) return false;
                if (enchants[i] < -1 || enchants[i] > 1) return false;
                if (enchants[i] >= 0 && (!owned[i] || !unlockedEnchants[i * 2 + enchants[i]])) return false;
            }
            if (Has("clear") && AdventureRules.Shields(this) != 0) return false;
            return question == null || question.IsValid();
        }
        private static bool Finite(float v) { return !float.IsNaN(v) && !float.IsInfinity(v); }
        public AdventureState Copy()
        {
            var c = (AdventureState)MemberwiseClone();
            c.materials = (int[])materials.Clone(); c.pending = (int[])pending.Clone();
            c.owned = (bool[])owned.Clone(); c.equipped = (bool[])equipped.Clone();
            c.enchants = (int[])enchants.Clone(); c.unlockedEnchants = (bool[])unlockedEnchants.Clone();
            c.visited = (bool[])visited.Clone(); c.flags = new List<string>(flags);
            c.question = question == null ? null : question.Copy();
            return c;
        }
    }

    [Serializable]
    public sealed class AdventureQuestion
    {
        public string source, prompt;
        public int answer, attempts;
        public int[] choices;
        public bool multipleChoice, solved;
        public bool IsValid()
        {
            return (source == "math" || source == "boss.math") && !string.IsNullOrWhiteSpace(prompt) && prompt.Length <= 100 &&
                answer >= 0 && answer <= 99 && attempts >= 0 && attempts <= 100000 && choices != null && choices.Length == 4 &&
                choices.Distinct().Count() == 4 && choices.Count(v => v == answer) == 1;
        }
        public AdventureQuestion Copy()
        {
            var q = (AdventureQuestion)MemberwiseClone(); q.choices = (int[])choices.Clone(); return q;
        }
    }
}
