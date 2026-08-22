using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ABResManager : Singleton<ABResManager>
{
    private bool isDebug = false;
    private ABResManager() { }

    public void LoadResAsync<T>(string abName, string resName, UnityAction<T> callback, bool isSync) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (isDebug)
        {
            //”√editor
            T obj = EditorResManager.Instance.LoadEditorRes<T>($"{abName}/{resName}");
            Debug.Log("obj:" + obj);
            callback?.Invoke(obj);
        }
        else
        {
            ABManager.Instance.LoadRes<T>(abName, resName, callback, isSync);
        }
#else
            ABManager.Instance.LoadRes<T>(abName, resName, callback, isSync);
#endif
    }
}
