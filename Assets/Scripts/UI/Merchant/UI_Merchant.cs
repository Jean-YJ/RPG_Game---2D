using TMPro;
using UnityEngine;

public class UI_Merchant : MonoBehaviour
{
    private Inventory_Player playerInventory;
    private Inventory_Merchant merchant;

    [SerializeField] private UI_ItemSlotGroup inventorySlotsParent;
    [SerializeField] private UI_ItemSlotGroup merchantSlotsParent;
    [SerializeField] private UI_EquipSlotGroup equipSlotsParent;
    [SerializeField] private TextMeshProUGUI glodPoints;

    public void SetUpMerchant(Inventory_Merchant merchant, Inventory_Player playerInventory)
    {
        this.merchant = merchant;
        this.playerInventory = playerInventory;

        this.merchant.onInventoryUpdateded += UpdateMerchantUI;
        this.playerInventory.onInventoryUpdateded += UpdateMerchantUI;
        UpdateMerchantUI();

        UI_MerchantSlot[] merchantSlots = this.merchantSlotsParent.GetComponentsInChildren<UI_MerchantSlot>();
        foreach (var slot in merchantSlots)
            slot.SetUpMerchant(this.merchant);

        // 也为玩家背包的槽位设置 merchant 引用（用于卖出）
        UI_MerchantSlot[] playerSlots = this.inventorySlotsParent.GetComponentsInChildren<UI_MerchantSlot>();
        foreach (var slot in playerSlots)
            slot.SetUpMerchant(this.merchant);
    }
    private void UpdateMerchantUI()
    {
        this.inventorySlotsParent.UpdateSlots(this.playerInventory.itemList);
        this.merchantSlotsParent.UpdateSlots(this.merchant.itemList);
        this.equipSlotsParent.UpdateEquipmentSlots(this.playerInventory.equipmentSlots);
        this.glodPoints.text = this.playerInventory.gold.ToString("N0") + "g.";
    }
}
