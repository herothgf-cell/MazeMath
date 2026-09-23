using System.IO;
using NUnit.Framework;

namespace MazeMath.CoreTests
{
    public sealed class AdventureBlockStyleRegressionTests
    {
        private static string Read(string path)
        {
            return File.ReadAllText(Path.Combine(SourceSyntaxTests.RepositoryRoot(), path));
        }

        [Test]
        public void ThemeKeepsTextCrispInsteadOfShrinkingEverything()
        {
            var source = Read("Assets/MazeMath/Scripts/Adventure/Runtime/AdventureTheme.cs");

            StringAssert.DoesNotContain("resizeTextForBestFit=true", source);
            StringAssert.Contains("FontStyle.Bold", source);
            StringAssert.Contains("AddComponent<Outline>", source);
        }

        [Test]
        public void ThemeUsesCuteBlockVisualTokens()
        {
            var source = Read("Assets/MazeMath/Scripts/Adventure/Runtime/AdventureTheme.cs");

            StringAssert.Contains("BlockStone", source);
            StringAssert.Contains("Grass", source);
            StringAssert.Contains("Wood", source);
            StringAssert.Contains("SlotDark", source);
            StringAssert.Contains("BlockBorder", source);
        }

        [Test]
        public void GeneratedAdventureArtUsesCrispPointFiltering()
        {
            var source = Read("Assets/MazeMath/Scripts/Adventure/Runtime/AdventureArt.cs");

            StringAssert.Contains("filterMode=FilterMode.Point", source);
        }

        [Test]
        public void HudUsesReadableTypeSizesAndBlockHotbar()
        {
            var source = Read("Assets/MazeMath/Scripts/Adventure/Runtime/AdventureHud.cs");

            StringAssert.Contains("BlockHotbar", source);
            StringAssert.Contains("quest=t.Label(header,\"\",19", source);
            StringAssert.Contains("tip=t.Label(dock,\"\",16", source);
        }
    }
}
