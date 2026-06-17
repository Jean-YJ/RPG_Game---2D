using System;
using System.IO;
using LitJson;
using Unity.VisualScripting;
using UnityEngine;

public class FileDataHandler
{
    private string saveDirPath;
    private string fullSavePath;
    private bool ifEncrypt;
    private string codeWord = "HoHoHo";

    public FileDataHandler(string dataDirPath, string dataFileName, bool ifEncrpy)
    {
        this.saveDirPath = dataDirPath;
        this.fullSavePath = Path.Combine(dataDirPath, dataFileName);
        this.ifEncrypt = ifEncrpy;
    }

    public void SaveData(GameData data)
    {
        try
        {
            //检测路径文件夹是否存在
            if (!Directory.Exists(this.saveDirPath))
                Directory.CreateDirectory(this.saveDirPath); //不存在则创建

            //将数据转换为Json字符串
            string dataJsonStr = JsonMapper.ToJson(data);

            if (this.ifEncrypt)
                dataJsonStr = EncryptDecrypt(dataJsonStr);

            // 创建Create和Write文件流
            using (FileStream fs = new FileStream(this.fullSavePath, FileMode.Create))
            using (StreamWriter write = new StreamWriter(fs))
            {
                //写入数据
                write.Write(dataJsonStr);

            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error on trying to save data to file: " + this.fullSavePath + "\n" + ex);
        }
    }

    public GameData LoadData()
    {
        GameData data = null;

        if (!File.Exists(this.fullSavePath))
        {
            Debug.LogWarning("Save data is not Exists: " + this.fullSavePath);
            return data;
        }

        try
        {
            string dataJsonStr = "";
            using (FileStream fs = new FileStream(this.fullSavePath, FileMode.Open))
            using (StreamReader reader = new StreamReader(fs))
            {
                dataJsonStr = reader.ReadToEnd();

            }

            if (this.ifEncrypt)
                dataJsonStr = EncryptDecrypt(dataJsonStr);

            data = JsonMapper.ToObject<GameData>(dataJsonStr);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error on trying to load data to file: " + this.fullSavePath + "\n" + ex);
        }

        return data;
    }

    public void Delete()
    {
        if (File.Exists(this.fullSavePath))
            File.Delete(this.fullSavePath);
    }

    private string EncryptDecrypt(string data)
    {
        Debug.Log("Encrypt Decrypt");
        char[] result = new char[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (char)(data[i] ^ codeWord[i % codeWord.Length]); //异或加密
        }

        return new string(result);
    }

}
