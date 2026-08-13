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
    /// <summary>
    /// 同步加载ab包
    /// </summary>
    /// <param name="abName"></param>
    public void LoadAB(string abName)
    {
        //加载依赖AB包
        if(mainAB == null)
        {
            mainAB = AssetBundle.LoadFromFile(abPath + mainABName);
            manifestAB = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        string[] dependencies = manifestAB.GetAllDependencies(abName);
        foreach(string name in dependencies)
        {
            LoadAB(name);
        }
        if (!abDic.ContainsKey(abName))
        {
            AssetBundle ab = AssetBundle.LoadFromFile(abPath + abName);
            abDic.Add(abName, ab);
        }
    }
    //同步加载
    /// <summary>
    /// 同步加载AB包的资源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <returns></returns>
    public UnityEngine.Object LoadRes(string abName,string resName)
    {
        LoadAB(abName);
        AssetBundle ab = abDic[abName];
        return ab.LoadAsset(resName);
    }
    public UnityEngine.Object LoadRes(string abName, string resName, System.Type type)
    {
        LoadAB(abName);
        AssetBundle ab = abDic[abName];
        return ab.LoadAsset(resName,type);
    }
    public T LoadRes<T>(string abName, string resName) where T:UnityEngine.Object
    {
        LoadAB(abName);
        AssetBundle ab = abDic[abName];
        return ab.LoadAsset<T>(resName);
    }
    //异步加载 这里先写的是同步加载AB包 再异步加载指定的资源
    /// <summary>
    /// 异步加载AB包中的资源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="callback"></param>
    public void LoadResAsync(string abName, string resName,UnityAction<UnityEngine.Object> callback)
    {
        Mono.Instance.StartCoroutine(reallyLoadAsync(abName, resName, callback));
    }
    private IEnumerator reallyLoadAsync(string abName, string resName, UnityAction<UnityEngine.Object> callback)
    {
        LoadAB(abName);
        AssetBundle ab = abDic[abName];
        AssetBundleRequest abr = ab.LoadAssetAsync(resName);
        yield return abr;
        callback?.Invoke(abr.asset);
    }
    public void LoadResAsync(string abName, string resName, System.Type type, UnityAction<UnityEngine.Object> callback)
    {
        Mono.Instance.StartCoroutine(reallyLoadAsync(abName, resName, type, callback));
    }
    private IEnumerator reallyLoadAsync(string abName, string resName, System.Type type, UnityAction<UnityEngine.Object> callback)
    {
        LoadAB(abName);
        AssetBundle ab = abDic[abName];
        AssetBundleRequest abr = ab.LoadAssetAsync(resName,type);
        yield return abr;
        callback?.Invoke(abr.asset);
    }
    public void LoadResAsync<T>(string abName, string resName, UnityAction<T> callback) where T:UnityEngine.Object
    {
        Mono.Instance.StartCoroutine(reallyLoadAsync<T>(abName, resName, callback));
    }
    private IEnumerator reallyLoadAsync<T>(string abName, string resName, UnityAction<T> callback) where T : UnityEngine.Object
    {
        LoadAB(abName);
        AssetBundle ab = abDic[abName];
        AssetBundleRequest abr = ab.LoadAssetAsync<T>(resName);
        yield return abr;
        callback?.Invoke(abr.asset as T);
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
        AssetBundle.UnloadAllAssetBundles(unLoadAll);
        abDic.Clear();
        mainAB = null;
        manifestAB = null;
    }
}
