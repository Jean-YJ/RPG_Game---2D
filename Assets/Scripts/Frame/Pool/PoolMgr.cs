using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 抽屉（池子中的数据）对象
/// </summary>
public class Drawer
{
    private GameObject drawerRoot;
    //存储空闲对象的容器
    private Stack<GameObject> stack = new Stack<GameObject>();
    public int Count => stack.Count;

    //存储正在使用中的对象的容器
    private List<GameObject> used = new List<GameObject>();
    public int usedCount => used.Count;

    private int maxNum;
    public bool NeedInstantitate => usedCount < maxNum;

    //创建新类型的Drawer时，会成为GameObject容器对象的子物体
    public Drawer(GameObject root,string typeName,GameObject usedObj) 
    {
        if(PoolMgr.isOpenLayout)
        {
            this.drawerRoot = new GameObject(typeName + "Root");
            drawerRoot.transform.SetParent(root.transform, false);
        }
        PushUsedList(usedObj);

        PoolObj poolObj = usedObj.GetComponent<PoolObj>();
        if (poolObj != null) 
        {
            maxNum = poolObj.maxNum;
        }
        else
        {
            Debug.LogError("请为使用缓存池功能的预设体对象挂载PoolObj脚本 用于设置数量上限");
        }
    }
    /// <summary>
    /// 从抽屉中弹出数据对象
    /// </summary>
    /// <returns>想要的对象数据</returns>
    public GameObject Pop()
    {
        GameObject obj;
        if (Count > 0)
        {
            obj = stack.Pop();
            used.Add(obj);
        }
        else
        {
            //在使用中的列表里取出最旧的，返回给obj并添加到使用中的列表的末尾
            obj = used[0];
            used.RemoveAt(0);
            used.Add(obj);
        }
        obj.SetActive(true);
        if (PoolMgr.isOpenLayout)
            obj.transform.SetParent(null, false);
        return obj;
    }
    /// <summary>
    /// 将物体放入到抽屉对象中
    /// </summary>
    /// <param name="obj"></param>
    public void Push(GameObject obj) 
    {
        obj.SetActive(false);
        if(PoolMgr.isOpenLayout)
            obj.transform.SetParent(drawerRoot.transform, false);
        stack.Push(obj);
        //这个对象已经不再使用了 应该把它从记录容器中移除
        used.Remove(obj);
    }

    /// <summary>
    /// 将对象压入到使用中的容器中记录
    /// </summary>
    /// <param name="obj"></param>
    public void PushUsedList(GameObject obj)
    {
        used.Add(obj);
    }
}

/// <summary>
/// 方便在字典当中用里式替换原则 存储子类对象
/// </summary>
public abstract class DrawerWithOutMonoBase { }
/// <summary>
/// 用于存储 数据结构类 和 逻辑类 （不继承mono的）容器类
/// </summary>
/// <typeparam name="T"></typeparam>
public class DrawerWithOutMono<T> : DrawerWithOutMonoBase where T : class
{
    public Queue<T> queue = new Queue<T>();
}
/// <summary>
/// 想要被复用的 数据结构类、逻辑类 都必须要继承该接口
/// </summary>
public interface IPoolClass
{
    /// <summary>
    /// 重置数据的方法
    /// </summary>
    void ResetInfo();
}

/// <summary>
/// 缓存池（对象池）管理器
/// </summary>
public class PoolMgr:BaseSingleton<PoolMgr>
{
    private PoolMgr() { }
    private GameObject poolRoot;
    //GameObject容器,key为一类型的GameObject的名称，如：bullet；value为存储实例化的该类型GameObject对象的数据结构
    private Dictionary<string, Drawer> gameObjDic = new Dictionary<string, Drawer>();
    private GameObject gobjDicRoot;

    /// <summary>
    /// 用于存储数据结构类、逻辑类对象的 池子的字典容器
    /// </summary>
    private Dictionary<string, DrawerWithOutMonoBase> classObjDic = new Dictionary<string, DrawerWithOutMonoBase>();

    public static bool isOpenLayout = true;

