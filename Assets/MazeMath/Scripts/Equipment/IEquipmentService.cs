namespace MazeMath.Equipment
{
    public interface IAbilityProvider
    {
        bool HasAbility(string abilityId);
    }

    public interface IEquipmentService : IAbilityProvider
    {
        string GetEquipped(EquipmentSlot slot);
        bool Equip(string equipmentId);
        bool Unequip(EquipmentSlot slot);
        int GetEquipmentTier(string equipmentId);
        EquipmentDefinition GetDefinition(string equipmentId);
    }
}
