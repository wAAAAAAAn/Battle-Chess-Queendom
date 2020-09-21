using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SINGLETON <T>: MonoBehaviour
{

    private static T _Instance;

    public static T Instance
    {
        get
        {
            return _Instance;
        }
    }

    protected virtual void Awake()
    {
        _Instance = this.GetComponent<T>();


    }
}
