using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SoulRobberController : EnnemiController {

    public enum SoulRobberState { WANDERING, FIRERING, ESCAPING }; // Don't forgot to teleport ! ;)

    [Header("Mouvement")]

    [Header("Events")]
    public UnityEvent startFireringEvents;
    public UnityEvent stopFireringEvents;

    protected Ennemi ennemi;
    protected SoulRobberState state;
    protected bool hasRobbedPlayer = false;

    public override void Start() {
        base.Start();
        ennemi = GetComponent<Ennemi>();
        SetState(SoulRobberState.WANDERING);
    }

    protected override void UpdateSpecific () {
        SetCurrentState();

        switch(state) {
            case SoulRobberState.WANDERING:
                break;
            case SoulRobberState.FIRERING:
                break;
            case SoulRobberState.ESCAPING:
                break;
        }
	}

    protected void SetCurrentState() {
        if(hasRobbedPlayer) {
            SetState(SoulRobberState.ESCAPING);
        } else {
            if(IsPlayerVisible()) {
                SetState(SoulRobberState.FIRERING);
            } else {
                SetState(SoulRobberState.WANDERING);
            }
        }
    }

    protected void SetState(SoulRobberState newState) {
        SoulRobberState oldState = state;
        state = newState;
        if (newState == SoulRobberState.FIRERING && oldState != SoulRobberState.FIRERING) {
            startFireringEvents.Invoke();
        }
        if (newState != SoulRobberState.FIRERING && oldState == SoulRobberState.FIRERING) {
            stopFireringEvents.Invoke();
        }
    }

    public SoulRobberState GetState() {
        return state;
    }

    public override bool IsInactive() {
        return state == SoulRobberState.WANDERING;
    }

    public override bool IsMoving() {
        return state != SoulRobberState.FIRERING;
    }
}
