using UnityEngine;

namespace MazeMath.Core.Save
{
    public sealed class PlayerPrefsSaveStore : ISaveStore
    {
        public void Save(string key, string json)
        {
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        public bool TryLoad(string key, out string json)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                json = null;
                return false;
            }

            json = PlayerPrefs.GetString(key);
            return true;
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}
