using MazeMath.Content;
using MazeMath.Editor;
using NUnit.Framework;
using UnityEditor;

namespace MazeMath.Tests.Editor
{
    public sealed class Chapter1ContentSetupTests
    {
        [Test]
        public void SetupProject_CreatesChapterOneCatalog()
        {
            ProjectSetup.ConfigureProject();

            var catalog = AssetDatabase.LoadAssetAtPath<Chapter1ContentCatalog>(
                Chapter1ContentSetup.CatalogPath);

            Assert.IsNotNull(catalog);
            Assert.IsNotNull(catalog.chapter);
            Assert.AreEqual(10, catalog.items.Count);
            Assert.AreEqual(5, catalog.equipment.Count);
            Assert.GreaterOrEqual(catalog.recipes.Count, 7);
            Assert.AreEqual(10, catalog.enchants.Count);
            Assert.GreaterOrEqual(catalog.questionTemplates.Count, 5);
        }
    }
}
