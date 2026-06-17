using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Merchant : Inventory_Base
{
    private Inventory_Player playerInventory;
    [SerializeField] private ItemListData_SO shopItemDate;
    [SerializeField] private int minItemsAmount = 1;
    // [SerializeField] private int maxItemsAmount = 5;

    protected override void Awake()
    {
        base.Awake();

        FillShopList();
    }
    public void FillShopList()
    {
        //每次会随机刷新售卖物品
        //填充物品前先清空
        this.itemList.Clear();
        List<Inventory_Item> possibleItems = new List<Inventory_Item>();

        //根据配置的可售卖物品数据shopItemDate，生成所有的可售卖物品列表，
        // 数量根据minShopStackSize和maxShopStackSize随机生成
        foreach (var itemData in this.shopItemDate.itemList)
        {
            Inventory_Item itemToAdd = new Inventory_Item(itemData);

            int randomStackSize = UnityEngine.Random.Range(itemData.minShopStackSize, itemData.maxShopStackSize);
            int finalStackSize = Math.Clamp(randomStackSize, 1, itemData.maxStackSize); // 限制不超过最大堆叠数量
            itemToAdd.stackSize = finalStackSize;

            possibleItems.Add(itemToAdd);
        }

        //从所有的可售卖物品列表中随机出几个，作为该次的售卖物品
        //随机出售卖物品的数量
        int randomItemAmount = UnityEngine.Random.Range(this.minItemsAmount, this.maxInventorySize + 1);
        int finalItemAmount = Math.Clamp(randomItemAmount, 1, possibleItems.Count); //限制不超过所有的可售卖物品的数量

        for (int i = 0; i < finalItemAmount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, possibleItems.Count);
            Inventory_Item itemToAdd = possibleItems[randomIndex];

            if (CanAddItem(itemToAdd))
            {
                possibleItems.Remove(itemToAdd);
                AddItem(itemToAdd);
            }
        }

        onInventoryUpdateded?.Invoke();
    }

    public void SetInventory(Inventory_Player playerInventory) => this.playerInventory = playerInventory;

    public void Try2BuyItem(Inventory_Item itemToBuy, bool buyFullStack)
    {
        int amountToBuy = buyFullStack ? itemToBuy.stackSize : 1;

        for (int i = 0; i < amountToBuy; i++)
        {
            if (this.playerInventory.gold < itemToBuy.buyPrice)
            {
                Debug.Log("No Enough Money!");
                return;
            }


            if (itemToBuy.itemData.itemType == ItemType.Material)
            {
                // 避免同一引用
                Inventory_Item itemToAdd = new Inventory_Item(itemToBuy.itemData);
                this.playerInventory.storage.AddMaterialToStash(itemToAdd);

                this.playerInventory.gold -= itemToBuy.buyPrice;
                RemoveOneItem(itemToBuy);
            }
            else
            {
                if (this.playerInventory.CanAddItem(itemToBuy))
                {
                    // 避免同一引用
                    Inventory_Item itemToAdd = new Inventory_Item(itemToBuy.itemData);
                    this.playerInventory.AddItem(itemToAdd);

                    this.playerInventory.gold -= itemToBuy.buyPrice;
                    RemoveOneItem(itemToBuy);
                }
            }

        }

        onInventoryUpdateded?.Invoke();
    }

    public void Try2SellItem(Inventory_Item itemToSell, bool sellFullStack)
    {
        int sellAmount = sellFullStack ? itemToSell.stackSize : 1;
        for (int i = 0; i < sellAmount; i++)
        {
            int sellPrice = itemToSell.sellPrice;

            this.playerInventory.gold += sellPrice;
            this.playerInventory.RemoveOneItem(itemToSell);
        }
        onInventoryUpdateded?.Invoke();
    }

}
