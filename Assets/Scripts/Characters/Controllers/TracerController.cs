using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TracerController : EnnemiController {

    public enum TracerState { WAITING, RUSHING, EMITING };

    public float dureeEmiting = 4.0f;
    public float dureePauseEntreNodes = 0.1f;
    public bool goToPosJustBeforePlayer = false;
    public UnityEvent startEmitingEvents;
    public UnityEvent stopEmitingEvents;

    protected TracerState state;
    protected List<Vector3> path;
    protected float lastTimePause;

    protected bool bIsStuck = false;
    protected Vector3 lastPosition;
    protected float debutStuck;
    protected float dureeMaxStuck = 0.1f;
    protected Coroutine stopEmitingCoroutine = null;

    public override void Start() {
        base.Start();
        SetState(TracerState.WAITING);
        lastTimePause = Time.timeSinceLevelLoad;
    }

    protected override void UpdateSpecific () {
        GetEtat();

        switch(state) {
            case TracerState.WAITING:
                break;
            case TracerState.RUSHING:
                if(Vector3.Distance(transform.position, path[0]) < 0.001f) {
                    path.RemoveAt(0);
                    lastTimePause = Time.timeSinceLevelLoad;
                }
                if(path.Count == 0) {
                    SetState(TracerState.EMITING);
                    return;
                }

                // On va au premier point du chemin !
                if (Time.timeSinceLevelLoad - lastTimePause > dureePauseEntreNodes) {
                    Vector3 move = Move(path[0]);

                    // On essaye de se débloquer si on est bloqué !
                    TryUnStuck();
                }
                break;
            case TracerState.EMITING:
                break;
        }
        lastPosition = transform.position;
	}

    protected void TryUnStuck() {
        if(transform.position == lastPosition) {
            if (!bIsStuck) {
                debutStuck = Time.timeSinceLevelLoad;
                bIsStuck = true;
            }
            if(bIsStuck && Time.timeSinceLevelLoad - debutStuck > dureeMaxStuck) {
                ComputePath(path[path.Count - 1]); // On va au même endroit que précédemment !
                bIsStuck = false;
            }
        }
    }

    void GetEtat() {
        if(state == TracerState.WAITING) {
            if (IsPlayerVisible()) {
                DetectPlayer();
            }
        }
    }

    public void DetectPlayer() {
        StopEmiting();
        SetState(TracerState.RUSHING);
        ComputePath(player.transform.position);
    }

    protected virtual void ComputePath(Vector3 end) {
        Vector3 start = MathTools.Round(transform.position);
        end = MathTools.Round(end);
        List<Vector3> posToDodge = gm.ennemiManager.GetAllRoundedEnnemisPositions();
        path = gm.map.GetPath(start, end, posToDodge, bIsRandom: true);
        if (goToPosJustBeforePlayer)
            path.RemoveAt(path.Count - 1);
        if(path == null) {
            SetState(TracerState.EMITING);
        }
    }

    protected void SetState(TracerState newState) {
        TracerState oldState = state;
        state = newState;
        if(newState == TracerState.EMITING && oldState != TracerState.EMITING) {
            startEmitingEvents.Invoke();
            stopEmitingCoroutine = StartCoroutine(CStopEmitingIn());
        }
        if (newState == TracerState.RUSHING && oldState != TracerState.RUSHING) {
            StopEmiting();
            gm.soundManager.PlayDetectionClip(transform.position, transform);
        }
    }

    protected void StopEmiting() {
        stopEmitingEvents.Invoke();
        if (stopEmitingCoroutine != null)
            StopCoroutine(stopEmitingCoroutine);
    }

    protected IEnumerator CStopEmitingIn() {
        yield return new WaitForSeconds(dureeEmiting);
        StopEmiting();
        SetState(TracerState.WAITING);
    }

    public override bool IsInactive() {
        return state == TracerState.WAITING;
    }

    public override bool IsMoving() {
        return !IsInactive();
    }
}
