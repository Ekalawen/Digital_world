using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class OverrideLumiereType : Override {

    public GameObject newLumierePrefab;

    protected override void InitializeSpecific() {
        map.lumierePrefab = newLumierePrefab;
    }
}
