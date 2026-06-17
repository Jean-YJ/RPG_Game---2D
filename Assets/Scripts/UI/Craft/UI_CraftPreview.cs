using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CraftPreview : MonoBehaviour
{
    private Inventory_Item itemToCraft;
    private Inventory_Storage storage;

    [Header("Recipe Slot Detail")]
    [SerializeField] private Transform materialSlotsParent;
    private UI_CraftMaterialSlot[] materialSlots;

    [Header("Item Preview Detail")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemInfo;
    [SerializeField] private Button craftBtn;
    [SerializeField] private TextMeshProUGUI craftBtnText;


    public void SetUpCraftPreview(Inventory_Storage storage)
    {
        this.storage = storage;

        this.materialSlots = this.materialSlotsParent.GetComponentsInChildren<UI_CraftMaterialSlot>();
        // 默认所有的制作材料不显示
        foreach (var slot in this.materialSlots)
            slot.gameObject.SetActive(false);
    }


    public void UpdateCraftPreview(ItemData_SO itemData)
    {
        this.itemToCraft = new Inventory_Item(itemData);

        this.icon.sprite = this.itemToCraft.itemData.itemIcon;
        this.itemName.text = this.itemToCraft.itemData.itemName;
        this.itemInfo.text = this.itemToCraft.GetInfoText();

        UpdateCraftPreviewSlots();
    }

    private void UpdateCraftPreviewSlots()
    {
        if (this.materialSlots == null || this.materialSlots.Length == 0)
        {
            Debug.LogWarning("materialSlots not initialized. Call SetUpCraftPreview first or ensure slots exist.");
            return;
        }

        //每次更新显示，都先把所有的隐藏了
        foreach (var slot in this.materialSlots)
            slot.gameObject.SetActive(false);

        for (int i = 0; i < itemToCraft.itemData.craftRecipe.Count; i++)
        {
            Inventory_Item itemRequired = itemToCraft.itemData.craftRecipe[i];
            int avaliableAmount = this.storage.GetAmountOf(itemRequired);
            int requiredAmount = itemRequired.stackSize;

            this.materialSlots[i].gameObject.SetActive(true);
            this.materialSlots[i].SetUpMaterialSlot(itemRequired.itemData, avaliableAmount, requiredAmount);

        }
    }

    public void ConfirmCraft()
    {
        if (this.itemToCraft == null)
        {
            this.craftBtnText.text = "Pick an item!";
            return;
        }

        //材料是否足够  背包是否有空间
        if (this.storage.CanCraftItem(this.itemToCraft))
        {
            this.storage.CraftItem(this.itemToCraft);
        }

        UpdateCraftPreviewSlots();
    }

}
