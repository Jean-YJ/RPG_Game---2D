using System;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG SetUp/Item Data/Equipment item", fileName = "Equipment data - ")]
public class EquipmentData_SO : ItemData_SO
{
    [Header("Equipment Modifiers")]
    public ItemModifier[] modifiers;
}

[Serializable]
public class ItemModifier
{
    public StatType type;
    public float value;
}
