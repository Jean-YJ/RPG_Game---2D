using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//1.创建MonoMgr继承 自动挂载式的继承MonoBehaviour的单例模式基类
//2.实现Update、FixedUpdate、LateUpdate生命周期函数
//3.声明对应事件或委托用于存储外部函数，并提供添加移除方法，从而达到让不继承MonoBehaviour的脚本可以执行帧更新或定时更新的目的
//4.声明协同程序开启关闭函数，从而达到让不继承MonoBehaviour的脚本可以执行协同程序的目的


/// <summary>
/// 公共Mono模块管理器
/// </summary>
public class MonoMgr : AutoMonoSingleton<MonoMgr>
{
    public Action updateAction;
    public Action lateUpdateAction;
    public Action fixedUpdateAction;

    /// <summary>
    /// 添加Update帧更新监听函数
    /// </summary>
    /// <param name="action"></param>
    public void AddUpdateAction(Action action){updateAction += action;}
    /// <summary>
    /// 移除Update帧更新监听函数
    /// </summary>
    /// <param name="action"></param>
    public void RemoveUpdateAction(Action action) { updateAction -= action; }

    /// <summary>
    /// 添加LateUpdate帧更新监听函数
    /// </summary>
    /// <param name="action"></param>
    public void AddLateUpdateAction(Action action) { lateUpdateAction += action; }
    /// <summary>
    /// 移除LateUpdate帧更新监听函数
    /// </summary>
    /// <param name="action"></param>
    public void RemoveLateUpdateAction(Action action) { lateUpdateAction -= action; }

    /// <summary>
    /// 添加FixedUpdate帧更新监听函数
    /// </summary>
    /// <param name="action"></param>
    public void AddFixedUpdateAction(Action action) { fixedUpdateAction += action; }
    /// <summary>
    /// 移除FixedUpdate帧更新监听函数
    /// </summary>
    /// <param name="action"></param>
    public void RemoveFixedUpdateAction(Action action) { fixedUpdateAction -= action; }

    // Update is called once per frame
    void Update()
    {
        updateAction?.Invoke();
    }

    private void LateUpdate()
    {
        lateUpdateAction?.Invoke();
    }

    private void FixedUpdate()
    {
        fixedUpdateAction?.Invoke();
    }

    private void OnDestroy()
    {
        updateAction = null;
        lateUpdateAction = null;
        fixedUpdateAction = null;
    }
}
