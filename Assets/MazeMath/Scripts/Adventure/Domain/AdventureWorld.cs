using System;
using System.Collections.Generic;

namespace MazeMath.Adventure
{
    public struct AdventureSolid
    {
        public float left, bottom, right, top;
        public bool platform;
        public AdventureSolid(float left, float bottom, float right, float top, bool platform = false)
        { this.left = left; this.bottom = bottom; this.right = right; this.top = top; this.platform = platform; }
    }
    public struct AdventureLadder
    {
        public float x, bottom, top;
        public AdventureLadder(float x, float bottom, float top) { this.x = x; this.bottom = bottom; this.top = top; }
    }
    public static class AdventureWorld
    {
        public const float Width = 60, FloorHeight = 8, RoomWidth = 12;
        public static bool MiningOpen(AdventureState s) { return s.Has("mined"); }
        public static int RoomAt(float x, float y)
        {
            int floor = Math.Max(0, Math.Min(2, (int)Math.Floor((y + .25f) / 8)));
            return floor * 5 + Math.Max(0, Math.Min(4, (int)(x / 12)));
        }
        // The renderer and motor consume this same geometry; no hidden collision-only shortcuts.
        public static void Solids(AdventureState s, List<AdventureSolid> output)
        {
            output.Clear();
            output.Add(new AdventureSolid(0, -.5f, 47, 0, true));
            output.Add(new AdventureSolid(50, -.5f, 60, 0, true));
            if (s.Has("bridge")) output.Add(new AdventureSolid(47, -.25f, 50, 0, true));
            output.Add(new AdventureSolid(0, 7.5f, 60, 8, true));
            output.Add(new AdventureSolid(0, 15.5f, 60, 16, true));
            output.Add(new AdventureSolid(-1, -4, 0, 26)); output.Add(new AdventureSolid(60, -4, 61, 26));
            if (!MiningOpen(s)) output.Add(new AdventureSolid(35.6f, 0, 36.4f, 6.8f));
            if (!s.Has("bridge")) output.Add(new AdventureSolid(46.0f, 0, 46.5f, 6.8f));
            if (!s.Has("laser")) output.Add(new AdventureSolid(35.6f, 8, 36.4f, 14.8f));
            if (!s.Has("repaired")) output.Add(new AdventureSolid(11.6f, 8, 12.4f, 14.8f));
            if (!s.Has("repaired")) output.Add(new AdventureSolid(11.6f, 16, 12.4f, 22.8f));
            output.Add(new AdventureSolid(3, 18.7f, 9, 19, true));
        }
        public static void Ladders(AdventureState s, List<AdventureLadder> output)
        {
            output.Clear(); output.Add(new AdventureLadder(54, 0, 8));
            if (s.Has("sequence")) { output.Add(new AdventureLadder(30, 8, 16)); output.Add(new AdventureLadder(18, 0, 8)); }
        }
        public static void Objective(AdventureState s, out float x, out float y, out string key)
        {
            x = 30; y = 0; key = "math";
            if (!s.Has("math")) return;
            if (!s.owned[0]) { x = 18; key = "craft"; return; }
            if (!s.Has("mined")) { x = 34; key = "mine"; return; }
            if (!s.Has("bridge")) { x = 44; key = "bridge"; return; }
            if (!s.Has("laser")) { x = 45; y = 8; key = "laser"; return; }
            if (!s.Has("sequence")) { x = 18; y = 8; key = "sequence"; return; }
            y = 16;
            if (!s.Has("boss.math")) { x = 38; key = "boss.math"; return; }
            if (!s.Has("boss.sequence")) { x = 44; key = "boss.sequence"; return; }
            if (!s.Has("boss.laser")) { x = 54; key = "boss.laser"; return; }
            x = 57; key = s.Has("clear") ? "clear" : "finish";
        }
    }
}
