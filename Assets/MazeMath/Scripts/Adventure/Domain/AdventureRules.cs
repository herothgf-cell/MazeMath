using System;

namespace MazeMath.Adventure
{
    public static class AdventureRules
    {
        private static readonly int[][] Costs = {
            new[]{2,1,0,0,0}, new[]{2,0,1,1,0}, new[]{2,0,2,1,0}, new[]{0,2,0,2,0}, new[]{2,0,2,1,0}
        };
        public static int[] Cost(int tool) { CheckTool(tool); return (int[])Costs[tool].Clone(); }
        public static int[] Pattern(int tool)
        {
            if (tool == 4) return new[]{2,-1,2, -1,3,-1, 0,-1,0};
            if (tool == 0) return new[]{1,1,-1, 0,2,-1, -1,0,-1};
            return null;
        }
        public static bool Craft(AdventureState s, int tool, int[] grid = null)
        {
            if (s == null || tool < 0 || tool > 4 || s.owned[tool] || (tool != 0 && !s.owned[0])) return false;
            var costs = Cost(tool);
            if (grid != null)
            {
                var pattern = Pattern(tool);
                if (pattern == null || grid.Length != 9) return false;
                costs = new int[5];
                for (int i = 0; i < 9; i++) { if (grid[i] != pattern[i]) return false; if (grid[i] >= 0) costs[grid[i]]++; }
            }
            for (int i = 0; i < 5; i++) if (s.materials[i] < costs[i]) return false;
            for (int i = 0; i < 5; i++) s.materials[i] -= costs[i];
            s.owned[tool] = s.equipped[tool] = true;
            Flush(s); return true;
        }
        public static bool Enchant(AdventureState s, int tool, int variant)
        {
            if (tool < 0 || tool > 4 || variant < 0 || variant > 1 || !s.owned[tool]) return false;
            int id = tool * 2 + variant;
            if (!s.unlockedEnchants[id])
            {
                if (s.xp < 10) return false;
                s.xp -= 10; s.unlockedEnchants[id] = true;
            }
            s.enchants[tool] = variant; s.equipped[tool] = true; return true;
        }
        public static bool HasTool(AdventureState s, int tool) { return s.owned[tool] && s.equipped[tool]; }
        public static bool HasEnchant(AdventureState s, int tool, int variant)
        {
            return HasTool(s, tool) && s.enchants[tool] == variant && s.unlockedEnchants[tool * 2 + variant];
        }
        public static bool Claim(AdventureState s, string id, int xp, int[] items)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length > 64 || xp < 0 || items == null || items.Length != 5) throw new ArgumentException("Invalid reward.");
            foreach (int n in items) if (n < 0 || n > 99) throw new ArgumentException("Invalid reward count.");
            if (s.Has("reward/" + id)) return false;
            s.flags.Add("reward/" + id); s.xp = Math.Min(99999, s.xp + xp);
            for (int i = 0; i < 5; i++) s.pending[i] = Math.Min(9999, s.pending[i] + items[i]);
            Flush(s); return true;
        }
        public static void Flush(AdventureState s)
        {
            for (int i = 0; i < 5; i++) { int count = Math.Min(99 - s.materials[i], s.pending[i]); s.materials[i] += count; s.pending[i] -= count; }
        }
        public static bool Complete(AdventureState s, string id)
        {
            if (id != "math" && id != "bridge" && id != "laser" && id != "sequence" && id != "boss.math" && id != "boss.sequence" && id != "boss.laser") throw new ArgumentException("Unknown challenge.");
            if (s.Has(id)) return false;
            s.flags.Add(id);
            Claim(s, id, id.StartsWith("boss.", StringComparison.Ordinal) ? 20 : 10,
                id == "math" ? new[]{2,0,0,0,0} : new[]{2,1,1,1,0});
            if (HasEnchant(s, 3, 0)) s.shield = 1;
            return true;
        }
        public static bool OpenMiningGate(AdventureState s)
        {
            if (s.Has("mined") || !HasTool(s, 0)) return false;
            s.flags.Add("mined"); return true;
        }
        public static int Shields(AdventureState s)
        {
            return 3 - (s.Has("boss.math") ? 1 : 0) - (s.Has("boss.sequence") ? 1 : 0) - (s.Has("boss.laser") ? 1 : 0);
        }
        public static bool Finish(AdventureState s)
        {
            if (s.Has("clear") || Shields(s) != 0) return false;
            s.flags.Add("clear"); Claim(s, "chapter-clear", 30, new[]{0,0,0,0,1}); return true;
        }
        public static void Recover(AdventureState s, bool falling = true)
        {
            if (falling && HasEnchant(s, 2, 0)) return;
            if (HasTool(s, 3) && s.shield > 0) { s.shield--; return; }
            s.health = s.health <= 1 ? 5 : s.health - 1;
        }
        private static void CheckTool(int tool) { if (tool < 0 || tool > 4) throw new ArgumentOutOfRangeException(nameof(tool)); }
    }
}
