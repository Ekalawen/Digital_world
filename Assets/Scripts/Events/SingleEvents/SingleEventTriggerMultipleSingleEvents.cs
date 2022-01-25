using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleEventTriggerMultipleSingleEvents : SingleEvent {

    public List<SingleEvent> otherEvents;

    public override void TriggerSpecific() {
        foreach(SingleEvent otherEvent in otherEvents) {
            otherEvent.Initialize();
            otherEvent.Trigger();
        }
    }
}
