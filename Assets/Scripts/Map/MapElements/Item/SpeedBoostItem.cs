using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpeedBoostItem : Item {

    [Header("Speed Boost")]
    public SpeedMultiplier speedMultiplier;

    public override void OnTrigger(Collider hit) {
        gm.player.AddMultiplier(speedMultiplier);
    }
}
