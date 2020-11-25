using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBouncer : MonoBehaviour {

    public float intervalTime = 0.6f;
    public float timeToBounceSize = 0.1f;
    public float timeToNormalSize = 0.2f;
    public float bounceSize = 1.5f;

    protected Timer timer;

    void Start() {
        timer = new Timer(intervalTime);
    }

    void Update() {
        if(timer.IsOver()) {
            StartCoroutine(CBounce());
            timer.Reset();
        }
    }

    protected IEnumerator CBounce() {
        Timer toBounceTimer = new Timer(timeToBounceSize);
        Vector3 startScale = transform.localScale;
        while(!toBounceTimer.IsOver()) {
            float avancement = toBounceTimer.GetAvancement();
            transform.localScale = startScale * (1 + (bounceSize - 1) * avancement);
            yield return null;
        }
        Timer toNormalTimer = new Timer(timeToNormalSize);
        while(!toNormalTimer.IsOver()) {
            float avancement = toNormalTimer.GetAvancement();
            transform.localScale = startScale * (1 + (bounceSize - 1) * (1 - avancement));
            yield return null;
        }
        transform.localScale = startScale;
    }
}
