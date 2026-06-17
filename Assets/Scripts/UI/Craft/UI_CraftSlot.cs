using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 工坊中展示每个种类可制作物品的槽位
/// </summary>
public class UI_CraftSlot : MonoBehaviour
{
    private ItemData_SO item;
    [SerializeField] private UI_CraftPreview craftPreview;

    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI itemName;

    public void SetUpSlot(ItemData_SO item)
    {
        this.item = item;

        this.icon.sprite = this.item.itemIcon;
        this.itemName.text = this.item.itemName;
    }

    // 自身被点击后，将自身物品的数据传递给craftPreview并进行显示更新
    public void UpdateCraftPreview() => this.craftPreview.UpdateCraftPreview(this.item);
}
