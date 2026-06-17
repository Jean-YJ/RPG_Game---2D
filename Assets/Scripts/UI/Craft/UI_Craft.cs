using UnityEngine;

public class UI_Craft : MonoBehaviour
{
    private Inventory_Player playerInventory;
    [SerializeField] private UI_ItemSlotGroup inventoryParent;
    [SerializeField] private Transform categoryParent; // UI_CraftCategoryButton的父对象
    [SerializeField] private Transform categorySlotsParent; // UI_CraftSlot的父对象
    private UI_CraftCategoryButton[] categoryButtons;
    private UI_CraftSlot[] craftSlots;
    private UI_CraftPreview craftPreview;


    public void SetUpCraftUI(Inventory_Storage storage)
    {
        // craft界面背包相关设置
        this.playerInventory = storage.playerInventory;
        this.playerInventory.onInventoryUpdateded += UpdateInventoryUI;
        UpdateInventoryUI();

        // craft界面物品预览和所需材料相关设置
        this.craftPreview = this.GetComponentInChildren<UI_CraftPreview>();
        this.craftPreview.SetUpCraftPreview(storage);

        // // craft界面可制作物品列表相关设置
        SetUpCraftCategoryButtons();

    }
    private void SetUpCraftCategoryButtons()
    {
        this.categoryButtons = this.categoryParent.GetComponentsInChildren<UI_CraftCategoryButton>();
        this.craftSlots = this.categorySlotsParent.GetComponentsInChildren<UI_CraftSlot>();

        foreach (var slot in this.craftSlots)
            slot.gameObject.SetActive(false);

        foreach (var btn in this.categoryButtons)
            btn.SetUpCraftSlots(this.craftSlots);
    }

    /// <summary>
    /// 更新玩家背包的显示
    /// </summary>
    private void UpdateInventoryUI() => this.inventoryParent.UpdateSlots(this.playerInventory.itemList);
}
