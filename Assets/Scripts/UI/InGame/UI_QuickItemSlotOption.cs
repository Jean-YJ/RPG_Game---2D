using UnityEngine;
using UnityEngine.EventSystems;

public class UI_QuickItemSlotOption : UI_InventoryItemSlot
{
    private UI_QuickItemSlot quickItemSlot;
    private UI_InGame inGame;

    protected override void Awake()
    {
        base.Awake();
        this.inGame = GetComponentInParent<UI_InGame>();
    }

    public void SetUpOptions(UI_QuickItemSlot quickItemSlot, Inventory_Item itemToSet)
    {
        this.quickItemSlot = quickItemSlot;
        UpdateSlot(itemToSet);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        // base.OnPointerDown(eventData);
        this.quickItemSlot.SetQuickSlotItem(this.currentItemInSlot);
        this.inGame.HideQuickItemOptions();
        this.canvasRoot.CloseAllToolTip();
    }
}
