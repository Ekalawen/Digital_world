using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleEventAddRandomEvent : SingleEvent {

    public GameObject randomEventPrefab;
    public bool triggerRandomEventOnAdd = true;

    public override void TriggerSpecific() {
        RandomEvent newEvent = eventManager.AddRandomEvent(randomEventPrefab);
        newEvent.Start();
        if (triggerRandomEventOnAdd) {
            newEvent.TriggerEvent();
        }
    }
}
