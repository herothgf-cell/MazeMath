using System;

namespace MazeMath.Adventure
{
    /// <summary>One question's UI lifecycle. Tick with unscaled time, even while gameplay is paused.</summary>
    public sealed class QuestionFeedbackFlow
    {
        public const double SuccessDelay = 0.60;
        private bool open, pending;
        private double deadline;
        public bool CanSubmit => open && !pending;
        public bool IsPending => pending;
        public void Begin() { open = true; pending = false; deadline = 0; }
        public bool RecordAnswer(bool correct, double now)
        {
            if (!CanSubmit || !correct || double.IsNaN(now) || double.IsInfinity(now)) return false;
            pending = true; deadline = now + SuccessDelay; return true;
        }
        public bool ShouldClose(double now)
        {
            if (!open || !pending || double.IsNaN(now) || now < deadline) return false;
            Cancel(); return true;
        }
        public void Cancel() { open = false; pending = false; deadline = 0; }
    }
}
