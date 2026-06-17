using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Unity.VisualScripting;

public abstract class BaseSingleton<T> where T : class //,new()
{
    private static T instance;

    //是否已被实例化
    private static bool initialized = false;
    //通过反射或Activator创建实例本质是通过调用构造函数来实现
    //在基类的构造函数中通过一个静态标志位来判断是否已经构造过该单例了
    protected BaseSingleton() 
    {
        if(initialized)
        {
            Debug.LogWarning($"{typeof(T).Name}的实例以创建，禁止再次创建");
            return;
        }
        initialized = true;
    }

    //用于加锁的对象
    protected static readonly object lockobj = new object();

    //通过反射获取并执行类型T私有的无参构造函数
    public static T Instance
    {
        get 
        {
            //多一层判断是为了避免获取已实例化对象的等待解锁时间
            if (instance == null)
            {
                lock (lockobj)
                {
                    if (instance == null)
                    {
                        //通过反射获取并执行类型T私有的无参构造函数
                        Type type = typeof(T);

                        //BindingFlags.Instance | BindingFlags.NonPublic, //表示私有实例成员方法  '|'按位或，表示两个条件同时生效
                        //  null,                                         //表示没有绑定对象
                        //  Type.EmptyTypes,                              //表示没有参数
                        //  null);                                        //表示没有参数修饰符
                        ConstructorInfo info = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                                                                    null,
                                                                    Type.EmptyTypes,
                                                                    null);
                        if (info != null)
                        {
                            instance = info.Invoke(null) as T;
                        }
                        else
                            Debug.LogError("没有找到对应类型的无参构造函数");
                    }
                }
            }
            return instance; 
        }
    }
    //通过Activator创建实例
    public static T GetInstance()
    {
        if (instance == null)
        {
            lock (lockobj)
            {
                if (instance == null)
                {
                    Type t = typeof(T);
                    object obj = Activator.CreateInstance(t, true) as T;
                    if (obj != null)
                    {
                        instance = obj as T;
                    }
                    else
                    {
                        Debug.LogError("创建对应类型的实例失败");
                    }
                }
            }
        }
        return instance;
    }
}
