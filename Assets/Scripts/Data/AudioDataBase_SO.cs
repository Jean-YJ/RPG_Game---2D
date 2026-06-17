using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AUDIO DATA BASE", menuName = "RPG SetUp/Audio Data")]
public class AudioDataBase_SO : ScriptableObject
{
    public List<AudioClipData> playerAudio;
    public List<AudioClipData> uiAudio;
    public List<AudioClipData> mainMenuMusic;
    public List<AudioClipData> levelMusic;

    private Dictionary<string, AudioClipData> audioCollection;

    void OnEnable()
    {
        this.audioCollection = new Dictionary<string, AudioClipData>();

        AddAudioClipDataToCollection(this.playerAudio);
        AddAudioClipDataToCollection(this.uiAudio);
        AddAudioClipDataToCollection(this.mainMenuMusic);
        AddAudioClipDataToCollection(this.levelMusic);
    }

    private void AddAudioClipDataToCollection(List<AudioClipData> listToAdd)
    {
        foreach (var data in listToAdd)
        {
            if (data != null && !this.audioCollection.ContainsKey(data.audioName))
                this.audioCollection.Add(data.audioName, data);
        }
    }

    public AudioClipData GetAudioClipData(string dataName)
    {
        if (this.audioCollection.TryGetValue(dataName, out var result))
            return result;

        return null;
    }

}

[System.Serializable]
public class AudioClipData
{
    public string audioName;
    public List<AudioClip> clips = new List<AudioClip>();
    [Range(0, 1f)] public float maxVolume = 1f;

    public AudioClip GetRandomClip()
    {
        if (this.clips == null || this.clips.Count == 0)
            return null;

        return this.clips[Random.Range(0, this.clips.Count)];
    }
}
