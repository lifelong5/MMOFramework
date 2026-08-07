using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public abstract class Singleton<T> where T:class
{
    private static T instance;
    private static object lockObj = new object();
    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                lock (lockObj) {
                    if (instance == null)
                    {
                        Type type = typeof(T);
                        ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);//实例的私有无参构造函数
                        instance = constructor?.Invoke(null) as T;
                    }
                }
            }
            return instance;
        }
    }
}
