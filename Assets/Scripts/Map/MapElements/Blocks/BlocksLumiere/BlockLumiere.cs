using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class BlockLumiere : MonoBehaviour {

    protected List<Lumiere> lumieres;
    protected InfiniteMap infiniteMap;

    public void Initialize() {
        infiniteMap = GameManager.Instance.GetInfiniteMap();
    }

    protected void InitializeLumieres() {
        Lumiere lumiere = GetComponent<Lumiere>();
        if (lumiere) {
            lumieres = new List<Lumiere>() { lumiere };
            return;
        }
        lumieres = new List<Lumiere>();
        foreach (Transform child in transform) {
            lumieres.Add(child.gameObject.GetComponent<Lumiere>());
        }
    }

    public virtual bool CanBePicked() {
        return true;
    }

    public List<Lumiere> GetLumieres() {
        if (lumieres == null) {
            InitializeLumieres();
        }
        return lumieres;
    }

    public void OnCapture(Lumiere lumiere) {
        infiniteMap.CaptureLumiere(lumiere);
    }
}
