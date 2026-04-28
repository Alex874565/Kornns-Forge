using System;
using System.Collections.Generic;
using UnityEngine;

public class TirednessServiceLocator : MonoBehaviour
{
    private static Dictionary<Type, object> services = new Dictionary<Type, object>();
    public static void RegisterService<T>(T service)
    {
        var type = typeof(T);
        if (!services.ContainsKey(type))
        {
            services[type] = service;
        }
    }

    public static T GetService<T>()
    {
        var type = typeof(T);
        if (services.ContainsKey(type))
        {
            return (T)services[type];
        }
        else
        {
            throw new Exception($"Service of type {type} not found.");
        }
    }
}
