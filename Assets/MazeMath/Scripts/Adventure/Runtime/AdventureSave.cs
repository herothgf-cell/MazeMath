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
        [Serializable] private sealed class Envelope
        {
            public int version, format;
            public bool questionPresent;
            public string payload, checksum;
        }
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
            bool present = state.question != null;
            var envelope = new Envelope { version = 2, format = 1, questionPresent = present, payload = payload };
            envelope.checksum = Hash((present ? "question:1\n" : "question:0\n") + payload);
            string json = JsonUtility.ToJson(envelope);
            if (!Decode(json, out _)) throw new InvalidOperationException("Save verification failed.");
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
                if (e == null || e.version != 2 || e.format < 0 || e.format > 1 || string.IsNullOrEmpty(e.payload)) return false;
                string signed = e.format == 1 ? (e.questionPresent ? "question:1\n" : "question:0\n") + e.payload : e.payload;
                if (e.checksum != Hash(signed)) return false;
                var candidate = JsonUtility.FromJson<AdventureState>(e.payload);
                if (candidate == null) return false;
                // Unity inline serialization can replace a null custom class with a default-valued object.
                // Preserve absence explicitly rather than interpreting that object as a corrupt active question.
                if (e.format == 1)
                {
                    if (!e.questionPresent) candidate.question = null;
                    else if (candidate.question == null || !candidate.question.IsValid()) return false;
                }
                else if (IsEmptyInlineQuestion(candidate.question)) candidate.question = null;
                if (!candidate.IsValid()) return false;
                state = candidate; return true;
            }
            catch (ArgumentException) { return false; }
        }
        private static bool IsEmptyInlineQuestion(AdventureQuestion q)
        {
            return q != null && string.IsNullOrEmpty(q.source) && string.IsNullOrEmpty(q.prompt) &&
                q.answer == 0 && q.attempts == 0 && !q.solved && !q.multipleChoice &&
                (q.choices == null || q.choices.Length == 0);
        }
        private static string Hash(string value)
        {
            using (var sha = SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }
    }
}
