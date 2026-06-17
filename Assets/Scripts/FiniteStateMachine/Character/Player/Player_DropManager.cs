using System;
using System.Collections.Generic;
using UnityEngine;

public class Player_DropManager : Entity_DropManager
{
    [SerializeField] private float chanceToLoseItem = 90.0f;
    private Inventory_Player playerInventory;

    void Awake()
    {
        this.playerInventory = GetComponent<Inventory_Player>();
    }

    public override void DropItems()
    {
        // base.DropItems();
        List<Inventory_Item> inventoryCopy = new List<Inventory_Item>(this.playerInventory.itemList);
        List<Inventory_EquipmentSlot> equipCopy = new List<Inventory_EquipmentSlot>(this.playerInventory.equipmentSlots);

        foreach (var item in inventoryCopy)
        {
            if (UnityEngine.Random.Range(0, 100) < this.chanceToLoseItem)
            {
                CreateItem(item.itemData);
                //背包中移除整个堆栈
                this.playerInventory.RemoveAllStack(item);
            }
        }

        foreach (var equipSlot in equipCopy)
        {
            if (equipSlot.HasItem() && UnityEngine.Random.Range(0, 100) < this.chanceToLoseItem)
            {
                var item = equipSlot.equippedItem;
                CreateItem(item.itemData);
                this.playerInventory.UnequipItemFromSlot(item);
                this.playerInventory.RemoveAllStack(item);
            }
        }
    }
}
