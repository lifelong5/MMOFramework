using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ABResManager : Singleton<ABResManager>
{
    private ABResManager() { }

    public void LoadResAsync<T>(string abName, string resName, UnityAction<T> callback, bool isSync, bool debug) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (debug)
        {
            //”√editor
            T obj = EditorResManager.Instance.LoadEditorRes<T>($"{abName}/{resName}");
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
