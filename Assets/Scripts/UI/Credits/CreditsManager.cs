using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditsManager : MonoBehaviour {

    [Header("Parameters")]
    public float scrollSpeed = 1.0f;
    public float delayBeforeFirstText = 2.0f;
    public float returnButtonBlinkDuration = 2.0f;

    [Header("Links")]
    public CreditsCamera creditsCamera;
    public Button returnButton;
    public RectTransform mainText;
    public RectTransform mainTextParent;

    protected Fluctuator returnButtonAlphaFluctuator;
    protected CanvasGroup returnButtonCanvasGroup;
    protected bool pointerIsOnReturnButton = false;


    public void Start()
    {
        creditsCamera.Initialize();
        returnButtonCanvasGroup = returnButton.GetComponent<CanvasGroup>();
        returnButtonAlphaFluctuator = new Fluctuator(this,
            () => returnButtonCanvasGroup.alpha,
            (value) => returnButtonCanvasGroup.alpha = value);
        returnButtonCanvasGroup.alpha = 0;
        InitMainTextHeight();
        // Particles
        // Text défilement
        // Display Return Button
    }

    private void InitMainTextHeight() {
        //Canvas.ForceUpdateCanvases(); // Because the ContentSizeFitter is not calculated yet!
        float height = -mainTextParent.rect.height - scrollSpeed * delayBeforeFirstText;
        SetMainTextHeight(height);
    }

    public void Exit() {
        SceneManager.LoadScene($"MenuScene");
    }

    public void Update() {
        TestBlink();
        ScrollMainText();
    }

    protected void ScrollMainText() {
        IncreaseMainTextHeight(scrollSpeed * Time.deltaTime);
        if(mainText.anchoredPosition.y >= mainText.rect.height) {
            InitMainTextHeight();
        }
    }

    protected void IncreaseMainTextHeight(float offset) {
        SetMainTextHeight(mainText.anchoredPosition.y + offset);
    }

    protected void SetMainTextHeight(float value) {
        Vector2 anchoredPos = mainText.anchoredPosition;
        anchoredPos.y = value;
        mainText.anchoredPosition = anchoredPos;
        LayoutRebuilder.ForceRebuildLayoutImmediate(mainText);
    }

    public void TestBlink() {
        if (InputManager.Instance.GetAnyKeyOrButtonDown()
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
