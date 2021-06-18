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

    public Fluctuator(MonoBehaviour monoBehavior, Getter getter, Setter setter) {
        this.monoBehavior = monoBehavior;
        this.GetValue = getter;
        this.SetValue = setter;
        this.coroutine = null;
        // Rajouter la possibilité d'ajouter une curve !
    }

    public void GoTo(float targetValue, float duration) {
        if(coroutine != null) {
            monoBehavior.StopCoroutine(coroutine);
        }
        coroutine = monoBehavior.StartCoroutine(CGoTo(targetValue, duration));
    }

    protected IEnumerator CGoTo(float targetValue, float duration) {
        Timer timer = new Timer(duration);
        float startValue = GetValue();
        while(!timer.IsOver()) {
            float avancement = timer.GetAvancement();
            SetValue(MathCurves.Linear(startValue, targetValue, avancement));
            yield return null;
        }
        SetValue(targetValue);
    }
}