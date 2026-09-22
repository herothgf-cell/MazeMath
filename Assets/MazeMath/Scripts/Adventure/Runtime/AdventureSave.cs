using System;
using System.Security.Cryptography;
using System.Text;
using MazeMath.Core.Save;
using UnityEngine;

namespace MazeMath.Adventure
{
    public sealed class AdventureSave
    {
        public const string Key = "mazemath.adventure.v2";
        private readonly ISaveStore store;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        public AdventureSave(ISaveStore store = null) { this.store = store ?? new PlayerPrefsSaveStore(); }
        public bool TryLoad(out AdventureState state, out bool recoveredBackup)
        {
            recoveredBackup = false;
            if (store.TryLoad(Key, out var json) && Decode(json, out state)) return true;
            if (store.TryLoad(Key + ".backup", out json) && Decode(json, out state)) { recoveredBackup = true; return true; }
            state = null; return false;
        }
        public void Save(AdventureState state)
        {
            if (state == null || !state.IsValid()) throw new ArgumentException("Refusing to save invalid Adventure state.");
            string payload = JsonUtility.ToJson(state);
            string json = JsonUtility.ToJson(new Envelope { version = 2, payload = payload, checksum = Hash(payload) });
            if (!Decode(json, out _)) throw new InvalidOperationException("Save verification failed.");
            // Never replace a good backup with malformed primary data.
            if (store.TryLoad(Key, out var old) && Decode(old, out _)) store.Save(Key + ".backup", old);
            store.Save(Key, json);
        }
        private static bool Decode(string json, out AdventureState state)
        {
            state = null;
            if (string.IsNullOrWhiteSpace(json) || json.Length > 200000) return false;
            try
            {
                var e = JsonUtility.FromJson<Envelope>(json);
                if (e == null || e.version != 2 || string.IsNullOrEmpty(e.payload) || e.checksum != Hash(e.payload)) return false;
                var candidate = JsonUtility.FromJson<AdventureState>(e.payload);
                if (candidate == null || !candidate.IsValid()) return false;
                state = candidate; return true;
            }
            catch (ArgumentException) { return false; }
        }
        private static string Hash(string value)
        {
            using (var sha = SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }
    }
}
