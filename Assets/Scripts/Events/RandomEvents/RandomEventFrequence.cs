using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RandomEventFrequence : RandomEvent {

    [Header("Apparition")]
    public float nbApparitionsBySeconds = 10;

    protected Timer lastTimeStartEvent;

    public override void Initialize() {
        esperanceApparition = 1.0f / nbApparitionsBySeconds;
        lastTimeStartEvent = new Timer();
        lastTimeStartEvent.Stop();
        base.Initialize();
    }

    protected override void CallStartEvent() {
        int nbTimesTriggerEvent = Mathf.RoundToInt(lastTimeStartEvent.GetElapsedTime() * nbApparitionsBySeconds);
        Debug.Log($"nbtimesTriggerEvent this frame = {nbTimesTriggerEvent}");
        for (int i = 0; i < nbTimesTriggerEvent; i++) {
            StartEvent();
        }
        lastTimeStartEvent.Reset();
    }
}
