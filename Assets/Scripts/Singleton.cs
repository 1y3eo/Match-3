using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
                Debug.LogWarning("no exists : " + typeof(T));

            return instance;
        }
    }


    protected void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            Init();
        }
        else
        {
            Debug.LogWarning("already exists : " + typeof(T));
            Destroy(gameObject);
        }
    }


    protected void OnDestroy()
    {
        if (instance == null)
        {
            instance = null;
        }
    }


    protected virtual void Init() { }
}