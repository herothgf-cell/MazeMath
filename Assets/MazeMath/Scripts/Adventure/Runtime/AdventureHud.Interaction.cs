using UnityEngine;

namespace MazeMath.Adventure
{
    public sealed partial class AdventureHud
    {
        /// <summary>
        /// The success timer is intentionally independent of layout, world labels and camera styling.
        /// Even if a presentation update fails, a solved answer cannot leave the input modal stuck.
        /// </summary>
        private void Update()
        {
            if (game == null || t == null) return;
            if (kind == "question" && questionFlow.ShouldClose(Time.unscaledTime))
            {
                ForceClose();
                Toast(t.T("정답! 모모와 다시 출발해요.", "Great job! Let's explore with Momo."));
            }
            if (eyebrow != null)
                eyebrow.text = "MOMO / CHAPTER 01  ·  " + AdventureStartupPolicy.UiRevision;
        }
    }
}
