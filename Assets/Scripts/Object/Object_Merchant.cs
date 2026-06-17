using UnityEngine;

public class Object_Merchant : Object_NPC, IInteractable
{
    private Inventory_Player playerInventory;
    private Inventory_Merchant merchant;

    protected override void Awake()
    {
        base.Awake();

        this.merchant = GetComponent<Inventory_Merchant>();
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Z))
            this.merchant.FillShopList();
    }

    public void Interact()
    {
        this.canvasRoot.merchant.SetUpMerchant(this.merchant, this.playerInventory);
        this.canvasRoot.ShowMerchantUI(true);
    }




    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        this.playerInventory = collision.GetComponent<Inventory_Player>();
        this.merchant.SetInventory(this.playerInventory);
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);

        if (this.canvasRoot != null && this.canvasRoot.merchant != null)
        {
            this.canvasRoot.CloseAllToolTip();
            this.canvasRoot.ShowMerchantUI(false);
        }
        this.playerInventory = null;
    }
}
