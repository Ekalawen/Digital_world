using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VerticalMenuHandler : MonoBehaviour {

    [Header("Parameters")]
    public float openPercentage = 0.25f;

    [Header("Links")]
    public LayoutElement verticalMenuLayout;
    public CanvasScaler canvasScaler;

    [Header("Animations")]
    public float openTime = 0.4f;
    public float closeTime = 0.2f;
    public AnimationCurve openCurve;
    public AnimationCurve closeCurve;

    protected Fluctuator verticalMenuPercentageFluctuator;
    protected bool isOpen = false;

    public void Initialize() {
        verticalMenuPercentageFluctuator = new Fluctuator(this, GetVerticalMenuPercentage, SetVerticalMenuPercentage);
        Close(instantClose: true);
    }

    public float GetVerticalMenuPercentage() {
        float verticalMenuWidth = verticalMenuLayout.preferredWidth;
        return verticalMenuWidth / canvasScaler.referenceResolution.x;
    }

    public void SetVerticalMenuPercentage(float percentageValue) {
        float pixelValue = canvasScaler.referenceResolution.x * percentageValue;
        verticalMenuLayout.preferredWidth = pixelValue;
    }

    public void Open(bool instantOpen = false) {
        isOpen = true;
        verticalMenuPercentageFluctuator.GoTo(openPercentage, instantOpen ? 0 : openTime, openCurve);
    }

    public void Close(bool instantClose = false) {
        isOpen = false;
        verticalMenuPercentageFluctuator.GoTo(0, instantClose ? 0 : closeTime, closeCurve);
    }

    public bool IsOpen() {
        return isOpen;
    }
}
