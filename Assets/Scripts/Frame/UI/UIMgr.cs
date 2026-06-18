using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// 层级枚举
/// </summary>
public enum E_UILayer
{
    /// <summary>
    /// 最底层
    /// </summary>
    Bottom,
    /// <summary>
    /// 中层
    /// </summary>
    Middle,
    /// <summary>
    /// 高层
    /// </summary>
    Top,
    /// <summary>
    /// 系统层 最高层
    /// </summary>
    System,
}

public class UIMgr : BaseSingleton<UIMgr>
{
    /// <summary>
    /// 主要用于里式替换原则 在字典中 用父类容器装载子类对象
    /// </summary>
    private abstract class BasePanelInfo { }
    /// <summary>
    /// 用于存储面板信息 和加载完成的回调函数的
    /// </summary>
    /// <typeparam name="T">面板的类型</typeparam>
    private class PanelInfo<T> : BasePanelInfo where T : BasePanel
    {
        public T panel;
        public UnityAction<T> callback;
        public bool isHide;

        public PanelInfo(UnityAction<T> callback)
        {
            this.callback += callback;
        }
    }

    private Camera uiCamera;
    private Canvas uiCanvas;
    private EventSystem uiEventSystem;

    //层级父对象
    private Transform bottomLayer;
    private Transform middleLayer;
    private Transform topLayer;
    private Transform systemLayer;

    //存储所有显示中的面板的容器，key为面板名
    //注意：面板名要与面板类型一致！！！！！！！！
    private Dictionary<string,BasePanelInfo> panelDic = new Dictionary<string,BasePanelInfo>();

    private UIMgr()
    {
        //动态创建唯一的Canvas、EventSystem和UI摄像机
        GameObject camera = GameObject.Instantiate(Resources.Load<GameObject>("UI/UICamera"));
        this.uiCamera = camera.GetComponent<Camera>();
        //过场景不移除
        GameObject.DontDestroyOnLoad(camera);

        GameObject canvas = GameObject.Instantiate(Resources.Load<GameObject>("UI/Canvas"));
        this.uiCanvas = canvas.GetComponent<Canvas>();
        //设置使用的UI摄像机
        this.uiCanvas.worldCamera = this.uiCamera;
        GameObject.DontDestroyOnLoad(canvas);

        GameObject eventSystem = GameObject.Instantiate(Resources.Load<GameObject>("UI/EventSystem"));
        this.uiEventSystem = eventSystem.GetComponent<EventSystem>();
        GameObject.DontDestroyOnLoad(eventSystem);

        //找到层级父对象
        bottomLayer = uiCanvas.transform.Find("Bottom");
        middleLayer = uiCanvas.transform.Find("Middle");
        topLayer = uiCanvas.transform.Find("Top");
        systemLayer = uiCanvas.transform.Find("System");
    }

