using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CounterDisplayerUpdater : MonoBehaviour {

    public delegate int Getter();

    public string prefix = "";
    public string suffix = "";
    public float updateDuration = 0.6f;
    public bool useCreditsFormating = false;

    protected CounterDisplayer displayer;
    protected Getter getCurrentValue;
    protected int currentlyDisplayedValue;
    protected SingleCoroutine coroutine;

    public void Initialize(CounterDisplayer displayer, Getter getCurrentValue) {
        this.displayer = displayer;
        this.getCurrentValue = getCurrentValue;
        coroutine = new SingleCoroutine(this);
        UpdateValueInstantly();
    }

    public void UpdateValue() {
        coroutine.Start(CUpdateValue());
    }

    public void UpdateValueInstantly() {
        DisplayScore(getCurrentValue());
    }

    private IEnumerator CUpdateValue() {
        Timer timer = new UnpausableTimer(updateDuration);
        int startValue = currentlyDisplayedValue;
        int currentValue = getCurrentValue();
        startValue = startValue <= currentValue ? Mathf.Min(startValue + 1, currentValue) : Mathf.Max(startValue - 1, currentValue);
        while (!timer.IsOver()) {
            float avancement = MathCurves.Power(0, 1, timer.GetAvancement(), 1f / 7.0f);
            int value = Mathf.CeilToInt(MathCurves.Linear(startValue, currentValue, avancement));
            DisplayScore(value);
            yield return null;
        }
        DisplayScore(currentValue);
    }

    protected void DisplayScore(int value) {
        string valueString = ApplyToCreditsFormating(value);
        displayer.Display($"{prefix}{valueString}{suffix}");
        currentlyDisplayedValue = value;
    }

    public string ApplyToCreditsFormating(int value) {
        return useCreditsFormating ? StringHelper.ToCreditsFormat(value) : value.ToString();
    }
}
