using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectorCameraController : MonoBehaviour {

    public SelectorManager selectorManager;
    public CharacterController controller;
    public float dureeRotations = 0.3f;

    protected Vector3 lastClosestLevelPosition = Vector3.zero;
    protected Coroutine rotationCoroutine = null;

    public void Update() {
        LookAtClosestLevel();
    }

    protected void LookAtClosestLevel() {
        if (rotationCoroutine != null)
            return;

        List<Vector3> levelsPositions = selectorManager.GetLevels().Select(l => l.transform.position).ToList();
        Vector3 closest = levelsPositions.OrderBy(pos => Vector3.Distance(transform.position, pos)).First();
        if(closest == lastClosestLevelPosition)
            transform.LookAt(closest);
        else
            LookAt(closest);

        lastClosestLevelPosition = closest;
    }

    public void LookAt(Vector3 point) {
        rotationCoroutine = StartCoroutine(ProgressiveLookAt(point));
    }

    protected IEnumerator ProgressiveLookAt(Vector3 pointToLookAt) {
        Timer totalTimer = new Timer(dureeRotations);
        Timer currentTimer = new Timer(dureeRotations);
        Vector3 lastPosition = transform.position;

        while (!totalTimer.IsOver()) {
            Vector3 directionToLookAt = pointToLookAt - transform.position;
            Vector3 currentDirection = transform.forward;
            float angle = Vector3.Angle(currentDirection, directionToLookAt);
            Vector3 axe = Vector3.Cross(currentDirection, directionToLookAt);

            while (!currentTimer.IsOver()) {
                yield return null;
                float newAvancement = currentTimer.GetNewAvancement();
                float value = angle * newAvancement;
                transform.RotateAround(transform.position, axe, value);
                transform.LookAt(transform.position + transform.forward, Vector3.up);  // Essayer de rajouter cette ligne dans le GravityManager ça pourrait faire gagner un jump de caméra !
                if (transform.position != lastPosition)
                    break;
            }
            if (transform.position != lastPosition) {
                currentTimer = new Timer(totalTimer.GetRemainingTime());
                lastPosition = transform.position;
            }
        }
        transform.LookAt(pointToLookAt, Vector3.up);
        rotationCoroutine = null;
    }

}
