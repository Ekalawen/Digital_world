using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBouncer : MonoBehaviour {

    public float intervalTime = 0.6f;
    public float timeToBounceSize = 0.1f;
    public float inBounceTime = 0.0f;
    public float timeToNormalSize = 0.2f;
    public float startingTime = 0.0f;
    public float bounceSize = 1.5f;

    protected Timer timer;
    protected Vector3 startScale = Vector3.one;

    void Start() {
        startScale = transform.localScale;
        timer = new Timer(intervalTime);
        timer.SetElapsedTime(startingTime);
        StartCoroutine(CBounce());
    }

    protected void OnEnable() {
        startScale = transform.localScale;
    }

    void Update() {
        if(timer.IsOver()) {
            timer.Reset();
            StartCoroutine(CBounce());
        }
    }

    protected IEnumerator CBounce() {
        while (timer.GetElapsedTime() <= timeToBounceSize) {
            float avancement = timer.GetElapsedTime() / timeToBounceSize;
            SetScale(avancement);
            yield return null;
        }
        SetScale(1.0f);
        while (timer.GetElapsedTime() <= timeToBounceSize + inBounceTime) {
            yield return null;
        }
        while (timer.GetElapsedTime() <= timeToBounceSize + inBounceTime + timeToNormalSize) {
            float avancement = 1 - (timer.GetElapsedTime() - timeToBounceSize - inBounceTime) / timeToNormalSize;
            SetScale(avancement);
            yield return null;
        }
        SetScale(0.0f);
    }

    private void SetScale(float avancement) {
        transform.localScale = Vector3.one * MathCurves.Linear(1, bounceSize, avancement);
        //transform.localScale = startScale * (1 + (bounceSize - 1) * avancement);
    }

    protected void OnDisable() {
        transform.localScale = startScale;
    }
}
