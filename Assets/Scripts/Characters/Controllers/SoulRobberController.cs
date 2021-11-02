using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SoulRobberController : EnnemiController {

    public enum SoulRobberState { WANDERING, FIRERING, ESCAPING }; // Don't forgot to teleport ! ;)

    [Header("Mouvement")]
    public Vector2 wanderingTeleportWaitingTimeRange = new Vector2(5, 15);
    public RunAwayPlayerController runAwayController;

    [Header("Events")]
    public UnityEvent startFireringEvents;
    public UnityEvent stopFireringEvents;
    public UnityEvent startEscapingEvents;
    public UnityEvent stopEscapingEvents;
    public UnityEvent shouldTeleportEvents;

    protected Ennemi ennemi;
    protected SoulRobberState state;
    protected Timer wanderingTeleportWaitingTimeTimer;

    public override void Start() {
        base.Start();
        ennemi = GetComponent<Ennemi>();
        runAwayController.enabled = false;
        InitWanderingTimer();
        SetState(SoulRobberState.WANDERING);
    }

    protected override void UpdateSpecific () {
        SetCurrentState();

        switch(state) {
            case SoulRobberState.WANDERING:
                if(wanderingTeleportWaitingTimeTimer.IsOver()) {
                    Debug.Log($"ElapsedTime = {wanderingTeleportWaitingTimeTimer.GetElapsedTime()}/{wanderingTeleportWaitingTimeTimer.GetDuree()}");
                    shouldTeleportEvents.Invoke();
                    InitWanderingTimer();
                }
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
        if(newState == SoulRobberState.WANDERING && oldState != SoulRobberState.WANDERING) {
            InitWanderingTimer();
        }
        if(newState != SoulRobberState.ESCAPING && oldState == SoulRobberState.ESCAPING) {
            stopEscapingEvents.Invoke();
            runAwayController.enabled = false;
        }
        if(newState == SoulRobberState.ESCAPING && oldState != SoulRobberState.ESCAPING) {
            startEscapingEvents.Invoke();
            runAwayController.enabled = true;
        }
    }

    protected void InitWanderingTimer() {
        wanderingTeleportWaitingTimeTimer = new Timer(UnityEngine.Random.Range(wanderingTeleportWaitingTimeRange.x, wanderingTeleportWaitingTimeRange.y));
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
