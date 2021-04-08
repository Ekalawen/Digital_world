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

    public float cooldown = 0.0f;
    public float timerMalus = 0.0f;
    public bool timerMalusTimeProportional = false;
    public float timerMalusTimeProportion = 1.0f / 3.0f;

    public AudioClipParams activationAudioClips;

    protected bool pouvoirAvailable;
    protected bool freezePouvoir = false;
    protected GameManager gm;
    protected Player player;
    protected Timer cooldownTimer;
    protected KeyCode binding;

    public virtual void Start() {
        pouvoirAvailable = true;
        gm = FindObjectOfType<GameManager>();
        player = gm.player;
        cooldownTimer = new Timer(cooldown);
        cooldownTimer.SetOver();
    }

    // La fonction appelée lorsque le joueur appui sur une touche
    public void TryUsePouvoir(KeyCode binding) {
        this.binding = binding;
        if(IsAvailable() && IsTimerOver()) {
            if(UsePouvoir()) {
                cooldownTimer.Reset();
                ApplyTimerMalus();
                gm.soundManager.PlayActivationPouvoirClip((activationAudioClips.clips.Count > 0) ? activationAudioClips : null);
            } else {
                gm.soundManager.PlayDeniedPouvoirClip();
            }
        } else {
            gm.soundManager.PlayDeniedPouvoirClip();
        }
    }

    public bool IsAvailable() {
        return pouvoirAvailable && !freezePouvoir;
    }

    public bool IsTimerOver() {
        return cooldownTimer.IsOver();
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

    // La véritable fonction qui appelle le pouvoir
    protected abstract bool UsePouvoir();

    public void FreezePouvoir(bool value = true) {
        freezePouvoir = value;
    }

    public float GetCurrentCooldown() {
        if (cooldownTimer == null)
            return 0.0f;
        return cooldownTimer.GetRemainingTime();
    }

    public void SetCooldown(float cooldownValue) {
        cooldown = cooldownValue;
        cooldownTimer.SetDuree(cooldownValue);
    }

    public void SetTimerMalus(float timerMalusValue) {
        timerMalus = timerMalusValue;
        timerMalusTimeProportional = false;
    }
}
