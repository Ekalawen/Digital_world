using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TimeMultiplierController : MonoBehaviour {

    [SerializeField]
    protected List<TimeMultiplier> timeMultipliers;
    [SerializeField]
    protected List<TimeMultiplier> ennemiTimeMultipliers;

    protected TimerManager timerManager;

    public void Initialize(TimerManager timerManager) {
        this.timerManager = timerManager;
        timeMultipliers = new List<TimeMultiplier>();
        ennemiTimeMultipliers = new List<TimeMultiplier>();
    }

    public TimeMultiplier AddMultiplier(TimeMultiplier timeMultiplier, bool isEnnemiMultiplier = false) {
        timeMultiplier.Initialize(this);
        if(!isEnnemiMultiplier)
            timeMultipliers.Add(timeMultiplier);
        else
            ennemiTimeMultipliers.Add(timeMultiplier);
        StartCoroutine(CUnregisterAtEnd(timeMultiplier));
        return timeMultiplier;
    }

    protected IEnumerator CUnregisterAtEnd(TimeMultiplier timeMultiplier) {
        yield return new WaitForSeconds(timeMultiplier.GetTotalDuration());
        RemoveMultiplier(timeMultiplier);
    }

    public void RemoveMultiplier(TimeMultiplier timeMultiplier) {
        timeMultipliers.Remove(timeMultiplier);
        ennemiTimeMultipliers.Remove(timeMultiplier);
    }

    public float GetMultiplier() {
        float multiplier = 1.0f;
        foreach(TimeMultiplier timeMultiplier in GetAllMultipliers().FindAll(tm => tm.type == TimeMultiplier.VariationType.ADD)) {
            if(timeMultiplier.GetMultiplier() == float.NaN) {
                Debug.Log("Ici !! (multiplier = Nan !)");
            }
            multiplier += timeMultiplier.GetMultiplier();
        }
        multiplier = Mathf.Max(multiplier, 0);
        foreach(TimeMultiplier timeMultiplier in GetAllMultipliers().FindAll(tm => tm.type == TimeMultiplier.VariationType.MULTIPLY)) {
            if(timeMultiplier.GetMultiplier() == float.NaN) {
                Debug.Log("Ici !! (multiplier = Nan !)");
            }
            multiplier *= timeMultiplier.GetMultiplier();
        }
        return Mathf.Max(multiplier, 0);
    }

    public List<TimeMultiplier> GetAllMultipliers() {
        List<TimeMultiplier> allMultipliers = timeMultipliers.Select(t => t).ToList();
        allMultipliers.AddRange(ennemiTimeMultipliers);
        return allMultipliers;
    }

    public void RemoveAllEnnemisMultipliers() {
        ennemiTimeMultipliers.Clear();
    }
}
