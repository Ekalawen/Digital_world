using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    protected List<TooltipActivator> activatedActivators;
    protected SingleCoroutine alwaysTriesToHideCoroutine;

    public void Awake() {
        instance = this;
        gameObject.SetActive(false);
        activatedActivators = new List<TooltipActivator>();
        alwaysTriesToHideCoroutine = new SingleCoroutine(this);
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

    protected void ShowProtected(string message, TooltipActivator activator) {
        if(activatedActivators.Contains(activator)) {
            return;
        }
        activatedActivators.Add(activator);
        text.SetText(message);
        background.sizeDelta = text.GetPreferredValues(message);
        SetPositionToMouse();
        gameObject.SetActive(true);
        alwaysTriesToHideCoroutine.Start(CAlwaysTriesToHideCoroutine());
    }

    protected IEnumerator CAlwaysTriesToHideCoroutine() {
        while(true) {
            yield return null;
            TooltipActivator lastActivator = activatedActivators.Last();
            if(!lastActivator.IsHovering()) {
                HideProtected(lastActivator);
            }
        }
    }

    protected void HideProtected(TooltipActivator activator) {
        if(!activatedActivators.Contains(activator)) {
            return;
        }
        if(activatedActivators.Last() != activator) {
            activatedActivators.Remove(activator);
            return;
        }
        activatedActivators.Remove(activator);
        gameObject.SetActive(false);
        lastHideTime.Reset();
        ShowNextActivator();
    }

    protected void ShowNextActivator() {
        if(activatedActivators.Count <= 0) {
            alwaysTriesToHideCoroutine.Stop();
            return;
        }
        TooltipActivator lastActivator = activatedActivators.Last();
        if(lastActivator.IsHovering()) {
            activatedActivators.Remove(lastActivator);
            lastActivator.ShowImmediate();
        } else {
            activatedActivators.Remove(lastActivator);
            ShowNextActivator();
        }
    }

    public static void Show(string message, TooltipActivator activator) {
        instance.ShowProtected(message, activator);
    }

    public static void Hide(TooltipActivator activator) {
        instance.HideProtected(activator);
    }

    public static void HideAll() {
        instance.HideAllProtected();
    }

    protected void HideAllProtected() {
        List<TooltipActivator> activators = activatedActivators.Select(a => a).ToList();
        foreach (TooltipActivator activator in activators) {
            Hide(activator);
        }
    }

}
