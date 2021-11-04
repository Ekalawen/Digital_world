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

    protected bool bEventIsOn = false;
    protected float dureeCourante = 0.0f;
    protected GameManager gm;

    public virtual void Initialize() {
        gm = GameManager.Instance;
        StartCoroutine(CStartEvents());
    }

    protected IEnumerator CStartEvents() {
        yield return new WaitForSeconds(startOffset);
        while (!gm.eventManager.IsGameOver()) {
            float nextTime = NextTime();
            yield return new WaitForSeconds(nextTime);
            if(!bEventIsOn) {
                TriggerEvent();
            }
        }
    }

    public void TriggerEvent() {
        if (CanBeStarted())
        {
            bEventIsOn = true;
            dureeCourante = GaussianGenerator.Next(esperanceDuree, varianceDuree, 0.0f, 2 * esperanceDuree);
            StartEvent();
            PlayStartSound();
            StartEventConsoleMessage();
            StartCoroutine(CEndEvent());
        }
    }

    protected virtual void PlayStartSound() {
        gm.soundManager.PlayEventStartClip();
    }

    protected IEnumerator CEndEvent() {
        yield return new WaitForSeconds(dureeCourante);
        bEventIsOn = false;
        EndEvent();
        if(bPlayEndSound)
            gm.soundManager.PlayEventEndClip();
    }

    protected float NextTime() {
        return GaussianGenerator.Next(esperanceApparition, varianceApparition, 0.0f, 2 * esperanceApparition) + dureeCourante;
    }

    public virtual bool CanBeStarted() {
        return true;
    }

    protected abstract void StartEvent();
    protected abstract void EndEvent();  // End naturally
    public abstract void StopEvent();  // Stopped by an external source
    protected abstract void StartEventConsoleMessage();
}
