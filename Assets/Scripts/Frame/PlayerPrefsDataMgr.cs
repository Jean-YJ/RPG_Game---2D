using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Xml.Linq;
using Unity.VisualScripting;

public class PlayerPrefsDataMgr
{
    //构建单例模式
    private static PlayerPrefsDataMgr instance = new PlayerPrefsDataMgr();
    public static PlayerPrefsDataMgr Instance { get { return instance; } }
    private PlayerPrefsDataMgr() { }

    /// <summary>
    /// 存储数据
    /// </summary>
    /// <param name="data">想要存储的数据对象</param>
    /// <param name="keyName">数据对象的唯一key  我们自己控制</param>
    /// <param name="isCustomType">是否时自定义类型数据</param>
    public void SaveData(object data,string keyName,bool isCustomType = false)
    {
        if (data == null)
        {
            Debug.LogWarning("SaveData 失败：data 为 null");
            return;
        }
        //获取data的数据类型
        Type dataType = data.GetType();

        //若是自定义类型，将该类型的fullkey存储为一个string数据，避免加载数据时HasKey()导致数据加载失败
        //以TestInfo中的Item对象为例：data为item时，fullkey为keyname_TestInfo_Item_item
        //存储一个键和值都为“keyname_TestInfo_Item_item”的数据
        if (isCustomType)
            SaveValue(keyName, keyName);

        //获取data数据类型 该类下的所有公共成员变量
        FieldInfo[] fieldInfos = dataType.GetFields();
        string fullKey;
        object value;
        foreach (FieldInfo fieldInfo in fieldInfos)
        {
            //设置key的规则 们自己控制的唯一key + data对象的数据类型 + 该公共成员变量的数据类型 +该公共成员变量的名称
            fullKey = $"{keyName}_{dataType.Name}_{fieldInfo.FieldType.Name}_{fieldInfo.Name}";
            //获取该公共成员变量的值 参数data为被获取的实例化对象 data.fieldInfo.value
            value = fieldInfo.GetValue(data);
            SaveValue(value, fullKey);
        }

        PlayerPrefs.Save();
    }
    public void SaveValue(object value, string fullKey)
    {
        Type valueType = value.GetType();
        //常用类型
        if (valueType == typeof(int))
            PlayerPrefs.SetInt(fullKey, (int)value);
        else if (valueType == typeof(float))
            PlayerPrefs.SetFloat(fullKey, (float)value);
        else if (valueType == typeof(string))
            PlayerPrefs.SetString(fullKey, (string)value);
        else if (valueType == typeof(bool))
            //PlayerPrefs不支持bool类型的存储，自定义一个存储规则；true存1，false存0
            PlayerPrefs.SetInt(fullKey, (bool)value ? 1 : 0);
        //如何判断 泛型类的类型
        //通过反射 判断 父子关系
        //IList 是 List<>继承的一个接口，此处用IsAssignableFrom判断value的类型是否是继承了IList
        //以此来判断数据是否是List类型
        else if ((typeof(IList).IsAssignableFrom(valueType)))
        {
            //无法确定List的类型，故使用父类IList来盛放value
            IList valueToList = value as IList;
            //用于加在fullkey后来确保value中每个对象的键的唯一性
            int keyIndex = 0;
            //存储value中对象的个数
            PlayerPrefs.SetInt(fullKey, valueToList.Count);
            foreach (var item in valueToList)
            {
                //递归判断value中每个对象的类型（常用类型），并进行相应的存储
                SaveValue(item, $"{fullKey}{keyIndex}");
                keyIndex++;
            }
        }
        //判断数据是否是Dictionary类型
        else if ((typeof(IDictionary).IsAssignableFrom(valueType)))
        {
            IDictionary valueDictionary = value as IDictionary;
            int keyIndex = 0;
            PlayerPrefs.SetInt(fullKey, valueDictionary.Count);
            foreach (var item in valueDictionary.Keys)
            {
                SaveValue(item, $"{fullKey}_DicKey_{keyIndex}");
                SaveValue(valueDictionary[item], $"{fullKey}_DicValue_{keyIndex}");
                keyIndex++;
            }
        }
        //如不是上述类型，则是自定义类型数据
        else
        {
            //Debug.Log($"自定义的字段类型：{valueType.Name}  fullKey:{fullKey}");
            SaveData(value, fullKey, true);
        }
    }
    public object LoadValue(Type fieldType, string fullKey)
    {
        //通过fullkey获取到该唯一key下的某个公共成员变量的值，并将该值赋给obj.fieldInfo.value
        if (fieldType == typeof(int))
        {
            return PlayerPrefs.GetInt(fullKey);
        }
        else if (fieldType == typeof(float))
        {
            return PlayerPrefs.GetFloat(fullKey);
        }
        else if (fieldType == typeof(string))
        {
            return PlayerPrefs.GetString(fullKey);
        }
        else if (fieldType == typeof(bool))
        {
            return PlayerPrefs.GetInt(fullKey) == 1 ? true : false;
        }
        //List
        else if ((typeof(IList).IsAssignableFrom(fieldType)))
        {
            //得到List长度
            int listCount = PlayerPrefs.GetInt(fullKey);
            IList list = Activator.CreateInstance(fieldType) as IList;
            for (int i = 0; i < listCount; i++)
            {
                //获取List<>的泛型类型数组，由于List<>只存在一个泛型类型，所以[0]是该List的泛型数据类型
                list.Add(LoadValue(fieldType.GetGenericArguments()[0], $"{fullKey}{i}"));
            }
            return list;
        }
        //Dictionary
        else if ((typeof(IDictionary).IsAssignableFrom(fieldType)))
        {
            int dicCount = PlayerPrefs.GetInt(fullKey);
            IDictionary dic = Activator.CreateInstance(fieldType) as IDictionary;
            for (int i = 0; i < dicCount; i++)
            {
                dic.Add(LoadValue(fieldType.GetGenericArguments()[0], $"{fullKey}_DicKey_{i}"),
                    LoadValue(fieldType.GetGenericArguments()[1], $"{fullKey}_DicValue_{i}"));
            }
            return dic;
        }
        else
        {
            //Debug.LogWarning($"自定义的字段类型：{fieldType.Name}");
            //return null;
            return LoadData(fieldType, fullKey);
        }
    }
    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="dataType">想要读取的数据的数据类型</param>
    /// <param name="keyName">数据对象的唯一key  我们自己控制</param>
    /// <returns>将函数内部读取到数据对象输出到外部对象上</returns>
    public object LoadData(Type dataType,string keyName) 
    {
        //创建该类型的实例，需要使用dataType类型 类中的无参构造函数
        object obj = Activator.CreateInstance(dataType);
        FieldInfo[] fieldInfos = dataType.GetFields();
        string fullKey;
        foreach (FieldInfo fieldInfo in fieldInfos) 
        {
            fullKey = $"{keyName}_{dataType.Name}_{fieldInfo.FieldType.Name}_{fieldInfo.Name}";

            //PlayerPrefs.HasKey(fullKey)在加载自定义类型是会出问题，以TestInfo中的Item对象为例：
            //fieldInfo为item时，fullkey为keyname_TestInfo_Item_item
            //但Item对象被分为了常用类型何集合类型存储，不存在fullkey为keyname_TestInfo_Item_item的数据
            //仅能通过(keyname_TestInfo_Item_item)_Item_Int32_id 这种跟底层类型的fullkey来查找到item对象中的数据
            //解决方法：savedata时存储一个键和值都为“keyname_TestInfo_Item_item”的数据；或去掉PlayerPrefs.HasKey(fullKey)

            //Returns true if the given key exists in PlayerPrefs, otherwise returns false.
            if (!PlayerPrefs.HasKey(fullKey))
            {
                Debug.LogWarning($"PlayerPrefs 中未找到字段：{fullKey}，跳过");
                continue;
            }
            
            fieldInfo.SetValue(obj, LoadValue(fieldInfo.FieldType,fullKey));
        }

        return obj;
    }
}
