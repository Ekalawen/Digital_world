using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RandomEventFrequence : RandomEvent {

    [Header("Apparition")]
    public float nbApparitionsBySeconds = 10;

    protected Timer lastTimeStartEvent;

    public override void Initialize() {
        SetNbApparitions(nbApparitionsBySeconds);
        lastTimeStartEvent = new Timer();
        lastTimeStartEvent.Stop();
        base.Initialize();
    }

    protected override void CallStartEvent() {
        int nbTimesTriggerEvent = Mathf.RoundToInt(lastTimeStartEvent.GetElapsedTime() * nbApparitionsBySeconds);
        for (int i = 0; i < nbTimesTriggerEvent; i++) {
            StartEvent();
        }
        lastTimeStartEvent.Reset();
    }

    public void SetNbApparitions(float newNbApparitions) {
        nbApparitionsBySeconds = newNbApparitions;
        esperanceApparition = 1.0f / nbApparitionsBySeconds;
    }

    public void ChangeFrequenceFor(float newNbApparitionsBySeconds, float duration) {
        StartCoroutine(CChangeFrequenceFor(newNbApparitionsBySeconds, duration));
    }

    protected IEnumerator CChangeFrequenceFor(float newNbApparitionsBySeconds, float duration) {
        float oldFrequence = nbApparitionsBySeconds;
        SetNbApparitions(newNbApparitionsBySeconds);
        yield return new WaitForSeconds(duration);
        SetNbApparitions(oldFrequence);
    }
}
