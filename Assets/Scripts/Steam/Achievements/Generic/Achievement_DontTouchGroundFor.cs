using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_DontTouchGroundFor : Achievement {

    [Header("Parameters")]
    public float duration = 30.0f;

    protected Timer timer;

    protected override void InitializeSpecific() {
        timer = new Timer(duration);
        timer.Stop(); // We need to jump at least once !
        gm.player.onLand.AddListener(OnLand);
    }

    protected void OnLand(GameObject thingLandedOn) {
        if(timer.IsOver()) {
            Unlock();
        }
        timer.UnStop(); // We need to jump at least once !
        if (thingLandedOn.GetComponent<Cube>() != null) { // We want to be able to be touched by Sondes ! :)
            timer.Reset();
        }
    }
}
