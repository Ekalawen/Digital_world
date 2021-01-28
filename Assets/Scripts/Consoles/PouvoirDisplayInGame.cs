﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class PouvoirDisplayInGame : MonoBehaviour {

    public Image image;
    public Image bordure;
    public GameObject onCooldownGroup;
    public Text cooldown;
    public Image loadingCircle;
    public Color bordureColorActive;
    public Color bordureColorSpecial;
    public float vitesseRotationLoadingCircle = 5.0f;
    public Text keyText;
    public LocalizedString keyLocalizedString;

    protected IPouvoir pouvoir;

    public void Initialize(IPouvoir pouvoir) {
        if (pouvoir == null) {
            bordure.gameObject.SetActive(false);
            return;
        }
        StartCoroutine(CInitialize(pouvoir));
    }

    protected IEnumerator CInitialize(IPouvoir pouvoir) {
        bordure.gameObject.SetActive(true);
        this.pouvoir = pouvoir;
        string nom = PouvoirDisplay.GetNullName();
        string description = PouvoirDisplay.GetNullDescription();
        if (pouvoir != null) {
            var handleNom = pouvoir.nom.GetLocalizedString();
            var handleDescrition = pouvoir.description.GetLocalizedString();
            yield return handleNom;
            yield return handleDescrition;
            nom = handleNom.Result;
            description = handleDescrition.Result;
        }
        Sprite sprite = pouvoir ? pouvoir.sprite : null;

        var handleKeyText = keyLocalizedString.GetLocalizedString();
        yield return handleKeyText;
        keyText.text = keyText.text.Replace("%PouvoirName%", handleKeyText.Result);

        if (nom != PouvoirDisplay.GetNullName()) {
            if (nom != "PathFinder" && nom != "Localisateur")
                bordure.color = bordureColorSpecial;
            else
                bordure.color = bordureColorActive;
        }
        this.image.sprite = sprite;
        if (sprite != null)
            this.image.color = Color.white; // Sinon c'est transparent !
    }

    public void Update() {
        if (pouvoir != null) {
            if(!pouvoir.IsAvailable() || !pouvoir.IsTimerOver()) {
                if(!pouvoir.IsAvailable() || pouvoir.cooldown >= 0.1f) // On ne veut pas afficher les cooldown trop courts
                    onCooldownGroup.SetActive(true);
            } else {
                onCooldownGroup.SetActive(false);
            }

            float rotationAngle = Time.timeSinceLevelLoad * vitesseRotationLoadingCircle % 360;
            loadingCircle.rectTransform.rotation = Quaternion.Euler(0, 0, rotationAngle);

            float cooldownTime = Mathf.Max(pouvoir.GetCurrentCooldown(), 0.0f);
            cooldown.text = TimerManager.TimerToClearString(cooldownTime);
        }
    }
}
