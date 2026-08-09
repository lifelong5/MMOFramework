using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
public class EventInfoBase{}
/// <summary>
/// 有参数的委托的类
/// </summary>
/// <typeparam name="T"></typeparam>
public class EventInfo<T> : EventInfoBase
{
    public Action<T> actions;
    public EventInfo(Action<T> action)
    {
        actions += action;
    }
}
/// <summary>
/// 没有参数的委托的类
/// </summary>
public class EventInfo : EventInfoBase
{
    public Action actions;
    public EventInfo(Action action)
    {
        actions += action;
    }
}
/// <summary>
/// 时间中心
/// </summary>
public class EventManager : Singleton<EventManager>
{
    /// <summary>
    /// E_EventType 用来避免发生事件拼写错误 封装EventInfoBase是为了不在EventManager层面使用泛型 而是针对委托管理做一层封装 这样就可以通用多种类型的参数
    /// </summary>
    private Dictionary<E_EventType, EventInfoBase> eventDic = new Dictionary<E_EventType, EventInfoBase>();
    private EventManager() { }
    /// <summary>
    /// 有参数的事件触发
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="name">事件名字</param>
    /// <param name="info">参数</param>
    public void eventTrigger<T>(E_EventType name,T info)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo<T>).actions?.Invoke(info);
        }
    }
    public void eventTrigger(E_EventType name)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo).actions?.Invoke();
        }
    }
    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public void addEventListener<T>(E_EventType name,Action<T> action)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo<T>).actions += action;
        }
        else
        {
            eventDic.Add(name, new EventInfo<T>(action));
        }
    }
    public void addEventListener(E_EventType name, Action action)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo).actions += action;
        }
        else
        {
            eventDic.Add(name, new EventInfo(action));
        }
    }
    /// <summary>
    /// 移除事件监听
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public void removeEventListener<T>(E_EventType name,Action<T> action)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo<T>).actions -= action;
        }
    }
    public void removeEventListener(E_EventType name, Action action)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo).actions -= action;
        }
    }
    public void clear()
    {
        eventDic.Clear();
    }

    public void clear(E_EventType name)
    {
        eventDic.Remove(name);
    }
}
