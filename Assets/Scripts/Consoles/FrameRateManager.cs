using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FrameRateManager : MonoBehaviour {

    public Transform withConsoleAnchor;
    public Transform withoutConsoleAnchor;
    public GameObject frameRateVisual;
    public TMP_Text frameRateText;

    protected bool showFrameRate;
    protected List<float> deltaTimeList;
    protected Timer frameRateTimer;

    protected List<float> speedCoefs;

    public void Initialize() {
        deltaTimeList = new List<float>();
        speedCoefs = new List<float>();
        frameRateTimer = new Timer(0.1f, setOver: true);
        showFrameRate = PrefsManager.GetBool(PrefsManager.FPS_COUNTER_KEY, MenuOptions.defaultFpsCounter);
        SetVisibility(showFrameRate);
    }

    public void Tick() {
        //PrintMeanSpeed();
        if (!showFrameRate)
            return;
        if (frameRateTimer.IsOver())
        {
            frameRateTimer.Reset();
            float deltaTime = Time.deltaTime;
            deltaTimeList.Add(deltaTime);
            if (deltaTimeList.Count > 10)
            {
                deltaTimeList.RemoveAt(0);
            }
            float meanDeltaTime = deltaTimeList.Average();
            int fps = Mathf.RoundToInt(1.0f / meanDeltaTime);
            int ms = Mathf.RoundToInt(meanDeltaTime * 1000.0f);
            frameRateText.text = $"FPS : {fps} ({ms}ms)";
        }
    }

    protected void PrintMeanSpeed() {
        speedCoefs.Add(GameManager.Instance.player.GetSpeedMultiplier());
        float meanCoef = speedCoefs.Average();
        Debug.Log($"Vitesse moyenne = {meanCoef}");
    }

    public void SetToAccordingConsolePosition(bool isConsoleDisplayed) {
        if(isConsoleDisplayed) {
            SetToWithConsoleAnchor();
        } else {
            SetToWithoutConsoleAnchor();
        }
    }

    public void SetToWithConsoleAnchor() {
        SetToTransform(withConsoleAnchor);
    }

    public void SetToWithoutConsoleAnchor() {
        SetToTransform(withoutConsoleAnchor);
    }

    protected void SetToTransform(Transform targetTransform) {
        RectTransform rect = frameRateVisual.GetComponent<RectTransform>();
        Vector2 savedLocalPos = rect.localPosition;
        frameRateVisual.transform.SetParent(targetTransform);
        rect.localPosition = savedLocalPos;
    }

    public void SetVisibility(bool isVisible) {
        frameRateVisual.gameObject.SetActive(isVisible);
    }
}
