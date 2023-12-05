﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PouvoirDisplayInGame : MonoBehaviour {

    public GameObject contentHolder;
    public Image image;
    public Image bordure;
    public GameObject onCooldownGroup;
    public TMP_Text cooldownText;
    public Image loadingCircle;
    public float vitesseRotationLoadingCircle = 5.0f;
    public TMP_Text keyText;
    public MessageZoneBindingParameters.Bindings binding;
    public PouvoirDisplayFlash flash;

    [Header("Bordures Colors")]
    public Color bordureColorDash;
    public Color bordureColorPathfinder;
    public Color bordureColorHack;
    public Color bordureColorReset;
    public Color bordureColorDefault;

    [Header("Text params")]
    public Color defaultTextColor;
    public Color specialTextColor;
    public int defaultFontSize = 30;
    public int specialFontSize = 60;

    protected GameManager gm;
    protected IPouvoir pouvoir;
    protected Cooldown cooldown;

    public void Initialize(IPouvoir pouvoir) {
        gm = GameManager.Instance;
        if (pouvoir == null) {
            contentHolder.gameObject.SetActive(false);
            return;
        }
        StartCoroutine(CInitialize(pouvoir));
    }

    protected IEnumerator CInitialize(IPouvoir pouvoir)
    {
        contentHolder.gameObject.SetActive(true);
        this.pouvoir = pouvoir;
        this.cooldown = pouvoir.GetCooldown();
        string nom = PouvoirDisplay.GetNullName();
        if (pouvoir != null)
        {
            AsyncOperationHandle<string> handleNom = pouvoir.nom.GetLocalizedString();
            yield return handleNom;
            nom = handleNom.Result;
        }
        Sprite sprite = pouvoir ? pouvoir.sprite : null;

        UpdateBinding();

        SetBordureColor(pouvoir.pouvoirType);

        this.image.sprite = sprite;
        if (sprite != null)
            this.image.color = Color.white; // Sinon c'est transparent !
    }

    public void UpdateBinding() {
        string bindingString = InputManager.Instance.GetCurrentInputController().GetStringForBinding(binding);
        keyText.text = UIHelper.SurroundWithColorWithoutB(bindingString, UIHelper.GREEN);
    }

    protected void SetBordureColor(PouvoirDisplay.PouvoirType pouvoirType) {
        switch (pouvoirType) {
            case PouvoirDisplay.PouvoirType.DASH:
                bordure.color = bordureColorDash;
                break;
            case PouvoirDisplay.PouvoirType.GRIP_DASH:
                bordure.color = bordureColorDash;
                break;
            case PouvoirDisplay.PouvoirType.PATHFINDER:
                bordure.color = bordureColorPathfinder;
                break;
            case PouvoirDisplay.PouvoirType.HACK:
                bordure.color = bordureColorHack;
                break;
            case PouvoirDisplay.PouvoirType.RESET:
                bordure.color = bordureColorReset;
                break;
            case PouvoirDisplay.PouvoirType.DEFAULT:
                bordure.color = bordureColorDefault;
                break;
        }
    }

    public void Update() {
        if (pouvoir != null) {
            bool onCooldownGroupOldState = onCooldownGroup.activeInHierarchy;
            if(!pouvoir.IsEnabled() || cooldown.IsCharging()) {
                if(!pouvoir.IsEnabled() || cooldown.cooldown >= 0.1f) { // C'est important de répéter la condition pour pouvoir toujours désactiver les pouvoirs cooldown = 0 !
                    onCooldownGroup.SetActive(true);
                }
            } else {
                onCooldownGroup.SetActive(false);
                if(onCooldownGroupOldState == true) {
                    FlashPouvoirAvailable();
                }
            }

            float rotationAngle = Time.timeSinceLevelLoad * vitesseRotationLoadingCircle % 360;
            loadingCircle.rectTransform.rotation = Quaternion.Euler(0, 0, rotationAngle);

            if (cooldown.ShouldDisplayTextOnPouvoirDisplay() && (cooldown.cooldown >= 0.1f || !pouvoir.IsEnabled())) {
                cooldownText.gameObject.SetActive(true);
                float cooldownTime = Mathf.Max(cooldown.GetTextToDisplayOnPouvoirDisplay(), 0.0f);
                if (cooldown.IsTextToDisplayOnPouvoirDisplayATimer()) {
                    cooldownText.text = TimerManager.TimerToClearString(cooldownTime);
                    cooldownText.color = defaultTextColor;
                    cooldownText.fontSize = defaultFontSize;
                } else {
                    cooldownText.text = ((int)cooldownTime).ToString();
                    cooldownText.color = specialTextColor;
                    cooldownText.fontSize = specialFontSize;
                }
            } else {
                cooldownText.gameObject.SetActive(false);
            }
        }
    }

    public void FlashPouvoirAvailable() {
        flash.Flash();
        gm.soundManager.PlayPouvoirAvailableClip();
    }
}
