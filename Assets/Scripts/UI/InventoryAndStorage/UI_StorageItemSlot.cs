using UnityEngine;
using UnityEngine.EventSystems;

public class UI_StorageItemSlot : UI_InventoryItemSlot
{
    private Inventory_Storage storage;

    public StorageSlotType slotType;

    /// <summary>
    /// 绑定UI显示槽位和仓库
    /// </summary>
    /// <param name="storage">仓库引用对象</param>
    public void SetSortage(Inventory_Storage storage) => this.storage = storage;

    public override void OnPointerDown(PointerEventData eventData)
    {
        // base.OnPointerDown(eventData);
        if (this.storage == null) return;
        if (this.currentItemInSlot == null) return;

        bool transferFullStack = Input.GetKey(KeyCode.LeftControl);
        // 点击槽位后，根据槽位的类型来调用storage的对应的移动物品的方法
        if (this.slotType == StorageSlotType.StorageSlot) this.storage.FromStorageToInventory(this.currentItemInSlot, transferFullStack);
        if (this.slotType == StorageSlotType.PlayerInventorySlot) this.storage.FromInventoryToStorage(this.currentItemInSlot, transferFullStack);

        this.canvasRoot.itemToolTip.ShowToolTip(false);
    }
}

public enum StorageSlotType { StorageSlot, PlayerInventorySlot, MaterialSlot }
