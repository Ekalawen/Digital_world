using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleEventDelay : SingleEvent {

    public float delay = 1.0f;
    public SingleEvent singleEvent;

    public override void TriggerSpecific() {
        StartCoroutine(CTrigger());
    }

    protected IEnumerator CTrigger() {
        yield return new WaitForSeconds(delay);
        singleEvent.Initialize();
        singleEvent.Trigger();
    }

    protected override void TriggerSound() {
        // No sound, it's a delay :p
    }
}
