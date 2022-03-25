using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HelperFirstSonde : MonoBehaviour {

    public HelperMessage readDocMessage;
    public HelperMessage touchSondeMessage;
    public HelperMessage jumpOnSondeMessage;

    protected GameManager gm;
    protected bool hasBeenHit = false;

    public void Start() {
        gm = GameManager.Instance;
        gm.player.RegisterOnHit(new UnityAction(SetHitted));
    }

    public void Update() {
        float dureePartie = gm.timerManager.initialTime;
        if (gm.timerManager.GetRemainingTime() <= dureePartie - readDocMessage.GetTiming()) {
            readDocMessage.DisplayMessage();
        }
        if (!hasBeenHit && gm.timerManager.GetRemainingTime() <= dureePartie - touchSondeMessage.GetTiming()) {
            touchSondeMessage.DisplayMessage();
        }
        if (gm.timerManager.GetRemainingTime() <= dureePartie - jumpOnSondeMessage.GetTiming()) {
            jumpOnSondeMessage.DisplayMessage();
        }
    }

    public void SetHitted() {
        hasBeenHit = true;
    }
}
