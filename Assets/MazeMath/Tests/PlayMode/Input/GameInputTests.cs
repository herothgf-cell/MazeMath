using System.Collections;
using MazeMath.Input;
using MazeMath.UI.Mobile;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MazeMath.Tests.Input
{
    public sealed class GameInputTests
    {
        [UnityTest]
        public IEnumerator MobileBridge_UsesSameLogicalInputState()
        {
            var inputObject = new GameObject("GameInput");
            var service = inputObject.AddComponent<GameInputService>();

            var bridgeObject = new GameObject("MobileBridge");
            var bridge = bridgeObject.AddComponent<MobileControlBridge>();
            bridge.Bind(service);

            yield return null;

            bridge.MoveRightDown();
            Assert.AreEqual(Vector2.right, service.Move);

            bridge.PressJump();
            Assert.IsTrue(service.JumpPressed);

            bridge.MoveStop();
            Assert.AreEqual(Vector2.zero, service.Move);

            Object.Destroy(inputObject);
            Object.Destroy(bridgeObject);
        }
    }
}
