using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory_Base : MonoBehaviour, ISaveable
{
    public int maxInventorySize = 10;
    public List<Inventory_Item> itemList = new List<Inventory_Item>();

    public UnityAction onInventoryUpdateded;
    [SerializeField] protected ItemListData_SO itemDataBase;

    protected virtual void Awake()
    {
        // Initialize inventory if needed
    }

    /// <summary>
    /// 检查背包是否可以堆叠该物品或还有槽位可以添加物品
    /// </summary>
    /// <returns></returns>
    public bool CanAddItem(Inventory_Item itemToAdd)
    {
        bool hasStackable = FindItemCanAddStack(itemToAdd.itemData) != null;
        return hasStackable || itemList.Count < maxInventorySize;
    }

    /// <summary>
    /// 检查是否可以将物品添加到现有堆叠中
    /// </summary>
    /// <param name="item">物品对象</param>
    /// <returns></returns>
    public bool CanAddToStack(Inventory_Item item)
    {

        List<Inventory_Item> stackableItems = itemList.FindAll(i => i.itemData == item.itemData &&
                                                                i.CanAddStack());
        return stackableItems.Count > 0;
    }

    public void AddItem(Inventory_Item item)
    {
        Inventory_Item itemInInventory = FindItemCanAddStack(item.itemData);

        //可以叠加就叠加，否则就新占一个槽位
        if (itemInInventory != null)
        {
            itemInInventory.AddStackSize();
        }
        else
        {
            itemList.Add(item);
        }

        onInventoryUpdateded?.Invoke();
        // Debug.Log("Added item to inventory: " + item.itemData.itemName);
    }

    // 查找可以叠加的物品，如果找到了就返回该物品，否则返回null
    // 某个物品到达了最大叠加数量，就不能叠加了，需要新占一个槽位来存储剩余的数量
    protected Inventory_Item FindItemCanAddStack(ItemData_SO itemData)
    {
        return itemList.Find(item => item.itemData == itemData && item.CanAddStack());
    }

    public Inventory_Item FindItem(Inventory_Item itemToFind)
    {
        //此处判断条件必须使用item == itemToUse，而不能使用item.itemData == itemToUse.itemData
        //因为背包里可能有多个相同类型的物品，使用item.itemData == itemToUse.itemData的条件会导致
        // 找到第一个相同类型的物品，而不是要找的这个物品
        return itemList.Find(item => item == itemToFind);
    }

    public Inventory_Item FindSameItem(Inventory_Item itemToFind)
    {
        //此处判断条件与上面相反，使用item.itemData == itemToUse.itemData的条件会找到第一个相同类型的物品，
        // 因为本函数的目的是找到与指定物品类型相同的第一个物品
        return itemList.Find(item => item.itemData == itemToFind.itemData);
    }

    /// <summary>
    /// 从自身的物品列表中移除单个目标物品
    /// </summary>
    /// <param name="itemToRemove">目标物品</param>
    public void RemoveOneItem(Inventory_Item itemToRemove)
    {
        Inventory_Item item = this.itemList.Find(i => i.ItemID == itemToRemove.ItemID);
        if (item == null) return;

        //有堆叠，数量减一
        if (item.stackSize > 1)
            item.RemoveStackSize();
        // 无堆叠或堆叠数量小于等于1，移除物品
        else
            itemList.Remove(item);
        onInventoryUpdateded?.Invoke();
    }
    /// <summary>
    /// 从自身的物品列表中移除目标物品的整个堆叠
    /// </summary>
    /// <param name="itemToRemove">目标物品</param>
    public void RemoveAllStack(Inventory_Item itemToRemove)
    {
        Inventory_Item item = this.itemList.Find(i => i.ItemID == itemToRemove.ItemID);
        if (item == null) return;

        itemList.Remove(item);
        onInventoryUpdateded?.Invoke();
    }

    public virtual void LoadData(GameData data)
    {
        // throw new System.NotImplementedException();
    }

    public virtual void SaveData(ref GameData data)
    {
        // throw new System.NotImplementedException();
    }
}
