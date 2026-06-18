using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public enum E_EventType
{
    /// <summary>
    /// 怪物死亡  -- 参数类型：Monster
    /// </summary>
    E_MonsterDead,
    /// <summary>
    /// 增加计数  -- 参数类型：int
    /// </summary>
    E_AddCount,
    /// <summary>
    /// 测试  -- 参数类型：无参
    /// </summary>
    E_Test,
    /// <summary>
    /// 更新加载进度  -- 参数类型：float
    /// </summary>
    E_UpdateSceneLoad,
    /// <summary>
    /// 键盘按键检测  -- 参数类型：KeyCode
    /// </summary>
    //E_Input_KeyBoard,
    /// <summary>
    /// 鼠标按键检测  -- 参数类型：int
    /// </summary>
    //E_Input_Mouse,
    /// <summary>
    /// 水平热键检测  -- 参数类型：float
    /// </summary>
    E_Input_Horizontal,
    /// <summary>
    /// 垂直热键检测  -- 参数类型：float
    /// </summary>
    E_Input_Vertical,
    /// <summary>
    /// 技能行为检测，被输入事件触发
    /// </summary>
    E_Skill,
    /// <summary>
    /// 攻击行为检测，被输入事件触发
    /// </summary>
    E_Attack,
    /// <summary>
    /// 移动行为检测，被输入事件触发
    /// </summary>
    E_Move,
}

/// <summary>
/// 用于 里式替换原则 装载 子类的父类
/// </summary>
public abstract class EventInfoBase{ }

/// <summary>
/// 用来包裹 对应观察者 有参函数委托的 类
/// </summary>
/// <typeparam name="T"></typeparam>
public class EventInfo<T> : EventInfoBase
{
    public UnityAction<T> actions;

    public EventInfo(UnityAction<T> action)
    {
        actions += action;
    }
}


/// <summary>
/// 主要用来记录无参无返回值委托
/// </summary>
public class EventInfo : EventInfoBase
{
    public UnityAction actions;

    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}


/// <summary>
/// 事件中心模块 
/// </summary>
public class EventCenter : BaseSingleton<EventCenter>
{
    //key为事件名，value为关心该事件的模块在事件触发后所要进行的操作逻辑
    private Dictionary<E_EventType, EventInfoBase> eventDic = new Dictionary<E_EventType, EventInfoBase>();
    private EventCenter() { }

    /// <summary>
    /// 触发无参事件 
    /// </summary>
    /// <param name="eventName">事件名字</param>
    public void EventTrigger(E_EventType eventName)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo).actions?.Invoke();
        }
    }
    /// <summary>
    /// 触发有参事件 
    /// </summary>
    /// <param name="eventName">事件名字</param>
    public void EventTrigger<T>(E_EventType eventName,T info)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T>).actions?.Invoke(info);
        }
    }
    /// <summary>
    /// 添加无参事件监听者
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="action"></param>
    public void AddEventListener(E_EventType eventName, UnityAction action) 
    {
        //存储容器中已记录该事件，将触发后进行的操作函数添加进容器中对应的事件下
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo).actions += action;
        }
        //不存在则先在容器中记录事件再添加操作函数
        else
        {
            eventDic.Add(eventName, new EventInfo(action));
        }
    }
    /// <summary>
    /// 添加有参事件监听者
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="action"></param>
    public void AddEventListener<T>(E_EventType eventName, UnityAction<T> action)
    {
        //存储容器中已记录该事件，将触发后进行的操作函数添加进容器中对应的事件下
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T>).actions += action;
        }
        //不存在则先在容器中记录事件再添加操作函数
        else
        {
            eventDic.Add(eventName, new EventInfo<T>(action));
        }
    }
    /// <summary>
    /// 移除事件监听者
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="action"></param>
    public void RemoveEventListener(E_EventType eventName, UnityAction action)
    {
        if (eventDic.ContainsKey(eventName))
            (eventDic[eventName] as EventInfo).actions -= action;
    }

    /// <summary>
    /// 移除有参事件监听者
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="action"></param>
    public void RemoveEventListener<T>(E_EventType eventName, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(eventName))
            (eventDic[eventName] as EventInfo<T>).actions -= action;
    }

    /// <summary>
    /// 清空所有事件的监听
    /// </summary>
    public void Clear()
    {
        eventDic.Clear();
    }
    /// <summary>
    /// 清除指定某一个事件的所有监听
    /// </summary>
    /// <param name="eventName"></param>
    public void Clear(E_EventType eventName) 
    {
        if(!eventDic.ContainsKey(eventName))
            eventDic.Remove(eventName);
    }
}
