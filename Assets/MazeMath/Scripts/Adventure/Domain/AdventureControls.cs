using System.Collections.Generic;

namespace MazeMath.Adventure
{
    public sealed class AdventureControls
    {
        private readonly Dictionary<int, int> held = new Dictionary<int, int>();
        private bool jump, interact;
        // token = PointerEventData.pointerId; direction: 0 left, 1 right, 2 up, 3 down.
        public void Hold(int token, int direction) { held[token] = direction; }
        public void Release(int token) { held.Remove(token); }
        public float Horizontal { get { int v = 0; foreach (int d in held.Values) v += d == 0 ? -1 : d == 1 ? 1 : 0; return System.Math.Max(-1, System.Math.Min(1, v)); } }
        public float Vertical { get { int v = 0; foreach (int d in held.Values) v += d == 3 ? -1 : d == 2 ? 1 : 0; return System.Math.Max(-1, System.Math.Min(1, v)); } }
        public void Jump() { jump = true; }
        public void Interact() { interact = true; }
        public bool ConsumeJump() { bool v = jump; jump = false; return v; }
        public bool ConsumeInteract() { bool v = interact; interact = false; return v; }
        public void Clear() { held.Clear(); jump = interact = false; }
    }
}
