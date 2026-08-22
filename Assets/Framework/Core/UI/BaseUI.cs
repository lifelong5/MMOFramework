using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// UI基类
/// 需要重写ShowUI、HideUI类 如果是需要有逻辑处理的组件不可以重名
/// </summary>
public abstract class BaseUI : MonoSingleton<BaseUI>
{
    private Dictionary<string, UIBehaviour> controlDic = new Dictionary<string, UIBehaviour>();//用于存储Panel下的组件对象 key:对象的名字 value:组件

    /// <summary>
    /// 如果是没有修改名称的节点 就表示不会使用掉 不用进行识别
    /// </summary>
    private static string[] defaultUIName = { "Image", "Text (TMP)", "RawImage", "Background", "Checkmark", "Label", "Text (Legacy)", "Arrow", "Placeholder", "Fill", "Handle", "Viewport", "Scrollbar", "Scrollbar Horizontal", "Scrollbar Vertical" };

    private void Awake()
    {
        //可能会存在一个对象有多个组件的 优先匹配比较重要的组件 不重要的可以通过重要组件获取
        FindChirldrenControl<Button>();
        FindChirldrenControl<Toggle>();
        FindChirldrenControl<Slider>();
        FindChirldrenControl<InputField>();
        FindChirldrenControl<ScrollRect>();
        FindChirldrenControl<Dropdown>();
        FindChirldrenControl<Text>();
        FindChirldrenControl<TextMeshProUGUI>();
        FindChirldrenControl<Image>();
    }
    /// <summary>
    /// 显示相关的函数 子类中必须重写
    /// </summary>
    public abstract void ShowUI();
    /// <summary>
    /// 隐藏相关的函数 子类中必须重写
    /// </summary>
    public abstract void HideUI();
    /// <summary>
    /// 按钮的点击回调 虚函数 子类可以重写
    /// </summary>
    /// <param name="controlName">将节点名称传出去</param>
    protected virtual void OnButtonClick(string controlName){}
    /// <summary>
    /// 滑动条的回调函数
    /// </summary>
    /// <param name="controlName">节点的名字</param>
    /// <param name="value">当前的滑动条的值</param>
    protected virtual void OnSliderValueChanged(string controlName,float value) { }
    /// <summary>
    /// Toggle的回调函数
    /// </summary>
    /// <param name="controlName"></param>
    /// <param name="value"></param>
    protected virtual void OnToggleValueChanged(string controlName, bool value) { }
    private void FindChirldrenControl<T>() where T:UIBehaviour
    {
        T[] controls = this.GetComponentsInChildren<T>();
        foreach(T control in controls)
        {
            string controlName = control.gameObject.name;
            //没有重复添加
            if (!controlDic.ContainsKey(controlName))
            {
                //是需要使用的组件
                if (!defaultUIName.Contains(controlName))
                {
                    controlDic.Add(controlName, control);
                }
                if(control is Button)
                {
                    (control as Button).onClick.AddListener(() =>
                    {
                        OnButtonClick(controlName);
                    });
                }else if(control is Slider)
                {
                    (control as Slider).onValueChanged.AddListener((value) =>
                    {
                        OnSliderValueChanged(controlName, value);
                    });
                }else if(control is Toggle)
                {
                    (control as Toggle).onValueChanged.AddListener((value) =>
                    {
                        OnToggleValueChanged(controlName, value);
                    });
                }
            }
        }
    }
    /// <summary>
    /// 获取某个组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T GetControl<T>(string name) where T:UIBehaviour
    {
        if (controlDic.ContainsKey(name))
        {
            T control = controlDic[name] as T;
            if(control != null)
            {
                return control;
            }
        }
        return null;
    }
}
