using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UI_Inventory : MonoBehaviour
{
    private Inventory_Player playerInventory;
    // private UI_InventoryItemSlot[] uiItemSlots;


    [SerializeField] private UI_ItemSlotGroup itemSlotParent;
    [SerializeField] private UI_EquipSlotGroup equipSlotParent;
    [SerializeField] private TextMeshProUGUI glodPoints;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        // this.uiItemSlots = this.itemSlotParent.GetComponentsInChildren<UI_InventoryItemSlot>();
        // this.uiEquipSlots = this.equipSlotParent.GetComponentsInChildren<UI_InventoryEquipSlot>();
        this.playerInventory = FindAnyObjectByType<Inventory_Player>();

        // UpdateUISlots();
    }

    void OnEnable()
    {
        if (this.playerInventory == null) return;

        this.playerInventory.onInventoryUpdateded += UpdateUISlots;
        UpdateUISlots();  // 确保UI显示时数据最新
    }
    void OnDisable()
    {
        this.playerInventory.onInventoryUpdateded -= UpdateUISlots;
    }

    private void UpdateUISlots()
    {
        // UpdateItemSlots();
        this.itemSlotParent.UpdateSlots(this.playerInventory.itemList);
        this.equipSlotParent.UpdateEquipmentSlots(this.playerInventory.equipmentSlots);
        this.glodPoints.text = this.playerInventory.gold.ToString("N0") + "g.";
    }
    // private void UpdateItemSlots()
    // {
    //     List<Inventory_Item> items = playerInventory.itemList;

    //     for (int i = 0; i < uiItemSlots.Length; i++)
    //     {
    //         if (i < items.Count) //一个槽位对应一个物品
    //         {
    //             uiItemSlots[i].UpdateSlot(items[i]);
    //         }
    //         else //如果物品数量不足以填满所有槽位，剩余的槽位显示为空
    //         {
    //             uiItemSlots[i].UpdateSlot(null);
    //         }
    //     }
    // }


}
