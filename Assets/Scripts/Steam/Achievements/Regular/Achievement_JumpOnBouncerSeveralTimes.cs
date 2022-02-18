using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_JumpOnBouncerSeveralTimes : Achievement {

    [Header("Parameters")]
    public int nbTimesToJumpOnBouncer = 5;

    protected List<BouncyCube> bouncyCubesUsed = new List<BouncyCube>();

    protected override void InitializeSpecific() {
        gm.player.onBouncerJump.AddListener(IncrementCount);
        gm.player.onLand.AddListener(ResetCountIfDontLandOnBouncer);
        gm.player.onGrip.AddListener(ResetCount);
    }

    protected void IncrementCount(BouncyCube bouncyCube) {
        if(!bouncyCubesUsed.Contains(bouncyCube)) {
            bouncyCubesUsed.Add(bouncyCube);
        }
        if(bouncyCubesUsed.Count >= nbTimesToJumpOnBouncer) {
            Unlock();
        }
    }

    protected void ResetCount() {
        bouncyCubesUsed.Clear();
    }

    protected void ResetCountIfDontLandOnBouncer(GameObject thingLandedOn) {
        if(thingLandedOn.GetComponent<BouncyCube>() == null) {
            ResetCount();
        }
    }
}
