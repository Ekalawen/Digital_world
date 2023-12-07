using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class CooldownModifier_ResetOnLand : CooldownModifier {

    public bool resetWhileOnGround = false;

    protected Player player;

    public override void Initialize(Cooldown cooldown) {
        base.Initialize(cooldown);
        player = gm.player;
        player.onLand.AddListener(go => OnLand());
        if(resetWhileOnGround) {
            StartCoroutine(CResetWhileOnGround());
        }
    }

    protected void OnLand() {
        Reset();
    }

    protected void Reset() {
        cooldown.RechargeEntirely();
    }

    protected void ResetIfGrounded() {
        if (player.IsOnGround()) {
            Reset();
        }
    }

    protected IEnumerator CResetWhileOnGround() {
        while(true) {
            ResetIfGrounded();
            yield return null;
        }
    }
}
