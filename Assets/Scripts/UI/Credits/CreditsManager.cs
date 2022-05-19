using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditsManager : MonoBehaviour {

    [Header("Parameters")]
    public float scrollSpeed = 1.0f;
    public float pressKeyScrollSpeed = 2.5f;
    public float delayBeforeFirstText = 2.0f;
    public float returnButtonBlinkDuration = 2.0f;

    [Header("Links")]
    public CreditsCamera creditsCamera;
    public Button returnButton;
    public RectTransform textHolder;
    public RectTransform textHolderParent;
    public TMP_Text mainText;
    public LocalizedTextAsset textAsset;

    protected Fluctuator returnButtonAlphaFluctuator;
    protected CanvasGroup returnButtonCanvasGroup;
    protected bool pointerIsOnReturnButton = false;


    public IEnumerator Start() {
        UISoundManager.Instance.PlayCreditsMusic();
        creditsCamera.Initialize();
        returnButtonCanvasGroup = returnButton.GetComponent<CanvasGroup>();
        returnButtonAlphaFluctuator = new Fluctuator(this,
            () => returnButtonCanvasGroup.alpha,
            (value) => returnButtonCanvasGroup.alpha = value);
        returnButtonCanvasGroup.alpha = 0;
        yield return InitMainTextContent();
        InitMainTextHeight();
        // Particles :3
        // Lier aux EndGamePopups ! :)
        // Traduction
        // Relecture Maman
    }

    protected IEnumerator InitMainTextContent() {
        AsyncOperationHandle<TextAsset> handle = textAsset.LoadAssetAsync();
        yield return handle;
        mainText.text = handle.Result.text;
    }

    private void InitMainTextHeight() {
        float height = -textHolderParent.rect.height - scrollSpeed * delayBeforeFirstText;
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
        float pressKeyCoef = InputManager.Instance.GetAnyKeyOrButton() ? pressKeyScrollSpeed : 1;
        IncreaseMainTextHeight(scrollSpeed * pressKeyCoef * Time.deltaTime);
        if(textHolder.anchoredPosition.y >= textHolder.rect.height) {
            InitMainTextHeight();
        }
    }

    protected void IncreaseMainTextHeight(float offset) {
        SetMainTextHeight(textHolder.anchoredPosition.y + offset);
    }

    protected void SetMainTextHeight(float value) {
        Vector2 anchoredPos = textHolder.anchoredPosition;
        anchoredPos.y = value;
        textHolder.anchoredPosition = anchoredPos;
        LayoutRebuilder.ForceRebuildLayoutImmediate(textHolder);
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
