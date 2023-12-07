using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class PouvoirHolder : MonoBehaviour {

    [Header("SkillTree Pouvoirs")]
    public GameObject pouvoirFirstDashPrefab;

    [Header("Setted Pouvoirs")]
    public GameObject pouvoirAPrefab; // Le pouvoir de la touche A (souvent la détection)
    public GameObject pouvoirEPrefab; // Le pouvoir de la touche E (souvent la localisation)
    public GameObject pouvoirLeftBoutonPrefab; // Le pouvoir du bouton gauche de la souris
    public GameObject pouvoirRightBoutonPrefab; // Le pouvoir du bouton droit de la souris

    protected GameManager gm;
    protected InputManager inputManager;
    protected IPouvoir pouvoirA; // Le pouvoir de la touche A (souvent la détection)
    protected IPouvoir pouvoirE; // Le pouvoir de la touche E (souvent la localisation)
    protected IPouvoir pouvoirLeftBouton; // Le pouvoir du bouton gauche de la souris
    protected IPouvoir pouvoirRightBouton; // Le pouvoir du bouton droit de la souris

    public void Initialize() {
        gm = GameManager.Instance;
        inputManager = InputManager.Instance;
        InitializePouvoirs();
    }

    public void InitializePouvoirs() {
        InitializeSettedPouvoirs();
        InitializeSkillTreePouvoirs();
    }
    protected void InitializeSettedPouvoirs() {
        pouvoirA = InitializePouvoirWith(pouvoirAPrefab);
        pouvoirE = InitializePouvoirWith(pouvoirEPrefab);
        pouvoirLeftBouton = InitializePouvoirWith(pouvoirLeftBoutonPrefab);
        pouvoirRightBouton = InitializePouvoirWith(pouvoirRightBoutonPrefab);
    }

    protected IPouvoir InitializePouvoirWith(GameObject pouvoirPrefab) {
        if (!pouvoirPrefab) {
            return null;
        }
        IPouvoir newPouvoir = Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
        newPouvoir.Initialize();
        return newPouvoir;
    }

    protected void InitializeSkillTreePouvoirs() {
        if(SkillTreeManager.Instance.IsEnabled(SkillKey.FIRST_DASH)) {
            pouvoirLeftBouton = InitializePouvoirWith(pouvoirFirstDashPrefab);
        }
    }

    public void SetPouvoir(GameObject pouvoirPrefab, PouvoirGiverItem.PouvoirBinding pouvoirBinding)
    {
        switch (pouvoirBinding)
        {
            case PouvoirGiverItem.PouvoirBinding.A:
                pouvoirA = pouvoirPrefab == null ? null : Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
                if (pouvoirA != null) {
                    pouvoirA.Initialize();
                }
                break;
            case PouvoirGiverItem.PouvoirBinding.E:
                pouvoirE = pouvoirPrefab == null ? null : Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
                if (pouvoirE != null)
                {
                    pouvoirE.Initialize();
                }
                break;
            case PouvoirGiverItem.PouvoirBinding.LEFT_CLICK:
                pouvoirLeftBouton = pouvoirPrefab == null ? null : Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
                if (pouvoirLeftBouton != null)
                {
                    pouvoirLeftBouton.Initialize();
                }
                break;
            case PouvoirGiverItem.PouvoirBinding.RIGHT_CLICK:
                pouvoirRightBouton = pouvoirPrefab == null ? null : Instantiate(pouvoirPrefab, parent: this.transform).GetComponent<IPouvoir>();
                if (pouvoirRightBouton != null)
                {
                    pouvoirRightBouton.Initialize();
                }
                break;
        }

        gm.console.InitPouvoirsDisplays();

        // Just kill me
        switch (pouvoirBinding) { 
            case PouvoirGiverItem.PouvoirBinding.A:
                gm.console.ZoomInPouvoir(pouvoirA.GetPouvoirDisplay());
                break;
            case PouvoirGiverItem.PouvoirBinding.E:
                gm.console.ZoomInPouvoir(pouvoirE.GetPouvoirDisplay());
                break;
            case PouvoirGiverItem.PouvoirBinding.LEFT_CLICK:
                gm.console.ZoomInPouvoir(pouvoirLeftBouton.GetPouvoirDisplay());
                break;
            case PouvoirGiverItem.PouvoirBinding.RIGHT_CLICK:
                gm.console.ZoomInPouvoir(pouvoirRightBouton.GetPouvoirDisplay());
                break;
        }
    }

    public void SetPouvoirsCooldownZeroSwap() {
        List<IPouvoir> pouvoirs = new List<IPouvoir>() { pouvoirA, pouvoirE, pouvoirLeftBouton, pouvoirRightBouton };
        if (pouvoirs.FindAll(p => p != null).Select(p => p.GetCooldown().cooldown).Contains(0.0f)) {
            InitializePouvoirs();
            gm.console.InitPouvoirsDisplays();
            gm.pointeur.Initialize();
            gm.console.UnsetPouvoirsCooldownZero();
            gm.soundManager.PlayGetItemClip(transform.position);
        } else {
            foreach (IPouvoir pouvoir in pouvoirs) {
                if (pouvoir != null) {
                    pouvoir.SetCooldownDuration(0.0f);
                    pouvoir.SetTimerMalus(0.0f);
                    NoAutomaticRechargeCooldown noAutomaticCooldown = pouvoir.GetComponent<NoAutomaticRechargeCooldown>();
                    if(noAutomaticCooldown != null) {
                        noAutomaticCooldown.GainMultipleChargeOverMax(99999999);
                    }
                }
            }
            gm.console.SetPouvoirsCooldownZero();
            gm.soundManager.PlayGetItemClip(transform.position);
        }
    }

    public void TryUsePouvoirs() {
        if (gm.eventManager.IsGameOver() || gm.IsPaused())
            return;
        // A
        if(inputManager.GetPouvoirADown()) {
            if (pouvoirA != null)
                pouvoirA.TryUsePouvoir(inputManager.GetPouvoirAKeyCode());
            else
                gm.soundManager.PlayNotFoundPouvoirClip();
        }
        // E
        if(inputManager.GetPouvoirEDown()) {
            if (pouvoirE != null)
                pouvoirE.TryUsePouvoir(inputManager.GetPouvoirEKeyCode());
            else
                gm.soundManager.PlayNotFoundPouvoirClip();
        }
        // Click Gauche
        if(inputManager.GetPouvoirLeftClickDown()) {
            if (pouvoirLeftBouton != null)
                pouvoirLeftBouton.TryUsePouvoir(inputManager.GetPouvoirLeftClickKeyCode());
            else
                gm.soundManager.PlayNotFoundPouvoirClip();
        }
        // Click Droit
        if(inputManager.GetPouvoirRightClickDown()) {
            if (pouvoirRightBouton != null)
                pouvoirRightBouton.TryUsePouvoir(inputManager.GetPouvoirRightClickKeyCode());
            else
                gm.soundManager.PlayNotFoundPouvoirClip();
        }
    }

    public void FreezePouvoirs(bool value = true) {
        pouvoirA?.FreezePouvoir(value);
        pouvoirE?.FreezePouvoir(value);
        pouvoirLeftBouton?.FreezePouvoir(value);
        pouvoirRightBouton?.FreezePouvoir(value);
    }

    public void RemoveAllPouvoirs() {
        SetPouvoir(null, PouvoirGiverItem.PouvoirBinding.A);
        SetPouvoir(null, PouvoirGiverItem.PouvoirBinding.E);
        SetPouvoir(null, PouvoirGiverItem.PouvoirBinding.LEFT_CLICK);
        SetPouvoir(null, PouvoirGiverItem.PouvoirBinding.RIGHT_CLICK);
    }

    public IPouvoir GetPouvoirA() {
        return pouvoirA;
    }

    public IPouvoir GetPouvoirE() {
        return pouvoirE;
    }

    public IPouvoir GetPouvoirLeftClick() {
        return pouvoirLeftBouton;
    }

    public IPouvoir GetPouvoirRightClick() {
        return pouvoirRightBouton;
    }
}
