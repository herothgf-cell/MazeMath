using System.Collections;
using MazeMath.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MazeMath.Tests.Core
{
    public sealed class GameBootstrapTests
    {
        [UnityTest]
        public IEnumerator CreatingSecondBootstrap_DoesNotCreateSecondLiveBootstrap()
        {
            var firstObject = new GameObject("BootstrapA");
            firstObject.AddComponent<GameBootstrap>();
            yield return null;

            var firstRegistry = GameServices.Instance;
            Assert.IsNotNull(firstRegistry);

            var secondObject = new GameObject("BootstrapB");
            secondObject.AddComponent<GameBootstrap>();
            yield return null;

            Assert.AreSame(firstRegistry, GameServices.Instance);
            Assert.AreEqual(
                1,
                Object.FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None).Length);
        }
    }
}
