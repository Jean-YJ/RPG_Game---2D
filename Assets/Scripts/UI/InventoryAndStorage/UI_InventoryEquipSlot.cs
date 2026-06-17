using UnityEngine;
using UnityEngine.EventSystems;

public class UI_InventoryEquipSlot : UI_InventoryItemSlot
{
    public ItemType slotType;

    void OnValidate()
    {
        this.gameObject.name = "UI_EquipmentSlot: " + slotType.ToString();
    }

    public override void UpdateSlot(Inventory_Item item)
    {
        // base.UpdateSlot(item);
        this.currentItemInSlot = item;

        if (this.currentItemInSlot == null)
        {
            this.itemIcon.sprite = this.defaultIcon;
            Color color = Color.white;
            color.a = 0.3f;
            this.itemIcon.color = color;
            this.itemStackSize.text = "";
            return;
        }

        this.itemIcon.sprite = this.currentItemInSlot.itemData.itemIcon;
        this.itemIcon.color = Color.white;
        this.itemStackSize.text = this.currentItemInSlot.stackSize > 0 ? this.currentItemInSlot.stackSize.ToString() : "";
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        // base.OnPointerDown(eventData);
        if (this.currentItemInSlot == null) return; // 如果这个装备槽位上没有装备，就不执行任何操作

        this.playerInventory.UnequipItemFromSlot(this.currentItemInSlot); // 卸下这个装备

    }
}
