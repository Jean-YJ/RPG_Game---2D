using System.Collections.Generic;
using UnityEngine;

public class UI_ItemSlotGroup : MonoBehaviour
{
    private UI_InventoryItemSlot[] slots;

    public void UpdateSlots(List<Inventory_Item> itemList)
    {
        if (slots == null)
            this.slots = GetComponentsInChildren<UI_InventoryItemSlot>();

        for (int i = 0; i < this.slots.Length; i++)
        {
            if (i < itemList.Count)
                slots[i].UpdateSlot(itemList[i]);
            else
                slots[i].UpdateSlot(null);

        }
    }
}
