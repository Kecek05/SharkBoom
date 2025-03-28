using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{

    private static Dictionary<Type, object> services = new Dictionary<Type, object>();

    public static void Register<T>(T service) where T : class
    {
        services[typeof(T)] = service;
    }
    

    public static T Get<T>() where T : class
    {
        if (services.TryGetValue(typeof(T), out object service))
        {
            return service as T;
        }
        else
        {
            Debug.LogError($"Service of type {typeof(T)} not found");
            return null;
        }
    }

    public static void Unregister<T> () where T : class
    {
        services.Remove(typeof(T));
    }
}
