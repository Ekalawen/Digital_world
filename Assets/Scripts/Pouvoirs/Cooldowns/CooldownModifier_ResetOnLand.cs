using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class CooldownModifier_ResetOnLand : CooldownModifier {

    public override void Initialize(Cooldown cooldown) {
        base.Initialize(cooldown);
        gm.player.onLand.AddListener(go => OnLand());
    }

    protected void OnLand() {
        cooldown.RechargeEntirely();
    }
}
