using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    private void Awake()
    {
        UIManager.Instance.init();
    }
    private void Start()
    {
        UIManager.Instance.ShowPanel<StartGamePanel>(E_UILayer.MiddleLayer,(panel) =>{
            UIManager.AddCustomEventListener(panel.GetControl<Button>("StartGameBtn"), EventTriggerType.PointerEnter, (data) =>
            {
                Debug.Log("½øÈë°´Å¥");
            });
        });
        
        //UIManager.Instance.HidePanel<StartGamePanel>();
        //UIManager.Instance.ShowPanel<StartGamePanel>(E_UILayer.MiddleLayer, (panel) => { });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            UIManager.Instance.ShowPanel<StartGamePanel>(E_UILayer.MiddleLayer, (panel) => { });
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            UIManager.Instance.HidePanel<StartGamePanel>(true);
        }
    }
}
