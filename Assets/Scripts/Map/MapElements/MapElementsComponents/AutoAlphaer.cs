﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoAlphaer : MonoBehaviour {

    public float intervalTime = 1.2f;
    public float timeToMinAlphaSize = 0.6f;
    public float timeToMaxAlphaSize = 0.6f;
    public float minAlpha = 0.0f;
    public float maxAlpha = 1.0f;

    protected Timer timer;
    protected Vector3 startScale = Vector3.one;
    protected Image image;

    void Start() {
        timer = new Timer(intervalTime);
        timer.SetOver();
        image = GetComponent<Image>();
        if(image == null) {
            Debug.LogWarning("Un AutoAlphaer doit posséder un Image component ! :)");
        }
    }

    void Update() {
        if(timer.IsOver()) {
            StartCoroutine(CChangeAlpha());
            timer.Reset();
        }
    }

    protected IEnumerator CChangeAlpha() {
        Timer toMinAlphaTimer = new Timer(timeToMinAlphaSize);
        while(!toMinAlphaTimer.IsOver()) {
            float avancement = toMinAlphaTimer.GetAvancement();
            ChangeAlpha(MathCurves.Linear(maxAlpha, minAlpha, avancement));
            yield return null;
        }
        Timer toMaxAlphaTimer = new Timer(timeToMaxAlphaSize);
        while(!toMaxAlphaTimer.IsOver()) {
            float avancement = toMaxAlphaTimer.GetAvancement();
            ChangeAlpha(MathCurves.Linear(minAlpha, maxAlpha, avancement));
            yield return null;
        }
        ChangeAlpha(maxAlpha);
    }

    protected void OnDisable() {
        transform.localScale = startScale;
    }

    protected void ChangeAlpha(float newAlpha) {
        Color currentColor = image.color;
        currentColor.a = newAlpha;
        image.color = currentColor;
    }
}