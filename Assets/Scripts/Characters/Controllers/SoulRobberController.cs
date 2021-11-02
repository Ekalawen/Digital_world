using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SoulRobberController : EnnemiController {

    public enum SoulRobberState { WANDERING, FIRERING, ESCAPING }; // Don't forgot to teleport ! ;)

    [Header("Mouvement")]
    public RunAwayPlayerController runAwayController;

    [Header("Events")]
    public UnityEvent startFireringEvents;
    public UnityEvent stopFireringEvents;
    public UnityEvent startEscaping;
    public UnityEvent stopEscaping;

    protected Ennemi ennemi;
    protected SoulRobberState state;

    public override void Start() {
        base.Start();
        ennemi = GetComponent<Ennemi>();
        runAwayController.enabled = false;
        SetState(SoulRobberState.WANDERING);
    }

    protected override void UpdateSpecific () {
        SetCurrentState();

        //MoveToTarget(player.transform.position);

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
        if(SoulRobber.IsPlayerRobbed()) {
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
        if(newState == SoulRobberState.ESCAPING && oldState != SoulRobberState.ESCAPING) {
            startEscaping.Invoke();
            runAwayController.enabled = true;
        }
        if(newState != SoulRobberState.ESCAPING && oldState == SoulRobberState.ESCAPING) {
            stopEscaping.Invoke();
            runAwayController.enabled = false;
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
