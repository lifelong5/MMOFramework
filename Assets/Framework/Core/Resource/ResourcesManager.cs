using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 和事件管理器中的一样的为了使用泛型而设计的
/// </summary>
public abstract class ResInfoBase
{}
public class ResInfo<T>:ResInfoBase
{
    public T asset;//加载成功的资源
    public UnityAction<T> callback;//资源加载完成之后的回调
    public Coroutine coroutine;//加载资源的协程
}
public class ResourcesManager : Singleton<ResourcesManager>
{
    /// <summary>
    /// 存放加载的资源的信息 key:资源路径_资源类型 避免资源同名不同类型的情况
    /// </summary>
    private Dictionary<string, ResInfoBase> resDic = new Dictionary<string, ResInfoBase>();
    private ResourcesManager() { }
    /// <summary>
    /// 异步加载Resources资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="callback"></param>
    public void loadAsync<T>(string path,UnityAction<T> callback) where T:UnityEngine.Object
    {
        string resName = path + typeof(T).Name;
        if (!resDic.ContainsKey(resName))
        {
            //没有加载过这个资源
            ResInfo<T> info = new ResInfo<T>();
            resDic.Add(resName, info);
            //这些数据是无论是否加载成功都会调用的
            info.callback += callback;//存储加载完成回调
            info.coroutine = Mono.Instance.StartCoroutine(reallyLoadAsync<T>(path));
        }
        else
        {
            //加载过
            ResInfo<T> info = resDic[resName] as ResInfo<T>;
            if(info.asset == null)
            {
                //没加载完
                //!!!没有加载完的只需要添加加载完成的处理委托就好
                info.callback += callback;
            }
            else
            {
                //加载完,直接使用资源
                callback?.Invoke(info.asset);
            }
        }
    }
    /// <summary>
    /// 加载Resouces资源的协程
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    private IEnumerator reallyLoadAsync<T>(string path) where T:UnityEngine.Object
    {
        ResourceRequest rq = Resources.LoadAsync<T>(path);
        yield return rq;
        string resName = path + typeof(T).Name;
        if (resDic.ContainsKey(resName))
        {
            //加载成功存储加载成功的资源
            T asset = rq.asset as T;
            ResInfo<T> info = resDic[resName] as ResInfo<T>;
            //调用回调们
            info.callback?.Invoke(asset);
            //加载成功后这些就不要了
            info.callback = null;
            info.coroutine = null;
        }
    }
    /// <summary>
    /// 同步加载Resources资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T load<T>(string path) where T : UnityEngine.Object
    {
        string resName = path + typeof(T).Name;
        ResInfo<T> info = null;
        if (!resDic.ContainsKey(resName))
        {
            //没有加载过
            info = new ResInfo<T>();
            T asset = Resources.Load<T>(path);
            info.asset = asset;
            resDic.Add(resName, info);
            return asset;
        }
        else
        {
            info = resDic[resName] as ResInfo<T>;
            if (info.asset == null)
            {
                //异步加载正在进行中
                Mono.Instance.StopCoroutine(info.coroutine);
                info.asset = Resources.Load<T>(path);
                info.callback?.Invoke(info.asset);//调用异步加载成功的回调函数
                info.callback = null;
                info.coroutine = null;
                return info.asset as T;
            }
            else
            {
                //加载完毕
                return info.asset as T;
            }
        }
        
    }
    //现在不知道资源是否正在使用中 也不知道资源是否使用中 无法正确的卸载
    /// <summary>
    /// 卸载指定资源
    /// </summary>
    /// <param name="asset"></param>
    public void unloadAsset(UnityEngine.Object asset)
    {
        Resources.UnloadAsset(asset);
    }
    /// <summary>
    /// 卸载没有引用的资源
    /// </summary>
    public void unloadUnusedAssets()
    {
        Resources.UnloadUnusedAssets();
    }
}
