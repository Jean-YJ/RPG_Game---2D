using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_QuickItemSlot : UI_InventoryItemSlot
{
    [SerializeField] private int quickSlotNumber;
    private Button button;

    protected override void Awake()
    {
        base.Awake();
        this.button = GetComponent<Button>();
    }

    public void SetQuickSlotItem(Inventory_Item item)
    {
        //在playerInventory中进行数据更新，UI只负责显示数据，UI不直接修改数据
        this.playerInventory.SetQuickItemInSlot(this.quickSlotNumber, item);
    }

    public void UpdateQuickSlot(Inventory_Item item)
    {

        if (item == null || item.itemData == null)
        {
            this.itemIcon.sprite = this.defaultIcon;
            this.itemStackSize.text = "";
            return;
        }
        this.currentItemInSlot = item;
        this.itemIcon.sprite = this.currentItemInSlot.itemData.itemIcon;
        this.itemStackSize.text = this.currentItemInSlot.stackSize.ToString();
    }

    /// <summary>
    /// 模拟按钮点击反馈，用于使用快捷键触发快捷栏物品时的视觉反馈
    /// </summary>
    public void SimulateButtonClickFeedBack()
    {
        EventSystem.current.SetSelectedGameObject(this.button.gameObject);
        ExecuteEvents.Execute(this.button.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        // base.OnPointerDown(eventData);
        this.canvasRoot.inGameUI.ShowQuickItemOptions(this, this.rect);

    }
}
