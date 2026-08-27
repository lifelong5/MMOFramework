using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

    //临时存储
    private InputInfo currentInputInfo;
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
    private void InputUpdate()
    {
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
