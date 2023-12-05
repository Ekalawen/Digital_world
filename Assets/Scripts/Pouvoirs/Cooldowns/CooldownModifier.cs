using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public abstract class CooldownModifier : MonoBehaviour {

    protected GameManager gm;
    protected Cooldown cooldown;

    public virtual void Initialize(Cooldown cooldown) {
        this.gm = GameManager.Instance;
        this.cooldown = cooldown;
    }
}
