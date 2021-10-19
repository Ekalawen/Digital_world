using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SpeedMultiplier {
    public float speedAdded = 1.0f;
    public float duration = 3.0f;
    public float inDuration = 0.3f;
    public float outDuration = 0.5f;
    public AnimationCurve inCurve;
    public AnimationCurve outCurve;
    public bool affectHeight = false;

    protected SpeedMultiplierController controller;
    protected Timer timer;

    public void Initialize(SpeedMultiplierController controller) {
        this.controller = controller;
        timer = new Timer(GetTotalDuration());
    }

    public float GetTotalDuration() {
        return inDuration + duration + outDuration;
    }

    public float GetMultiplier() {
        float time = timer.GetElapsedTime();
        if(time <= inDuration) {
            return inCurve.Evaluate(MathCurves.LinearReversed(0, inDuration, time)) * speedAdded;
        } else if (time <= inDuration + duration) {
            return speedAdded;
        } else if (time <= GetTotalDuration()) {
            return outCurve.Evaluate(MathCurves.LinearReversed(inDuration + duration, GetTotalDuration(), time)) * speedAdded;
        } else {
            return 0.0f;
        }
    }

    public bool IsOver() {
        return timer.GetElapsedTime() > GetTotalDuration();
    }
}