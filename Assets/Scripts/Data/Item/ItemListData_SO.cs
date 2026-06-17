using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG SetUp/Item Data/Item List", fileName = "List of Item - ")]
public class ItemListData_SO : ScriptableObject
{
    public List<ItemData_SO> itemList;

    public ItemData_SO GetDataByID(string saveID)
    {
        return itemList.FirstOrDefault(item => item != null && item.saveID == saveID);
    }

#if UNITY_EDITOR

    //收集所有的ItemData_SO数据
    [ContextMenu("Auto fill all ItemData_SO")]
    public void CollectItemsData()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemData_SO");

        this.itemList = guids.Select(guid => AssetDatabase.LoadAssetAtPath<ItemData_SO>(AssetDatabase.GUIDToAssetPath(guid)))
                        .Where(item => item != null).ToList();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

#endif
}
