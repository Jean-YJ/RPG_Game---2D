using System.Text;
using TMPro;
using UnityEngine;

public class UI_ItemToolTip : UI_ToolTip
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI itemInfoText;

    [SerializeField] private TextMeshProUGUI itemPrice;
    [SerializeField] private Transform merchnatNPCInfo;
    [SerializeField] private Transform merchantInventoryInfo;
    [SerializeField] private Transform inventoryInfo;

    public void ShowToolTip(bool show, Inventory_Item itemToShow, RectTransform target = null,
                                bool buyPrice = false, bool showInventoryInfo = false,
                                bool showMerchantInfo = false, bool showMI = false)
    {
        if (!show)
        {
            base.ShowToolTip(false);
            return;
        }

        if (itemToShow == null)
        {
            return;
        }


        inventoryInfo.gameObject.SetActive(showInventoryInfo);
        merchnatNPCInfo.gameObject.SetActive(showMerchantInfo);
        merchantInventoryInfo.gameObject.SetActive(showMI);

        int price = buyPrice ? itemToShow.buyPrice : Mathf.FloorToInt(itemToShow.sellPrice);
        int totalPrice = price * itemToShow.stackSize;

        string fullStackPrice = ($"Price:{price}x{itemToShow.stackSize} - {totalPrice}g.");
        string singleStackPrice = ($"Price:{price}g.");

        string color = GetColorByRarity(itemToShow.itemData.itemRarity);
        this.itemNameText.text = GetColoredText(color, itemToShow.itemData.itemName);

        this.itemPrice.text = itemToShow.stackSize > 1 ? fullStackPrice : singleStackPrice;
        this.itemTypeText.text = itemToShow.itemData.itemType.ToString();
        this.itemInfoText.text = itemToShow.GetInfoText();

        // 最后再激活并定位
        base.ShowToolTip(show, target);
    }

    private string GetColorByRarity(int rarity)
    {
        if (rarity <= 100) return "white"; // Common
        if (rarity <= 300) return "green"; // Uncommon
        if (rarity <= 600) return "blue";  // Rare
        if (rarity <= 850) return "purple";// Epic
        return "orange";                   // Legendary
    }

}
