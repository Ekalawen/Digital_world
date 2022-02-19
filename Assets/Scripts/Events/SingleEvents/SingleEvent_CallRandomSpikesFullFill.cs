using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class SingleEvent_CallRandomSpikesFullFill : SingleEvent {

    public float duration = 2.0f;
    public float nbApparitionsBySeconds = 50;

    public override void TriggerSpecific() {
        RandomSpikesFillEvent spikeEvent = eventManager.GetOneRandomEventOfType<RandomSpikesFillEvent>();
        if(spikeEvent == null) {
            Debug.Log($"We need at least one RandomSpikesFillEvent in the RandomEvent to be able to call SingleEvent_CallRandomSpikesFullFill !");
            return;
        }
        spikeEvent.ChangeFrequenceFor(nbApparitionsBySeconds, duration);
    }
}
