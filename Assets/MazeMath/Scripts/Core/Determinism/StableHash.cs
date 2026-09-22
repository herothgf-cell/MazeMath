using System.Text;

namespace MazeMath.Core.Determinism
{
    public static class StableHash
    {
        private const uint OffsetBasis = 2166136261u;
        private const uint Prime = 16777619u;

        public static uint Fnv1A32(string? value)
        {
            unchecked
            {
                var hash = OffsetBasis;
                var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);

                for (var i = 0; i < bytes.Length; i++)
                {
                    hash ^= bytes[i];
                    hash *= Prime;
                }

                return hash;
            }
        }
    }
}
