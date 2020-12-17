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
    public float speedLookAtCenter = 10;
    public float dureeBeforeFocusToInterestPoint = 0.1f;
    public float mouseSmoothing = 0.3f;

    [Header("InterestPoints")]
    public float poidsDistanceInInterestScore = 0.5f;

    [Header("ElasticitySphere")]
    public Vector2 sizeElasticitySphere;
    public Vector2 elasticitySphereCoefficiant;
    public float idealDistanceFromLevel = 20f;

    [Header("Links")]
    public SelectorManager selectorManager;
    public CharacterController controller;

    protected Vector3 lastClosestLevelPosition = Vector3.zero;
    protected bool isMoving;
    protected Timer lastIsMovingTimer;
    protected Coroutine rotationCoroutine = null;
    protected Coroutine lookAtCentralProjectionCoroutine = null;
    protected float oldMouseSpeedX = 0;
    protected float oldMouseSpeedY = 0;

    public void Start() {
        speedMouse *= 100;
        lastIsMovingTimer = new Timer(dureeBeforeFocusToInterestPoint);
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

        float speedX = Input.GetAxis("HorizontalFullGravity") * speedKeyboard.x;
        float speedY = Input.GetAxis("Depth") * speedKeyboard.y;
        float speedZ = Input.GetAxis("Vertical") * speedKeyboard.z;

        if (Input.GetMouseButton(0)) {
            float mouseSpeedX = -Input.GetAxis("Mouse X") * (1 - mouseSmoothing) + oldMouseSpeedX * mouseSmoothing;
            float mouseSpeedY = -Input.GetAxis("Mouse Y") * (1 - mouseSmoothing) + oldMouseSpeedY * mouseSmoothing;
            oldMouseSpeedX = mouseSpeedX;
            oldMouseSpeedY = mouseSpeedY;
            speedX += mouseSpeedX * speedMouse.x;
            speedY += mouseSpeedY * speedMouse.y;
        }
        speedZ += Input.GetAxis("Mouse ScrollWheel") * speedMouse.z;

        Vector3 move = Vector3.zero;
        Vector3 centralProjection = GetCentralProjection();
        Vector3 right = -Vector3.Cross(centralProjection - transform.position, Vector3.up).normalized;
        move += right * speedX;
        List<Vector3> interestPoints = GetAllInterestPoints();
        float maxY = interestPoints.Max(p => p.y);
        float minY = interestPoints.Min(p => p.y);
        if (speedX != 0 || transform.position.y > maxY || transform.position.y < minY) {
            if (rotationCoroutine != null) {
                StopCoroutine(rotationCoroutine);
                rotationCoroutine = null;
            }
            if (lookAtCentralProjectionCoroutine == null) {
                lookAtCentralProjectionCoroutine = LookAtCentralPoint();
            }
        } else {
            if (lookAtCentralProjectionCoroutine != null) {
                StopCoroutine(lookAtCentralProjectionCoroutine);
                lookAtCentralProjectionCoroutine = null;
            }
        }
        move += Vector3.up * speedY;
        move += transform.forward * speedZ;
        move *= Time.deltaTime;
        controller.Move(move);

        isMoving = move != Vector3.zero || Input.GetMouseButton(0) || transform.position.y > maxY || transform.position.y < minY;
        if(isMoving) {
            lastIsMovingTimer.Reset();
        }
    }

    protected void LookAtClosestInterestPoint() {
        if (!lastIsMovingTimer.IsOver()) {
            lastClosestLevelPosition = Vector3.one * -1;
            if(rotationCoroutine != null) {
                StopCoroutine(rotationCoroutine);
                rotationCoroutine = null;
            }
            return;
        }

        //Vector3 closest = GetClosestInterestPoint(transform.position);
        Vector3 bestPoint = GetBestInterestPoint();
        if (rotationCoroutine != null) {
            if (bestPoint == lastClosestLevelPosition)
                return;
            else {
                StopCoroutine(rotationCoroutine);
                rotationCoroutine = null;
            }
        }

        if(bestPoint == lastClosestLevelPosition)
            transform.LookAt(bestPoint);
        else
            rotationCoroutine = ProgressivelyLookAt(bestPoint);

        lastClosestLevelPosition = bestPoint;
    }

    protected Vector3 GetCentralProjection() {
        Vector3 centralProjection = Vector3.Project(transform.position, Vector3.up);
        List<Vector3> interestPoints = GetAllInterestPoints();
        float maxY = interestPoints.Max(p => p.y);
        float minY = interestPoints.Min(p => p.y);
        centralProjection.y = Mathf.Clamp(centralProjection.y, minY, maxY);
        return centralProjection;
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
        List<Vector3> interestPoints = GetAllInterestPoints();
        return interestPoints.OrderBy(p => Vector3.Distance(pos, p)).First();
    }

    protected List<Vector3> GetAllInterestPoints() {
        List<Vector3> levelsPositions = GetLevelsPositions();
        List<Vector3> cadenasPositions = GetCadenasPositions();
        List<Vector3> interestPoints = levelsPositions.Concat(cadenasPositions).ToList();
        return interestPoints;
    }

    protected Vector3 GetBestInterestPoint() {
        List<Vector3> interestPoints = GetAllInterestPoints();
        return interestPoints.OrderBy(p => ComputeIntereset(p)).First();
    }

    protected float ComputeIntereset(Vector3 point) {
        float distance = Vector3.Distance(point, transform.position);
        float distanceToCentreVision = Vector3.Distance(point, transform.position + Vector3.Project(point, transform.forward));
        return distance * poidsDistanceInInterestScore + distanceToCentreVision * (1 - poidsDistanceInInterestScore);
    }

    public Coroutine ProgressivelyLookAt(Vector3 point) {
        return StartCoroutine(CProgressiveLookAt(point));
    }

    protected IEnumerator CProgressiveLookAt(Vector3 pointToLookAt) {
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

    public Coroutine LookAtCentralPoint() {
        return StartCoroutine(CLookAtCentralPoint());
    }

    protected IEnumerator CLookAtCentralPoint() {
        while(true) {
            Vector3 centralPoint = GetCentralProjection();
            Vector3 directionToLookAt = centralPoint - transform.position;
            Vector3 currentDirection = transform.forward;
            float angle = Vector3.Angle(currentDirection, directionToLookAt);
            Vector3 axe = Vector3.Cross(currentDirection, directionToLookAt);
            float value = angle * Time.deltaTime * speedLookAtCenter;
            transform.RotateAround(transform.position, axe, value);
            transform.LookAt(transform.position + transform.forward, Vector3.up);
            yield return null;
        }
    }

    public float GetIdealDistanceFromLevel() {
        return idealDistanceFromLevel;
    }

    public void PlaceAt(Vector3 posToGoTo) {
        transform.position = posToGoTo;
    }
}
