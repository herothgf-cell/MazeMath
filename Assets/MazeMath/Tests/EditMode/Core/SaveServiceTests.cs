using System;
using System.Collections.Generic;
using MazeMath.Core.Save;
using NUnit.Framework;

namespace MazeMath.Tests.Core
{
    public sealed class SaveServiceTests
    {
        [Serializable]
        private sealed class SamplePayload
        {
            public int value;
            public string name;
        }

        private sealed class MemorySaveStore : ISaveStore
        {
            private readonly Dictionary<string, string> values = new();

            public void Save(string key, string json) => values[key] = json;

            public bool TryLoad(string key, out string json) =>
                values.TryGetValue(key, out json);

            public void Delete(string key) => values.Remove(key);

            public void PutRaw(string key, string json) => values[key] = json;
        }

        [Test]
        public void SaveThenLoad_RoundTripsPayload()
        {
            var store = new MemorySaveStore();
            var service = new SaveService(store);

            service.Save("profile", new SamplePayload { value = 7, name = "Momo" });

            Assert.IsTrue(service.TryLoad("profile", out SamplePayload loaded));
            Assert.AreEqual(7, loaded.value);
            Assert.AreEqual("Momo", loaded.name);
        }

        [Test]
        public void MissingKey_ReturnsFalse()
        {
            var service = new SaveService(new MemorySaveStore());

            Assert.IsFalse(service.TryLoad("missing", out SamplePayload _));
        }

        [Test]
        public void CorruptEnvelope_ReturnsFalseWithoutThrowing()
        {
            var store = new MemorySaveStore();
            store.PutRaw("profile", "{this-is-not-json");
            var service = new SaveService(store);

            Assert.DoesNotThrow(() =>
            {
                Assert.IsFalse(service.TryLoad("profile", out SamplePayload _));
            });
        }

        [Test]
        public void UnsupportedVersion_ReturnsFalse()
        {
            var store = new MemorySaveStore();
            store.PutRaw("profile", "{\"version\":999,\"payload\":\"{}\"}");
            var service = new SaveService(store);

            Assert.IsFalse(service.TryLoad("profile", out SamplePayload _));
        }
    }
}
