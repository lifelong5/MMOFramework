using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ResourcesManager.Instance.loadAsync<GameObject>("Pool/Cube",(obj) =>
        {
            Debug.Log("加载第一次");
            GameObject.Instantiate(obj);
        });
        ResourcesManager.Instance.loadAsync<GameObject>("Pool/Cube", (obj) =>
        {
            Debug.Log("加载第二次");
            GameObject.Instantiate(obj);
        });
        GameObject.Instantiate(ResourcesManager.Instance.load<GameObject>("Pool/Cube"));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GameObject obj = PoolManager.Instance.getObject("Cube");
            obj.transform.position = Vector3.zero;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            GameObject obj = PoolManager.Instance.getObject("Sphere");
            obj.transform.position = Vector3.zero;
        }
    }
}
