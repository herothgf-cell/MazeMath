namespace MazeMath.Core.Save
{
    public interface ISaveStore
    {
        void Save(string key, string json);
        bool TryLoad(string key, out string json);
        void Delete(string key);
    }
}
