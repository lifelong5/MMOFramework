using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    private string name = "monster";
    public void MonsterDead()
    {
        EventManager.Instance.eventTrigger<Monster>(E_EventType.E_Monster_Dead,this);
        EventManager.Instance.eventTrigger(E_EventType.E_Player_Dead);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MonsterDead();
        }
    }
}
