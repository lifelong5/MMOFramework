using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TestData : IPoolClass
{
    public int i = 0;
    public void Reset()
    {
        i = 0;
        Debug.Log("Reset当前i" + i);
    }
    public void Add()
    {
        i++;
        Debug.Log("Add当前i"+i);
    }
}
public class PoolTest : MonoBehaviour
{
    private TestData testData;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            testData = PoolManager.Instance.getObject<TestData>();
            testData.Add();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            PoolManager.Instance.putObject<TestData>(testData);
        }
    }
}
