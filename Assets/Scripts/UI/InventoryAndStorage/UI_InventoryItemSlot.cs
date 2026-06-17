using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_InventoryItemSlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Inventory_Item currentItemInSlot { get; protected set; }
    protected Inventory_Player playerInventory;
    protected UI_CanvasRoot canvasRoot;
    protected RectTransform rect;

    [Header("UI Slot SetUp")]
    [SerializeField] protected Sprite defaultIcon;
    [SerializeField] protected Image itemIcon;
    [SerializeField] protected TextMeshProUGUI itemStackSize;

    protected virtual void Awake()
    {
        this.playerInventory = FindAnyObjectByType<Inventory_Player>();
        this.canvasRoot = GetComponentInParent<UI_CanvasRoot>();
        this.rect = this.GetComponent<RectTransform>();
    }
    public virtual void UpdateSlot(Inventory_Item item)
    {
        this.currentItemInSlot = item;

        if (this.currentItemInSlot == null)
        {
            this.itemIcon.sprite = this.defaultIcon;
            // Color color = Color.white;
            // color.a = 0.5f;
            // this.itemIcon.color = color;
            this.itemStackSize.text = "";
            return;
        }

        this.itemIcon.sprite = this.currentItemInSlot.itemData.itemIcon;
        // this.itemIcon.color = Color.white;
        this.itemStackSize.text = this.currentItemInSlot.stackSize > 0 ? this.currentItemInSlot.stackSize.ToString() : "";
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        // throw new System.NotImplementedException();
        if (eventData.button == PointerEventData.InputButton.Right)
            return;

        if (this.currentItemInSlot == null)
        {
            this.canvasRoot.itemToolTip.ShowToolTip(false);
            return;
        }

        if (this.currentItemInSlot.itemData.itemType == ItemType.Material)
            return;

        bool alternativeInput = Input.GetKey(KeyCode.LeftControl);
        if (alternativeInput)
        {
            this.playerInventory.RemoveAllStack(this.currentItemInSlot);
        }
        else
        {
            if (this.currentItemInSlot.itemData.itemType == ItemType.Consumable)
            {
                this.playerInventory.TryToUseItem(this.currentItemInSlot);
            }
            else
            {
                this.playerInventory.TryToEquipItem(this.currentItemInSlot);
            }
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        // throw new System.NotImplementedException();
        this.canvasRoot.itemToolTip.ShowToolTip(true, this.currentItemInSlot, this.rect, false, true, false, false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // throw new System.NotImplementedException();
        this.canvasRoot.itemToolTip.ShowToolTip(false);
    }
}
