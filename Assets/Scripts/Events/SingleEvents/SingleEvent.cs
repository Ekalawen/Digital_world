using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingleEvent : MonoBehaviour {

    protected GameManager gm;
    protected EventManager eventManager;

    public virtual void Start() {
        gm = GameManager.Instance;
        eventManager = gm.eventManager;
    }

    public abstract void Trigger();
}