    /// <summary>
    /// 根据类型名获取对应的实例化对象
    /// </summary>
    /// <param name="typeName">要获取的GameObject的类型名称</param>
    /// <returns></returns>
    public GameObject GetGameObject(string typeName)
    {
        //对象池根对象不存在就创建一个
        if (poolRoot == null && isOpenLayout)
            poolRoot = new GameObject("PoolRoot");

        //容器根对象不存在就创建一个
        if (gobjDicRoot == null && isOpenLayout)
        {
            gobjDicRoot = new GameObject("GameObjectRoot");
            gobjDicRoot.transform.SetParent(poolRoot.transform, false);
        }

        GameObject gameObj;
        #region  没有设置对象数量上限时的逻辑
        ////对应类型GameObject对象的数据结构存在且有空闲对象
        //if (gameObjDic.ContainsKey(typeName) && gameObjDic[typeName].Count > 0)
        //{
        //    gameObj = gameObjDic[typeName].Pop();
        //    //gameObj.SetActive(true);
        //}
        ////否则 在资源中加载并实例化
        //else
        //{
        //    gameObj = GameObject.Instantiate(Resources.Load<GameObject>(typeName));
        //    //统一名称为类型名称，便于后续归还
        //    gameObj.name = typeName;
        //}
        #endregion
        //每次取对象时应该分情况考虑
        //  情况1：没有抽屉时
        //  情况2：有抽屉，并且抽屉里有没用的对象或者使用中对象超过上限时
        //  情况3：有抽屉，但是抽屉里没有对象，使用中对象也没有超过上限时
        if (!gameObjDic.ContainsKey(typeName)) //创建Drawer，加载并实例化对象，将对象放入used
        {
            gameObj = GameObject.Instantiate(Resources.Load<GameObject>(typeName));
            gameObj.name = typeName;

            Drawer drawer = new Drawer(gobjDicRoot,typeName,gameObj);
            gameObjDic.Add(typeName, drawer);
        }
        //直接拿出来用，具体used还是stack由Pop中的逻辑确定
        else if (gameObjDic[typeName].Count > 0 || !gameObjDic[typeName].NeedInstantitate) 
        {
            gameObj = gameObjDic[typeName].Pop();
        }
        //加载并实例化对象，将对象放入used
        else if (gameObjDic[typeName].Count == 0 && gameObjDic[typeName].NeedInstantitate)
        {
            gameObj = GameObject.Instantiate(Resources.Load<GameObject>(typeName));
            gameObj.name = typeName;
            gameObjDic[typeName].PushUsedList(gameObj);
        }
        else
        {
            gameObj = null;
        }

        return gameObj;
    }

    /// <summary>
    /// 获取自定义的数据结构类和逻辑类对象 （不继承Mono的）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="nameSpace">命名空间，区分不同用处的同类型的类对象</param>
    /// <returns></returns>
    public T GetClassObject<T>(string nameSpace = "") where T : class, IPoolClass, new()
    {
        string poolName = nameSpace + "_" + typeof(T).Name;
        //容器中存在该类型的对象池
        if (this.classObjDic.ContainsKey(poolName))
        {
            DrawerWithOutMono<T> drawer = this.classObjDic[poolName] as DrawerWithOutMono<T>;
            //Drawer中是否有可以复用的内容
            if (drawer.queue.Count > 0)
            {
                T obj = drawer.queue.Dequeue();
                return obj;
            }
            else
            {
                //必须保证存在无参构造函数
                T obj = new T();
                return obj;
            }
        }
        else//不存在
        {
            T obj = new T();
            return obj;
        }
    }

    /// <summary>
    /// 归还取出的实例化对象
    /// </summary>
    /// <param name="gameObj">归还的对象</param>
    public void PushGameObject(GameObject gameObj) 
    {

        //对应类型GameObject对象的数据结构若不存在，则创建一个
        //if (!gameObjDic.ContainsKey(gameObj.name))
        //{
        //    gameObjDic.Add(gameObj.name, new Drawer(gobjDicRoot, gameObj.name));
        //}

        //存储归还的对象
        gameObjDic[gameObj.name].Push(gameObj);
    }

    public void PushClassObject<T>(T obj, string nameSpace = "") where T : class, IPoolClass
    {
        if (obj == null)
            return;
        string poolName = nameSpace + "_" + typeof(T).Name;
        DrawerWithOutMono<T> drawer;
        if (this.classObjDic.ContainsKey(poolName))
            drawer = this.classObjDic[poolName] as DrawerWithOutMono<T>;
        else
        {
            drawer = new DrawerWithOutMono<T>();
            this.classObjDic.Add(poolName, drawer);
        }

        //在放入池子中之前 先重置对象的数据
        obj.ResetInfo();
        drawer.queue.Enqueue(obj);
    }

    /// <summary>
    /// 清空容器   切换场景时所有的实例化对象都会被移除
    /// 容器中记录的对象都会不存在，不清空会造成空引用
    /// </summary>
    public void ClearDic()
    {
        gameObjDic.Clear();
        poolRoot = null;
        gobjDicRoot = null;
        classObjDic.Clear();
    }
}
