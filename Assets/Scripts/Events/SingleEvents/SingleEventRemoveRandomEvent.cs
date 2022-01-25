using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleEventRemoveRandomEvent : SingleEvent {

    public bool removeAllEvents = false;
    public List<int> eventIndicesToRemoves = new List<int>() { 0 };

    public override void TriggerSpecific() {
        if (removeAllEvents) {
            eventManager.RemoveAllEvents();
        } else {
            eventManager.RemoveEventsOfIndices(eventIndicesToRemoves);
        }
    }

}
