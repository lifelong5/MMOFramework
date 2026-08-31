using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 计时器数据类
/// </summary>
public class TimerItem : IPoolClass
{
    private int timerID;
    public UnityAction overCallback;//计时结束回调
    public UnityAction callback;//间隔时间回调

    /// <summary>
    /// 总时间 毫秒 1s = 1000ms
    /// </summary>
    public int allTime;
    /// <summary>
    /// 用于记录总时间
    /// </summary>
    private int maxAllTime;

    /// <summary>
    /// 间隔时间 毫秒 1s = 1000ms
    /// </summary>
    public int intervalTime;
    private int maxIntervalTime;

    /// <summary>
    /// 计时器是否开启
    /// </summary>
    public bool isRunning;
    public void Init(int timerID, int allTime, UnityAction overCallback, int intervalTime = 0, UnityAction callback = null)
    {
        this.timerID = timerID;
        this.maxAllTime = this.allTime = allTime;
        this.overCallback = overCallback;
        this.maxIntervalTime = this.intervalTime = intervalTime;
        this.callback = callback;
        this.isRunning = true;
    }
    public int TimerID
    {
        get
        {
            return this.timerID;
        }
    }
    /// <summary>
    /// 重新计时
    /// </summary>
    public void ResetTime()
    {
        this.intervalTime = this.maxIntervalTime;
        this.allTime = this.maxAllTime;
        this.isRunning = true;
    }
    public void ResetAllTime()
    {
        this.allTime = this.maxAllTime;
    }
    public void ResetIntervalTime()
    {
        this.intervalTime = this.maxIntervalTime;
    }
    public void Reset()
    {
        overCallback = null;
        callback = null;
    }
}
