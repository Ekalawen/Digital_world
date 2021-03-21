using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
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
    public PouvoirDisplayFlash flash;

    protected GameManager gm;
    protected IPouvoir pouvoir;

    public void Initialize(IPouvoir pouvoir) {
        gm = GameManager.Instance;
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
        if (pouvoir != null) {
            AsyncOperationHandle<string> handleNom = pouvoir.nom.GetLocalizedString();
            yield return handleNom;
            nom = handleNom.Result;
        }
        Sprite sprite = pouvoir ? pouvoir.sprite : null;

        AsyncOperationHandle<string> handleKeyText = keyLocalizedString.GetLocalizedString();
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
            bool onCooldownGroupOldState = onCooldownGroup.activeInHierarchy;
            if(!pouvoir.IsAvailable() || !pouvoir.IsTimerOver()) {
                if (!pouvoir.IsAvailable() || pouvoir.cooldown >= 0.1f) {// On ne veut pas afficher les cooldown trop courts
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

            float cooldownTime = Mathf.Max(pouvoir.GetCurrentCooldown(), 0.0f);
            cooldown.text = TimerManager.TimerToClearString(cooldownTime);
        }
    }

    protected void FlashPouvoirAvailable() {
        flash.Flash();
        gm.soundManager.PlayPouvoirAvailableClip();
    }
}
