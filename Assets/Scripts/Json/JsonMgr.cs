using LitJson;
using System.IO;
using UnityEngine;

public enum Json_Type
{
    JsonUtlity,LitJson
}

public class JsonMgr
{
    private static JsonMgr instance = new JsonMgr();
    public static JsonMgr Instance => instance ?? (instance = new JsonMgr());
    private JsonMgr() { }

    public void SaveData(object data, string fileName, Json_Type type = Json_Type.LitJson)
    {
        string path = Application.persistentDataPath + "/" + fileName + ".json";
        string jsonStr = "";
        switch (type)
        {
            case Json_Type.JsonUtlity:
                jsonStr = JsonUtility.ToJson(data);
                break;
            case Json_Type.LitJson:
                jsonStr = JsonMapper.ToJson(data);
                break;
        }

        File.WriteAllText(path, jsonStr);
    }

    public T LoadData<T>(string fileName, Json_Type type = Json_Type.LitJson) where T : new()
    {
        //string path = Application.streamingAssetsPath + "/" + fileName + ".json";
        string path = Application.persistentDataPath + "/" + fileName + ".json";
        if (!File.Exists(path))
        {
            //path = Application.persistentDataPath + "/" + fileName + ".json";
            path = Application.streamingAssetsPath + "/" + fileName + ".json";

        }
        if (!File.Exists(path))
            return new T();

        string jsonStr = File.ReadAllText(path);
        T data = default(T);
        switch (type)
        {
            case Json_Type.JsonUtlity:
                data = JsonUtility.FromJson<T>(jsonStr);
                break;
            case Json_Type.LitJson:
                data = JsonMapper.ToObject<T>(jsonStr);
                break;
        }

        return data;
    }
}
