using System;
using System.Text;

namespace MazeMath.Questions.Runtime
{
    public sealed class NumericInputBuffer
    {
        private readonly int maxLength;
        private readonly StringBuilder buffer = new StringBuilder();

        public string Text => buffer.ToString();

        public NumericInputBuffer(int maxLength)
        {
            if (maxLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength));
            }

            this.maxLength = maxLength;
        }

        public bool AppendDigit(int digit)
        {
            if (digit < 0 || digit > 9)
            {
                throw new ArgumentOutOfRangeException(nameof(digit));
            }

            if (buffer.Length >= maxLength)
            {
                return false;
            }

            buffer.Append((char)('0' + digit));
            return true;
        }

        public bool Backspace()
        {
            if (buffer.Length == 0)
            {
                return false;
            }

            buffer.Length--;
            return true;
        }

        public void Clear()
        {
            buffer.Clear();
        }
    }
}