    /// <summary>
    /// 获取对应层级的父对象
    /// </summary>
    /// <param name="layer">层级枚举值</param>
    /// <returns></returns>
    public Transform GetLayer(E_UILayer layer)
    {
        switch (layer)
        {
            case E_UILayer.Bottom:
                return bottomLayer;
            case E_UILayer.Middle:
                return middleLayer;
            case E_UILayer.Top:
                return topLayer;
            case E_UILayer.System:
                return systemLayer;
            default:
                return null;
        }
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <typeparam name="T">面板的类型</typeparam>
    /// <param name="uILayer">面板所在的层级</param>
    /// <param name="callback">由于是异步加载 因此通过委托回调的形式 将加载完成的面板传递出去进行使用</param>
    public void ShowPanel<T>(E_UILayer uILayer = E_UILayer.Middle, UnityAction<T> callback = null) where T : BasePanel
    {
        //面板名要与面板类型一致！！！！！！！！
        string panelName = typeof(T).Name;

        //容器中是否存储了该面板
        if (panelDic.ContainsKey(panelName))
        {
            PanelInfo<T> panelInfo = panelDic[panelName] as PanelInfo<T>;
            //异步加载中
            //容器中存在但panel变量为null，表示正在加载中
            if (panelInfo.panel == null)
            {
                //如果之前显示了又隐藏 现在又想显示 那么隐藏标志位直接设为false
                panelInfo.isHide = false;

                //如果正在异步加载 应该等待它加载完毕 只需要记录回调函数 加载完后去调用即可
                if (callback != null)
                    panelInfo.callback += callback;
            }
            //异步加载已结束
            else
            {
                if(!panelInfo.panel.gameObject.activeSelf)
                {
                    panelInfo.panel.gameObject.SetActive(true);
                }
                //由于要显示面板，所以执行一次面板类中的Show()
                panelInfo.panel.Show();
                callback?.Invoke(panelInfo.panel);
            }

            return;
        }
        //不存在面板
        //先将面板名称和空的PanelInfo存入容器进行展位（解决异步加载相关问题）
        panelDic.Add(panelName,new PanelInfo<T>(callback));
        //再异步加载资源
        AddressablesMgr.Instance.LoadAssetAsync<GameObject>(panelName, (handler) =>
        {
            PanelInfo<T> panelInfo = this.panelDic[panelName] as PanelInfo<T>;
            //异步加载结束前 就想要隐藏该面板了 
            if (panelInfo.isHide)
            {
                this.panelDic.Remove(panelName);
                return;
            }

            //实例化面板对象并设置层级
            GameObject panelObj = GameObject.Instantiate(handler.Result);
            Transform layer = GetLayer(uILayer);
            //避免没有按指定规则传递层级参数 避免为空
            if (layer == null)
                layer = this.middleLayer;
            panelObj.transform.SetParent(layer, false);

            //获取面板组件
            T panel = panelObj.GetComponent<T>();
            panelInfo.panel = panel;
            //显示并执行回调
            panel.Show();
            panelInfo.callback?.Invoke(panel);
            //回调执行完 将其清空 避免内存泄漏
            panelInfo.callback = null;
        });
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <param name="isDestory">隐藏时是否销毁</param>
    /// <typeparam name="T">面板的类型</typeparam>
    public void HidePanel<T>(bool isDestory = false) where T : BasePanel
    {
        //面板名要与面板类型一致！！！！！！！！
        string panelName = typeof(T).Name;
        if (this.panelDic.ContainsKey(panelName))
        {
            PanelInfo<T> panelInfo = panelDic[panelName] as PanelInfo<T>;
            //异步加载中
            //容器中存在但panel变量为null，表示正在加载中
            if (panelInfo.panel == null)
            {
                //如果正在异步加载 修改隐藏表示这个面板即将要隐藏，不再显示
                panelInfo.isHide = true;
                //既然要隐藏了 回调函数都不会调用了 直接置空
                panelInfo.callback = null;
            }
            //异步加载已结束
            else
            {
                //执行一次面板类中的Hide()
                panelInfo.panel.Hide();
                if(isDestory)
                {
                    //销毁并移除面板
                    GameObject.Destroy(panelInfo.panel.gameObject);
                    this.panelDic.Remove(panelName);
                }
                else
                {
                    panelInfo.panel.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 获取面板对象
    /// </summary>
    /// <typeparam name="T">面板的类型</typeparam>
    /// <returns>面板类对象</returns>
    public void GetPanel<T>(UnityAction<T> callback) where T: BasePanel
    {
        //面板名要与面板类型一致！！！！！！！！
        string panelName = typeof(T).Name;
        if (this.panelDic.ContainsKey(panelName))
        {
            PanelInfo<T> panelInfo = panelDic[panelName] as PanelInfo<T>;
            if (panelInfo.panel == null)
            {
                panelInfo.callback += callback;
            }
            else if(!panelInfo.isHide)
            {
                callback?.Invoke(panelInfo.panel);
            }

        }
    }

    /// <summary>
    /// 为控件添加自定义事件
    /// </summary>
    /// <param name="control">对应的控件</param>
    /// <param name="type">事件的类型</param>
    /// <param name="callBack">响应的函数</param>
    public static void AddCustomEventListener(UIBehaviour control, EventTriggerType type, UnityAction<BaseEventData> callback)
    {
        EventTrigger trigger = control.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = control.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(callback);

        trigger.triggers.Add(entry);
    }

}
