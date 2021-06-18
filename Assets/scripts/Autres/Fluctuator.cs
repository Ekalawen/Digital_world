using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

public class Fluctuator {

    public delegate void Setter(float value);
    public delegate float Getter();

    protected MonoBehaviour monoBehavior;
    protected Coroutine coroutine;
    protected Getter GetValue;
    protected Setter SetValue;
    protected AnimationCurve globalCurve;

    public Fluctuator(MonoBehaviour monoBehavior, Getter getter, Setter setter, AnimationCurve curve = null) {
        this.monoBehavior = monoBehavior;
        this.GetValue = getter;
        this.SetValue = setter;
        this.coroutine = null;
        this.globalCurve = curve;
    }

    public void GoTo(float targetValue, float duration, AnimationCurve oneTimeCurve = null) {
        if(coroutine != null) {
            monoBehavior.StopCoroutine(coroutine);
        }
        coroutine = monoBehavior.StartCoroutine(CGoTo(targetValue, duration, oneTimeCurve));
    }

    protected IEnumerator CGoTo(float targetValue, float duration, AnimationCurve oneTimeCurve) {
        Timer timer = new Timer(duration);
        float startValue = GetValue();
        while(!timer.IsOver()) {
            float avancement = ApplyCurve(timer.GetAvancement(), oneTimeCurve, globalCurve);
            SetValue(MathCurves.Linear(startValue, targetValue, avancement));
            yield return null;
        }
        SetValue(targetValue);
    }

    protected float ApplyCurve(float value, AnimationCurve oneTimeCurve, AnimationCurve globalCurve) {
        return oneTimeCurve != null ? oneTimeCurve.Evaluate(value) : (globalCurve != null ? globalCurve.Evaluate(value) : value);
    }
}