using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirGripDash : IPouvoir {

    [Header("Dash")]
    public float dashSpeed = 15.0f;
    public float dashDurationAdjustment = 0.1f;
    public TimeMultiplier dashSlowmotion;

    [Header("Targeting")]
    public float maxTargetingDistance = 15.0f;
    public float maxTargetingDuration = 1.0f;
    public TimeMultiplier targetingSlowmotion;

    protected RaycastHit currentHit;
    protected Cube currentHittedCube;
    protected Poussee currentPoussee = null;
    protected Coroutine targetingCoroutine = null;
    protected Coroutine unStunCoroutine = null;
    private Coroutine restoreGravityCoroutine = null;
    protected float computedDuration = 0;

    public override bool IsAvailable() {
        return base.IsAvailable() && targetingCoroutine == null;
    }

    protected override void ApplyUsePouvoir() {
        targetingCoroutine = StartCoroutine(CTargeting());
    }

    protected IEnumerator CTargeting() {
        Timer timer = new Timer(maxTargetingDuration);
        GameObject previsualizationObject = CreatePrevisualization();
        while(!timer.IsOver()) {
            UpdatePrevisualization(previsualizationObject);
            if(InputManager.Instance.GetKeyUp(binding)) {
                if (CanGripDash()) {
                    UsePouvoir();
                    ApplyUsePouvoirConsequences();
                } else {
                    gm.soundManager.PlayDeniedPouvoirClip();
                }
                break;
            }
            yield return null;
        }
        DestroyPrevisualization(previsualizationObject);
        // Faudra se débrouiller pour mettre un cooldown de 1s (ou plus) ici ! :)
        targetingCoroutine = null;
    }

    protected GameObject CreatePrevisualization() {
        return null;
    }

    protected void UpdatePrevisualization(GameObject previsualizationObject) {
    }

    protected void DestroyPrevisualization(GameObject previsualizationObject) {
    }

    protected override bool UsePouvoir() {
        Vector3 finalTarget = ComputeTargetFromCube(currentHittedCube);
        float distance = (finalTarget - player.transform.position).magnitude;
        RegisterComputedDurationAccordingToDistance(distance);
        currentPoussee = new PousseePrecise(finalTarget, player.transform.position, dashDurationAdjustment, dashSpeed, callback: RestorePlayerMouvementEarlier);
        player.AddPoussee(currentPoussee);
        player.RemoveAllNegativePoussees();
        player.ResetGrip();
        RemoveGravityEffectForDashDuration();
        StunForDashDuration();
        StartVfx();
        gm.timerManager.timeMultiplierController.RemoveAllEnnemisMultipliers();
        return true;
    }

    protected void RegisterComputedDurationAccordingToDistance(float distance) {
        computedDuration = distance / dashSpeed;
    }

    protected void StunForDashDuration() {
        player.StunAndKeepPouvoirs();
        unStunCoroutine = StartCoroutine(CUnStunAtEndOfDash());
    }

    protected void UnStunEarlier() {
        if(unStunCoroutine != null) {
            StopCoroutine(unStunCoroutine);
            unStunCoroutine = null;
        }
        player.UnStun();
    }

    protected IEnumerator CUnStunAtEndOfDash() {
        yield return new WaitForSeconds(GetDuration());
        player.UnStun();
        unStunCoroutine = null;
    }

    protected void RemoveGravityEffectForDashDuration() {
        player.RemoveGravityEffect();
        restoreGravityCoroutine = StartCoroutine(CRestoreGravityAtEndOfDash());
    }

    protected void RestoreGravityEarlier() {
        if(restoreGravityCoroutine != null) {
            StopCoroutine(restoreGravityCoroutine);
            restoreGravityCoroutine = null;
        }
        player.RestoreGravityEffect();
    }

    protected IEnumerator CRestoreGravityAtEndOfDash() {
        yield return new WaitForSeconds(GetDuration());
        player.RestoreGravityEffect();
        restoreGravityCoroutine = null;
    }

    protected void RestorePlayerMouvementEarlier() {
        UnStunEarlier();
        RestoreGravityEarlier();
    }

    protected Vector3 ComputeTargetFromCube(Cube cube) {
        Vector3 direction = -player.camera.transform.forward;
        Vector3 closestNormal = MathTools.GetClosestToNormals(cube.transform, direction);
        if (MathTools.IsNormalHorizontal(closestNormal, gm.gravityManager)) {
            Vector3 aboveCube = cube.transform.position + gm.gravityManager.Up();
            if(!gm.map.IsCubeAt(aboveCube)) {
                Debug.Log($"Horizontal (above)");
                return cube.transform.position + gm.gravityManager.Up() * 0.75f;
            }
            Debug.Log($"Horizontal (side)");
            return cube.transform.position;
        } else if (closestNormal == gm.gravityManager.Down()) {
            Debug.Log($"Vers le haut");
            Vector3 aboveCube = cube.transform.position + gm.gravityManager.Up();
            if(!gm.map.IsCubeAt(aboveCube)) {
                Debug.Log($"Up (above)");
                return cube.transform.position + gm.gravityManager.Up() * 0.75f;
            }
            Debug.Log($"Up (side)");
            return cube.transform.position;
        } else {
            Debug.Log($"Vers le bas");
            return cube.transform.position;
        }
    }

    protected virtual void StartVfx() {
        //gm.postProcessManager.StartDashVfx(duree);
    }

    public Poussee GetCurrentPoussee() {
        return currentPoussee;
    }

    protected bool CanGripDash() {
        Ray ray = new Ray(player.transform.position, player.camera.transform.forward);
        Physics.Raycast(ray, out currentHit, maxTargetingDistance);
        if(currentHit.collider == null) {
            return false;
        }
        currentHittedCube = currentHit.collider.gameObject.GetComponent<Cube>();
        return currentHittedCube != null;
    }

    public float GetDuration() {
        return computedDuration + dashDurationAdjustment;
    }
}
