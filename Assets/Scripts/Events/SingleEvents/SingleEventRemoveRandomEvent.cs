using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleEventRemoveRandomEvent : SingleEvent {

    public bool removeAllEvents = false;
    public List<int> eventIndicesToRemoves = new List<int>() { 0 };
    public bool waitUntilEventIsOver = false;

    public override void TriggerSpecific() {
        if (removeAllEvents) {
            if (!waitUntilEventIsOver) {
                eventManager.RemoveAllEvents();
            } else {
                eventManager.RemoveAllEventsWhenTheyAreDone();
            }
        } else {
            if (!waitUntilEventIsOver) {
                eventManager.RemoveEventsOfIndices(eventIndicesToRemoves);
            } else {
                eventManager.RemoveEventsOfIndicesWhenTheyAreDone(eventIndicesToRemoves);
            }
        }
    }
}
