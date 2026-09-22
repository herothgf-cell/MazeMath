using MazeMath.Questions.Runtime;
using NUnit.Framework;

namespace MazeMath.Tests.Questions
{
    public sealed class NumericInputBufferTests
    {
        [Test]
        public void AppendAndBackspace_UpdatesValue()
        {
            var buffer = new NumericInputBuffer(3);

            buffer.AppendDigit(2);
            buffer.AppendDigit(3);
            Assert.AreEqual("23", buffer.Text);

            buffer.Backspace();
            Assert.AreEqual("2", buffer.Text);
        }

        [Test]
        public void AppendDigit_DoesNotExceedMaxLength()
        {
            var buffer = new NumericInputBuffer(2);

            Assert.IsTrue(buffer.AppendDigit(1));
            Assert.IsTrue(buffer.AppendDigit(2));
            Assert.IsFalse(buffer.AppendDigit(3));
            Assert.AreEqual("12", buffer.Text);
        }

        [Test]
        public void AppendDigit_RejectsValuesOutsideZeroToNine()
        {
            var buffer = new NumericInputBuffer(3);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => buffer.AppendDigit(10));
        }

        [Test]
        public void Clear_EmptiesInput()
        {
            var buffer = new NumericInputBuffer(3);
            buffer.AppendDigit(9);

            buffer.Clear();

            Assert.AreEqual(string.Empty, buffer.Text);
        }
    }
}
