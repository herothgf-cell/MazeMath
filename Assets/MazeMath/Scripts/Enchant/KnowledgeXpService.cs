using System;

namespace MazeMath.Enchant
{
    public sealed class KnowledgeXpService
    {
        public int Balance { get; private set; }

        public void Add(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Balance += amount;
        }

        public bool CanSpend(int amount)
        {
            return amount >= 0 && Balance >= amount;
        }

        public bool Spend(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (Balance < amount) return false;
            Balance -= amount;
            return true;
        }

        public void Restore(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Balance = amount;
        }
    }
}
