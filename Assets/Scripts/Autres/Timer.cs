using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer {

    protected float duree;
    protected float debut;

    public Timer(float duree) {
        this.duree = duree;
        debut = Time.timeSinceLevelLoad;
    }

    public bool IsOver() {
        return Time.timeSinceLevelLoad - debut > duree;
    }

    public void Reset() {
        debut = Time.timeSinceLevelLoad;
    }

    public void Enable() {
        debut = Time.timeSinceLevelLoad - duree;
    }

    public float GetAvancement() {
        return (Time.timeSinceLevelLoad - debut) / duree;
    }

    public float GetElapsedTime() {
        return Time.timeSinceLevelLoad - debut;
    }
}
