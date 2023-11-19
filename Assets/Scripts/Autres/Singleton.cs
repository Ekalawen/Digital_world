using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component {

    protected static T _instance;

    public static T Instance {
        get {
            if (_instance == null) {
                T[] objs = FindObjectsOfType(typeof(T)) as T[];
                if (objs.Length >= 1) {
                    _instance = objs.First();
                }
                if (objs.Length >= 2) {
                    throw new System.Exception($"There are more than one {typeof(T).Name} objects in the scene!");
                }
                if (_instance == null) {
                    GameObject obj = new GameObject(typeof(T).Name);
                    //obj.hideFlags = HideFlags.HideAndDontSave;
                    _instance = obj.AddComponent<T>();
                    Initialize(_instance);
                }
            }
            return _instance;
        }
    }

    public static bool IsInstantiated { get { return _instance != null; } }

    public virtual void Awake() {
        if (!_instance) {
            _instance = this as T;
        }
    }

    public virtual void Initialize(T instance) {
    }
}
