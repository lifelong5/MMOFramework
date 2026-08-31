using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PoolData
{
    //存储空闲的对象的池子
    private Stack<GameObject> poolStack = new Stack<GameObject>();
    //存储正在使用中的对象的池子
    private List<GameObject> usedList = new List<GameObject>();
    private int usedMax;

    public int emptyCount => poolStack.Count;
    public int usedCount => usedList.Count;
    public bool needCreate => usedList.Count < usedMax;

    //抽屉的父节点
    private GameObject rootObj;
    
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="root">对象池父节点</param>
    /// <param name="name">对象池节点的name</param>
    /// <param name="obj">对象</param>
    public PoolData(GameObject root,string name,GameObject obj)
    {
        if (PoolManager.Instance.useLayout)
        {
            rootObj = new GameObject(name);
            rootObj.transform.SetParent(root.transform);
        }
        pushUsed(obj);
        //从PoolObj组件上拿最大的对象数量
        PoolObj poolObj = obj.GetComponent<PoolObj>();
        if (poolObj == null)
        {
            Debug.LogError("请设置使用对象池对象的最大的对象池数量");
        }
        else
        {
            usedMax = poolObj.poolMax;
        }
    }

    /// <summary>
    /// 取出对象
    /// </summary>
    /// <returns></returns>
    public GameObject pop()
    {
        GameObject obj = null;
        //因为在外面筛选了 所以这里只用考虑这两种情况
        if(poolStack.Count > 0)
        {
            //有空闲的对象 直接拿空闲的
            obj = poolStack.Pop();
            pushUsed(obj);
        }
        else
        {
            //没有空闲的但是超过了能使用的最大限度 直接拿使用时间最长的
            obj = usedList[0];
            usedList.RemoveAt(0);
            usedList.Add(obj);
        }
        if (PoolManager.Instance.useLayout)
            obj.transform.SetParent(null);
        return obj;
    }

    /// <summary>
    /// 放回对象
    /// </summary>
    /// <param name="obj"></param>
    public void push(GameObject obj)
    {
        obj.SetActive(false);
        if (PoolManager.Instance.useLayout)
            obj.transform.SetParent(rootObj.transform);
        poolStack.Push(obj);
        usedList.Remove(obj);
    }

    /// <summary>
    /// 压入使用中的list
    /// </summary>
    /// <param name="obj"></param>
    public void pushUsed(GameObject obj)
    {
        obj.SetActive(true);
        if (PoolManager.Instance.useLayout)
        {
            obj.transform.SetParent(null);
        }
        usedList.Add(obj);
    }
}
public abstract class ClassPoolBase
{ }
public class ClassPool<T>: ClassPoolBase where T:class
{
    public Queue<T> objQueue = new Queue<T>();
}
/// <summary>
/// 使用对象池的数据结构类或者逻辑类需要继承的接口 并且需要实现重置其中数据的方法
/// </summary>
public interface IPoolClass
{
    public void Reset();
}
/// <summary>
/// 对象池管理器
/// </summary>
public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();
    private Dictionary<string, ClassPoolBase> classPoolDic = new Dictionary<string, ClassPoolBase>();
    //Pool对象的resource的路径
    private string path = "Pool/";
    //存放对象的对象池父节点
    private GameObject root;
    //是否整理对象池结构
    public bool useLayout = true;

    private PoolManager()
    {
    }

    public GameObject getObject(string name)
    {
        if(root == null && PoolManager.Instance.useLayout)
        {
            root = new GameObject("PoolRoot");
        }

        //如果没有对应的对象池 或者 池子里面没有备用的 使用的对象也没有超过上限 需要创建对象
        if (!poolDic.ContainsKey(name) || (poolDic.ContainsKey(name) && poolDic[name].emptyCount == 0 && poolDic[name].needCreate))
        {
            GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>(path + name));

            if (!poolDic.ContainsKey(name))
            {
                poolDic.Add(name, new PoolData(root, name, obj));
            }
            else
            {
                poolDic[name].pushUsed(obj);
            }
            return obj;
        }
        else
        {
            //其他情况就不需要 要不然就是从备用池里拿 要不然就是从正在使用的对象中取使用最久的那个
            return poolDic[name].pop();
        }
    }
    /// <summary>
    /// 取出数据结构类对象或者逻辑对象的方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="namespaceName"></param>
    /// <returns></returns>
    public T getObject<T>(string namespaceName = "") where T:class,new()
    {
        string poolName = namespaceName == "" ? typeof(T).Name : namespaceName + "_" + typeof(T).Name;
        if (classPoolDic.ContainsKey(poolName))
        {
            ClassPool<T> pool = classPoolDic[poolName] as ClassPool<T>;
            if (pool.objQueue.Count > 0)
            {
                Debug.Log("从池子中获取");
                return pool.objQueue.Dequeue();
            }
        }
        Debug.Log("新建一个对象");
        return new T();
    }

    public void putObject(string name,GameObject obj)
    {
        poolDic[name].push(obj);
    }
    public void putObject<T>(T obj, string namespaceName = "") where T:class, IPoolClass
    {
        string poolName = namespaceName == "" ? typeof(T).Name : namespaceName + "_" + typeof(T).Name;
        ClassPool<T> pool;
        if (classPoolDic.ContainsKey(poolName))
        {
            pool = classPoolDic[poolName] as ClassPool<T>;
        }
        else
        {
            Debug.Log("新建一个池子");
            pool = new ClassPool<T>();
            classPoolDic.Add(poolName, pool);
        }
        Debug.Log("回收对象");
        obj.Reset();//清除对象的状态
        pool.objQueue.Enqueue(obj);
    }
    public void clear()
    {
        poolDic.Clear();
        root = null;
        classPoolDic.Clear();
    }
}
