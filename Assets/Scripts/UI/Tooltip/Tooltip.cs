using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour {

    protected static Tooltip instance;

    public bool useOverlay = false;
    public RectTransform background;
    public TMPro.TMP_Text text;
    public new Camera camera;
    public RectTransform screen;

    protected float planeDistance;
    protected Timer lastHideTime;

    public void Awake() {
        instance = this;
        gameObject.SetActive(false);
        planeDistance = FindObjectOfType<Canvas>().planeDistance;
        lastHideTime = new UnpausableTimer();
        lastHideTime.SetOver();
    }

    public void Update() {
        SetPositionToMouse();
    }

    protected void SetPositionToMouse() {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (useOverlay) {
            Vector2 localPoint2D;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(screen, Input.mousePosition, null, out localPoint2D);
            rectTransform.localPosition = localPoint2D;
        } else {
            Vector3 localPoint;
            Vector3 screenPoint = Input.mousePosition;
            screenPoint.z = planeDistance;
            localPoint = camera.ScreenToWorldPoint(screenPoint);
            rectTransform.position = localPoint;
        }


        Vector2 minPosition = screen.rect.min - rectTransform.rect.min;
        Vector2 maxPosition = screen.rect.max - text.GetPreferredValues();
        Vector2 clampedPos = new Vector2(
            Mathf.Clamp(rectTransform.localPosition.x, minPosition.x, maxPosition.x),
            Mathf.Clamp(rectTransform.localPosition.y, minPosition.y, maxPosition.y));
        rectTransform.localPosition = clampedPos;
    }

    protected void ShowProtected(string message, float timeBeforeShowingUsed) {
        if(lastHideTime.GetElapsedTime() < timeBeforeShowingUsed) {
            return;
        }
        text.SetText(message);
        background.sizeDelta = text.GetPreferredValues(message);
        SetPositionToMouse();
        gameObject.SetActive(true);
    }

    protected void HideProtected() {
        gameObject.SetActive(false);
        lastHideTime.Reset();
    }

    public static void Show(string message, float timeBeforeShowingUsed) {
        instance.ShowProtected(message, timeBeforeShowingUsed);
    }

    public static void Hide() {
        instance.HideProtected();
    }
}
