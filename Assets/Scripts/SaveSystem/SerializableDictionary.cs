using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();


    public void OnAfterDeserialize()
    {
        this.Clear();

        if (this.keys.Count != this.values.Count)
            Debug.LogWarning("keys count is not equals values count");

        for (int i = 0; i < this.keys.Count; i++)
        {
            this.Add(this.keys[i], this.values[i]);
        }
    }

    public void OnBeforeSerialize()
    {
        this.keys.Clear();
        this.values.Clear();

        foreach (KeyValuePair<TKey, TValue> kvp in this)
        {
            this.keys.Add(kvp.Key);
            this.values.Add(kvp.Value);
        }
    }
}
