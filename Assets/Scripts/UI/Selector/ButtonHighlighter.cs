using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHighlighter : MonoBehaviour {

    public float intervalTime = 0.5f;
    public float timeToBounceSize = 0.033f;
    public float timeToNormalSize = 0.166f;
    public float bounceSize = 1.125f;

    protected Timer timer;
    protected RectTransform rect;
    protected Vector3 startScale = Vector3.one;

    void Start() {
        timer = new Timer(intervalTime);
        rect = GetComponent<RectTransform>();
        if(rect == null) {
            Debug.LogWarning("Un ButtonHighlighter doit posséder un RectTransform ! ;)");
        }
    }

    void Update() {
        if (timer.IsOver()) {
            StartCoroutine(CBounce());
            timer.Reset();
        }
    }

    protected IEnumerator CBounce() {
        Timer toBounceTimer = new Timer(timeToBounceSize);
        startScale = rect.localScale;
        while (!toBounceTimer.IsOver()) {
            float avancement = toBounceTimer.GetAvancement();
            rect.localScale = startScale * (1 + (bounceSize - 1) * avancement);
            yield return null;
        }
        Timer toNormalTimer = new Timer(timeToNormalSize);
        while (!toNormalTimer.IsOver())
        {
            float avancement = toNormalTimer.GetAvancement();
            rect.localScale = startScale * (1 + (bounceSize - 1) * (1 - avancement));
            yield return null;
        }
        rect.localScale = startScale;
    }

    protected void OnDisable() {
        rect.localScale = startScale;
    }
}
