using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InputManager.Instance.StartInput(true);
        InputManager.Instance.ChangeKeyCode(E_EventType.E_Skill1, KeyCode.Q, InputInfo.InputType.Down);
        InputManager.Instance.ChangeKeyCode(E_EventType.E_Skill2, KeyCode.W, InputInfo.InputType.Up);
        InputManager.Instance.ChangeMouse(E_EventType.E_Skill3, 0, InputInfo.InputType.Down);

        EventManager.Instance.addEventListener(E_EventType.E_Skill1, () =>
        {
            Debug.Log("E_Skill1");
        });
        EventManager.Instance.addEventListener(E_EventType.E_Skill2, () =>
        {
            Debug.Log("E_Skill2");
        });
        EventManager.Instance.addEventListener(E_EventType.E_Skill3, () =>
        {
            Debug.Log("E_Skill3");
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            InputManager.Instance.StartChangeInput((data) =>
            {
                if(data.keycodeOrMouse == InputInfo.KeycodeOrMouse.Key)
                {
                    //改为按键
                    Debug.Log("改键成功" + data.keycode);
                    InputManager.Instance.ChangeKeyCode(E_EventType.E_Skill1, data.keycode, InputInfo.InputType.Down);
                }
                else
                {
                    //改为鼠标
                    Debug.Log("改键成功" + data.mouseID);
                    InputManager.Instance.ChangeMouse(E_EventType.E_Skill1, data.mouseID, InputInfo.InputType.Down);
                }
            });
        }
    }
}
