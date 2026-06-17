using System;
using System.Collections.Generic;
using UnityEngine;

public class Entity_DropManager : MonoBehaviour
{
    [SerializeField] private GameObject itemDropPrefab;
    [SerializeField] private ItemListData_SO itemDropData;

    [Header("Drop Restrictions")]
    [SerializeField] private int maxRarityAmount = 1200;
    [SerializeField] private int maxItemsToDrop = 3;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            DropItems();
    }

    public List<ItemData_SO> RollDrop()
    {
        List<ItemData_SO> possibleDrops = new List<ItemData_SO>();
        List<ItemData_SO> finalDrops = new List<ItemData_SO>();

        float maxRarityAmount = this.maxRarityAmount;
        // 遍历itemDropData，并根据 基于稀有度itemRarity 得到的DropChance判断该物品是否会掉落
        // 把会掉落的加入possibleDrops中
        foreach (var item in this.itemDropData.itemList)
        {
            float dropChance = item.GetDropChance();
            if (UnityEngine.Random.Range(0, 100) < dropChance)
                possibleDrops.Add(item);
        }

        // 将得到的possibleDrops进行排序，依据：稀有度itemRarity从高到低
        possibleDrops.Sort((a, b) => { return a.itemRarity > b.itemRarity ? -1 : 1; });
        // possibleDrops = possibleDrops.OrderByDescending(item => item.itemRarity).ToList();

        // 稀有度itemRarity从高到低,如果自身实体的maxRarityAmount大于掉落物的itemRarity，
        // 将其加入掉落列表，并相应的减少实体的maxRarityAmount
        foreach (var item in possibleDrops)
        {
            if (maxRarityAmount >= item.itemRarity)
            {
                finalDrops.Add(item);
                maxRarityAmount -= item.itemRarity;
            }
        }

        return finalDrops;
    }

    public virtual void DropItems()
    {
        List<ItemData_SO> itemsToDrop = RollDrop();

        int amountToDrop = Math.Min(this.maxItemsToDrop, itemsToDrop.Count);
        for (int i = 0; i < amountToDrop; i++)
        {
            CreateItem(itemsToDrop[i]);
        }
    }
    protected void CreateItem(ItemData_SO itemData)
    {
        GameObject newItem = Instantiate(this.itemDropPrefab, this.transform.position, Quaternion.identity);
        newItem.GetComponent<Object_ItemPickup>().SetUpItem(itemData);
    }
}
