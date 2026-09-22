using System;

namespace MazeMath.Maze.Generation
{
    public sealed class DeterministicRandom
    {
        private uint state;

        public DeterministicRandom(int seed)
        {
            state = unchecked((uint)seed);
            if (state == 0u)
            {
                state = 0x6D2B79F5u;
            }
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            }

            var range = unchecked((uint)(maxExclusive - minInclusive));
            var value = NextUInt() % range;
            return minInclusive + unchecked((int)value);
        }

        public bool NextBool()
        {
            return (NextUInt() & 1u) == 1u;
        }

        public uint NextUInt()
        {
            var x = state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            state = x;
            return x;
        }
    }
}
