using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using UnityEngine;

public class UI_Storage : MonoBehaviour
{
    private Inventory_Player playerInventory;
    private Inventory_Storage storage;

    [SerializeField] private UI_ItemSlotGroup inventoryParent;
    [SerializeField] private UI_ItemSlotGroup storageParent;
    [SerializeField] private UI_ItemSlotGroup materialParent;

    /// <summary>
    /// 设置Storage界面的相关数据，并绑定更新事件
    /// </summary>
    /// <param name="playerInventory">玩家背包引用</param>
    /// <param name="storage">仓库引用</param>
    public void SetStorage(Inventory_Storage storage)
    {
        if (this.storage != null) this.storage.onInventoryUpdateded -= UpdateUI;
        if (this.playerInventory != null) this.playerInventory.onInventoryUpdateded -= UpdateUI;
        //获取数据
        this.storage = storage;
        this.playerInventory = storage.playerInventory;

        //获取所有的显示槽位
        UI_StorageItemSlot[] slots = GetComponentsInChildren<UI_StorageItemSlot>();
        // 绑定槽位与仓库，需要在槽位中调用storage的移动物品的方法
        foreach (var slot in slots)
        {
            slot.SetSortage(this.storage);
        }

        //订阅事件，onInventoryUpdateded事件会在背包和仓库物品发生改变时调用
        this.storage.onInventoryUpdateded += UpdateUI;
        this.playerInventory.onInventoryUpdateded += UpdateUI;
        // 默认在设置时进行一次更新
        UpdateUI();
    }

    // 更新背包和仓库的所有槽位显示
    private void UpdateUI()
    {
        if(this.storage == null) return;

        this.inventoryParent.UpdateSlots(this.playerInventory.itemList);
        this.storageParent.UpdateSlots(this.storage.itemList);
        this.materialParent.UpdateSlots(this.storage.materialStash);
    }

    void OnEnable()
    {
        UpdateUI();

        if (this.storage != null) this.storage.onInventoryUpdateded += UpdateUI;
        if (this.playerInventory != null) this.playerInventory.onInventoryUpdateded += UpdateUI;
    }

    private void OnDisable()
    {
        if (this.storage != null) this.storage.onInventoryUpdateded -= UpdateUI;
        if (this.playerInventory != null) this.playerInventory.onInventoryUpdateded -= UpdateUI;
    }
}
