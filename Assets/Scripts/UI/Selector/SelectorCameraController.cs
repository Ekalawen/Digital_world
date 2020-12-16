using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectorCameraController : MonoBehaviour {
    [Header("Mouvement and Rotations")]
    public Vector3 speedMouse;
    public Vector3 speedKeyboard;
    public float dureeRotations = 0.3f;

    [Header("ElasticitySphere")]
    public Vector2 sizeElasticitySphere;
    public Vector2 elasticitySphereCoefficiant;
    public float idealDistanceFromLevel = 20f;

    [Header("Links")]
    public SelectorManager selectorManager;
    public CharacterController controller;

    protected Vector3 lastClosestLevelPosition = Vector3.zero;
    protected bool isMoving;
    protected Coroutine rotationCoroutine = null;

    public void Start() {
        speedMouse *= 100;
    }

    public void Update() {
        MoveByDragging();
        ApplyElasticitySphere();
        LookAtClosestInterestPoint();
    }

    protected void MoveByDragging() {
        if (selectorManager.HasSelectorLevelOpen()
         || selectorManager.PopupIsEnabled()
         || selectorManager.HasSelectorPathUnlockScreenOpen())
            return;

        float speedX = Input.GetAxis("Horizontal") * speedKeyboard.x;
        float speedY = Input.GetAxis("Depth") * speedKeyboard.y;
        float speedZ = Input.GetAxis("Vertical") * speedKeyboard.z;

        if(Input.GetMouseButton(0)) {
            speedX += - Input.GetAxis("Mouse X") * speedMouse.x;
            speedY += - Input.GetAxis("Mouse Y") * speedMouse.y;
        }
        speedZ += Input.GetAxis("Mouse ScrollWheel") * speedMouse.z;

        Vector3 move = Vector3.zero;
        move += transform.right * speedX;
        move += Vector3.up * speedY;
        move += transform.forward * speedZ;
        move *= Time.deltaTime;
        controller.Move(move);

        isMoving = move != Vector3.zero;
        //if(rotationCoroutine != null) {
        //    //StopCoroutine(rotationCoroutine);
        //    //rotationCoroutine = null;
        //}
    }

    protected void ApplyElasticitySphere() {
        Vector3 closest = GetClosestInterestPoint(transform.position);
        float distance = Vector3.Distance(closest, transform.position);
        Vector3 direction = (closest - transform.position).normalized;
        Vector3 move = Vector3.zero;
        if(distance > sizeElasticitySphere[1]) {
            float distanceToCatchBack = (distance - sizeElasticitySphere[1]);
            move += direction * distanceToCatchBack * elasticitySphereCoefficiant[1];
        }
        if(distance < sizeElasticitySphere[0]) {
            float distanceToCatchBack = (sizeElasticitySphere[0] - distance);
            Vector3 attemptedMove = -direction * distanceToCatchBack * elasticitySphereCoefficiant[0];
            attemptedMove = EnsureNotAttractedByAnOtherSphere(attemptedMove, closest);
            move += attemptedMove;
        }
        move *= Time.deltaTime;
        controller.Move(move);
    }

    protected Vector3 EnsureNotAttractedByAnOtherSphere(Vector3 attemptedMove, Vector3 closest) {
        while (true) {
            Vector3 newPosition = transform.position + attemptedMove;
            Vector3 newClosest = GetClosestInterestPoint(newPosition);
            if (newClosest != closest)
                attemptedMove /= 2;
            else
                break;
        }
        return attemptedMove;
    }

    protected List<Vector3> GetLevelsPositions() {
        return selectorManager.GetLevels().Select(l => l.transform.position).ToList();
    }

    protected List<Vector3> GetCadenasPositions() {
        return selectorManager.GetPaths().Select(p => p.cadena.transform.position).ToList();
    }

    protected Vector3 GetClosestInterestPoint(Vector3 pos) {
        List<Vector3> levelsPositions = GetLevelsPositions();
        List<Vector3> cadenasPositions = GetCadenasPositions();
        List<Vector3> interestPoints = levelsPositions.Concat(cadenasPositions).ToList();
        return interestPoints.OrderBy(p => Vector3.Distance(pos, p)).First();
    }

    protected void LookAtClosestInterestPoint() {
        Vector3 closest = GetClosestInterestPoint(transform.position);

        if (rotationCoroutine != null) {
            if (closest == lastClosestLevelPosition)
                return;
            else {
                StopCoroutine(rotationCoroutine);
                rotationCoroutine = null;
            }
        }

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

    public float GetIdealDistanceFromLevel() {
        return idealDistanceFromLevel;
    }

    public void PlaceAt(Vector3 posToGoTo) {
        transform.position = posToGoTo;
    }
}
