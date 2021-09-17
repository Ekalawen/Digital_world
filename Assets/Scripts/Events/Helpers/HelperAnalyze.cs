using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HelperAnalyze : MonoBehaviour {

    public HelperMessage elleFuitVotreRegardHelper;
    public HelperMessage openDocHelper;
    public HelperMessage revelationMessage;

    protected GameManager gm;
    protected bool hasCapturedFirstData = false;

    public void Start() {
        gm = GameManager.Instance;
    }

    public void Update() {
        if (!hasCapturedFirstData && gm.timerManager.GetElapsedTime() >= elleFuitVotreRegardHelper.GetTiming()) {
            elleFuitVotreRegardHelper.DisplayMessage();
        }
        if (!hasCapturedFirstData && gm.timerManager.GetElapsedTime() >= openDocHelper.GetTiming()) {
            openDocHelper.DisplayMessage();
        }
        if (!hasCapturedFirstData && gm.timerManager.GetElapsedTime() >= revelationMessage.GetTiming()) {
            revelationMessage.DisplayMessage();
        }
    }

    public void SetCaptureFirstData() {
        hasCapturedFirstData = true;
    }
}
