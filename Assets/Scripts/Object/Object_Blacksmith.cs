using Unity.VisualScripting;
using UnityEngine;

public class Object_Blacksmith : Object_NPC, IInteractable
{
    private Animator animator;
    private Inventory_Player playerInventory;
    private Inventory_Storage storage;

    protected override void Awake()
    {
        base.Awake();
        this.animator = GetComponentInChildren<Animator>();
        animator.SetBool("isBlackSmith", true);

        this.storage = GetComponent<Inventory_Storage>();
    }

    /// <summary>
    /// Player与该NPC进行交互
    /// </summary>
    public void Interact()
    {
        // throw new System.NotImplementedException();
        // Debug.Log("Open BlackSmith's UI");
        this.canvasRoot.storage.SetStorage(this.storage);
        this.canvasRoot.craft.SetUpCraftUI(this.storage);

        this.canvasRoot.ShowStorageUI(true);
        // this.canvasRoot.craft.gameObject.SetActive(true);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        //Player进入到碰撞区间，记录player的inventory并和storage连接起来
        this.playerInventory = this.player.playerInventory;
        this.storage.ConnectToInventory(this.playerInventory);

    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);

        this.canvasRoot.CloseAllToolTip();
        // this.canvasRoot.storage.gameObject.SetActive(false);
        if (this.canvasRoot != null && this.canvasRoot.storage != null)
        {
            this.canvasRoot.ShowStorageUI(false);
            this.canvasRoot.craft.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("UI_CanvasRoot or storage is null when Blacksmith OnTriggerExit2D called.");
        }
    }
}
