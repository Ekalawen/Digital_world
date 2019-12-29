using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RandomEvent : MonoBehaviour {

    public float esperanceApparition = 15.0f;
    public float varianceApparition = 4.0f;
    public float esperanceDuree = 5.0f;
    public float varianceDuree = 0.0f;
    public bool bPlayEndSound = true;

    protected Timer timer;
    protected GameManager gm;

    protected virtual void Start() {
        gm = GameManager.Instance;
        timer = new Timer(GaussianGenerator.Next(esperanceApparition, varianceApparition));
    }

    protected void Update() {
        if(timer.IsOver()) {
            StartEvent();
            gm.soundManager.PlayEventStartClip();
            StartEventConsoleMessage();
            StartCoroutine(CEndEvent());
            timer = new Timer(GaussianGenerator.Next(esperanceApparition, varianceApparition));
        }
    }

    protected IEnumerator CEndEvent() {
        yield return new WaitForSeconds(GaussianGenerator.Next(esperanceDuree, varianceDuree));
        EndEvent();
        if(bPlayEndSound)
            gm.soundManager.PlayEventEndClip();
    }

    public abstract void StartEvent();
    public abstract void EndEvent();
    public abstract void StartEventConsoleMessage();
}
