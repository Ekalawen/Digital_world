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
    public float vitesseRotationLoadingCircle = 5.0f;
    public Text keyText;
    public LocalizedString keyLocalizedString;
    public PouvoirDisplayFlash flash;

    [Header("Bordures Colors")]
    public Color bordureColorDash;
    public Color bordureColorPathfinder;
    public Color bordureColorHack;
    public Color bordureColorReset;
    public Color bordureColorDefault;

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

        SetBordureColor(pouvoir.pouvoirType);

        this.image.sprite = sprite;
        if (sprite != null)
            this.image.color = Color.white; // Sinon c'est transparent !
    }

    protected void SetBordureColor(PouvoirDisplay.PouvoirType pouvoirType) {
        switch (pouvoirType) {
            case PouvoirDisplay.PouvoirType.DASH:
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
