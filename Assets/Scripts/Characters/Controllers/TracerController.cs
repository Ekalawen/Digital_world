using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TracerController : EnnemiController {

    public enum TracerState { WAITING, RUSHING, ATTACKING };

    [Header("Mouvement")]
    public float dureePauseEntreNodes = 0.1f;
    public bool goToPosJustBeforePlayer = false;
    public bool doesPathAvoidCubes = false;

    [Header("Attack")]
    public float dureeAttack = 4.0f;
    public UnityEvent startAttackEvents;
    public UnityEvent stopAttackEvents;

    protected TracerState state;
    protected List<Vector3> path;
    protected Timer timerNodePause;

    protected bool bIsStuck = false;
    protected Vector3 lastPosition;
    protected Timer timerStuck;
    protected float dureeMaxStuck = 0.1f;
    protected Coroutine stopAttackCoroutine = null;

    public override void Start() {
        base.Start();
        SetState(TracerState.WAITING);
        timerNodePause = new Timer(dureePauseEntreNodes, setOver: true);
        timerStuck = new Timer(dureeMaxStuck, setOver: true);
    }

    protected override void UpdateSpecific () {
        SetCurrentEtat();

        switch(state) {
            case TracerState.WAITING:
                break;
            case TracerState.RUSHING:
                if(path.Count == 0) { // Car on peut générer des chemins vides si on est très proche de la cible
                    SetState(TracerState.ATTACKING);
                    break;
                }
                if(Vector3.Distance(transform.position, path[0]) < 0.001f) {
                    path.RemoveAt(0);
                    timerNodePause.Reset();
                }
                if(path.Count == 0) {
                    SetState(TracerState.ATTACKING);
                    break;
                }

                // On va au premier point du chemin !
                if (timerNodePause.IsOver()) {
                    Vector3 move = MoveToTarget(path[0]);

                    // On essaye de se débloquer si on est bloqué !
                    TryUnStuck();
                }
                break;
            case TracerState.ATTACKING:
                break;
        }
        lastPosition = transform.position;
	}

    protected void TryUnStuck() {
        if(transform.position == lastPosition) {
            if (!bIsStuck) {
                timerStuck.Reset();
                bIsStuck = true;
            }
            if(bIsStuck && timerStuck.IsOver()) {
                ComputePath(path.Last()); // On va au même endroit que précédemment !
                bIsStuck = false;
            }
        }
    }

    protected void SetCurrentEtat() {
        if(state == TracerState.WAITING) {
            if (IsPlayerVisible()) {
                RushToPlayer();
            }
        }
    }

    public void RushToPlayer() {
        StopAttacking();
        SetState(TracerState.RUSHING);
    }

    protected virtual void ComputePath(Vector3 end) {
        Vector3 start = MathTools.Round(transform.position);
        end = MathTools.Round(end);
        List<Vector3> posToDodge = gm.ennemiManager.GetAllRoundedEnnemisPositions();
        if (doesPathAvoidCubes) {
            path = gm.map.GetPath(start, end, posToDodge, bIsRandom: true, useNotInMapVoisins: true);
        } else {
            path = gm.map.GetStraitPath(start, end, isDeterministic: false);
        }

        if (path == null) {
            SetState(TracerState.ATTACKING);
        } else {
            if (goToPosJustBeforePlayer) {
                path.RemoveAt(path.Count - 1);
            }
        }
    }

    protected void SetState(TracerState newState) {
        TracerState oldState = state;
        state = newState;
        if(newState == TracerState.ATTACKING && oldState != TracerState.ATTACKING) {
            startAttackEvents.Invoke();
            stopAttackCoroutine = StartCoroutine(CStopAttackingIn(dureeAttack));
        }
        if (newState == TracerState.RUSHING && oldState != TracerState.RUSHING) {
            StopAttacking();
            ComputePath(player.transform.position);
            gm.soundManager.PlayDetectionClip(transform.position, transform);
        }
    }

    protected void StopAttacking() {
        Debug.Log($"StopAttackingEvent");
        stopAttackEvents.Invoke();
        if (stopAttackCoroutine != null) {
            StopCoroutine(stopAttackCoroutine);
        }
    }

    protected IEnumerator CStopAttackingIn(float duree) {
        yield return new WaitForSeconds(duree);
        StopAttacking();
        if (!IsPlayerVisible()) {
            SetState(TracerState.WAITING);
        } else {
            SetState(TracerState.RUSHING);
        }
    }

    public override bool IsInactive() {
        return state == TracerState.WAITING;
    }

    public override bool IsMoving() {
        return !IsInactive();
    }
}
