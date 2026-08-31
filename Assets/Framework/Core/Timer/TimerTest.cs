using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerTest : MonoBehaviour
{
    private int TimerID;
    private void Start()
    {
        Time.timeScale = 0;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            TimerID = TimerManager.Instance.CreateTimer(true,5000, () =>
            {
                Debug.Log("5√Î");
            }, 1000, () =>
            {
                Debug.Log("√ø√Î");
            });
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            TimerManager.Instance.RemoveTimer(TimerID);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            TimerManager.Instance.StopTimer(TimerID);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            TimerManager.Instance.StartTimer(TimerID);
        }
    }
}
