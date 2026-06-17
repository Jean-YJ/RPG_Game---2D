using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory_Storage : Inventory_Base
{
    public Inventory_Player playerInventory;

    public List<Inventory_Item> materialStash;

    /// <summary>
    /// 连接到玩家背包
    /// </summary>
    /// <param name="playerInventory">玩家背包对象</param>
    public void ConnectToInventory(Inventory_Player playerInventory) => this.playerInventory = playerInventory;

    /// <summary>
    /// 把物品从背包移动到仓库
    /// </summary>
    /// <param name="item">移动的物品</param>
    public void FromInventoryToStorage(Inventory_Item item, bool transferFullStack)
    {
        if (this.playerInventory == null) return;
        if (!CanAddItem(item)) return;

        int transferAmount = transferFullStack ? item.stackSize : 1;
        for (int i = 0; i < transferAmount; i++)
        {

            this.playerInventory.RemoveOneItem(item);

            // 由于RemoveOneItem在处理堆叠时只会把数量减一，若此时使用 AddItem(item)，
            // 会导致Inventory和Storage共享同一个引用，从而导致各种bug
            // new一个新实例，默认 stackSize = 1。避免使用旧的实例而导致的共享引用bug
            Inventory_Item transformed = new Inventory_Item(item.itemData);
            AddItem(transformed);
        }

    }

    /// <summary>
    /// 把物品从仓库移动到背包
    /// </summary>
    /// <param name="item">移动的物品</param>
    public void FromStorageToInventory(Inventory_Item item, bool transferFullStack)
    {
        if (this.playerInventory == null) return;
        if (!playerInventory.CanAddItem(item)) return;

        int transferAmount = transferFullStack ? item.stackSize : 1;
        for (int i = 0; i < transferAmount; i++)
        {
            this.RemoveOneItem(item);

            // 由于RemoveOneItem在处理堆叠时只会把数量减一，若此时使用 AddItem(item)，
            // 会导致Inventory和Storage共享同一个引用，从而导致各种bug
            // new一个新实例，默认 stackSize = 1。避免使用旧的实例而导致的共享引用bug
            Inventory_Item transformed = new Inventory_Item(item.itemData);
            this.playerInventory.AddItem(transformed);
        }

        // onInventoryUpdateded?.Invoke();
    }

    /// <summary>
    /// 把物品添加到材料仓库中
    /// </summary>
    /// <param name="itemToAdd">目标物品</param>
    public void AddMaterialToStash(Inventory_Item itemToAdd)
    {
        Inventory_Item stackableItem = FindItemCanAddToStash(itemToAdd);
        // 材料仓库有所不同，我们设计它的容量是无限的
        // 所以假如材料仓库中没有该物品或以达到最大堆叠数量，只需添加即可，不用检查是否有空位
        if (stackableItem == null)
            this.materialStash.Add(itemToAdd);
        else
            stackableItem.AddStackSize();

        onInventoryUpdateded?.Invoke();
        this.materialStash.OrderBy(i => i.itemData.itemName).ToList(); // 按照物品名称排序，方便UI显示
    }

    /// <summary>
    /// 查找materialStash中可以和目标叠加的物品，如果找到了就返回该物品，否则返回null
    /// </summary>
    /// <param name="itemToAdd">目标物品</param>
    /// <returns></returns>
    public Inventory_Item FindItemCanAddToStash(Inventory_Item itemToAdd)
    {
        return this.materialStash.Find(i => i.itemData == itemToAdd.itemData && i.CanAddStack());
    }

    /// <summary>
    /// 获取所持有的目标物品的数量
    /// </summary>
    /// <param name="itemToFind">目标物品</param>
    /// <returns></returns>
    public int GetAmountOf(Inventory_Item itemToFind)
    {
        int amount = 0;
        // 统计背包
        foreach (var item in this.playerInventory.itemList)
        {
            if (item.itemData == itemToFind.itemData)
                amount += item.stackSize;
        }
        // 统计仓库
        foreach (var item in this.itemList)
        {
            if (item.itemData == itemToFind.itemData)
                amount += item.stackSize;
        }
        // 统计材料仓库
        foreach (var item in this.materialStash)
        {
            if (item.itemData == itemToFind.itemData)
                amount += item.stackSize;
        }

        return amount;
    }

    /// <summary>
    /// 判断是否持有足够的制作材料
    /// </summary>
    /// <param name="itemToCraft"></param>
    /// <returns></returns>
    private bool HasEnoughMaterials(Inventory_Item itemToCraft)
    {
        foreach (var item in itemToCraft.itemData.craftRecipe)
        {
            if (GetAmountOf(item) < item.stackSize)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 消耗制作材料
    /// </summary>
    /// <param name="itemToCraft">制作的物品</param>
    private void ConsumeMaterials(Inventory_Item itemToCraft)
    {
        foreach (var itemRequired in itemToCraft.itemData.craftRecipe)
        {
            int amountToConsume = itemRequired.stackSize;

            //背包中减去
            amountToConsume = amountToConsume - ConsumeMaterialAmount(this.playerInventory.itemList, itemRequired);
            //仓库中减去
            if (amountToConsume > 0)
                amountToConsume = amountToConsume - ConsumeMaterialAmount(this.itemList, itemRequired);
            //材料库中减去
            if (amountToConsume > 0)
                amountToConsume = amountToConsume - ConsumeMaterialAmount(this.materialStash, itemRequired);

        }
    }

    // 在物品列表中减去所需物品并返回消耗的数量
    private int ConsumeMaterialAmount(List<Inventory_Item> itemList, Inventory_Item itemRequired)
    {
        int amountNeeded = itemRequired.stackSize;
        int consumeAmount = 0;
        List<Inventory_Item> deleteList = new List<Inventory_Item>();
        foreach (var item in itemList)
        {
            if (item.itemData != itemRequired.itemData) continue;

            // 将背包中所需物品的数量和所需数量进行比较，存在两种情况：
            // 1.第一个匹配物品的堆叠数量 大于等于 所需数量。减去堆叠数量并统计即可
            // 2.第一个匹配物品的堆叠数量 小于 所需数量。先统计上该堆叠所有的数量，所需数量减去堆叠所有的数量，
            // 移除堆叠，再继续遍历
            int removedAmount = Math.Min(item.stackSize, amountNeeded - consumeAmount);

            //减少堆叠数量
            item.stackSize = item.stackSize - removedAmount;
            if (item.stackSize <= 0)
                deleteList.Add(item);

            consumeAmount = consumeAmount + removedAmount; //统计所需数量
            if (consumeAmount >= amountNeeded) break;
        }

        foreach (var item in deleteList)
        {
            itemList.Remove(item);
        }
        deleteList = null;


        return consumeAmount;
    }

    public bool CanCraftItem(Inventory_Item itemToCraft)
    {
        return this.HasEnoughMaterials(itemToCraft) && this.playerInventory.CanAddItem(itemToCraft);
    }
    public void CraftItem(Inventory_Item itemToCraft)
    {
        ConsumeMaterials(itemToCraft);
        this.playerInventory.AddItem(itemToCraft);
    }

    public override void LoadData(GameData data)
    {
        // base.LoadData(data);
        this.itemList.Clear();
        this.materialStash.Clear();

        foreach (var entry in data.storageItems)
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
        foreach (var entry in data.storageMaterials)
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
                AddMaterialToStash(itemToLoad);
            }
        }

        this.onInventoryUpdateded?.Invoke();
    }

    public override void SaveData(ref GameData data)
    {
        // base.SaveData(ref data);
        data.storageItems.Clear();
        foreach (var item in this.itemList)
        {
            if (item != null && item.itemData != null)
            {
                string saveID = item.itemData.saveID;
                int stackSize = item.stackSize;

                if (!data.storageItems.ContainsKey(saveID))
                    data.storageItems[saveID] = 0;

                data.storageItems[saveID] += stackSize;
            }
        }

        data.storageMaterials.Clear();
        foreach (var item in this.materialStash)
        {
            if (item != null && item.itemData != null)
            {
                string saveID = item.itemData.saveID;
                int stackSize = item.stackSize;

                if (!data.storageMaterials.ContainsKey(saveID))
                    data.storageMaterials[saveID] = 0;

                data.storageMaterials[saveID] += stackSize;
            }
        }
    }
}
