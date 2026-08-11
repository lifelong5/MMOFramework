using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public enum ResLoadState
{
    Loading,
    Success,
    None
}
/// <summary>
/// 和事件管理器中的一样的为了使用泛型而设计的
/// </summary>
public abstract class ResInfoBase
{
    public int refCount = 0;//当前资源引用计数
    public ResLoadState state;//当前资源的加载状态
}
public class ResInfo<T>:ResInfoBase
{
    public T asset;//加载成功的资源
    public UnityAction<T> callback;//资源加载完成之后的回调
    public Coroutine coroutine;//加载资源的协程
    public bool unloadImmediately = true;//立马卸载
    /// <summary>
    /// 引用计数加一
    /// </summary>
    public void AddRefCount()
    {
        ++refCount;
    }
    /// <summary>
    /// 引用计数减一
    /// </summary>
    public bool SubRefCount()
    {
        if (refCount <= 0)
        {
            Debug.LogError("检查资源加载和卸载是否匹配使用");
            return false;
        }
        --refCount;
        return true;
    }
}
public class ResourcesManager : Singleton<ResourcesManager>
{
    /// <summary>
    /// 存放加载的资源的信息 key:资源路径_资源类型 避免资源同名不同类型的情况
    /// </summary>
    private Dictionary<string, ResInfoBase> resDic = new Dictionary<string, ResInfoBase>();
    private ResourcesManager() { }
    /// <summary>
    /// 获取字典的Key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    private string GetResKey<T>(string path) where T:UnityEngine.Object
    {
        return path + "_" + typeof(T).Name;
    }
    /// <summary>
    /// 异步加载Resources资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="callback"></param>
    public void LoadAsync<T>(string path,UnityAction<T> callback) where T:UnityEngine.Object
    {
        string resName = GetResKey<T>(path);
        ResInfoBase baseInfo = null;
        if(resDic.TryGetValue(resName,out baseInfo))
        {
            //加载过
            ResInfo<T> info = baseInfo as ResInfo<T>;
            info.AddRefCount();//引用加一
            //还在加载中callback还没有执行
            if(info.state == ResLoadState.Loading)
            {
                if (callback != null) info.callback += callback;
                return;
            }
            //加载完毕直接调用回调函数返回对象
            if(info.state == ResLoadState.Success)
            {
                callback?.Invoke(info.asset);
                return;
            }
        }
        //处理没有加载的情况
        ResInfo<T> newInfo = new ResInfo<T>();
        newInfo.refCount = 1;
        newInfo.state = ResLoadState.Loading;
        if (callback != null) newInfo.callback += callback;
        resDic.Add(resName, newInfo);
        newInfo.coroutine = Mono.Instance.StartCoroutine(reallyLoadAsync<T>(path));
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
        string resName = GetResKey<T>(path);
        if (resDic.ContainsKey(resName))
        {
            T asset = rq.asset as T;
            ResInfo<T> info = resDic[resName] as ResInfo<T>;
            if (asset != null)
            {
                //加载成功 存储加载成功的资源
                info.asset = rq.asset as T;
                info.state = ResLoadState.Success;
                info.coroutine = null;
                if (info.refCount <= 0)
                {
                    //异步加载还没完成就已经卸载了的话 卸载资源
                    info.callback = null;//回调不用执行
                    resDic.Remove(resName);//从字典移除
                    UnloadResource(info.asset);
                    info.asset = null;
                    yield break;
                }
                else
                {
                    //调用回调们
                    info.callback?.Invoke(info.asset);
                    //加载成功后这些就不要了
                    info.callback = null;
                }
            }
            else
            {
                //加载失败
                info.asset = null;
                info.coroutine = null;
                Debug.LogError($"Resources异步加载失败，path:{path}");
                info.callback?.Invoke(null);//加载失败 执行回调 回调中需要处理加载失败的情况
                info.callback = null;
                //移除加入字典的数据
                resDic.Remove(resName);
                yield break;
            }

        }
    }
    /// <summary>
    /// 同步加载Resources资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T Load<T>(string path) where T : UnityEngine.Object
    {
        string resName = GetResKey<T>(path);
        ResInfoBase baseinfo = null;
        if(resDic.TryGetValue(resName,out baseinfo))
        {
            //加载过
            ResInfo<T> info = resDic[resName] as ResInfo<T>;
            info.AddRefCount();//引用加一
            if(info.state == ResLoadState.Success)
            {
                return info.asset;
            }
            //当前正在异步加载
            if(info.state == ResLoadState.Loading)
            {
                if (info.coroutine != null)
                {
                    Mono.Instance.StopCoroutine(info.coroutine);
                    info.coroutine = null;
                }
                T asset = Resources.Load<T>(path);
                if(asset == null)
                {
                    //同步加载失败 直接移除
                    info.asset = null;
                    info.callback?.Invoke(null);//加载失败回调 是转同步之后失败异步不一定失败
                    info.callback = null;
                    Debug.LogError($"Resources异步转同步加载失败，path:{path}");
                    resDic.Remove(resName);
                    return null;
                }
                //加载成功
                info.asset = asset;
                info.callback?.Invoke(asset);
                info.callback = null;
                info.state = ResLoadState.Success;
                return info.asset;
            }
        }
        //没有加载过
        ResInfo<T> newInfo = new ResInfo<T>();
        T newAsset = Resources.Load<T>(path);
        if (newAsset == null)
        {
            Debug.LogError($"Resources同步加载失败，path:{path}");
            return null;
        }
        newInfo.asset = newAsset;
        newInfo.refCount = 1;
        newInfo.state = ResLoadState.Success;
        resDic.Add(resName, newInfo);
        return newAsset;
    }
    /// <summary>
    /// 释放引用 每调用一次 count-- 当为0的时候 如果是独立资源直接卸载 如果是其他的则交给UnloadUnusedAssets 失败之后不用Release。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    public void Release<T>(string path) where T:UnityEngine.Object
    {
        string resName = GetResKey<T>(path);
        ResInfoBase baseInfo = null;
        if(!resDic.TryGetValue(resName,out baseInfo))
        {
            Debug.LogError($"释放资源失败，资源不存在，path:{path}");
            return;
        }
        ResInfo<T> info = baseInfo as ResInfo<T>;
        if (!info.SubRefCount())
        {
            //没有引用
            return;
        }
        //如果减去引用之后还有引用就返回
        if (info.refCount > 0) return;
        //没有引用
        if(info.state == ResLoadState.Loading)
        {
            //同理不在这里处理 因为无法确认加载状态 让协程自己处理
            info.callback = null;
            return;
        }
        if(info.state == ResLoadState.Success)
        {
            resDic.Remove(resName);
            UnloadResource(info.asset);
            info.asset = null;
        }
    }
    /// <summary>
    /// 卸载指定资源 GameObject\Component\AssetBundle资源解除引用等UnloadUnusedAssets来回收
    /// </summary>
    /// <param name="asset"></param>
    public void UnloadResource(UnityEngine.Object asset)
    {
        if (asset == null) return;
        //GameObject\Component\AssetBundle不能直接UnloadAsset
        if (asset is GameObject || asset is Component || asset is AssetBundle)
        {
            return;
        }
        Resources.UnloadAsset(asset);
    }
    /// <summary>
    /// 卸载没有引用的资源
    /// </summary>
    public void UnloadUnusedAssets(UnityAction callback)
    {
        Mono.Instance.StartCoroutine(ReallyUnloadUnusedAssets(callback));
    }
    private IEnumerator ReallyUnloadUnusedAssets(UnityAction callback)
    {
        //移除没有引用的资源
        List<string> removeList = new List<string>();
        foreach (string name in resDic.Keys)
        {
            ResInfoBase info = resDic[name];
            if (info.refCount <= 0 &&
                info.state != ResLoadState.Loading) //如果正在加载的话说明协程还在进行 这个时候让协程自己去处理
            {
                removeList.Add(name);
            }
        }
        foreach (string name in removeList)
        {
            resDic.Remove(name);
        }
        AsyncOperation ao = Resources.UnloadUnusedAssets();
        yield return ao;
        callback?.Invoke();
    }

    /// <summary>
    /// 获取某个资源的引用计数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public int GetResourcesRefCount<T>(string path) where T : UnityEngine.Object
    {
        string resName = GetResKey<T>(path);
        if (resDic.ContainsKey(resName))
        {
            ResInfo<T> info = resDic[resName] as ResInfo<T>;
            return info.refCount;
        }
        return 0;
    }

    /// <summary>
    /// 获取某个资源的加载状态
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public ResLoadState GetLoadState<T>(string path) where T:UnityEngine.Object
    {
        string resName = GetResKey<T>(path);
        if (resDic.ContainsKey(resName))
        {
            ResInfo<T> info = resDic[resName] as ResInfo<T>;
            return info.state;
        }
        return ResLoadState.None;
    }
}
