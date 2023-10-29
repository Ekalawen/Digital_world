using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class BlockLumiereConditionnal : BlockLumiere {

    public List<GameObject> necessaryGameObjects;

    public override bool CanBePicked() {
        return necessaryGameObjects.All(go => go != null);
    }

    public List<Lumiere> GetLumieres() {
        Lumiere lumiere = GetComponent<Lumiere>();
        if(lumiere) {
            return new List<Lumiere>() { lumiere };
        }
        List<Lumiere> lumieres = new List<Lumiere>();
        foreach (Transform child in transform) {
            lumieres.Add(child.gameObject.GetComponent<Lumiere>());
        }
        return lumieres;
    }
}
