using System;
using UnityEngine;

/// <summary>
/// 物品槽位数据类，表示一个可以穿戴装备的槽位
/// </summary>
[Serializable]
public class Inventory_EquipmentSlot
{
    public ItemType slotType; // 这个槽位可以放什么类型的物品

    public Inventory_Item equippedItem; // 当前装备在这个槽位上的物品

    public bool HasItem() => equippedItem != null && equippedItem.itemData != null;
}
