using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialTooltip : MonoBehaviour {

    public float dureeDestroy = 0.5f;

    public void OnOkPress() {
        DestroyTooltip();
    }

    protected void DestroyTooltip() {
        StartCoroutine(CDestroyTooltip());
    }

    protected IEnumerator CDestroyTooltip() {
        Timer timer = new Timer(dureeDestroy);
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 initialScale = rect.localScale;
        while(!timer.IsOver()) {
            float avancement = timer.GetAvancement();
            rect.localScale = initialScale * (1 - avancement);
            yield return null;
        }
        Destroy(gameObject);
    }
}
