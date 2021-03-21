using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PouvoirDisplayFlash : MonoBehaviour {

    public float dureeFlash = 0.35f;
    public AnimationCurve flashIntensityCurve;

    protected Image flashImage;

    public void Start() {
        flashImage = GetComponent<Image>();
    }

    public void Flash() {
        StartCoroutine(CFlash());
    }

    protected IEnumerator CFlash() {
        Timer timer = new Timer(dureeFlash);
        while(!timer.IsOver()) {
            float alpha = flashIntensityCurve.Evaluate(timer.GetAvancement());
            SetImageAlpha(alpha);
            yield return null;
        }
        SetImageAlpha(0);
    }

    protected void SetImageAlpha(float alpha) {
        Color color = flashImage.color;
        color.a = alpha;
        flashImage.color = color;
    }
}
