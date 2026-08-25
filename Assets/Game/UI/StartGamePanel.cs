using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartGamePanel : BaseUI
{
    public override void HideUI()
    {
        EventManager.Instance.removeEventListener<float>(E_EventType.E_SceneLoadChange, SceneLoadProgress);
    }

    public override void ShowUI()
    {
        EventManager.Instance.addEventListener<float>(E_EventType.E_SceneLoadChange, SceneLoadProgress);
    }
    private void SceneLoadProgress(float progress)
    {
        Debug.Log("当前的场景加载进度为" + progress);
    }
    protected override void OnButtonClick(string controlName)
    {
        base.OnButtonClick(controlName);
        if(controlName == "StartGameBtn")
        {
            Debug.Log("StartGameBtn");
            SceneMgr.Instance.LoadSceneAsync("Test", () =>
            {
                Debug.Log("加载Test场景成功");
            });
        }
        else if(controlName == "SettingBtn")
        {
            Debug.Log("SettingBtn");
        }
    }
}
