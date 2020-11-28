using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour {

    protected static Tooltip instance;

    public RectTransform background;
    public TMPro.TMP_Text text;

    public void Awake() {
        instance = this;
        gameObject.SetActive(false);
    }

    public void Update() {
        SetPositionToMouse();
    }

    protected void SetPositionToMouse() {
        Vector2 localPoint;
        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransform screen = transform.parent.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            screen, 
            Input.mousePosition, 
            null, 
            out localPoint);
        rectTransform.localPosition = localPoint;
        Vector2 minPosition = screen.rect.min - rectTransform.rect.min;
        Vector2 maxPosition = screen.rect.max - text.GetPreferredValues();
        Vector2 clampedPos = new Vector2(
            Mathf.Clamp(rectTransform.localPosition.x, minPosition.x, maxPosition.x),
            Mathf.Clamp(rectTransform.localPosition.y, minPosition.y, maxPosition.y));
        rectTransform.localPosition = clampedPos;
    }

    protected void ShowProtected(string message) {
        text.SetText(message);
        background.sizeDelta = text.GetPreferredValues(message);
        SetPositionToMouse();
        gameObject.SetActive(true);
    }

    protected void HideProtected() {
        gameObject.SetActive(false);
    }

    public static void Show(string message) {
        instance.ShowProtected(message);
    }

    public static void Hide() {
        instance.HideProtected();
    }
}
