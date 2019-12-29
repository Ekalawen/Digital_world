using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RandomEvent : MonoBehaviour {

    public float esperanceApparition = 15.0f;
    public float varianceApparition = 4.0f;
    public float esperanceDuree = 5.0f;
    public float varianceDuree = 0.0f;
    public bool bPlayEndSound = true;

    protected bool bEventIsOn = false;
    protected float dureeCourante = 0.0f;
    protected Timer timer;
    protected GameManager gm;

    protected virtual void Start() {
        gm = GameManager.Instance;
        timer = new Timer(NextTime());
    }

    protected void Update() {
        if(timer.IsOver() && !gm.eventManager.IsGameOver()) {
            if (!bEventIsOn) {
                bEventIsOn = true;
                dureeCourante = GaussianGenerator.Next(esperanceDuree, varianceDuree, 0.0f, 2 * esperanceDuree);
                StartEvent();
                gm.soundManager.PlayEventStartClip();
                StartEventConsoleMessage();
                StartCoroutine(CEndEvent());
            }
            timer = new Timer(NextTime());
        }
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

    public abstract void StartEvent();
    public abstract void EndEvent();
    public abstract void StartEventConsoleMessage();
}
