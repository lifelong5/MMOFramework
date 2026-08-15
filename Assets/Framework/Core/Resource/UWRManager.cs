using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class UWRManager : MonoSingleton<UWRManager>
{
    /// <summary>
    /// 加载资源
    /// </summary>
    /// <typeparam name="T">string 文本文件 byte[] 二进制数据 texture 图片 AssetBundle ab包</typeparam>
    /// <param name="path"></param>
    /// <param name="callback">成功回调</param>
    /// <param name="failedCallback">失败回调</param>
    public void LoadRes<T>(string path,UnityAction<T> callback,UnityAction failedCallback) where T:class
    {
        StartCoroutine(ReallyLoadRes(path, callback, failedCallback));
    }
    private IEnumerator ReallyLoadRes<T>(string path, UnityAction<T> callback, UnityAction failedCallback) where T : class
    {
        Type type = typeof(T);
        UnityWebRequest webRequest = null;
        if(type == typeof(string) || type == typeof(byte[]))
        {
            webRequest = UnityWebRequest.Get("file://" + Application.streamingAssetsPath + path);
        }else if(type == typeof(Texture))
        {
            webRequest = UnityWebRequestTexture.GetTexture("file://" + Application.streamingAssetsPath + path);
        }
        else if (type == typeof(AssetBundle))
        {
            webRequest = UnityWebRequestAssetBundle.GetAssetBundle("file://" + Application.streamingAssetsPath + path);
        }
        else
        {
            failedCallback?.Invoke();
        }
        yield return webRequest.SendWebRequest() ;
        if(webRequest.result == UnityWebRequest.Result.Success)
        {
            if (type == typeof(string))
            {
                callback?.Invoke(webRequest.downloadHandler.text as T);
            }
            else if(type == typeof(byte[]))
            {
                callback?.Invoke(webRequest.downloadHandler.data as T);
            }
            else if (type == typeof(Texture))
            {
                callback?.Invoke(DownloadHandlerTexture.GetContent(webRequest) as T);
            }
            else if (type == typeof(AssetBundle))
            {
                callback?.Invoke(DownloadHandlerAssetBundle.GetContent(webRequest) as T);
            }
        }
        else
        {
            failedCallback?.Invoke();
        }
    }
}
