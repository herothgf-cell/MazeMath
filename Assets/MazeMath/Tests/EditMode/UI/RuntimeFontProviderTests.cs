using MazeMath.UI;
using NUnit.Framework;

namespace MazeMath.Tests.UI
{
    public sealed class RuntimeFontProviderTests
    {
        [Test]
        public void Get_ReturnsBuiltInRuntimeFont()
        {
            var font = RuntimeFontProvider.Get();

            Assert.IsNotNull(font);
        }
    }
}
