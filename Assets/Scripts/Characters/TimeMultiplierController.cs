using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeMultiplierController : MonoBehaviour {

    [SerializeField]
    protected List<TimeMultiplier> timeMultipliers;

    protected TimerManager timerManager;

    public void Initialize(TimerManager timerManager) {
        this.timerManager = timerManager;
        timeMultipliers = new List<TimeMultiplier>();
    }

    public TimeMultiplier AddMultiplier(TimeMultiplier timeMultiplier) {
        timeMultiplier.Initialize(this);
        timeMultipliers.Add(timeMultiplier);
        StartCoroutine(CUnregisterAtEnd(timeMultiplier));
        return timeMultiplier;
    }

    protected IEnumerator CUnregisterAtEnd(TimeMultiplier timeMultiplier) {
        yield return new WaitForSeconds(timeMultiplier.GetTotalDuration());
        RemoveMultiplier(timeMultiplier);
    }

    public void RemoveMultiplier(TimeMultiplier timeMultiplier) {
        timeMultipliers.Remove(timeMultiplier);
    }

    public float GetMultiplier() {
        float multiplier = 1.0f;
        foreach(TimeMultiplier timeMultiplier in timeMultipliers.FindAll(tm => tm.type == TimeMultiplier.VariationType.ADD)) {
            if(timeMultiplier.GetMultiplier() == float.NaN) {
                Debug.Log("Ici !! (multiplier = Nan !)");
            }
            multiplier += timeMultiplier.GetMultiplier();
        }
        multiplier = Mathf.Max(multiplier, 0);
        foreach(TimeMultiplier timeMultiplier in timeMultipliers.FindAll(tm => tm.type == TimeMultiplier.VariationType.MULTIPLY)) {
            if(timeMultiplier.GetMultiplier() == float.NaN) {
                Debug.Log("Ici !! (multiplier = Nan !)");
            }
            multiplier *= timeMultiplier.GetMultiplier();
        }
        return Mathf.Max(multiplier, 0);
    }

    public List<TimeMultiplier> GetAllMultipliers() {
        return timeMultipliers;
    }
}
