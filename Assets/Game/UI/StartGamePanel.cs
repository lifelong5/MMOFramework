using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartGamePanel : BaseUI
{
    public override void HideUI()
    {
    }

    public override void ShowUI()
    {
    }
    protected override void OnButtonClick(string controlName)
    {
        base.OnButtonClick(controlName);
        if(controlName == "StartGameBtn")
        {
            Debug.Log("StartGameBtn");
        }
        else if(controlName == "SettingBtn")
        {
            Debug.Log("SettingBtn");
        }
    }
}
