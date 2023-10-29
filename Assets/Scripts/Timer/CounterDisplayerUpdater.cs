﻿using System;
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

    protected CounterDisplayer displayer;
    protected Getter getCurrentValue;
    protected int currentlyDisplayedValue;
    protected SingleCoroutine coroutine;

    public void Initialize(CounterDisplayer displayer, Getter getCurrentValue) {
        this.displayer = displayer;
        this.getCurrentValue = getCurrentValue;
        coroutine = new SingleCoroutine(this);
    }

    public void UpdateValue() {
        coroutine.Start(CUpdateValue());
    }

    public void UpdateValueInstantly() {
        DisplayScore(getCurrentValue());
    }

    private IEnumerator CUpdateValue() {
        Timer timer = new Timer(updateDuration);
        int startValue = currentlyDisplayedValue;
        int currentValue = getCurrentValue();
        startValue = Mathf.Min(startValue + 1, currentValue);
        while (!timer.IsOver()) {
            float avancement = MathCurves.QuadraticInverse(0, 1, timer.GetAvancement());
            int value = Mathf.CeilToInt(MathCurves.Linear(startValue, currentValue, avancement));
            DisplayScore(value);
            yield return null;
        }
        DisplayScore(currentValue);
    }

    protected void DisplayScore(int value) {
        displayer.Display($"{prefix}{value}{suffix}");
        currentlyDisplayedValue = value;
    }
}
