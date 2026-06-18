using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_InventoryItemSlot : MonoBehaviour, IUIView, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Inventory_Item currentItemInSlot { get; protected set; }
    protected Inventory_Player playerInventory;
    protected UI_CanvasRoot canvasRoot;
    protected RectTransform rect;
    protected UI_InventoryItemSlotPresenter presenter;

    [Header("UI Slot SetUp")]
    [SerializeField] protected Sprite defaultIcon;
    [SerializeField] protected Image itemIcon;
    [SerializeField] protected TextMeshProUGUI itemStackSize;

    public Inventory_Item CurrentItemInSlot => this.currentItemInSlot;
    public RectTransform Rect => this.rect;

    protected virtual void Awake()
    {
        this.playerInventory = FindAnyObjectByType<Inventory_Player>();
        this.canvasRoot = GetComponentInParent<UI_CanvasRoot>();
        this.rect = this.GetComponent<RectTransform>();
        EnsurePresenter();
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
        EnsurePresenter();
        this.presenter.HandlePointerDown(eventData);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        EnsurePresenter();
        this.presenter.HandlePointerEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EnsurePresenter();
        this.presenter.HandlePointerExit();
    }

    /// <summary>
    /// 创建普通背包 Slot 的 Presenter。
    /// 子类如果需要不同交互规则，可以重写该方法返回自己的 Presenter。
    /// </summary>
    protected virtual UI_InventoryItemSlotPresenter CreatePresenter()
    {
        return new UI_InventoryItemSlotPresenter(this.playerInventory, this.canvasRoot);
    }

    /// <summary>
    /// 确保 Slot View 已绑定 Presenter。
    /// Presenter 使用普通 C# 对象，避免本轮修改任何 prefab。
    /// </summary>
    protected void EnsurePresenter()
    {
        if (this.presenter != null)
            return;

        this.presenter = CreatePresenter();
        this.presenter.Attach(this);
    }

    protected virtual void OnDestroy()
    {
        this.presenter?.Detach();
    }
}
