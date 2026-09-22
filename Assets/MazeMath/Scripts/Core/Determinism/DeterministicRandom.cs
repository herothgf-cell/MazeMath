using System;

namespace MazeMath.Core.Determinism
{
    public sealed class DeterministicRandom
    {
        private const uint ZeroSeedFallback = 0x6D2B79F5u;
        private uint _state;

        public DeterministicRandom(uint seed)
        {
            _state = seed == 0u ? ZeroSeedFallback : seed;
        }

        public uint NextUInt()
        {
            var x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));

            var range = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextUInt() % range);
        }

        public bool NextBool()
        {
            return (NextUInt() & 1u) == 1u;
        }
    }
}
