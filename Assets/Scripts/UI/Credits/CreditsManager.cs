using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VFX;

public class CreditsManager : MonoBehaviour {

    [Header("Parameters")]
    public float scrollSpeed = 1.0f;
    public float pressKeyScrollSpeed = 2.5f;
    public float delayBeforeFirstText = 2.0f;
    public float delayBeforeLoopingText = 5.0f;
    public float returnButtonBlinkDuration = 2.0f;
    public float musicFadeInDuration = 3.0f;
    public float particlesDelay = 5.0f;

    [Header("Links")]
    public CreditsCamera creditsCamera;
    public Button returnButton;
    public RectTransform textHolder;
    public RectTransform textHolderParent;
    public TMP_Text mainText;
    public LocalizedTextAsset textAsset;
    public VisualEffect vfx;

    protected Fluctuator returnButtonAlphaFluctuator;
    protected Fluctuator musicVolumeFluctuator;
    protected CanvasGroup returnButtonCanvasGroup;
    protected bool pointerIsOnReturnButton = false;


    public IEnumerator Start() {
        InitVfx();
        StartMusic();
        creditsCamera.Initialize();
        returnButtonCanvasGroup = returnButton.GetComponent<CanvasGroup>();
        returnButtonAlphaFluctuator = new Fluctuator(this,
            () => returnButtonCanvasGroup.alpha,
            (value) => returnButtonCanvasGroup.alpha = value);
        returnButtonCanvasGroup.alpha = 0;
        yield return InitMainTextContent();
        InitMainTextHeight();
    }

    protected void InitVfx() {
        StartCoroutine(CInitVfx());
    }

    protected IEnumerator CInitVfx() {
        vfx.Stop();
        yield return new WaitForSeconds(particlesDelay);
        vfx.Play();
    }

    protected void StartMusic() {
        UISoundManager soundManager = UISoundManager.Instance;
        soundManager.PlayCreditsMusic();
        musicVolumeFluctuator = new Fluctuator(this, soundManager.GetMusicVolume, soundManager.SetMusicVolume);
        musicVolumeFluctuator.GoTo(PrefsManager.GetFloat(PrefsManager.MUSIC_VOLUME_KEY, MenuOptions.defaultMusicVolume), musicFadeInDuration);
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

    private void InitMainTextHeightForLooping() {
        float height = -textHolderParent.rect.height - scrollSpeed * delayBeforeLoopingText;
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
            InitMainTextHeightForLooping();
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
