using System;
using System.Collections.Generic;

namespace MazeMath.Adventure
{
    // Fixed-step kinematic AABB controller. Coordinates are feet, not sprite centre.
    public sealed class AdventureMotor
    {
        public const float HalfWidth = .38f, Height = 1.65f;
        public float X { get; private set; }
        public float Y { get; private set; }
        public float VelocityY { get; private set; }
        public bool Grounded { get; private set; } = true;
        public bool Climbing { get; private set; }
        public bool Recovered { get; private set; }
        private AdventureLadder ladder;
        private readonly List<AdventureSolid> solids = new List<AdventureSolid>(20);
        private readonly List<AdventureLadder> ladders = new List<AdventureLadder>(3);
        public AdventureMotor(float x, float y) { Place(x, y); }
        public void Place(float x, float y)
        {
            if (float.IsNaN(x) || float.IsInfinity(x) || float.IsNaN(y) || float.IsInfinity(y)) throw new ArgumentException("Non-finite position.");
            X = x; Y = y; VelocityY = 0; Climbing = false; Grounded = true;
        }
        public void Tick(float horizontal, float vertical, bool jump, float dt, AdventureState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0 || dt > .051f) throw new ArgumentOutOfRangeException(nameof(dt));
            if (float.IsNaN(horizontal) || float.IsNaN(vertical)) throw new ArgumentException("Non-finite input.");
            if (dt == 0) return;
            horizontal = Math.Max(-1, Math.Min(1, horizontal)); vertical = Math.Max(-1, Math.Min(1, vertical));
            Recovered = false; AdventureWorld.Solids(state, solids); AdventureWorld.Ladders(state, ladders);
            if (!Climbing && VelocityY <= 0)
            {
                Grounded = false;
                foreach (var floor in solids)
                    if (X + HalfWidth > floor.left && X - HalfWidth < floor.right && Math.Abs(Y - floor.top) < .003f) { Grounded = true; break; }
            }
            if (Math.Abs(horizontal) > .05f) Climbing = false;
            if (Math.Abs(vertical) > .1f)
            {
                foreach (var candidate in ladders)
                {
                    if (Math.Abs(X - candidate.x) > .7f || Y < candidate.bottom - .1f || Y > candidate.top + .1f) continue;
                    if (vertical > 0 && Y >= candidate.top - .001f) continue;
                    if (vertical < 0 && Y <= candidate.bottom + .001f) continue;
                    ladder = candidate; Climbing = true; X = candidate.x; break;
                }
            }
            if (jump && Grounded && !Climbing) { VelocityY = AdventureRules.HasTool(state, 2) ? 13 : 9.5f; Grounded = false; }
            float targetX = X + horizontal * 6 * dt;
            foreach (var solid in solids)
            {
                if (solid.platform || Y + Height <= solid.bottom + .001f || Y >= solid.top - .001f) continue;
                if (horizontal > 0 && X + HalfWidth <= solid.left + .001f && targetX + HalfWidth > solid.left) targetX = Math.Min(targetX, solid.left - HalfWidth);
                if (horizontal < 0 && X - HalfWidth >= solid.right - .001f && targetX - HalfWidth < solid.right) targetX = Math.Max(targetX, solid.right + HalfWidth);
            }
            X = Math.Max(.4f, Math.Min(59.6f, targetX));
            if (Climbing)
            {
                VelocityY = 0; Grounded = false;
                Y = Math.Max(ladder.bottom, Math.Min(ladder.top, Y + vertical * 4.5f * dt));
                if ((Y >= ladder.top && vertical > 0) || (Y <= ladder.bottom && vertical < 0)) { Climbing = false; Grounded = true; }
            }
            else
            {
                VelocityY = Math.Max(-18, VelocityY - 24 * dt); float nextY = Y + VelocityY * dt; Grounded = false;
                foreach (var solid in solids)
                {
                    if (X + HalfWidth <= solid.left || X - HalfWidth >= solid.right) continue;
                    if (VelocityY <= 0 && Y >= solid.top - .002f && nextY <= solid.top)
                    { nextY = Math.Max(nextY, solid.top); Grounded = true; VelocityY = 0; }
                    else if (!solid.platform && VelocityY > 0 && Y + Height <= solid.bottom && nextY + Height >= solid.bottom)
                    { nextY = solid.bottom - Height; VelocityY = 0; }
                }
                Y = nextY;
            }
            if (Y < -3)
            {
                AdventureRules.Recover(state); Place(state.checkpointX, state.checkpointY); Recovered = true;
            }
        }
    }
}
