using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

public class FullScreenSwapper : MonoBehaviour {

    public KeyCode key = KeyCode.F;
    public bool startFullScreen = true;

    protected bool isFullScreen = false;

    private void Start() {
        if(startFullScreen) {
            StartFullScreen();
        }
    }

    protected void Update() {
        if(Input.GetKeyDown(key)) {
            if(isFullScreen) {
                StopFullScreen();
            } else {
                StartFullScreen();
            }
        }
    }

    protected void StartFullScreen() {
        FullscreenGameView.StartFullScreen();
        isFullScreen = true;
    }

    protected void StopFullScreen() {
        FullscreenGameView.StopFullScreen();
        isFullScreen = false;
    }
}