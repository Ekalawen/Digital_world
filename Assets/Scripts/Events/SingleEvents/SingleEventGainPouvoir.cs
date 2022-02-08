using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SingleEventGainPouvoir : SingleEvent {

    public GameObject pouvoirPrefab;
    public PouvoirGiverItem.PouvoirBinding binding;

    public override void TriggerSpecific() {
        PouvoirGiverItem.GivePouvoir(gm, pouvoirPrefab, binding);
    }
}
