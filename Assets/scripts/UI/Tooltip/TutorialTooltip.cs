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

    public string keySuffix;
    public LocalizedString textString;
    public float dureeAnimation = 0.5f;
    public TMP_Text textField;
    public RectTransform background;

    protected TutorialTooltipManager tutorialTooltipManager;

    public void Initialize(TutorialTooltipManager tutorialTooltipManager) {
        this.tutorialTooltipManager = tutorialTooltipManager;
        SetText();
        LocalizationSettings.SelectedLocaleChanged += SetText;
        StartAnimation();
    }

    private void OnDestroy() {
        LocalizationSettings.SelectedLocaleChanged -= SetText;
    }

    public void SetText(Locale l = null) {
        StartCoroutine(CSetText());
    }

    protected IEnumerator CSetText() {
        AsyncOperationHandle<string> handle = textString.GetLocalizedString();
        yield return handle;
        textField.text = handle.Result;
        float buttonOkHeight = 15.0f;
        background.sizeDelta = new Vector2(background.sizeDelta.x, textField.preferredHeight + buttonOkHeight);
    }

    public void OnOkPress() {
        DestroyTooltipAndNotify();
    }

    public void DestroyTooltipAndNotify() {
        StartCoroutine(CDestroyTooltipAndNotify());
    }

    protected IEnumerator CDestroyTooltipAndNotify() {
        Timer timer = new Timer(dureeAnimation);
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 initialScale = rect.localScale;
        while(!timer.IsOver()) {
            float avancement = timer.GetAvancement();
            rect.localScale = initialScale * (1 - avancement);
            yield return null;
        }
        tutorialTooltipManager.NotifyTutorialTooltipPressed(keySuffix);
        Destroy(gameObject);
    }

    protected void StartAnimation() {
        StartCoroutine(CStartAnimation());
    }

    protected IEnumerator CStartAnimation() {
        Timer timer = new Timer(dureeAnimation);
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 initialScale = rect.localScale;
        while (!timer.IsOver()) {
            float avancement = timer.GetAvancement();
            rect.localScale = initialScale * avancement;
            yield return null;
        }
        rect.localScale = initialScale;
    }
}
