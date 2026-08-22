using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static UIManager;
public enum E_UILayer
{
    BottomLayer,
    MiddleLayer,
    TopLayer,
    SystemLayer
}
public class UIManager : Singleton<UIManager>
{
    public abstract class BasePanelInfo{}
    public class PanelInfo<T>:BasePanelInfo where T:BaseUI
    {
        public T panel;//ui对象
        public UnityAction<T> callback;//加载完成的回调委托
        public bool isHide = false;
        public PanelInfo(T panel,UnityAction<T> callback)
        {
            this.panel = panel;
            this.callback = callback;
        }
    }
    private EventSystem eventSyetem;
    private Camera uiCamera;
    private Canvas canvas;

    private Transform BottomLayer;
    private Transform MiddleLayer;
    private Transform TopLayer;
    private Transform SystemLayer;

    /// <summary>
    /// 存储当前现实的UI key:baseui子类的类名
    /// </summary>
    private Dictionary<string, BasePanelInfo> panelDic = new Dictionary<string, BasePanelInfo>();

    /// <summary>
    /// 初始化的时候就进行需要的一些基础的节点的创建和初始化
    /// </summary>
    private UIManager()
    {
        uiCamera = GameObject.Instantiate(ResourcesManager.Instance.Load<GameObject>("UI/UICamera")).GetComponent<Camera>();
        canvas = GameObject.Instantiate(ResourcesManager.Instance.Load<GameObject>("UI/Canvas")).GetComponent<Canvas>();
        canvas.worldCamera = uiCamera;
        BottomLayer = canvas.transform.Find("BottomLayer");
        MiddleLayer = canvas.transform.Find("MiddleLayer");
        TopLayer = canvas.transform.Find("TopLayer");
        SystemLayer = canvas.transform.Find("SystemLayer");

        eventSyetem = GameObject.Instantiate(ResourcesManager.Instance.Load<GameObject>("UI/EventSystem")).GetComponent<EventSystem>();
        GameObject.DontDestroyOnLoad(uiCamera.gameObject);
        GameObject.DontDestroyOnLoad(canvas.gameObject);
        GameObject.DontDestroyOnLoad(eventSyetem.gameObject);
    }

    public void init() { }
    /// <summary>
    /// 显示某个PanelUI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="layer"></param>
    /// <param name="callback"></param>
    /// <param name="isSync"></param>
    public void ShowPanel<T>(E_UILayer layer,UnityAction<T> callback,bool isSync = false) where T:BaseUI
    {
        string panelName = typeof(T).Name;
        if (!panelDic.ContainsKey(panelName))
        {
            //没有加载过
            panelDic.Add(panelName, new PanelInfo<T>(null, callback));
            ABResManager.Instance.LoadResAsync<GameObject>("ui", panelName, (res) =>
            {
                PanelInfo<T> panelInfo = panelDic[panelName] as PanelInfo<T>;
                if (panelInfo.isHide == true)
                {
                    //没加载完的时候就想隐藏了
                    panelDic.Remove(panelName);//删除存储的引用
                    return;
                }
                Transform parent = GetLayerParent(layer);

                GameObject obj = GameObject.Instantiate(res, parent == null ? MiddleLayer : parent,false);//实例化
                T panel = obj.GetComponent<T>();
                panel.ShowUI();//调用显示逻辑

                panelInfo.panel = panel;//加载成功进行赋值
                panelInfo.callback?.Invoke(panel);//将组件对象传出去
                panelInfo.callback = null;
            }, isSync);
        }
        else
        {
            PanelInfo<T> panelInfo = panelDic[panelName] as PanelInfo<T>;
            if (panelInfo.panel == null)
            {
                //正在加载中
                panelInfo.isHide = false;//避免多次显示隐藏最后无法得到合理的结果
                panelInfo.callback += callback;
            }
            else
            {
                if(panelInfo.panel.gameObject.activeSelf == false)
                {
                    panelInfo.panel.gameObject.SetActive(true);
                }
                panelInfo.panel.ShowUI();
                callback?.Invoke(panelInfo.panel as T);
            }
        }
    }
    /// <summary>
    /// 隐藏PanelUI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void HidePanel<T>(bool isDestory) where T : BaseUI
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            PanelInfo<T> panelInfo = panelDic[panelName] as PanelInfo<T>;
            if(panelInfo.panel == null)
            {
                //还在加载中
                panelInfo.isHide = true;
                panelInfo.callback = null;
            }
            else
            {
                if (isDestory)
                {
                    panelInfo.panel.HideUI();
                    GameObject.Destroy(panelInfo.panel.gameObject);
                    panelInfo.panel = null;
                    panelInfo.callback = null;
                    panelDic.Remove(panelName);
                }
                else
                {
                    panelInfo.panel.gameObject.SetActive(false);
                }

            }
        }
    }

    /// <summary>
    /// 获取某个PanelUI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callback">等待加载完成回调</param>
    public void GetPanel<T>(UnityAction<T> callback) where T : BaseUI
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            PanelInfo<T> panelInfo = panelDic[panelName] as PanelInfo<T>;
            if(panelInfo.panel == null)
            {
                panelInfo.callback += callback;
            }
            else if(!panelInfo.isHide)
            {
                callback?.Invoke(panelInfo.panel as T);
            }
        }
    }
    /// <summary>
    /// 获取对应层级的父节点
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    public Transform GetLayerParent(E_UILayer layer)
    {
        switch (layer)
        {
            case E_UILayer.BottomLayer:return BottomLayer;
            case E_UILayer.MiddleLayer:return MiddleLayer;
            case E_UILayer.TopLayer:return TopLayer;
            case E_UILayer.SystemLayer:return SystemLayer;
        }
        return null;
    }

    /// <summary>
    /// 基于EventTigger方便给控件添加想要的自定义事件监听
    /// </summary>
    /// <param name="control">UI控件</param>
    /// <param name="type">事件类型</param>
    /// <param name="callback">事件回调</param>
    public static void AddCustomEventListener(UIBehaviour control,EventTriggerType type,UnityAction<BaseEventData> callback)
    {
        EventTrigger trigger = control.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = control.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(callback);

        trigger.triggers.Add(entry);
    }
}
