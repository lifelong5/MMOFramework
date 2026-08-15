using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AB包管理器
/// </summary>
public class ABManager : Singleton<ABManager>
{
    private ABManager() { }

    //用于管理已加载的AB包 避免一个AB包的多次加载
    private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();

    private AssetBundle mainAB = null;//主包
    private AssetBundleManifest manifestAB = null;//主包的配置文件
    //AB包的路径 方便后期修改
    public string abPath
    {
        get
        {
            return Application.streamingAssetsPath + "/";
        }
    }
    //主包的包名 根据目标平台的不同设置不同的主包名
    public string mainABName
    {
        get
        {
#if UNITY_IOS
        return "IOS";
#elif UNITY_ANDROID
        return "ANDROID";
#else
        return "PC";
#endif
        }
    }
    private void LoadMainAB()
    {
        if (mainAB == null)
        {
            mainAB = AssetBundle.LoadFromFile(abPath + mainABName);
            manifestAB = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
    }
    /// <summary>
    /// 同步加载ab包
    /// </summary>
    /// <param name="abName"></param>
    private IEnumerator ReadllyLoadAB(string abName,bool isSync)
    {
        //加载依赖AB包
        LoadMainAB();
        string[] dependencies = manifestAB.GetAllDependencies(abName);
        foreach(string name in dependencies)
        {
            //LoadAB(name);
            yield return ReadllyLoadAB(name, isSync);
        }
        if (!abDic.ContainsKey(abName))
        {
            //说明没有加载过
            if (isSync)
            {
                //同步加载
                AssetBundle ab = AssetBundle.LoadFromFile(abPath + abName);
                abDic.Add(abName, ab);
            }
            else
            {
                abDic.Add(abName, null);
                AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(abPath + abName);
                yield return abcr;
                abDic[abName] = abcr.assetBundle;
            }
        }
        else
        {
            //加载过
            if (abDic[abName] == null)
            {
                //异步加载中没有加载完
                while (abDic[abName] == null)
                {
                    yield return null;//一直等待直到异步加载完成
                }
            }
            //加载好了就不用什么操作了
        }
    }
    /// <summary>
    /// 异步或者同步加载AB包中的资源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="callback"></param>
    public void LoadRes(string abName, string resName,UnityAction<UnityEngine.Object> callback,bool isSync = true)
    {
        Mono.Instance.StartCoroutine(reallyLoadAsync(abName, resName, callback, isSync));
    }
    private IEnumerator reallyLoadAsync(string abName, string resName, UnityAction<UnityEngine.Object> callback, bool isSync)
    {
        yield return ReadllyLoadAB(abName,isSync);
        AssetBundle ab = abDic[abName];
        if (isSync)
        {
            //同步加载资源
            callback?.Invoke(ab.LoadAsset(resName));
        }
        else
        {
            AssetBundleRequest abr = ab.LoadAssetAsync(resName);
            yield return abr;
            callback?.Invoke(abr.asset);
        }
    }
    public void LoadRes(string abName, string resName, System.Type type, UnityAction<UnityEngine.Object> callback, bool isSync = true)
    {
        Mono.Instance.StartCoroutine(reallyLoadAsync(abName, resName, type, callback,isSync));
    }
    private IEnumerator reallyLoadAsync(string abName, string resName, System.Type type, UnityAction<UnityEngine.Object> callback, bool isSync)
    {
        yield return ReadllyLoadAB(abName, isSync);
        AssetBundle ab = abDic[abName];
        if (isSync)
        {
            //同步加载资源
            callback?.Invoke(ab.LoadAsset(resName,type));
        }
        else
        {
            AssetBundleRequest abr = ab.LoadAssetAsync(resName, type);
            yield return abr;
            callback?.Invoke(abr.asset);
        }
    }
    public void LoadRes<T>(string abName, string resName, UnityAction<T> callback, bool isSync = true) where T:UnityEngine.Object
    {
        Mono.Instance.StartCoroutine(reallyLoadAsync<T>(abName, resName, callback, isSync));
    }
    private IEnumerator reallyLoadAsync<T>(string abName, string resName, UnityAction<T> callback, bool isSync) where T : UnityEngine.Object
    {
        yield return ReadllyLoadAB(abName, isSync);
        AssetBundle ab = abDic[abName];
        if (isSync)
        {
            //同步加载资源
            callback?.Invoke(ab.LoadAsset<T>(resName));
        }
        else
        {
            AssetBundleRequest abr = ab.LoadAssetAsync<T>(resName);
            yield return abr;
            callback?.Invoke(abr.asset as T);
        }
    }
    /// <summary>
    /// 释放某个AB包
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="unLoadAll"></param>
    public void UnLoad(string abName,bool unLoadAll)
    {
        if (abDic.ContainsKey(abName))
        {
            if (abDic[abName] == null)
            {
                //异步加载中
                return;
            }
            abDic[abName].Unload(unLoadAll);
            abDic.Remove(abName);
            Debug.Log("卸载" + abName);
        }
    }
    /// <summary>
    /// 释放所有AB包
    /// </summary>
    /// <param name="unLoadAll"></param>
    public void UnLoadAllAssetBundle(bool unLoadAll)
    {
        Mono.Instance.StopAllCoroutines();
        AssetBundle.UnloadAllAssetBundles(unLoadAll);
        abDic.Clear();
        mainAB = null;
        manifestAB = null;
    }
}
