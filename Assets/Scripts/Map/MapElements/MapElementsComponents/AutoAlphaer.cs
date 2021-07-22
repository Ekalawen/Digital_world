using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutoAlphaer : MonoBehaviour {

    public float intervalTime = 1.2f;
    public float timeToMinAlphaSize = 0.6f;
    public float timeToMaxAlphaSize = 0.6f;
    public float minAlpha = 0.0f;
    public float maxAlpha = 1.0f;
    public bool useCurves = false;
    [ConditionalHide("useCurves")]
    public AnimationCurve toMinCurve;
    [ConditionalHide("useCurves")]
    public AnimationCurve toMaxCurve;

    protected Timer timer;
    protected Vector3 startScale = Vector3.one;
    protected Image image;
    protected TMP_Text text;

    void Start() {
        timer = new Timer(intervalTime);
        timer.SetOver();
        image = GetComponent<Image>();
        if(image == null) {
            text = GetComponent<TMP_Text>();
            if (text == null) {
                Debug.LogWarning("Un AutoAlphaer doit posséder un Image component OU un TMP_Text component ! :)");
            }
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
            if(useCurves) {
                avancement = toMinCurve.Evaluate(avancement);
            }
            ChangeAlpha(MathCurves.Linear(maxAlpha, minAlpha, avancement));
            yield return null;
        }
        Timer toMaxAlphaTimer = new Timer(timeToMaxAlphaSize);
        while(!toMaxAlphaTimer.IsOver()) {
            float avancement = toMaxAlphaTimer.GetAvancement();
            if(useCurves) {
                avancement = toMaxCurve.Evaluate(avancement);
            }
            ChangeAlpha(MathCurves.Linear(minAlpha, maxAlpha, avancement));
            yield return null;
        }
        ChangeAlpha(maxAlpha);
    }

    protected void OnDisable() {
        transform.localScale = startScale;
    }

    protected void ChangeAlpha(float newAlpha) {
        if (image != null) {
            Color currentColor = image.color;
            currentColor.a = newAlpha;
            image.color = currentColor;
        } else if (text != null) {
            Color currentColor = text.color;
            currentColor.a = newAlpha;
            text.color = currentColor;
        }
    }
}
