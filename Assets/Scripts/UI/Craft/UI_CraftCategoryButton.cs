using UnityEngine;

/// <summary>
/// 工坊种类按钮
/// </summary>
public class UI_CraftCategoryButton : MonoBehaviour
{
    [SerializeField] private ItemListData_SO craftData; //该种类下可制作物品的列表数据
    private UI_CraftSlot[] slots; // ui展示槽位

    //获取槽位组件
    public void SetUpCraftSlots(UI_CraftSlot[] slots) => this.slots = slots;

    public void UpdateAllSlots()
    {
        if (this.slots == null) return;

        // 隐藏掉所有的槽位
        foreach (var slot in this.slots)
            slot.gameObject.SetActive(false);

        // 以数据为基准
        for (int i = 0; i < this.craftData.itemList.Count; i++)
        {
            // 显示相应数量的槽位
            this.slots[i].gameObject.SetActive(true);
            // 根据数据更新显示
            this.slots[i].SetUpSlot(this.craftData.itemList[i]);
        }
    }
}
