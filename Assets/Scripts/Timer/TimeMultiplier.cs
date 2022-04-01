using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TimeMultiplier {

    public enum VariationType { ADD, MULTIPLY };

    public VariationType type = VariationType.MULTIPLY;
    public float timeVariation = 1.0f;
    public float totalRealDuration = 3.0f;
    public bool useRealTimeDuration = false;
    public float inDurationPercentage = 0.1f;
    public float outDurationPercentage = 0.2f;
    public AnimationCurve inCurve;
    public AnimationCurve outCurve;

    protected TimeMultiplierController controller;
    protected Timer timer;

    public TimeMultiplier(TimeMultiplier other) {
        this.timeVariation = other.timeVariation;
        this.totalRealDuration = other.totalRealDuration;
        this.inDurationPercentage = other.inDurationPercentage;
        this.outDurationPercentage = other.outDurationPercentage;
        this.inCurve = other.inCurve;
        this.outCurve = other.outCurve;
    }

    public void Initialize(TimeMultiplierController controller) {
        this.controller = controller;
        timer = useRealTimeDuration ? new UnpausableTimer(GetTotalDuration()) : new Timer(GetTotalDuration());
    }

    public float GetTotalDuration() {
        return totalRealDuration;
    }

    public float GetInDuration() {
        return totalRealDuration * inDurationPercentage;
    }

    public float GetMidDuration() {
        return totalRealDuration * (1 - inDurationPercentage - outDurationPercentage);
    }

    public float GetOutDuration() {
        return totalRealDuration * outDurationPercentage;
    }

    public float GetMultiplier() {
        return type == VariationType.ADD ? GetAddMultiplier() : GetMultiplyMultiplier();
    }

    public void SetTotalDurationToZero() {
        totalRealDuration = 0.0f;
    }

    protected float GetAddMultiplier() {
        float time = timer.GetElapsedTime();
        if (time <= GetInDuration() && GetInDuration() > 0) {
            return inCurve.Evaluate(MathCurves.LinearReversed(0, GetInDuration(), time)) * timeVariation;
        } else if (time <= GetInDuration() + GetMidDuration() && GetMidDuration() > 0) {
            return timeVariation;
        } else if (time <= GetTotalDuration() && GetOutDuration() > 0) {
            return outCurve.Evaluate(MathCurves.LinearReversed(GetInDuration() + GetMidDuration(), GetTotalDuration(), time)) * timeVariation;
        } else {
            return 0.0f;
        }
    }

    protected float GetMultiplyMultiplier() {
        float time = timer.GetElapsedTime();
        if (time <= GetInDuration() && GetInDuration() > 0) {
            float avancement = inCurve.Evaluate(MathCurves.LinearReversed(0, GetInDuration(), time));
            return MathCurves.Linear(1, timeVariation, avancement);
        } else if (time <= GetInDuration() + GetMidDuration() && GetMidDuration() > 0) {
            return timeVariation;
        } else if (time <= GetTotalDuration() && GetOutDuration() > 0) {
            float avancement = outCurve.Evaluate(MathCurves.LinearReversed(GetInDuration() + GetMidDuration(), GetTotalDuration(), time));
            return MathCurves.Linear(1, timeVariation, avancement);
        } else {
            return 1.0f;
        }
    }

    public bool IsOver() {
        return timer.GetElapsedTime() > GetTotalDuration();
    }
}