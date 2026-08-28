using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
public class InputInfo
{
    public enum KeycodeOrMouse
    {
        Key,
        Mouse
    }
    public enum InputType
    {
        Down,
        Up,
        Always
    }
    public KeycodeOrMouse keycodeOrMouse;//鼠标还是按键输入
    public InputType inputType;//当前输入的类型
    public KeyCode keycode;
    public int mouseID;
    public InputInfo(InputType type,KeyCode key)
    {
        this.keycodeOrMouse = KeycodeOrMouse.Key;
        this.keycode = key;
        this.inputType = type;
    }
    public InputInfo(InputType type, int mouseID)
    {
        this.keycodeOrMouse = KeycodeOrMouse.Mouse;
        this.mouseID = mouseID;
        this.inputType = type;
    }
}
public class InputManager : Singleton<InputManager>
{
    private bool openKeycode = false;
    private bool openMouse = false;
    private bool openAxis = false;
    private bool openInput = false;
    private Dictionary<E_EventType, InputInfo> inputDic = new Dictionary<E_EventType, InputInfo>();
    private UnityAction<InputInfo> inputCallback = null;
    private bool startChangeInput = false;

    //临时存储
    private InputInfo currentInputInfo;
    private Array keyCodeArr;
    private InputInfo callbackInputInfo;
    private bool changeInputSuccess = false;
    private InputManager()
    {
        Mono.Instance.onUpdate += InputUpdate;//添加帧刷新事件
    }
    /// <summary>
    /// 开启按键检测
    /// </summary>
    /// <param name="openKeycode"></param>
    public void StartKeyCode(bool openKeycode)
    {
        this.openKeycode = openKeycode;
    }
    /// <summary>
    /// 开启鼠标检测
    /// </summary>
    /// <param name="openMouse"></param>
    public void StartMouse(bool openMouse)
    {
        this.openMouse = openMouse;
    }
    /// <summary>
    /// 开启热键检测
    /// </summary>
    /// <param name="openAxis"></param>
    public void StartAxis(bool openAxis)
    {
        this.openAxis = openAxis;
    }
    /// <summary>
    /// 开启输入检测
    /// </summary>
    /// <param name="openInput"></param>
    public void StartInput(bool openInput)
    {
        this.openInput = openInput;
    }
    /// <summary>
    /// 开始等待自定义输入改键
    /// </summary>
    /// <param name="callback"></param>
    public void StartChangeInput(UnityAction<InputInfo> callback)
    {
        Debug.Log("开始等待输入");
        this.inputCallback = callback;//因为无法马上得到换键的输入 所以用回调来进行获取
        Mono.Instance.StartCoroutine(StartChange());
    }
    /// <summary>
    /// 协程 等待一帧后再进行输入检测
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartChange()
    {
        yield return null;
        startChangeInput = true;
    }
    private void InputUpdate()
    {
        if (startChangeInput)
        {
            if (inputCallback!= null)
            {
                if (Input.anyKeyDown)
                {
                    //如果有按键按下的话这一帧才检测
                    if (keyCodeArr == null)
                    {
                        keyCodeArr = Enum.GetValues(typeof(KeyCode));
                    }
                    foreach (KeyCode key in keyCodeArr)
                    {
                        if (Input.GetKeyDown(key))
                        {
                            callbackInputInfo = new InputInfo(InputInfo.InputType.Down, key);
                            break;
                        }
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        if (Input.GetMouseButtonDown(i))
                        {
                            callbackInputInfo = new InputInfo(InputInfo.InputType.Down, i);
                            break;
                        }
                    }
                    Debug.Log("检测到输入");
                    inputCallback.Invoke(callbackInputInfo);
                    inputCallback = null;
                    startChangeInput = false;
                    changeInputSuccess = true;
                }
            }
        }
        if (changeInputSuccess)
        {
            changeInputSuccess = false;
            return;
        }
        if (!openInput) return;
        foreach(E_EventType eventType in inputDic.Keys)
        {
            currentInputInfo = inputDic[eventType];
            if(currentInputInfo.keycodeOrMouse == InputInfo.KeycodeOrMouse.Key)
            {
                if (!openInput && !openKeycode) continue;
                //键盘输入
                switch (currentInputInfo.inputType)
                {
                    case InputInfo.InputType.Down:
                        {
                            if (Input.GetKeyDown(currentInputInfo.keycode))
                            {
                                EventManager.Instance.eventTrigger(eventType);
                            }
                            break;
                        }
                    case InputInfo.InputType.Up:
                        {
                            if (Input.GetKeyUp(currentInputInfo.keycode))
                            {
                                EventManager.Instance.eventTrigger(eventType);
                            }
                            break;
                        }
                    case InputInfo.InputType.Always:
                        {
                            if (Input.GetKey(currentInputInfo.keycode))
                            {
                                EventManager.Instance.eventTrigger(eventType);
                            }
                            break;
                        }
                }
            }else if(currentInputInfo.keycodeOrMouse == InputInfo.KeycodeOrMouse.Mouse)
            {
                if (!openInput && !openMouse) continue;
                //鼠标输入
                switch (currentInputInfo.inputType)
                {
                    case InputInfo.InputType.Down:
                        {
                            if (Input.GetMouseButtonDown(currentInputInfo.mouseID))
                            {
                                EventManager.Instance.eventTrigger(eventType);
                            }
                            break;
                        }
                    case InputInfo.InputType.Up:
                        {
                            if (Input.GetMouseButtonUp(currentInputInfo.mouseID))
                            {
                                EventManager.Instance.eventTrigger(eventType);
                            }
                            break;
                        }
                    case InputInfo.InputType.Always:
                        {
                            if (Input.GetMouseButton(currentInputInfo.mouseID))
                            {
                                EventManager.Instance.eventTrigger(eventType);
                            }
                            break;
                        }
                }
            }
        }
        if(openInput || openAxis)
        {
            EventManager.Instance.eventTrigger(E_EventType.E_Axis, Input.GetAxis("Horizontal"));
            EventManager.Instance.eventTrigger(E_EventType.E_Axis, Input.GetAxis("Vertical"));
        }
    }
    /// <summary>
    /// 修改成按键输入
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="key"></param>
    /// <param name="type"></param>
    public void ChangeKeyCode(E_EventType eventType,KeyCode key,InputInfo.InputType type)
    {
        if (!inputDic.ContainsKey(eventType))
        {
            inputDic.Add(eventType, new InputInfo(type, key));
        }
        else
        {
            inputDic[eventType].keycodeOrMouse = InputInfo.KeycodeOrMouse.Key;
            inputDic[eventType].keycode = key;
            inputDic[eventType].inputType = type;
        }
    }
    /// <summary>
    /// 修改成鼠标输入
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="mouseID"></param>
    /// <param name="type"></param>
    public void ChangeMouse(E_EventType eventType, int mouseID, InputInfo.InputType type)
    {
        if (!inputDic.ContainsKey(eventType))
        {
            inputDic.Add(eventType, new InputInfo(type, mouseID));
        }
        else
        {
            inputDic[eventType].keycodeOrMouse = InputInfo.KeycodeOrMouse.Mouse;
            inputDic[eventType].mouseID = mouseID;
            inputDic[eventType].inputType = type;
        }
    }
    /// <summary>
    /// 移除某个输入事件
    /// </summary>
    /// <param name="eventType"></param>
    public void RemoveInputEvent(E_EventType eventType)
    {
        if (inputDic.ContainsKey(eventType))
        {
            inputDic.Remove(eventType);
        }
    }
}
