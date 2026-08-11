using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObjMove : MonoBehaviour
{
    public string poolName;
    private void OnEnable()
    {
        //StartCoroutine(destory());
    }

    IEnumerator destory()
    {
        yield return new WaitForSeconds(3f);
        PoolManager.Instance.putObject(poolName, gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        transform.Translate(10 * Time.deltaTime * Vector3.forward);
    }
}
