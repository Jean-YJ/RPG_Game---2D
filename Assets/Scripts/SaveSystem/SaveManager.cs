using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static SaveManager instance;
    public static SaveManager Instance => instance;
    private SaveManager() { }

    private GameData data;
    private FileDataHandler dataHandler;
    private List<ISaveable> allSaveables;

    [SerializeField] private string fileName = "unity_JeanYJ - RPG_Game.json";
    [SerializeField] private bool ifEncrypt = true;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        instance = this;
    }

    IEnumerator Start()
    {
        // Debug.Log(Application.persistentDataPath);

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, this.fileName, this.ifEncrypt);
        this.allSaveables = FindISaveables();

        yield return null;
        LoadGame();
    }

    //扫面场景中所有同时继承MonoBehaviour类和ISaveable接口的对象
    private List<ISaveable> FindISaveables()
    {
        //FindObjectsByType<MonoBehaviour> 获取场景中所有已实例化的继承MonoBehaviour类的对象
        //(FindObjectsInactive.Include) 包含Active = false 的对象
        //OfType<ISaveable> 筛选过滤掉没有继承ISaveable的对象
        //ToList() 将筛选结果转换成列表
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include).OfType<ISaveable>().ToList();
    }

    public void SaveGame()
    {
        foreach (var saveable in this.allSaveables)
            saveable.SaveData(ref this.data);

        //写入文件
        this.dataHandler.SaveData(this.data);
    }

    public void LoadGame()
    {
        this.data = this.dataHandler.LoadData();
        if (this.data == null)
            this.data = new GameData();

        foreach (var saveable in this.allSaveables)
            saveable.LoadData(this.data);
    }

    public GameData GetGameData() => this.data;

    [ContextMenu("*** Delete Save Data ***")]
    public void DeleteSaveData()
    {
        if (this.dataHandler == null)
            this.dataHandler = new FileDataHandler(Application.persistentDataPath, this.fileName, this.ifEncrypt);

        this.dataHandler.Delete();

        LoadGame();
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }
}
