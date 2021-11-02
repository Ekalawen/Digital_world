using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer {

    protected float duree;
    protected float debut;
    protected bool stoped = false;
    protected float stopedTiming;
    protected float lastAvancement;

    public Timer(float duree = 0, bool setOver = false) {
        this.duree = duree;
        this.debut = GetTimeSinceLevelLoad();
        this.lastAvancement = 0;
        if(setOver) {
            SetOver();
        }
    }

    public bool IsOver() {
        return !stoped && GetTimeSinceLevelLoad() - debut >= duree;
    }

    public void Reset() {
        stoped = false;
        debut = GetTimeSinceLevelLoad();
    }

    public void SetOver() {
        debut = GetTimeSinceLevelLoad() - duree;
    }

    public float GetAvancement() {
        return GetElapsedTime() / GetDuree();
    }

    public float GetNewAvancement() {
        float avancement = GetAvancement();
        float newAvancement = avancement - lastAvancement;
        lastAvancement = avancement;
        return newAvancement;
    }

    public float GetElapsedTime() {
        return GetTimeSinceLevelLoad() - debut;
    }

    public void SetElapsedTime(float value) {
        debut = GetTimeSinceLevelLoad() - value;
    }

    public float GetDuree() {
        return duree;
    }

    public void SetDuree(float newDuree) {
        duree = newDuree;
    }

    public void AddDuree(float addedDuree) {
        duree += addedDuree;
    }

    public float GetRemainingTime() {
        return debut + duree - GetTimeSinceLevelLoad();
    }

    public void SetRemainingTime(float remainingTime) {
        debut = remainingTime - duree + GetTimeSinceLevelLoad();
    }

    public void Stop() {
        stoped = true;
        stopedTiming = GetTimeSinceLevelLoad();
    }

    public void UnStop() {
        stoped = false;
        debut = debut + duree - stopedTiming;
    }

    protected virtual float GetTimeSinceLevelLoad() {
        return Time.timeSinceLevelLoad;
    }

    public void AdvanceTimerBy(float fixedDeltaTime) {
        debut -= fixedDeltaTime;
    }

    public static float TimeToSynchronize(float periode) {
        float modulo = Time.timeSinceLevelLoad % periode;
        float synchronizeTime = periode - modulo;
        Debug.Log($"time = {Time.timeSinceLevelLoad} periode = {periode} modulo = {modulo} synch = {synchronizeTime}");
        return synchronizeTime;
    }
}
