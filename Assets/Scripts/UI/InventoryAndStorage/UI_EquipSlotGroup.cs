using System.Collections.Generic;
using UnityEngine;

public class UI_EquipSlotGroup : MonoBehaviour
{
    private UI_InventoryEquipSlot[] equips;

    public void UpdateEquipmentSlots(List<Inventory_EquipmentSlot> equipList)
    {
        if (equipList == null)
            return;

        if (this.equips == null)
            this.equips = GetComponentsInChildren<UI_InventoryEquipSlot>();

        if (this.equips == null || this.equips.Length == 0)
            return;

        // Ensure every UI slot is updated. For each UI slot, find a matching equipment slot (if any)
        // and avoid reusing the same equipment entry for multiple UI slots.
        var used = new bool[equipList.Count];
        for (int i = 0; i < this.equips.Length; i++)
        {
            var slotUI = this.equips[i];
            Inventory_EquipmentSlot matched = null;

            for (int j = 0; j < equipList.Count; j++)
            {
                if (used[j])
                    continue;

                var slotItem = equipList[j];
                if (slotItem == null)
                    continue;

                if (slotItem.slotType == slotUI.slotType)
                {
                    matched = slotItem;
                    used[j] = true; // mark as consumed so another UI slot won't reuse it
                    break;
                }
            }

            if (matched == null || !matched.HasItem())
            {
                slotUI.UpdateSlot(null);
            }
            else
            {
                slotUI.UpdateSlot(matched.equippedItem);
            }
        }

        // for (int i = 0; i < this.equips.Length; i++)
        // {
        //     var slotItem = equipList[i];
        //     if (!slotItem.HasItem())
        //     {
        //         this.equips[i].UpdateSlot(null);
        //     }
        //     else
        //     {
        //         this.equips[i].UpdateSlot(slotItem.equippedItem);
        //     }
        // }
    }
}
