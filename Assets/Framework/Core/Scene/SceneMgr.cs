using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneMgr : Singleton<SceneMgr>
{
    private SceneMgr() { }

    /// <summary>
    /// 同步切换场景 其实可以不用callback 但是想和异步保持统一
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="callback"></param>
    public void LoadScene(string sceneName,UnityAction callback)
    {
        SceneManager.LoadScene(sceneName);
        callback?.Invoke();//加载成功回调
    }

    /// <summary>
    /// 异步加载
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="callback"></param>
    public void LoadSceneAsync(string sceneName,UnityAction callback)
    {
        Mono.Instance.StartCoroutine(ReallyLoadSceneAsync(sceneName,callback));
    }
    private IEnumerator ReallyLoadSceneAsync(string sceneName,UnityAction callback)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
        //为了更新加载进度
        while (!ao.isDone)
        {
            EventManager.Instance.eventTrigger<float>(E_EventType.E_SceneLoadChange, ao.progress);
            yield return 0;
        }
        EventManager.Instance.eventTrigger<float>(E_EventType.E_SceneLoadChange, 1.0f);//保证完成的时候进度回传
        callback?.Invoke();
    }
}
