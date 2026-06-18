using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 资源信息基类 主要用于里式替换原则 父类容器装子类对象
/// </summary>
public abstract class ResInfoBase
{ 
    //该资源的引用次数
    public int referenceCount;
}

/// <summary>
/// 资源信息对象 主要用于存储资源信息 异步加载委托信息 异步加载 协程信息
/// </summary>
/// <typeparam name="T">资源类型</typeparam>
public class ResInfo<T> : ResInfoBase
{
    //资源
    public T asset;
    //主要用于异步加载结束后 传递资源到外部的委托
    public UnityAction<T> callback;
    //用于存储异步加载时 开启的协同程序
    public Coroutine coroutine;
    //referenceCount为0时是否要立即移除
    public bool isDel;
    

    public void AddRefCount()
    {
        this.referenceCount++;
    }

    public void SubRefCount()
    {
        this.referenceCount--;
        if (this.referenceCount < 0)
        {
            this.referenceCount = 0;
            Debug.LogWarning("引用计数小于0了，请检查使用和卸载是否配对执行");
        }
    }
}

/// <summary>
/// Resources 资源加载模块管理器
/// </summary>
public class ResMgr : BaseSingleton<ResMgr>
{
    private ResMgr() { }

    //存储已加载或加载中的资源的容器
    private Dictionary<string, ResInfoBase> resDic = new Dictionary<string, ResInfoBase>();

