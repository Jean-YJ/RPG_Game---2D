using System.Collections.Generic;
using System.Linq;

//在 Player Build 中，`UnityEditor` 命名空间不可用。
//运行时脚本顶部直接 `using UnityEditor;` 可能导致打包编译失败。
#if UNITY_EDITOR
using UnityEditor;
#endif

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
