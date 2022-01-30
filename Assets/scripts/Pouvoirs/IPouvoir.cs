using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

/// <summary>
/// Un pouvoir est un effet que le joueur peut activer en appuyant sur une touche.
/// Il s'agit ici d'une classe abstraite dont hériteront les différents pouvoirs.
/// Le joueur ne pourra normalement posséder qu'un seul pouvoir à la fois =)
/// </summary>
public abstract class IPouvoir : MonoBehaviour {

    public LocalizedString nom;
    public LocalizedString description;
    public Sprite sprite;

    public PouvoirDisplay.PouvoirType pouvoirType = PouvoirDisplay.PouvoirType.DEFAULT;
    public float timerMalus = 0.0f;
    public bool timerMalusTimeProportional = false;
    [ConditionalHide("timerMalusTimeProportional")]
    public float timerMalusTimeProportion = 1.0f / 3.0f;

    public AudioClipParams activationAudioClips;

    protected bool pouvoirEnabled;
    protected bool pouvoirFreezed = false;
    protected GameManager gm;
    protected Player player;
    protected KeyCode binding;
    protected Cooldown cooldown;
    protected PouvoirDisplayInGame pouvoirDisplay;

    public virtual void Initialize() {
        pouvoirEnabled = true;
        gm = FindObjectOfType<GameManager>();
        player = gm.player;
        InitializeCooldown();
    }

    protected void InitializeCooldown() {
        cooldown = GetComponent<Cooldown>();
        if(cooldown == null) {
            Debug.LogWarning($"Un Cooldown par défault a été généré pour le pouvoir {name} ! ;)");
            cooldown = gameObject.AddComponent<Cooldown>();
        }
        cooldown.Initialize();
    }

    // La fonction appelée lorsque le joueur appui sur une touche
    public void TryUsePouvoir(KeyCode binding) {
        this.binding = binding;
        if(IsEnabled() && IsAvailable()) {
            ApplyUsePouvoir();
        } else {
            gm.soundManager.PlayDeniedPouvoirClip();
        }
    }

    protected virtual void ApplyUsePouvoir() {
        if (UsePouvoir()) {
            ApplyUsePouvoirConsequences();
        } else {
            gm.soundManager.PlayDeniedPouvoirClip();
        }
    }

    protected void ApplyUsePouvoirConsequences() {
        cooldown.Use();
        ApplyTimerMalus();
        gm.soundManager.PlayActivationPouvoirClip((activationAudioClips.clips.Count > 0) ? activationAudioClips : null);
    }

    public bool IsEnabled() {
        return pouvoirEnabled && !pouvoirFreezed;
    }

    public virtual bool IsAvailable() {
        return cooldown.IsAvailable();
    }

    protected virtual void ApplyTimerMalus() {
        if (!timerMalusTimeProportional) {
            if (timerMalus != 0.0f) {
                gm.timerManager.RemoveTime(timerMalus, EventManager.DeathReason.POUVOIR_COST);
            }
        } else {
            float timeToRemove = gm.timerManager.GetRemainingTime() * timerMalusTimeProportion;
            if (timeToRemove != 0.0f) {
                gm.timerManager.RemoveTime(timerMalus, EventManager.DeathReason.POUVOIR_COST);
            }
        }
    }

    public bool IsCharging() {
        return cooldown.IsCharging();
    }

    // La véritable fonction qui appelle le pouvoir
    protected abstract bool UsePouvoir();

    public void FreezePouvoir(bool value = true) {
        pouvoirFreezed = value;
    }

    public Cooldown GetCooldown() {
        return cooldown;
    }

    public float GetRemainingCooldown() {
        return cooldown.GetRemainingTimeBeforeUse();
    }

    public void SetCooldownDuration(float duration) {
        cooldown.SetCooldownDuration(duration, keepRemainingTime: false);
    }

    public void SetTimerMalus(float timerMalusValue) {
        timerMalus = timerMalusValue;
        timerMalusTimeProportional = false;
    }

    public void SetPouvoirDisplay(PouvoirDisplayInGame pouvoirDisplay) {
        this.pouvoirDisplay = pouvoirDisplay;
    }

    public PouvoirDisplayInGame GetPouvoirDisplay() {
        return pouvoirDisplay;
    }
}
