using System.Text;

namespace MazeMath.Maze.Generation
{
    public static class MazeSeedService
    {
        public static int Create(
            string profileId,
            string chapterId,
            int attemptIndex,
            int baseOffset)
        {
            var builder = new StringBuilder();
            builder.Append(profileId ?? string.Empty);
            builder.Append('|');
            builder.Append(chapterId ?? string.Empty);
            builder.Append('|');
            builder.Append(attemptIndex);
            builder.Append('|');
            builder.Append(baseOffset);

            return StableHash(builder.ToString());
        }

        public static int StableHash(string value)
        {
            unchecked
            {
                const uint offsetBasis = 2166136261u;
                const uint prime = 16777619u;

                var hash = offsetBasis;
                var text = value ?? string.Empty;

                for (var i = 0; i < text.Length; i++)
                {
                    var code = text[i];
                    hash ^= (byte)(code & 0xFF);
                    hash *= prime;
                    hash ^= (byte)((code >> 8) & 0xFF);
                    hash *= prime;
                }

                return (int)hash;
            }
        }
    }
}
