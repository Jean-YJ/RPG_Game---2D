using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 计时器对象 里面存储了计时器的相关数据
/// </summary>
public class TimerItem : IPoolClass
{
    /// <summary>
    /// 计时器对象的唯一ID
    /// </summary>
    public int tID;

    /// <summary>
    /// 计时结束后的回调函数
    /// </summary>
    public UnityAction completed;
    /// <summary>
    /// 间隔一定时间去执行的回调函数
    /// </summary>
    public UnityAction interval;

    /// <summary>
    /// 总计时时长 毫秒 毫秒：1s = 1000ms
    /// </summary>
    public int totalDuration;
    //记录总时长，用于计时器重置
    public int recordTotalDuration;

    /// <summary>
    /// 间隔执行回调的时间 毫秒 毫秒：1s = 1000ms
    /// </summary>
    public int intervalDuration;
    //记录间隔时长，用于计时器重置
    public int recordIntervalDuration;

    /// <summary>
    /// 是否在进行计时
    /// </summary>
    public bool isRuning;

    /// <summary>
    /// 初始化计时器数据
    /// </summary>
    /// <param name="id">唯一ID</param>
    /// <param name="total">总计时时长</param>
    /// <param name="completeAction">计时结束后的函数</param>
    /// <param name="inter">间隔执行回调的时间</param>
    /// <param name="intervalAction">间隔一定时间去执行的委托</param>
    public void Init(int id, int total, UnityAction completeAction, int inter = 0, UnityAction intervalAction = null)
    {
        this.tID = id;
        this.totalDuration = this.recordTotalDuration = total;
        this.intervalDuration = this.recordIntervalDuration = inter;
        this.completed = completeAction;
        this.interval = intervalAction;
        this.isRuning = true;
    }

    /// <summary>
    /// 重置计时器
    /// </summary>
    public void ResetTimer()
    {
        this.totalDuration = this.recordTotalDuration;
        this.intervalDuration = this.recordIntervalDuration;
        this.isRuning = true;
    }

    public void ResetInfo()
    {
        completed = null;
        interval = null;
        this.isRuning = false;
    }
}
