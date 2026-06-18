using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Player : Inventory_Base
{
    public int gold = 10000;
    //角色属性
    private Player player;
    //角色的装备槽
    public List<Inventory_EquipmentSlot> equipmentSlots = new List<Inventory_EquipmentSlot>();
    public Inventory_Storage storage { get; private set; }

    [Header("Quick Item Detail")]
    public Inventory_Item[] quickItems = new Inventory_Item[2];
    public event Action<int> onQuickItemUpdated; // 快捷栏物品更新事件，参数是快捷栏槽位编号

    protected override void Awake()
    {
        base.Awake();
        this.player = GetComponent<Player>();
        this.storage = FindAnyObjectByType<Inventory_Storage>();
    }

    public void TryToEquipItem(Inventory_Item item)
    {
        // Debug.Log("Try To EquipItem");
        if (item == null || item.itemData == null)
            return;

        //在背包里找到这个物品，确保这个物品确实在背包里，防止外部调用这个方法时传入一个不在背包里的物品
        Inventory_Item itemToEquip = FindItem(item);
        if (itemToEquip == null)
            return;

        //寻找对应类型的槽位
        List<Inventory_EquipmentSlot> matchingSlots = equipmentSlots.FindAll(slot => slot.slotType == item.itemData.itemType);
        if (matchingSlots.Count == 0)
            return;

        // Debug.Log("matchingSlots: " + matchingSlots.Count);

        //从类型相符的槽位中找到一个可以装备的空槽位
        foreach (Inventory_EquipmentSlot slot in matchingSlots)
        {
            if (!slot.HasItem())
            {
                EquipItemToSlot(itemToEquip, slot);
                return;
            }
        }

        //如果没有空槽位，替换掉同类型的第一个装备
        Inventory_EquipmentSlot slotToReplace = matchingSlots[0];
        ReplaceItemInSlot(itemToEquip, slotToReplace);
    }
    private void EquipItemToSlot(Inventory_Item equipment, Inventory_EquipmentSlot equipmentSlot)
    {
        float savedHealthPercentage = player.entity_Health.GetCurrentHealthPercentage();
        // Debug.Log("EquipItem");
        equipmentSlot.equippedItem = equipment;
        equipmentSlot.equippedItem.AddModifier(player.playerStats);
        equipmentSlot.equippedItem.AddUniqueEffect(player);

        this.player.entity_Health.SetCurrentHealthByPercentage(savedHealthPercentage); //保持当前血量百分比不变
        RemoveOneItem(equipment); //从背包中移除这个物品，因为它现在已经被装备了
    }

    private void ReplaceItemInSlot(Inventory_Item itemToEquip, Inventory_EquipmentSlot slotToReplace)
    {
        //记录被替换掉的装备，防止后续操作中这个装备被覆盖了，导致数据丢失
        Inventory_Item replacedItem = slotToReplace.equippedItem;
        replacedItem.RemoveUniqueEffect(); // 移除被替换掉的装备带来的独特效果
        // 移除被替换掉的装备带来的属性加成
        replacedItem.RemoveModifier(player.playerStats);
        // 把新的装备放到这个槽位上，
        // 如果没有记录被替换掉的装备直接把新的装备放到这个槽位上，新的装备会覆盖掉原来的装备，导致原来的装备数据丢失，无法把原来的装备放回背包了
        slotToReplace.equippedItem = itemToEquip;
        slotToReplace.equippedItem.AddModifier(player.playerStats);
        slotToReplace.equippedItem.AddUniqueEffect(player); // 给新的装备添加独特效果
        // 从背包中移除这个被装备的物品，因为它现在已经被装备了
        RemoveOneItem(itemToEquip);
        // 把被替换掉的装备放回背包 - 这会触发 onInventoryUpdateded 事件，UI 会刷新
        AddItem(replacedItem);
    }

    public void UnequipItemFromSlot(Inventory_Item itemToUnequip)
    {
        // 卸下装备前，先检查背包里是否有空位可以放这个装备
        if (!CanAddItem(itemToUnequip))
            return;

        float savedHealthPercentage = player.entity_Health.GetCurrentHealthPercentage();

        foreach (Inventory_EquipmentSlot slot in equipmentSlots)
        {
            if (slot.equippedItem == itemToUnequip)
            {
                slot.equippedItem.RemoveModifier(player.playerStats); // 先移除这个装备带来的属性加成
                slot.equippedItem.RemoveUniqueEffect(); // 移除这个装备带来的独特效果
                this.player.entity_Health.SetCurrentHealthByPercentage(savedHealthPercentage); //保持当前血量百分比不变

                slot.equippedItem = null; //清空这个槽位
                AddItem(itemToUnequip); //把这个装备放回背包 - 这会触发 onInventoryUpdateded 事件

                // 下面的顺序会导致UI更新时这个装备无法从装备栏清除，
                // AddItem() 把物品放回背包
                // AddItem() 立刻触发 UI 刷新
                // UI 刷新装备格时，slot.equippedItem 还没有被清空
                // UI 仍然认为装备槽里有物品
                // 后面虽然执行了 slot.equippedItem = null，但已经没有新的刷新事件了
                // AddItem(itemToUnequip);
                // slot.equippedItem = null;

                return;
            }
        }
    }

    public void TryToUseItem(Inventory_Item itemToUse)
    {
        //传入物品空值检查
        if (itemToUse == null || itemToUse.itemData == null)
            return;

        //查找消耗品时跳过空物品和空itemData
        //此处判断条件必须使用item == itemToUse，而不能使用item.itemData == itemToUse.itemData
        //因为背包里可能有多个相同类型的物品，使用item.itemData == itemToUse.itemData的条件会导致
        // 找到第一个相同类型的物品，而不是正在使用的这个物品
        Inventory_Item cosumableItem = itemList.Find(item => item != null && item == itemToUse && item.itemData != null && item.itemData.itemType == ItemType.Consumable);

        if (cosumableItem == null || cosumableItem.itemEffectData == null)
            return;

        // 运行时多态，cosumableItem.itemEffectData的实际类型可能是ItemBuffData_SO，也可能是其他继承自ItemEffectData_SO的类
        // 具体由Object_ItemPickup中创建Inventory_Item时（ItemBuffData_SO父类装载其子类）根据itemData.itemEffect来决定
        if (!cosumableItem.itemEffectData.CanBeUsed(this.player))
        {
            Debug.Log("Cannot use this item right now: " + cosumableItem.itemData.itemName);
            return;
        }

        //实现效果
        cosumableItem.itemEffectData.ExcuteEffect();

        //使用后移除
        if (cosumableItem.stackSize > 1)
        {
            cosumableItem.RemoveStackSize();
            onInventoryUpdateded?.Invoke();
        }
        else
            RemoveOneItem(cosumableItem);
    }

    public void SetQuickItemInSlot(int slotNumber, Inventory_Item itemToSet)
    {
        // slotNumber 从1开始，数值的索引从0开始，所以要减1
        this.quickItems[slotNumber - 1] = itemToSet;
        this.onInventoryUpdateded?.Invoke(); // 触发背包更新事件，UI会刷新显示新的快捷栏物品
    }

    public void TryToUseQuickItem(int slotNumber)
    {
        //边界检查
        if (slotNumber < 1 || slotNumber > this.quickItems.Length)
            return;

        int index = slotNumber - 1;
        var item = this.quickItems[index];

        if (item == null || item.itemData == null) return;

        this.TryToUseItem(item);

        if (FindItem(item) == null)
        {
            // 物品已经被使用掉了,使用同类型的物品替换掉快捷栏里已经被使用掉的物品，
            // 如果没有同类型的物品了就清空这个快捷栏槽位
            this.quickItems[index] = FindSameItem(item);
        }
        this.onInventoryUpdateded?.Invoke(); // 触发背包更新事件，UI会刷新显示新的快捷栏物品
        this.onQuickItemUpdated?.Invoke(slotNumber); // 触发快捷栏物品更新事件，参数是快捷栏槽位编号
    }

    public override void LoadData(GameData data)
    {
        // base.LoadData(data);
        this.gold = data.gold;

        //背包物品
        this.itemList.Clear();
        foreach (var entry in data.playerInventory)
        {
            string saveID = entry.Key;
            int stackSize = entry.Value;

            ItemData_SO itemData = this.itemDataBase.GetDataByID(saveID);
            if (itemData == null)
            {
                Debug.LogWarning("Item not found: " + saveID);
                continue;
            }

            for (int i = 0; i < stackSize; i++)
            {
                Inventory_Item itemToLoad = new Inventory_Item(itemData);
                AddItem(itemToLoad);
            }
        }

        //装备
        foreach (var entry in data.equipedItems)
        {
            string saveID = entry.Key;
            ItemType slotType = entry.Value;

            ItemData_SO itemData = this.itemDataBase.GetDataByID(saveID);
            Inventory_Item itemToLoad = new Inventory_Item(itemData);

            var slotToEquip = this.equipmentSlots.Find(slot => slot.slotType == slotType && !slot.HasItem());

            float savedHealthPercentage = player.entity_Health.GetCurrentHealthPercentage();
            slotToEquip.equippedItem = itemToLoad;
            slotToEquip.equippedItem.AddModifier(this.player.playerStats);
            slotToEquip.equippedItem.AddUniqueEffect(this.player);
            this.player.entity_Health.SetCurrentHealthByPercentage(savedHealthPercentage); //保持当前血量百分比不变
        }

        this.onInventoryUpdateded?.Invoke();
    }

    public override void SaveData(ref GameData data)
    {
        // base.SaveData(ref data);
        data.gold = this.gold;

        data.playerInventory.Clear();
        foreach (var item in this.itemList)
        {
            if (item != null && item.itemData != null)
            {
                string saveID = item.itemData.saveID;
                int stackSize = item.stackSize;

                if (!data.playerInventory.ContainsKey(saveID))
                    data.playerInventory[saveID] = 0;

                data.playerInventory[saveID] += stackSize;
            }
        }

        data.equipedItems.Clear();
        foreach (var slot in this.equipmentSlots)
        {
            if (slot.HasItem())
                data.equipedItems[slot.equippedItem.itemData.saveID] = slot.slotType;
        }
    }
}
