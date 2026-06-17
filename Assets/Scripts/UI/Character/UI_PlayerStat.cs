using UnityEngine;

public class UI_PlayerStat : MonoBehaviour
{
    private UI_StatSlot[] ui_StatSlots;
    // private Inventory_Player playerInventory;

    private void Awake()
    {
        this.ui_StatSlots = GetComponentsInChildren<UI_StatSlot>();
        // this.playerInventory = FindFirstObjectByType<Inventory_Player>();
    }

    // 逻辑已修改，当属性值发生变化时触发事件，UI界面监听该事件来更新显示的属性数值，
    // 因此不再需要在UI_PlayerStat中监听背包更新事件来刷新属性显示
    // void OnEnable()
    // {
    //     this.playerInventory.onInventoryUpdateded += UpdateAllStatSlots;
    // }

    // void OnDisable()
    // {
    //     this.playerInventory.onInventoryUpdateded -= UpdateAllStatSlots;
    // }

    void Start()
    {
        UpdateAllStatSlots();
    }

    public void UpdateAllStatSlots()
    {
        foreach (UI_StatSlot slot in ui_StatSlots)
        {
            slot.UpdateStatValue();
        }
    }
}
