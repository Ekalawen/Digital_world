using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_DeathReason : Achievement {

    [Header("Parameters")]
    public EventManager.DeathReason deathReason;

    protected override void InitializeSpecific() {
        gm.eventManager.onLoseGame.AddListener(OnLoseGame);
    }

    public void OnLoseGame(EventManager.DeathReason reason) {
        if(reason == deathReason) {
            Unlock();
        }
    }
}