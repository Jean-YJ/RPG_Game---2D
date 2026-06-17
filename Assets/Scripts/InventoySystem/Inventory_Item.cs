using System;
using System.Text;
using UnityEngine;

[Serializable]
public class Inventory_Item
{
    private string itemID;
    public string ItemID => this.itemID;
    public ItemData_SO itemData;
    public int stackSize = 1;
    public ItemEffectData_SO itemEffectData;

    public int buyPrice { get; private set; }
    public int sellPrice { get; private set; }


    public ItemModifier[] modifiers { get; private set; }

    public Inventory_Item(ItemData_SO itemData)
    {
        this.itemData = itemData;
        this.itemEffectData = this.itemData.itemEffect;
        this.stackSize = 1;
        this.buyPrice = this.itemData.itemPrice;
        this.sellPrice = (int)(this.itemData.itemPrice * 0.35);
        this.modifiers = this.EquipmentData()?.modifiers;
        // 生成一个唯一的ID，确保每个物品实例都是独一无二的
        this.itemID = itemData.name + "__" + Guid.NewGuid().ToString();
    }

    private EquipmentData_SO EquipmentData()
    {
        if (this.itemData is EquipmentData_SO equipmentData)
        {
            return equipmentData;
        }
        return null;
    }

    public bool CanAddStack() => this.stackSize < itemData.maxStackSize;
    public void AddStackSize() => this.stackSize++;
    public void RemoveStackSize() => this.stackSize--;

    public void AddModifier(Entity_Stats playerStats)
    {
        foreach (var modifier in this.modifiers)
        {
            Stat statTypeToModifier = playerStats.GetStatByType(modifier.type);
            statTypeToModifier.AddModifier(modifier.value, this.itemID);
        }
    }

    public void RemoveModifier(Entity_Stats playerStats)
    {
        foreach (var modifier in this.modifiers)
        {
            Stat statTypeToModifier = playerStats.GetStatByType(modifier.type);
            statTypeToModifier.RemoveModifier(this.itemID);
        }
    }

    public void AddUniqueEffect(Player player) => this.itemEffectData?.Subscribe(player);
    public void RemoveUniqueEffect() => this.itemEffectData?.Unsubscribe();



    public string GetInfoText()
    {
        StringBuilder sb = new StringBuilder();
        if (this.itemData.itemType == ItemType.Material)
        {
            sb.AppendLine(" ");
            sb.AppendLine("Used for crafting.");
            sb.AppendLine(" ");
            sb.AppendLine(" ");
            return sb.ToString();
        }

        if (this.itemData.itemType == ItemType.Consumable)
        {
            sb.AppendLine(" ");
            sb.AppendLine(this.itemData.itemEffect.effectDescription);
            sb.AppendLine(" ");
            sb.AppendLine(" ");
            return sb.ToString();
        }

        sb.AppendLine(" ");
        foreach (var modifier in this.modifiers)
        {
            string modTypeText = GetStatsTextByType(modifier.type);
            string modValueText = IsPercentageStat(modifier.type) ? $"{modifier.value}%" : modifier.value.ToString();
            sb.AppendLine($"+ {modTypeText}: {modValueText}");
        }

        if (this.itemData.itemEffect != null)
        {
            sb.AppendLine(" ");
            sb.AppendLine("Unique Effect:");
            sb.AppendLine(this.itemData.itemEffect.effectDescription);
        }

        sb.AppendLine(" ");
        sb.AppendLine(" ");

        return sb.ToString();
    }

    private string GetStatsTextByType(StatType type)
    {
        switch (type)
        {
            case StatType.Strength:
                return "Strength";
            case StatType.Agility:
                return "Agility";
            case StatType.Intelligence:
                return "Intelligence";
            case StatType.Vitality:
                return "Vitality";
            case StatType.MaxHealth:
                return "Max Health";
            case StatType.HealthRegen:
                return "Health Regenerateion";
            case StatType.AttackSpeed:
                return "Attack Speed";
            case StatType.Damage:
                return "Damage";
            case StatType.CritChance:
                return "Critical Chance";
            case StatType.CritPower:
                return "Critical Power";
            case StatType.ArmorReduction:
                return "Armor Reduction";
            case StatType.FireDamage:
                return "Fire Damage";
            case StatType.IceDamage:
                return "Ice Damage";
            case StatType.LightningDamage:
                return "Lightning Damage";
            case StatType.ElementalDamage:
                return "Elemental Damage";
            case StatType.Armor:
                return "Armor";
            case StatType.Evasion:
                return "Evasion";
            case StatType.IceResistance:
                return "Ice Resistance";
            case StatType.FireResistance:
                return "Fire Resistance";
            case StatType.LightningResistance:
                return "Lightning Resistance";
            default:
                return "Unknown Stat";
        }
    }

    private bool IsPercentageStat(StatType type)
    {
        switch (type)
        {
            case StatType.CritChance:
            case StatType.CritPower:
            case StatType.ArmorReduction:
            case StatType.IceResistance:
            case StatType.FireResistance:
            case StatType.LightningResistance:
            case StatType.AttackSpeed:
            case StatType.Evasion:
                return true;
            default:
                return false;
        }
    }
}
