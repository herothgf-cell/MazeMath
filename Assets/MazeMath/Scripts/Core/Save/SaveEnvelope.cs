using System;

namespace MazeMath.Core.Save
{
    [Serializable]
    public sealed class SaveEnvelope
    {
        public int version;
        public string payload;
    }
}
