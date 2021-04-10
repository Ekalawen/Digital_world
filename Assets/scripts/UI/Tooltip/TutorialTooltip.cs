using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class TutorialTooltip : MonoBehaviour {

    public LocalizedString textString;
    public float dureeDestroy = 0.5f;
    public TMP_Text textField;

    public void Start() {
        SetText();
        LocalizationSettings.SelectedLocaleChanged += SetText;
    }

    public void SetText(Locale l = null) {
        StartCoroutine(CSetText());
    }

    protected IEnumerator CSetText() {
        AsyncOperationHandle<string> handle = textString.GetLocalizedString();
        yield return handle;
        textField.text = handle.Result;
    }

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
