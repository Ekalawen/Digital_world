using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer {

    protected float duree;
    protected float debut;
    protected bool stoped = false;
    protected float stopedTiming;

    public Timer(float duree) {
        this.duree = duree;
        this.debut = Time.timeSinceLevelLoad;
    }

    public bool IsOver() {
        return !stoped && Time.timeSinceLevelLoad - debut > duree;
    }

    public void Reset() {
        stoped = false;
        debut = Time.timeSinceLevelLoad;
    }

    public void SetOver() {
        debut = Time.timeSinceLevelLoad - duree;
    }

    public float GetAvancement() {
        return (Time.timeSinceLevelLoad - debut) / duree;
    }

    public float GetElapsedTime() {
        return Time.timeSinceLevelLoad - debut;
    }

    public float GetDuree() {
        return duree;
    }

    public void SetDuree(float newDuree) {
        duree = newDuree;
    }

    public float GetRemainingTime() {
        return debut + duree - Time.timeSinceLevelLoad;
    }

    public void Stop() {
        stoped = true;
        stopedTiming = Time.timeSinceLevelLoad;
    }

    public void UnStop() {
        stoped = false;
        debut = debut + duree - stopedTiming;
    }
}
