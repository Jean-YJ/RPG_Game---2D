using System.Collections.Generic;

//在 Player Build 中，`UnityEditor` 命名空间不可用。
//运行时脚本顶部直接 `using UnityEditor;` 可能导致打包编译失败。
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

[CreateAssetMenu(menuName = "RPG SetUp/Item Data/Material item", fileName = "Material data - ")]
public class ItemData_SO : ScriptableObject
{
    public string saveID { get; private set; }

    [Header("Item details")]
    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;
    public int maxStackSize = 1;

    [Header("Item Effect")]
    public ItemEffectData_SO itemEffect;

    [Header("Craft Recipe")]
    public List<Inventory_Item> craftRecipe;

    [Header("Merchant Deatil")]
    [Range(1, 10000)]
    public int itemPrice = 100;
    public int minShopStackSize = 1;
    public int maxShopStackSize = 10;

    [Header("Drop Deatil")]
    [Range(1, 1000)]
    public int itemRarity = 100;
    [Range(1, 100)]
    public float dropChance;
    [Range(1, 100)]
    public float maxDropChance = 65f;

    void OnValidate()
    {
        this.dropChance = GetDropChance();

#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(this);
        this.saveID = AssetDatabase.AssetPathToGUID(path);
#endif

    }

    public float GetDropChance()
    {
        float maxRarity = 1000;
        float chance = (maxRarity - this.itemRarity + 1) / maxRarity * 100;

        return Mathf.Min(chance, this.maxDropChance);
    }
}


public enum ItemType
{
    Material,
    Weapon,
    Armor,
    Trinket, // ring, amulets, belt, accessory
    Consumable // potions, buff scrolls
}
