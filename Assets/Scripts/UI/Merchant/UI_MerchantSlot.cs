using UnityEngine;
using UnityEngine.EventSystems;

public class UI_MerchantSlot : UI_InventoryItemSlot
{
    private Inventory_Merchant merchant;
    public MerchantSlotType slotType;


    public override void OnPointerDown(PointerEventData eventData)
    {
        if (this.currentItemInSlot == null) return;
        bool pressLeft = eventData.button == PointerEventData.InputButton.Left;
        bool pressRight = eventData.button == PointerEventData.InputButton.Right;

        if (this.slotType == MerchantSlotType.PlayerInventorySlot)
        {
            if (pressLeft)
            {
                base.OnPointerDown(eventData);

            }

            if (pressRight)
            {
                if (this.merchant == null)
                {
                    Debug.LogWarning("Merchant is null on PlayerInventorySlot. Did you call UI_Merchant.SetUpMerchant?");
                    return;
                }
                //Sell Logic
                bool sellFullStack = Input.GetKey(KeyCode.LeftControl);
                this.merchant.Try2SellItem(this.currentItemInSlot, sellFullStack);
            }

        }
        if (this.slotType == MerchantSlotType.MerchantSlot)
        {
            if (pressLeft)
                return; //什么也不干
            if (pressRight)
            {
                //Buy Logic   
                bool buyFullStack = Input.GetKey(KeyCode.LeftControl);
                this.merchant.Try2BuyItem(this.currentItemInSlot, buyFullStack);
            }
        }

        this.canvasRoot.itemToolTip.ShowToolTip(false);
    }

    public void SetUpMerchant(Inventory_Merchant merchant) => this.merchant = merchant;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        // base.OnPointerEnter(eventData);
        if (this.currentItemInSlot == null) return;

        if (this.slotType == MerchantSlotType.MerchantSlot)
        {
            this.canvasRoot.itemToolTip.ShowToolTip(true, this.currentItemInSlot, this.rect, true, false, true, false);
        }
        else if (this.slotType == MerchantSlotType.PlayerInventorySlot)
        {
            this.canvasRoot.itemToolTip.ShowToolTip(true, this.currentItemInSlot, this.rect, false, false, false, true);
        }

    }
}

public enum MerchantSlotType { MerchantSlot, PlayerInventorySlot }
