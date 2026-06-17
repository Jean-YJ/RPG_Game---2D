using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour
{
    private Player player;
    private Inventory_Player playerInventory;

    [Header("HealthBar Details")]
    [SerializeField] private RectTransform healthRect;
    private const float MAXWIDTH = 1000f;
    private float identityWidth;
    [SerializeField] private Slider slider_HealthBar;
    [SerializeField] private TextMeshProUGUI txt_HealthValue;
    [SerializeField] private Transform skillSlotParent;

    [Header("Skill Slots")]
    private UI_SkillSlot[] skillSlots;
    [Header("Quick Item Slot")]
    [SerializeField] private float quickItemOptionsYOffset = 150f;
    [SerializeField] private Transform quickItemOptionsParent;
    private UI_QuickItemSlot[] quickItemSlots;
    private UI_QuickItemSlotOption[] quickItemSlotOptions;

    void Start()
    {
        this.quickItemSlots = GetComponentsInChildren<UI_QuickItemSlot>(true);
        this.player = FindAnyObjectByType<Player>();
        this.player.entity_Health.onUpdateHealth += UpdateHealthBar;
        this.playerInventory = this.player.playerInventory;
        this.playerInventory.onInventoryUpdateded += UpdateQuickSlotUI;
        this.playerInventory.onQuickItemUpdated += PlayQuickSlotFeedBack;

        this.identityWidth = this.healthRect.sizeDelta.x / this.player.playerStats.GetMaxHP();
        this.skillSlots = this.skillSlotParent.GetComponentsInChildren<UI_SkillSlot>(true);
    }

    public void PlayQuickSlotFeedBack(int slotNumber) => this.quickItemSlots[slotNumber - 1].SimulateButtonClickFeedBack();

    private void UpdateHealthBar()
    {
        int currentHealth = Mathf.CeilToInt(this.player.entity_Health.GetCurrentHealth());
        float maxHealth = this.player.playerStats.GetMaxHP();
        float maxWidth = Mathf.Clamp(this.identityWidth * maxHealth, 0f, MAXWIDTH);
        this.healthRect.sizeDelta = new Vector2(maxWidth, this.healthRect.sizeDelta.y);

        this.txt_HealthValue.text = currentHealth + "/" + maxHealth;
        this.slider_HealthBar.value = this.player.entity_Health.GetCurrentHealthPercentage();

        // Debug.Log("SizeDelta:" + this.healthRect.sizeDelta);
    }

    public UI_SkillSlot GetSkillSlot(SkillType skillType)
    {
        if (this.skillSlots == null)
        {
            if (this.skillSlotParent != null)
                this.skillSlots = this.skillSlotParent.GetComponentsInChildren<UI_SkillSlot>(true);
            else
                return null;
        }

        foreach (var slot in this.skillSlots)
        {
            if (slot != null && slot.skillType == skillType)
            {
                slot.gameObject.SetActive(true);
                return slot;
            }
        }

        return null;
    }

    public void ShowQuickItemOptions(UI_QuickItemSlot quickItemSlot, RectTransform targetRect)
    {
        this.quickItemOptionsParent.gameObject.SetActive(true);
        if (this.quickItemSlotOptions == null)
            this.quickItemSlotOptions = this.quickItemOptionsParent.GetComponentsInChildren<UI_QuickItemSlotOption>(true);

        List<Inventory_Item> consumableItemsInInventory = this.playerInventory.itemList.
                                        FindAll(item => item.itemData.itemType == ItemType.Consumable);

        for (int i = 0; i < this.quickItemSlotOptions.Length; i++)
        {
            if (i < consumableItemsInInventory.Count)
            {
                this.quickItemSlotOptions[i].gameObject.SetActive(true);
                this.quickItemSlotOptions[i].SetUpOptions(quickItemSlot, consumableItemsInInventory[i]);
            }
            else
            {
                this.quickItemSlotOptions[i].gameObject.SetActive(false);
            }
        }

        this.quickItemOptionsParent.position = targetRect.position + Vector3.up * (this.quickItemOptionsYOffset + this.quickItemOptionsParent.GetComponent<RectTransform>().rect.height / 2);
    }

    public void HideQuickItemOptions() => this.quickItemOptionsParent.gameObject.SetActive(false);

    public void UpdateQuickSlotUI()
    {
        if (this.playerInventory == null || this.playerInventory.quickItems == null || this.quickItemSlots == null)
            return;

        int count = Mathf.Min(this.playerInventory.quickItems.Length, this.quickItemSlots.Length);
        for (int i = 0; i < count; i++)
        {
            this.quickItemSlots[i].UpdateQuickSlot(this.playerInventory.quickItems[i]);
        }
    }

}
