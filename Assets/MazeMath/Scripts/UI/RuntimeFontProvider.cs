using UnityEngine;

namespace MazeMath.UI
{
    public static class RuntimeFontProvider
    {
        private const string BuiltInFontPath = "LegacyRuntime.ttf";
        private static Font cached;

        public static Font Get()
        {
            if (cached == null)
            {
                cached = Resources.GetBuiltinResource<Font>(BuiltInFontPath);
            }

            return cached;
        }
    }
}
