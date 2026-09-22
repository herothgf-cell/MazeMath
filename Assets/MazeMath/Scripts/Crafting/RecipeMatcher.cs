using System;
using System.Collections.Generic;

namespace MazeMath.Crafting
{
    public static class RecipeMatcher
    {
        public static bool Matches(
            RecipeDefinition recipe,
            IReadOnlyList<string> gridItemIds)
        {
            if (recipe == null || gridItemIds == null || gridItemIds.Count != 9)
                return false;
            if (recipe.patternItemIds == null || recipe.patternItemIds.Count != 9)
                return false;

            var basePattern = Normalize(recipe.patternItemIds);
            var grid = Normalize(gridItemIds);

            if (Equal(basePattern, grid))
                return true;

            var current = basePattern;
            if (recipe.allowRotation)
            {
                for (var i = 0; i < 3; i++)
                {
                    current = Rotate90(current);
                    if (Equal(current, grid))
                        return true;
                }
            }

            if (recipe.allowMirror)
            {
                var mirrored = Mirror(basePattern);
                if (Equal(mirrored, grid))
                    return true;

                if (recipe.allowRotation)
                {
                    current = mirrored;
                    for (var i = 0; i < 3; i++)
                    {
                        current = Rotate90(current);
                        if (Equal(current, grid))
                            return true;
                    }
                }
            }

            return false;
        }

        private static string[] Normalize(IReadOnlyList<string> values)
        {
            var result = new string[9];
            for (var i = 0; i < 9; i++)
                result[i] = string.IsNullOrWhiteSpace(values[i]) ? string.Empty : values[i].Trim();
            return result;
        }

        private static bool Equal(string[] left, string[] right)
        {
            for (var i = 0; i < 9; i++)
            {
                if (!string.Equals(left[i], right[i], StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        private static string[] Rotate90(string[] source)
        {
            var result = new string[9];
            for (var row = 0; row < 3; row++)
            {
                for (var col = 0; col < 3; col++)
                {
                    result[col * 3 + (2 - row)] = source[row * 3 + col];
                }
            }
            return result;
        }

        private static string[] Mirror(string[] source)
        {
            var result = new string[9];
            for (var row = 0; row < 3; row++)
            {
                for (var col = 0; col < 3; col++)
                {
                    result[row * 3 + (2 - col)] = source[row * 3 + col];
                }
            }
            return result;
        }
    }
}
