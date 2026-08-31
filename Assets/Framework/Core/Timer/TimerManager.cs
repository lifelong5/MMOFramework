using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class TimerManager : Singleton<TimerManager>
{
    private int TIMER_ID = 0;//唯一的计时器id

    private Dictionary<int, TimerItem> timerDic = new Dictionary<int, TimerItem>();
    private Dictionary<int, TimerItem> realTimerDic = new Dictionary<int, TimerItem>();//不受TimeScale影响的计时器
    /// <summary>
    /// 待删除的计时器
    /// </summary>
    private List<TimerItem> delList = new List<TimerItem>();
    /// <summary>
    /// 固定刷新的时间间隔 单位：s
    /// </summary>
    private static float intervalTime = 0.1f;

    /// <summary>
    /// 计时器协程
    /// </summary>
    private Coroutine timerCoro;
    private Coroutine realTimeCoro;

    WaitForSeconds timeWait = new WaitForSeconds(intervalTime);
    WaitForSecondsRealtime realTimeWait = new WaitForSecondsRealtime(intervalTime);
    private TimerManager() {
        Start();
    }
    public void Start()
    {
        //开启计时协程
        Coroutine timerCoro = Mono.Instance.StartCoroutine(StartTiming(false, timerDic));
        Coroutine realTimeCoro = Mono.Instance.StartCoroutine(StartTiming(true, realTimerDic));
    }

    public void Stop()
    {
        //停止计时协程
        Mono.Instance.StopCoroutine(timerCoro);
        Mono.Instance.StopCoroutine(realTimeCoro);
    }

    /// <summary>
    /// 计时器协程
    /// </summary>
    /// <returns></returns>
    IEnumerator StartTiming(bool isRealTime, Dictionary<int, TimerItem> timerDic)
    {
        while (true)
        {
            if (!isRealTime)
            {
                yield return timeWait;
            }
            else
            {
                yield return realTimeWait;
            }
            //更新计时器数据
            foreach(TimerItem timer in timerDic.Values)
            {
                if (!timer.isRunning) continue;
                //间隔
                if (timer.callback!=null)
                {
                    timer.intervalTime -= (int)(intervalTime * 1000);
                    if(timer.intervalTime <= 0)
                    {
                        timer.ResetIntervalTime();
                        timer.callback.Invoke();
                    }
                }
                timer.allTime -= (int)(intervalTime * 1000);
                if(timer.allTime <= 0)
                {
                    timer.ResetIntervalTime();
                    timer.overCallback.Invoke();
                    timer.isRunning = false;
                    delList.Add(timer);
                }
            }
            //移除结束计时器
            for(int i = 0; i < delList.Count; i++)
            {
                timerDic.Remove(delList[i].TimerID);
                PoolManager.Instance.putObject<TimerItem>(delList[i]);//放回缓存池子
            }
            delList.Clear();
        }
    }

    /// <summary>
    /// 创建单个计时器
    /// </summary>
    /// <param name="allTime"></param>
    /// <param name="overCallback"></param>
    /// <param name="intervalTime"></param>
    /// <param name="callback"></param>
    /// <returns>唯一计时器id</returns>
    public int CreateTimer(bool isRealTime, int allTime, UnityAction overCallback, int intervalTime = 0, UnityAction callback = null)
    {
        int key = ++TIMER_ID;//自增id
        TimerItem timer = PoolManager.Instance.getObject<TimerItem>();//从对象池中获取
        timer.Init(key, allTime, overCallback, intervalTime, callback);//初始化
        if(!isRealTime)
            timerDic.Add(key, timer);
        else
            realTimerDic.Add(key, timer);
        return key;
    }

    /// <summary>
    /// 移除单个计时器
    /// </summary>
    /// <param name="timerID"></param>
    public void RemoveTimer(int timerID)
    {
        if (timerDic.ContainsKey(timerID))
        {
            PoolManager.Instance.putObject<TimerItem>(timerDic[timerID]);//放入缓存池子中
            timerDic.Remove(timerID);//从字典移除
        }else if (realTimerDic.ContainsKey(timerID))
        {
            PoolManager.Instance.putObject<TimerItem>(realTimerDic[timerID]);//放入缓存池子中
            realTimerDic.Remove(timerID);//从字典移除
        }
    }

    /// <summary>
    /// 重置计时器
    /// </summary>
    /// <param name="timerID"></param>
    public void ResetTimer(int timerID)
    {
        if (timerDic.ContainsKey(timerID))
        {
            timerDic[timerID].ResetTime();
        }
        else if (realTimerDic.ContainsKey(timerID))
        {
            realTimerDic[timerID].ResetTime();
        }
    }

    /// <summary>
    /// 开启单个计时器
    /// </summary>
    /// <param name="timerID"></param>
    public void StartTimer(int timerID)
    {
        if (timerDic.ContainsKey(timerID))
        {
            timerDic[timerID].isRunning = true;
        }
        else if (realTimerDic.ContainsKey(timerID))
        {
            realTimerDic[timerID].isRunning = true;
        }
    }

    /// <summary>
    /// 关闭单个计时器
    /// </summary>
    /// <param name="timerID"></param>
    public void StopTimer(int timerID)
    {
        if (timerDic.ContainsKey(timerID))
        {
            timerDic[timerID].isRunning = false;
        }
        else if (realTimerDic.ContainsKey(timerID))
        {
            realTimerDic[timerID].isRunning = false;
        }
    }
}
