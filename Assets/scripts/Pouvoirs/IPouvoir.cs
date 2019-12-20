using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Un pouvoir est un effet que le joueur peut activer en appuyant sur une touche.
/// Il s'agit ici d'une classe abstraite dont hériteront les différents pouvoirs.
/// Le joueur ne pourra normalement posséder qu'un seul pouvoir à la fois =)
/// </summary>
public abstract class IPouvoir : MonoBehaviour {

    public float cooldown = 0.0f;
    public float timerMalus = 0.0f;

    protected bool pouvoirAvailable;
    protected bool freezePouvoir = false;
    protected GameManager gm;
    protected Player player;
    protected Timer cooldownTimer;

    public void Start() {
        pouvoirAvailable = true;
        gm = FindObjectOfType<GameManager>();
        player = gm.player;
        cooldownTimer = new Timer(cooldown);
        cooldownTimer.Enable();
    }

    // La fonction appelée lorsque le joueur appui sur une touche
    public void TryUsePouvoir() {
        if(pouvoirAvailable && !freezePouvoir && cooldownTimer.IsOver()) {
            cooldownTimer.Reset();
            ApplyTimerMalus();
            UsePouvoir();
        }
    }

    protected virtual void ApplyTimerMalus() {
        if (timerMalus != 0.0f)
            gm.timerManager.AddTime(-timerMalus);
    }

    // La véritable fonction qui appelle le pouvoir
    protected abstract void UsePouvoir();

    public void FreezePouvoir() {
        freezePouvoir = true;
    }
}
