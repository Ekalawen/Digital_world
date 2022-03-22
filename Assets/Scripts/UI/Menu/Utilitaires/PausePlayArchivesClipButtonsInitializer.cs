using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePlayArchivesClipButtonsInitializer : MonoBehaviour {

    public GameObject playButton;
    public GameObject pauseButton;

    public void OnEnable() {
        pauseButton.SetActive(true);
        playButton.SetActive(false);
    }

    public void SetPlayButtonVisible() {
        pauseButton.SetActive(false);
        playButton.SetActive(true);
    }

    public void Disable() {
        pauseButton.SetActive(false);
        playButton.SetActive(false);
    }
}
