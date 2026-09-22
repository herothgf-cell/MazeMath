using System.Collections;
using MazeMath.Core;
using MazeMath.Core.Save;
using MazeMath.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MazeMath.Tests.Core
{
    public sealed class GameBootstrapTests
    {
        [UnityTest]
        public IEnumerator Bootstrap_RegistersInputAndSaveServices()
        {
            var go = new GameObject("BootstrapWithServices");
            var input = go.AddComponent<GameInputService>();
            go.AddComponent<GameBootstrap>();

            yield return null;

            Assert.AreSame(input, GameServices.Instance.Get<IGameInput>());
            Assert.IsNotNull(GameServices.Instance.Get<SaveService>());

            Object.Destroy(go);
            yield return null;
        }

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
