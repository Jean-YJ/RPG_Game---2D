using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 普通背包物品格子的 Presenter。
/// 负责处理点击、使用、装备、整组移除和 Tooltip 逻辑；View 只负责显示和输入转发。
/// </summary>
public class UI_InventoryItemSlotPresenter : UIPresenterBase<UI_InventoryItemSlot>
{
    private readonly Inventory_Player playerInventory;
    private readonly UI_CanvasRoot canvasRoot;

    public UI_InventoryItemSlotPresenter(Inventory_Player playerInventory, UI_CanvasRoot canvasRoot)
    {
        this.playerInventory = playerInventory;
        this.canvasRoot = canvasRoot;
    }

    /// <summary>
    /// 处理普通背包格子的鼠标点击。
    /// 当前保持原行为：只响应左键；右键不处理。
    /// </summary>
    public virtual void HandlePointerDown(PointerEventData eventData)
    {
        if (!HasView || eventData.button == PointerEventData.InputButton.Right)
            return;

        Inventory_Item item = View.CurrentItemInSlot;
        if (item == null || item.itemData == null)
        {
            HideItemToolTip();
            return;
        }

        if (item.itemData.itemType == ItemType.Material)
            return;

        bool transferFullStack = Input.GetKey(KeyCode.LeftControl);
        if (transferFullStack)
        {
            playerInventory?.RemoveAllStack(item);
            return;
        }

        if (item.itemData.itemType == ItemType.Consumable)
        {
            playerInventory?.TryToUseItem(item);
            return;
        }

        playerInventory?.TryToEquipItem(item);
    }

    /// <summary>
    /// 鼠标进入格子时显示物品 Tooltip。
    /// Tooltip 的具体显示仍由 CanvasRoot 下的 Tooltip View 负责。
    /// </summary>
    public virtual void HandlePointerEnter()
    {
        if (!HasView)
            return;

        canvasRoot?.itemToolTip.ShowToolTip(true, View.CurrentItemInSlot, View.Rect, false, true, false, false);
    }

    /// <summary>
    /// 鼠标离开格子时隐藏物品 Tooltip。
    /// </summary>
    public virtual void HandlePointerExit()
    {
        HideItemToolTip();
    }

    protected void HideItemToolTip()
    {
        canvasRoot?.itemToolTip.ShowToolTip(false);
    }
}
