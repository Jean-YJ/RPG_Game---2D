using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesInfo
{
    public AsyncOperationHandle handle;
    public uint validCount;

    public AddressablesInfo(AsyncOperationHandle handle)
    {
        this.handle = handle;
        validCount++;
    }

}

public class AddressablesMgr : BaseSingleton<AddressablesMgr>
{
    private AddressablesMgr() { }

    //父类装子类 AsyncOperationHandle继承IEnumerator接口
    //存放AsyncOperationHandle的容器，key为资源名,value为异步加载的返回值
    //private Dictionary<string,AsyncOperationHandle> aphDic = new Dictionary<string, AsyncOperationHandle>();
    private Dictionary<string, AddressablesInfo> aphDic = new Dictionary<string, AddressablesInfo>();

    //加载资源方法
    public void LoadAssetAsync<T>(string path, Action<AsyncOperationHandle<T>> callback)
    {
        //由于存在同名 不同类型资源的区分加载
        //所以我们通过名字和类型拼接作为 key
        string key = path + "_" + typeof(T).Name;
        AsyncOperationHandle<T> handle;
        //判断是否加载过该资源
        //已加载过资源
        //非首次加载该资源
        if (aphDic.ContainsKey(key))
        {
            aphDic[key].validCount++;
            handle = aphDic[key].handle.Convert<T>();
            //判断是否加载完毕
            if (handle.IsDone)
            {
                //非首次加载该资源且IsDone为true，表示该资源一定已经加载成功，直接回调即可
                callback(handle);
            }
            else
            {
                //已加载但还未加载完毕，需注册回调等待加载完成即可
                handle.Completed += (obj) => {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                        callback(obj);
                    else
                    {
                        Debug.Log(key + "资源加载失败");
                        //此处判断aphDic是否包含key是为了避免连续加载两次相同资源时，
                        //其中第一次未加载（加载第二次时）完毕而后失败移除了该key，第二次失败后若直接移除会空引用
                        if (aphDic.ContainsKey(key))
                            aphDic.Remove(key);
                    }
                };
            }
        }
        //未加载过资源
        else
        {
            //进行异步加载并存储返回值
            handle = Addressables.LoadAssetAsync<T>(path);
            //handle.Completed += callback;
            //判断资源加载是否成功
            handle.Completed += (obj) =>
            {
                if (obj.Status == AsyncOperationStatus.Succeeded)
                    callback(obj);
                else
                {
                    Debug.Log(key + "资源加载失败");
                    //此处判断aphDic是否包含key是为了避免连续加载两次相同资源时，
                    //其中第一次未加载（加载第二次时）完毕而后失败移除了该key，第二次失败后若直接移除会空引用
                    if (aphDic.ContainsKey(key))
                        aphDic.Remove(key);
                }
            };
            AddressablesInfo info = new AddressablesInfo(handle);
            aphDic.Add(key, info);
        }
    }

    //释放资源
    public void Release<T>(string path)
    {
        string key = path + "_" + typeof(T).Name;
        if (aphDic.ContainsKey(key))
        {
            aphDic[key].validCount--;
            if (aphDic[key].validCount <= 0)
            {
                AsyncOperationHandle<T> handle = aphDic[key].handle.Convert<T>();
                Addressables.Release(handle);
                aphDic.Remove(key);
            }
        }
    }

    //加载多个资源
    public void LoadAssetsAsync<T>(Addressables.MergeMode mode, Action<T> callback, params string[] paths)
    {
        string key = "";
        List<string> list = new List<string>(paths);
        foreach (string path in list)
        {
            key += path + "_";
        }
        key += typeof(T).Name;

        AsyncOperationHandle<IList<T>> handle;
        if (aphDic.ContainsKey(key))
        {
            aphDic[key].validCount++;
            handle = aphDic[key].handle.Convert<IList<T>>();
            //非首次加载不会调用Addressables.LoadAssetsAsync<T> API并自行调用callback，需要手动调用callback
            if (handle.IsDone)
            {
                foreach (var item in handle.Result)
                {
                    callback(item);
                }
            }
            else
            {
                //非首次加载不会调用Addressables.LoadAssetsAsync<T> API并自行调用callback，
                //需要判断是否加载成功并手动调用callback
                handle.Completed += (obj) => {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                    {
                        foreach (var item in handle.Result)
                        {
                            callback(item);
                        }
                    }
                    else
                    {
                        Debug.LogError(key + "资源加载失败");
                        if (aphDic.ContainsKey(key))
                            aphDic.Remove(key);
                    }
                };
            }
            return;
        }
        //Addressables.LoadAssetsAsync<T> 该API若成功加载会自行调用callback
        //所以只需要写明加载失败要进行的操作
        handle = Addressables.LoadAssetsAsync<T>(list, callback, mode);
        //obj本质是handle自身
        handle.Completed += (obj) =>
        {
            if (handle.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError(key + "资源加载失败");
                if (aphDic.ContainsKey(key))
                    aphDic.Remove(key);
            }
        };
        AddressablesInfo info = new AddressablesInfo(handle);
        aphDic.Add(key, info);
    }
    public void LoadAssetsAsync<T>(Addressables.MergeMode mode, Action<AsyncOperationHandle<IList<T>>> callback, params string[] paths)
    {
        string key = "";
        List<string> list = new List<string>(paths);
        foreach (string path in list)
        {
            key += path + "_";
        }
        key += typeof(T).Name;

        AsyncOperationHandle<IList<T>> handle;
        if (aphDic.ContainsKey(key))
        {
            aphDic[key].validCount++;
            handle = aphDic[key].handle.Convert<IList<T>>();
            if (handle.IsDone)
            {
                callback(handle);
            }
            else
            {
                handle.Completed += (obj) =>
                {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                    {
                        callback(obj);
                    }
                    else
                    {
                        Debug.LogError(key + "资源加载失败");
                        if (aphDic.ContainsKey(key))
                            aphDic.Remove(key);
                    }
                };
            }
            return;
        }

        handle = Addressables.LoadAssetsAsync<T>(list, null, mode);
        //未使用Addressables.LoadAssetsAsync<T> API自动调用回调函数
        //将obj即handle本身返回出去，在外部调用时遍历加载的资源
        handle.Completed += (obj) =>
        {
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                callback(obj);
            }
            else
            {
                Debug.LogError(key + "资源加载失败");
                if (aphDic.ContainsKey(key))
                    aphDic.Remove(key);
            }
        };
        AddressablesInfo info = new AddressablesInfo(handle);
        aphDic.Add(key, info);
    }

    public void Release<T>(params string[] paths)
    {
        string key = "";
        List<string> list = new List<string>(paths);
        foreach (string path in list)
        {
            key += path + "_";
        }
        key += typeof(T).Name;

        if (aphDic.ContainsKey(key))
        {
            aphDic[key].validCount--;
            if (aphDic[key].validCount <= 0)
            {
                AsyncOperationHandle<IList<T>> handle = aphDic[key].handle.Convert<IList<T>>();
                Addressables.Release(handle);
                aphDic.Remove(key);
            }
        }
    }

    //清空资源
    public void Clear()
    {
        foreach (KeyValuePair<string, AddressablesInfo> keyValuePair in aphDic)
        {
            Addressables.Release(keyValuePair.Value.handle);
        }
        aphDic.Clear();
        AssetBundle.UnloadAllAssetBundles(true);
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
}
