using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditsManager : MonoBehaviour {

    [Header("Parameters")]
    public float returnButtonBlinkDuration = 2.0f;

    [Header("Links")]
    public CreditsCamera creditsCamera;
    public Button returnButton;

    protected Fluctuator returnButtonAlphaFluctuator;
    protected CanvasGroup returnButtonCanvasGroup;
    protected bool pointerIsOnReturnButton = false;


    public void Start() {
        creditsCamera.Initialize();
        returnButtonCanvasGroup = returnButton.GetComponent<CanvasGroup>();
        returnButtonAlphaFluctuator = new Fluctuator(this,
            () => returnButtonCanvasGroup.alpha,
            (value) => returnButtonCanvasGroup.alpha = value);
        returnButtonAlphaFluctuator.GoTo(0, returnButtonBlinkDuration);
        // Particles
        // Text défilement
        // Display Return Button
    }

    public void Exit() {
        SceneManager.LoadScene($"MenuScene");
    }

    public void Update() {
        TestBlink();
    }

    public void TestBlink() {
        if (Input.GetKey (KeyCode.Escape) 
         || Input.GetKey(KeyCode.KeypadEnter)
         || Input.GetKey(KeyCode.Return)
         || Input.GetKey(KeyCode.Space)
         || pointerIsOnReturnButton) {
            BlinkReturnButton();
        }
    }

    public void SetPointerOnButtonValue(bool value) {
        pointerIsOnReturnButton = value;
    }

    public void BlinkReturnButton() {
        returnButtonCanvasGroup.alpha = 1.0f;
        returnButtonAlphaFluctuator.GoTo(0, returnButtonBlinkDuration);
    }
}
