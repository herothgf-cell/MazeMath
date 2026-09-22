using System;
using UnityEngine;

namespace MazeMath.Core.Save
{
    public sealed class SaveService
    {
        public const int CurrentVersion = 1;

        private readonly ISaveStore store;

        public SaveService(ISaveStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public void Save<T>(string key, T data)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Save key must not be empty.", nameof(key));
            }

            var envelope = new SaveEnvelope
            {
                version = CurrentVersion,
                payload = JsonUtility.ToJson(data)
            };

            store.Save(key, JsonUtility.ToJson(envelope));
        }

        public bool TryLoad<T>(string key, out T data)
        {
            data = default;

            if (string.IsNullOrWhiteSpace(key) || !store.TryLoad(key, out var json) || string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                var envelope = JsonUtility.FromJson<SaveEnvelope>(json);
                if (envelope == null ||
                    envelope.version != CurrentVersion ||
                    string.IsNullOrWhiteSpace(envelope.payload))
                {
                    return false;
                }

                var loaded = JsonUtility.FromJson<T>(envelope.payload);
                if (loaded == null)
                {
                    return false;
                }

                data = loaded;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Delete(string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                store.Delete(key);
            }
        }
    }
}
