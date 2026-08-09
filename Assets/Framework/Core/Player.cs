using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.addEventListener<Monster>(E_EventType.E_Monster_Dead, MonsterDead);
        EventManager.Instance.addEventListener(E_EventType.E_Player_Dead, MonsterDead2);
    }
    public void MonsterDead(Monster monster)
    {
        Debug.Log("player get monster dead"+monster.name);
    }
    public void MonsterDead2()
    {
        Debug.Log("player get monster dead");
    }
    private void OnDestroy()
    {
        EventManager.Instance.removeEventListener<Monster>(E_EventType.E_Monster_Dead, MonsterDead);
        EventManager.Instance.removeEventListener(E_EventType.E_Player_Dead, MonsterDead2);
    }
}
