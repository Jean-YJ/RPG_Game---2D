using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 计时器管理器 主要用于开启、停止、重置等等操作来管理计时器
/// </summary>
public class TimerMgr : BaseSingleton<TimerMgr>
{
    private TimerMgr() { StartTimer(); }

    /// <summary>
    /// 用于记录当前将要创建的唯一ID的
    /// </summary>
    private int TIMER_KEY = 0;

    //存储计时器的容器，key为TimerItem的唯一ID
    private Dictionary<int, TimerItem> timerDic = new Dictionary<int, TimerItem>();
    private Dictionary<int, TimerItem> fixedTimerDic = new Dictionary<int, TimerItem>();

    private List<TimerItem> deleteList = new List<TimerItem>();

    /// <summary>
    /// 计时器管理器中的唯一计时用的协同程序 的间隔时间
    /// </summary>
    private const float intervalTime = 0.1f;

    //为了避免内存的浪费 每次while都会生成 
    //我们直接将其声明为成员变量
    private WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(intervalTime);
    private WaitForSeconds waitForSeconds = new WaitForSeconds(intervalTime);

    private Coroutine timer;
    private Coroutine fixedTimer;

    /// <summary>
    /// 开启计时管理器
    /// </summary>
    public void StartTimer()
    {
        this.timer = MonoMgr.Instance.StartCoroutine(StartTiming(false, this.timerDic));
        this.fixedTimer = MonoMgr.Instance.StartCoroutine(StartTiming(true, this.fixedTimerDic));
    }

    IEnumerator StartTiming(bool isReal, Dictionary<int, TimerItem> timerDic)
    {
        while (true)
        {
            //100毫秒进行一次计时
            if(isReal)
                yield return waitForSecondsRealtime;
            else
                yield return waitForSeconds;

            foreach (TimerItem item in timerDic.Values)
            {
                //未运行的计时器直接跳过
                if (!item.isRuning)
                    continue;
                //存在间隔执行委托的需求
                if (item.interval != null)
                {
                    item.intervalDuration -= (int)(intervalTime * 1000);
                    //达到间隔时间
                    if (item.intervalDuration <= 0)
                    {
                        //执行委托
                        item.interval.Invoke();
                        //重置间隔时间
                        item.intervalDuration = item.recordIntervalDuration;
                    }
                }
                //计时总时长
                item.totalDuration -= (int)(intervalTime * 1000);
                //计时时间到 需要执行完成回调函数
                if (item.totalDuration <= 0)
                {
                    item.completed?.Invoke();
                    //放入待删除列表中
                    this.deleteList.Add(item);
                }
            }
            //移除待移除列表中的数据
            for (int i = 0; i < deleteList.Count; i++)
            {
                PoolMgr.Instance.PushClassObject(deleteList[i]);
                timerDic.Remove(deleteList[i].tID);
            }
            deleteList.Clear();
        }
    }
    /// <summary>
    /// 停止计时管理器
    /// </summary>
    public void StopTimer()
    {
        MonoMgr.Instance.StopCoroutine(this.timer);
        MonoMgr.Instance.StopCoroutine(this.fixedTimer);
    }

    /// <summary>
    /// 创建单个计时器
    /// </summary>
    /// <param name="isReal">是否不受到 TimeScale 的影响</param>
    /// <param name="total">总计时时长</param>
    /// <param name="completeAction">计时结束后的函数</param>
    /// <param name="interval">间隔执行回调的时间</param>
    /// <param name="intervalAction">间隔一定时间去执行的委托</param>
    /// <returns></returns>
    public int CreateTimer(bool isReal, int total, UnityAction completeAction, int interval = 0, UnityAction intervalAction = null)
    {
        int tID = ++this.TIMER_KEY;
        TimerItem item = PoolMgr.Instance.GetClassObject<TimerItem>();
        item.Init(tID, total, completeAction, interval, intervalAction);
        if (isReal)
            this.fixedTimerDic.Add(tID, item);
        else
            this.timerDic.Add(tID, item);
        return tID;
    }

    /// <summary>
    /// 移除单个计时器
    /// </summary>
    /// <param name="id">TimerItem对象对应的唯一ID</param>
    public void RemoveTimer(int id)
    {
        if (timerDic.ContainsKey(id))
        {
            PoolMgr.Instance.PushClassObject<TimerItem>(this.timerDic[id]);
            //this.timerDic.Remove(id);
            this.deleteList.Add(this.timerDic[id]);
        }
        else if (this.fixedTimerDic.ContainsKey(id))
        {
            PoolMgr.Instance.PushClassObject<TimerItem>(this.fixedTimerDic[id]);
            //this.fixedTimerDic.Remove(id);
            this.deleteList.Add(this.fixedTimerDic[id]);
        }
    }

    /// <summary>
    /// 重置单个计时器
    /// </summary>
    /// <param name="id">TimerItem对象对应的唯一ID</param>
    public void ResetTimer(int id)
    {
        if (timerDic.ContainsKey(id))
        {
            this.timerDic[id].ResetTimer();
        }
        else if (this.fixedTimerDic.ContainsKey(id))
        {
            this.fixedTimerDic[id].ResetTimer();
        }
    }

    /// <summary>
    /// 开启单个计时器(暂停后再启动)
    /// </summary>
    /// <param name="id">TimerItem对象对应的唯一ID</param>
    public void StartTimer(int id)
    {
        if (timerDic.ContainsKey(id))
        {
            this.timerDic[id].isRuning = true;
        }
        else if (this.fixedTimerDic.ContainsKey(id))
        {
            this.fixedTimerDic[id].isRuning = true;
        }
    }

    /// <summary>
    /// 暂停单个计时器
    /// </summary>
    /// <param name="id">TimerItem对象对应的唯一ID</param>
    public void PauseTimer(int id)
    {
        if (timerDic.ContainsKey(id))
        {
            this.timerDic[id].isRuning = false;
        }
        else if (this.fixedTimerDic.ContainsKey(id))
        {
            this.fixedTimerDic[id].isRuning = false;
        }
    }
}
