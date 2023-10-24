using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class BoolTimer {

    protected Timer timer;
    protected MonoBehaviour monoBehavior;
    protected SingleCoroutine singleCoroutine;

    public UnityEvent onStart;
    public UnityEvent onStop;

    public BoolTimer(MonoBehaviour holder) {
        this.monoBehavior = holder;
        onStart = new UnityEvent();
        onStop = new UnityEvent();
        singleCoroutine = new SingleCoroutine(holder);
        timer = new Timer(setOver: true);
    }

    public void AddTime(float time) {
        Assert.IsTrue(time > 0);
        if (!value) {
            onStart.Invoke();
        }
        if (timer.IsOver()) {
            timer.SetRemainingTime(time);
        } else {
            timer.AddDuree(time);
        }
        singleCoroutine.Start(CCallEndInvulnerabilityIn(RemainingTime()));
    }

    public IEnumerator CCallEndInvulnerabilityIn(float duration) {
        yield return new WaitForSeconds(duration);
        onStop.Invoke();
    }

    public void SetTime(float time) {
        Assert.IsTrue(time > 0);
        if (!value) {
            onStart.Invoke();
        }
        if (timer.GetRemainingTime() < time) {
            timer.SetRemainingTime(time);
        }
        singleCoroutine.Start(CCallEndInvulnerabilityIn(RemainingTime()));
    }

    public bool value { get { return Value(); } }

    protected bool Value() {
        return !timer.IsOver();
    }

    public float RemainingTime() {
        return Mathf.Max(0, timer.GetRemainingTime());
    }

    public bool IsOver() {
        return timer.IsOver();
    }
}
