using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleEventDelay : SingleEvent {

    public float delay = 1.0f;
    public SingleEvent singleEvent;

    public override void Trigger() {
        StartCoroutine(CTrigger());
    }

    protected IEnumerator CTrigger() {
        yield return new WaitForSeconds(delay);
        singleEvent.Trigger();
    }
}