    /// <summary>
    /// 异步加载资源的方法
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径</param>
    /// <returns>加载得到的资源数据</returns>
    public T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        //资源的唯一ID，是通过 路径名_资源类型 拼接而成的
        string resName = path + "_" + typeof(T).Name;
        ResInfo<T> info;
        //容器中未记录该资源
        if (!resDic.ContainsKey(resName)) 
        {
            info = new ResInfo<T>();

            Debug.Log("实际同步加载");

            T res = Resources.Load<T>(path);
            info.AddRefCount();
            info.asset = res;
            this.resDic.Add(resName, info);
            return res;
        }
        //容器中记录了该资源
        else
        {
            info = this.resDic[resName] as ResInfo<T>;
            info.AddRefCount();
            //异步加载未完成
            if (info.asset == null)
            {
                //停止异步加载
                MonoMgr.Instance.StopCoroutine(info.coroutine);

                Debug.Log("实际同步加载");
                //通过同步加载得到资源
                info.asset = Resources.Load<T>(path);
                //还应该把那些等待着异步加载结束的委托去执行了
                info.callback?.Invoke(info.asset);
                //回调结束 异步加载也停了 所以清除无用的引用
                info.callback = null;
                info.coroutine = null;

                return info.asset;
            }
            //异步加载完成，资源已存在
            else
            {
                return info.asset;
            }
        }

    }

    /// <summary>
    /// 异步加载资源的方法
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径（Resources文件夹下的）</param>
    /// <param name="callback">加载结束后的回调函数 当异步加载资源结束后才会调用  
    /// 异步加载时传递的委托不能使用Lamaba表达式，应通过函数名来指定，否则后续无法移除对应的委托</param>
    public void LoadAssetAsync<T>(string path, UnityAction<T> callback) where T : UnityEngine.Object
    {
        //资源的唯一ID，是通过 路径名_资源类型 拼接而成的
        string resName = path + "_" + typeof(T).Name;

        ResInfo<T> info;
        //容器中不存在
        if (!resDic.ContainsKey(resName)) 
        {
            info = new ResInfo<T>();
            //添加到容器中并添加回调函数
            resDic.Add(resName, info);
            info.callback += callback;
            info.AddRefCount();
            //要通过协同程序去异步加载资源
            info.coroutine = MonoMgr.Instance.StartCoroutine(RealLoadAssetAsync<T>(path));
        }
        else
        {
            info = resDic[resName] as ResInfo<T>;
            info.AddRefCount();
            //加载中
            if (info.asset == null)
            {
                info.callback += callback;
            }
            //加载完毕
            else
            {
                callback(info.asset);
            }
        }
    }

    private IEnumerator RealLoadAssetAsync<T>(string path) where T : UnityEngine.Object
    {
        Debug.Log("实际异步加载开始");
        //异步加载资源
        ResourceRequest rq = Resources.LoadAsync<T>(path);
        //等待资源加载结束后 才会继续执行yield return后面的代码
        yield return rq;

        Debug.Log("实际异步加载完成");
        string resName = path + "_" + typeof(T).Name;
        //资源加载结束 将资源传到外部的委托函数去进行使用
        if (resDic.ContainsKey(resName))
        {
            ResInfo<T> info = resDic[resName] as ResInfo<T>;
            //取出资源信息 并且记录加载完成的资源
            info.asset = rq.asset as T;

            //如果异步加载未完成时要去移除资源，此时由于资源还不存在 Resources.UnloadAsset该API不会生效
            //所以在异步加载yield return后判断引用计数是否为0时，再调用一遍UnloadAsset真正去移除
            if (info.referenceCount == 0) 
            {
                this.UnloadAsset<T>(path, info.isDel, false);
            }
            else
            {
                //将加载完成的资源传递出去
                info.callback?.Invoke(info.asset);
                //加载完毕后 这些引用就可以清空 避免引用的占用 可能带来的潜在的内存泄漏问题
                info.callback = null;
                info.coroutine = null;
            }
        }
    }

    /// <summary>
    /// 异步加载资源的方法
    /// </summary>
    /// <param name="path">资源路径（Resources文件夹下的）</param>
    /// <param name="type">资源类型</param>
    /// <param name="callback">加载结束后的回调函数 当异步加载资源结束后才会调用</param>
    [Obsolete("注意：建议使用泛型加载方式，如果实在要用Type加载，一定不能和泛型加载混用去加载同类型同名资源," +
        "因为存储在容器中的统一资源的类型名分别为 T 和 Objcet，会导致唯一性出现问题")]
    public void LoadAssetAsync(string path, Type type, UnityAction<UnityEngine.Object> callback)
    {
        //资源的唯一ID，是通过 路径名_资源类型 拼接而成的
        string resName = path + "_" + type.Name;

        ResInfo<UnityEngine.Object> info;
        //容器中不存在
        if (!resDic.ContainsKey(resName))
        {
            info = new ResInfo<UnityEngine.Object>();
            info.AddRefCount();
            //添加到容器中并添加回调函数
            resDic.Add(resName, info);
            info.callback += callback;
            //要通过协同程序去异步加载资源 并且记录协同程序（用于之后可能的 停止）
            info.coroutine = MonoMgr.Instance.StartCoroutine(RealLoadAssetAsync(path, type));
        }
        else
        {
            info = resDic[resName] as ResInfo<UnityEngine.Object>;
            info.AddRefCount();
            //加载中
            if (info.asset == null)
            {
                info.callback += callback;
            }
            //加载完毕
            else
            {
                callback(info.asset);
            }
        }
    }

    private IEnumerator RealLoadAssetAsync(string path, Type type)
    {
        //异步加载资源
        ResourceRequest rq = Resources.LoadAsync(path, type);
        //等待资源加载结束后 才会继续执行yield return后面的代码
        yield return rq;
        
        string resName = path + "_" + type.Name;
        if (resDic.ContainsKey(resName)) 
        {
            ResInfo<UnityEngine.Object> info = resDic[resName] as ResInfo<UnityEngine.Object>;
            //取出资源信息 并且记录加载完成的资源
            info.asset = rq.asset;

            if(info.referenceCount == 0)
            {
                this.UnloadAsset(path, type, info.isDel, false);
            }
            else
            {
                //将加载完成的资源传递出去
                info.callback?.Invoke(info.asset);
                //加载完毕后 这些引用就可以清空 避免引用的占用 可能带来的潜在的内存泄漏问题
                info.callback = null;
                info.coroutine = null;
            }
        }
    }

    /// <summary>
    /// 指定卸载一个资源
    /// </summary>
    /// <param name="path">指定的要卸载的资源的路径</param>
    /// <param name="isDel">引用计数为0时，是否立即移除资源</param>
    /// <param name="isSub">卸除时是否减少引用计数，只有内部调用时为false，其余外部调用均应为true</param>
    /// <param name="callback">要移除的委托函数
    /// 注意：开启异步加载时传递的委托不能使用Lamaba表达式，应通过函数名来指定，否则无法移除对应的委托</param>
    public void UnloadAsset<T>(string path, bool isDel = false, bool isSub = true, UnityAction<T> callback = null)
    {
        string resName = path + "_" + typeof(T).Name;
        if (resDic.ContainsKey(resName)) 
        {
            ResInfo<T> info = this.resDic[resName] as ResInfo<T>;
            if (isSub)
                info.SubRefCount();
            info.isDel = isDel;
            if (info.asset == null)
            {
                //info.isDel = true;
                //当想让异步加载中断时 我们应该移除它的回调记录 而不是直接去卸载资源
                if (callback != null)
                    info.callback -= callback;
            }
            //资源已加载完毕，且引用计数为0
            else if (info.asset != null && info.referenceCount == 0 && info.isDel)
            {
                //GameObject / Prefab 不能直接 UnloadAsset
                T obj = info.asset;
                if (obj is GameObject)
                {
                    //只从字典中移除，让后续的 Resources.UnloadUnusedAssets 来真正卸载
                    this.resDic.Remove(resName);
                }
                else
                {
                    //卸除资源
                    Resources.UnloadAsset(info.asset as UnityEngine.Object);
                    //从字典中移除
                    this.resDic.Remove(resName);
                }
            }
        }
    }

    /// <summary>
    /// 指定卸载一个资源
    /// </summary>
    /// <param name="path">指定的要卸载的资源的路径</param>
    /// <param name="isDel">引用计数为0时，是否立即移除资源</param>
    /// <param name="isSub">卸除时是否减少引用计数，只有内部调用时为false，其余外部调用均应为true</param>
    /// <param name="callback">要移除的委托函数
    /// 注意：开启异步加载时传递的委托不能使用Lamaba表达式，应通过函数名来指定，否则无法移除对应的委托</param>
    public void UnloadAsset(string path, Type type, bool isDel = false, bool isSub = true, UnityAction<UnityEngine.Object> callback = null)
    {
        string resName = path + "_" + type.Name;
        if (resDic.ContainsKey(resName))
        {
            ResInfo<UnityEngine.Object> info = this.resDic[resName] as ResInfo<UnityEngine.Object>;
            if (isSub)
                info.SubRefCount();
            info.isDel = isDel;
            if (info.asset == null)
            {
                //info.isDel = true;
                if (callback != null)
                    info.callback -= callback;
            }
            else if (info.asset != null && info.referenceCount == 0 && info.isDel)
            {
                Resources.UnloadAsset(info.asset);
                this.resDic.Remove(resName);
            }
        }
        //Resources.UnloadAsset(asset);
    }

    /// <summary>
    /// 异步卸载没有使用的Resources相关的资源
    /// </summary>
    /// <param name="callback">回调函数</param>
    public void UnloadUnusedAssets(UnityAction callback)
    {
        MonoMgr.Instance.StartCoroutine(RealUnloadUnusedAssets(callback));
    }

    private IEnumerator RealUnloadUnusedAssets(UnityAction callback)
    {
        //就是在调用 Resources.UnloadUnusedAssets API移除不使用的资源之前
        //应该把我们自己记录的那些引用计数为0 并且没有被移除记录的资源移除掉
        List<string> list = new List<string>();
        foreach (string path in resDic.Keys)
        {
            if (resDic[path].referenceCount == 0)
                list.Add(path);
        }
        foreach (string path in list)
        {
            resDic.Remove(path);
        }

        AsyncOperation ao = Resources.UnloadUnusedAssets();
        yield return ao;
        //卸载完毕后 通知外部
        callback?.Invoke();
    }

    /// <summary>
    /// 清空字典
    /// </summary>
    /// <param name="callback"></param>
    public void ClearDic(UnityAction callback)
    {
        MonoMgr.Instance.StartCoroutine(ReallyClearDic(callback));
    }

    private IEnumerator ReallyClearDic(UnityAction callback)
    {
        resDic.Clear();
        AsyncOperation ao = Resources.UnloadUnusedAssets();
        yield return ao;
        //卸载完毕后 通知外部
        callback?.Invoke();
    }

    /// <summary>
    /// 获取当前某个资源的引用计数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public int GetRefCount<T>(string path)
    {
        string resName = path + "_" + typeof(T).Name;
        if (resDic.ContainsKey(resName))
        {
            return (resDic[resName] as ResInfo<T>).referenceCount;
        }
        return 0;
    }
    /// <summary>
    /// 取当前某个资源的引用计数
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public int GetRefCount(string path, Type type) 
    {
        string resName = path + "_" + type.Name;

        if (resDic.ContainsKey(resName))
        {
            return (resDic[resName] as ResInfo<UnityEngine.Object>).referenceCount;
        }
        return 0;
    }
}
