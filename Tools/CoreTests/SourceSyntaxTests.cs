using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace MazeMath.CoreTests
{
    public sealed class SourceSyntaxTests
    {
        public static string RepositoryRoot()
        {
            var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Assets"))) dir = dir.Parent;
            if (dir == null) throw new DirectoryNotFoundException("Repository root not found.");
            return dir.FullName;
        }
        [TestCase("UNITY_EDITOR")]
        [TestCase("UNITY_WEBGL")]
        [TestCase("UNITY_ANDROID")]
        public void UnitySourcesHaveValidCSharp9Syntax(string platform)
        {
            var options = new CSharpParseOptions(LanguageVersion.CSharp9, preprocessorSymbols:new[]{platform,"UNITY_2022_3","ENABLE_INPUT_SYSTEM"});
            foreach (var file in Directory.GetFiles(Path.Combine(RepositoryRoot(), "Assets"), "*.cs", SearchOption.AllDirectories))
            {
                var tree=CSharpSyntaxTree.ParseText(File.ReadAllText(file),options,file);
                var errors=tree.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).ToArray();
                Assert.IsEmpty(errors,string.Join("\n",errors.Select(e=>e.ToString())));
            }
        }
        [Test] public void LegacyInputFallbackHasValidSyntax()
        {
            var options=new CSharpParseOptions(LanguageVersion.CSharp9,preprocessorSymbols:new[]{"UNITY_EDITOR","UNITY_2022_3","ENABLE_LEGACY_INPUT_MANAGER"});
            foreach(var file in Directory.GetFiles(Path.Combine(RepositoryRoot(),"Assets/MazeMath/Scripts/Adventure"),"*.cs",SearchOption.AllDirectories))
                Assert.IsEmpty(CSharpSyntaxTree.ParseText(File.ReadAllText(file),options,file).GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).ToArray(),file);
        }
        [Test] public void AdventureSceneReferencesCommittedScriptGuid()
        {
            string root=RepositoryRoot(); string meta=File.ReadAllText(Path.Combine(root,"Assets/MazeMath/Scripts/Adventure/Runtime/AdventureGame.cs.meta"));
            string guid=meta.Split('\n').Single(v=>v.StartsWith("guid: ")).Substring(6).Trim();
            string scene=File.ReadAllText(Path.Combine(root,"Assets/MazeMath/Scenes/Adventure.unity"));
            StringAssert.Contains("guid: "+guid,scene);
        }
    }
}
