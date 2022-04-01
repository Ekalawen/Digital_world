using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RandomEvent : MonoBehaviour {

    public float esperanceApparition = 15.0f;
    public float varianceApparition = 4.0f;
    public float esperanceDuree = 5.0f;
    public float varianceDuree = 0.0f;
    public float startOffset = 0.0f;
    public bool bPlayEndSound = true;
    public bool playStartSound = true;
    public bool triggerMaxNumberOfTimes = false;
    [ConditionalHide("triggerMaxNumberOfTimes")]
    public int nbMaxTriggers = 1;

    protected bool bEventIsOn = false;
    protected float dureeCourante = 0.0f;
    protected GameManager gm;
    protected int nbTimesTriggered = 0;

    public virtual void Initialize() {
        gm = GameManager.Instance;
        StartCoroutine(CStartEvents());
    }

    protected IEnumerator CStartEvents() {
        yield return new WaitForSeconds(startOffset);
        while (!gm.eventManager.IsGameOver()) {
            float nextTime = NextTime();
            yield return new WaitForSeconds(nextTime);
            if(!bEventIsOn && !gm.eventManager.IsGameOver()) { // Double check necessary here cause of the wait
                TriggerEvent();
            }
        }
    }

    public void TriggerEvent() {
        if (CanBeStarted())
        {
            bEventIsOn = true;
            dureeCourante = GaussianGenerator.Next(esperanceDuree, varianceDuree, 0.0f, 2 * esperanceDuree);
            CallStartEvent();
            PlayStartSound();
            StartEventConsoleMessage();
            StartCoroutine(CEndEvent());
            nbTimesTriggered += 1;
        }
    }

    protected virtual void CallStartEvent() {
        StartEvent();
    }

    protected virtual void PlayStartSound() {
        if (playStartSound) {
            gm.soundManager.PlayEventStartClip();
        }
    }

    protected IEnumerator CEndEvent()
    {
        yield return new WaitForSeconds(dureeCourante);
        SetEventIsDoneOnEndEvent();
        EndEvent();
        if (bPlayEndSound)
            gm.soundManager.PlayEventEndClip();
    }

    protected virtual void SetEventIsDoneOnEndEvent() {
        bEventIsOn = false;
    }

    protected virtual float NextTime() {
        return GaussianGenerator.Next(esperanceApparition, varianceApparition, 0.0f, 2 * esperanceApparition) + dureeCourante;
    }

    public virtual bool CanBeStarted() {
        return !triggerMaxNumberOfTimes || nbTimesTriggered < nbMaxTriggers;
    }

    protected abstract void StartEvent();
    protected abstract void EndEvent();  // End naturally
    public abstract void StopEvent();  // Stopped by an external source
    protected abstract void StartEventConsoleMessage();

    public bool IsCurrentlyOn() {
        return bEventIsOn;
    }
}
