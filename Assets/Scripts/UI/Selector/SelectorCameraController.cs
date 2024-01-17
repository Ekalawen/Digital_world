using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SelectorCameraController : MonoBehaviour {
    [Header("Mouvement and Rotations")]
    public Vector3 speedMouse;
    public Vector3 speedKeyboard;
    public float dureeRotations = 0.3f;
    public float speedLookAtCenter = 10;
    public float dureeBeforeFocusToInterestPoint = 0.1f;
    public float mouseSmoothing = 0.3f;
    public float delayLeftClickRotation = 0.2f;

    [Header("InterestPoints")]
    public float poidsDistanceInInterestScore = 0.5f;

    [Header("ElasticitySphere")]
    public Vector2 sizeElasticitySphere;
    public Vector2 elasticitySphereCoefficiant;
    public float idealDistanceFromLevel = 20f;

    [Header("AutoMove")]
    public float autoMoveDuration = 0.7f;
    public AnimationCurve autoMoveCurve;

    [Header("Links")]
    public SelectorManager selectorManager;
    public CharacterController controller;
    public Volume luminosityVolume;

    protected Vector3 lastClosestLevelPosition = Vector3.zero;
    protected bool isMoving;
    protected Timer lastIsMovingTimer;
    protected Coroutine rotationCoroutine = null;
    protected Coroutine lookAtCentralProjectionCoroutine = null;
    protected Coroutine autoMoveCoroutine = null;
    protected float oldMouseSpeedX = 0;
    protected float oldMouseSpeedY = 0;
    protected Timer timeSincePressedLeftClick;

    public void Start() {
        speedMouse *= 100;
        lastIsMovingTimer = new Timer(dureeBeforeFocusToInterestPoint);
        timeSincePressedLeftClick = new Timer();
        InitLuminosityVolume();
    }

    public void Update() {
        MoveByDragging();
        ApplyElasticitySphere();
        //LookAtClosestInterestPoint();
        LookAtCentralProjection();
    }

    protected void MoveByDragging() {
        //if (selectorManager.HasSelectorLevelOpen()
        // || selectorManager.PopupIsEnabled()
        // || selectorManager.HasSelectorPathUnlockScreenOpen())
        if (selectorManager.PopupIsEnabled()
         || autoMoveCoroutine != null) {
            return;
        }

        Vector3 speeds = ComputeSpeedOnAxis();

        //LookAtCentralProjectionIfNeeded(speeds.x);

        Vector3 move = Move(speeds);

        SetIsMoving(move);
    }

    protected void SetIsMoving(Vector3 move) {
        Vector2 bornesY = ComputesBornesYPoints();
        isMoving = move != Vector3.zero || Input.GetMouseButton(0) || transform.position.y > bornesY[1] || transform.position.y < bornesY[0];
        if (isMoving) {
            lastIsMovingTimer.Reset();
        }
    }

    protected Vector3 Move(Vector3 speeds) {
        Vector3 move = Vector3.zero;
        Vector3 centralProjection = GetCentralProjection();
        Vector3 right = -Vector3.Cross(centralProjection - transform.position, Vector3.up).normalized;
        move += right * speeds.x;
        move += Vector3.up * speeds.y;
        move += transform.forward * speeds.z;
        move *= Time.deltaTime;
        controller.Move(move);
        return move;
    }

    protected Vector3 ComputeSpeedOnAxis() {
        Vector3 speed = !IsInInputField() ? InputManager.Instance.GetCameraSelectorMouvement() : Vector3.zero;
        float speedX = speed.x * speedKeyboard.x;
        float speedY = speed.y * speedKeyboard.y;
        float speedZ = speed.z * speedKeyboard.z;
        if (Input.GetMouseButton(0)) {
            if (timeSincePressedLeftClick.GetElapsedTime() > delayLeftClickRotation/* && !MouseIsInVerticalMenu()*/) {
                float mouseSpeedX = -Input.GetAxis("Mouse_X") * (1 - mouseSmoothing) + oldMouseSpeedX * mouseSmoothing;
                float mouseSpeedY = -Input.GetAxis("Mouse_Y") * (1 - mouseSmoothing) + oldMouseSpeedY * mouseSmoothing;
                oldMouseSpeedX = mouseSpeedX;
                oldMouseSpeedY = mouseSpeedY;
                speedX += mouseSpeedX * speedMouse.x;
                speedY += mouseSpeedY * speedMouse.y;
            }
        } else {
            timeSincePressedLeftClick.Reset();
        }
        speedZ += Input.GetAxis("Mouse ScrollWheel") * speedMouse.z;
        return new Vector3(speedX, speedY, speedZ);
    }

    protected bool IsInInputField() {
        return selectorManager.HasUnlockScreenOpen() && selectorManager.unlockScreen.IsInInput();
    }

    public bool MouseIsInVerticalMenu() {
        RectTransform verticalMenu = selectorManager.verticalMenuHandler.verticalMenuLayout.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(verticalMenu, Input.mousePosition, selectorManager.overlayCamera);
    }

    protected void LookAtCentralProjectionIfNeeded(float speedX) {
        Vector2 bornesY = ComputesBornesYPoints();
        if ((speedX != 0 && !Input.GetMouseButton(0))
        || (Input.GetMouseButton(0) && timeSincePressedLeftClick.GetElapsedTime() >= delayLeftClickRotation)
        || transform.position.y > bornesY[1] || transform.position.y < bornesY[0]) {
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
    }

    protected Vector2 ComputesBornesYPoints() {
        List<Vector3> interestPoints = GetAllInterestPoints();
        float minY = interestPoints.Min(p => p.y);
        float maxY = interestPoints.Max(p => p.y);
        return new Vector2(minY, maxY);
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
        if (autoMoveCoroutine != null)
            return;

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
        return selectorManager.GetAllPasswordPaths().Select(p => p.cadena.transform.position).ToList();
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

    protected void LookAtCentralProjection() {
        Vector3 centralPoint = GetCentralProjection();
        centralPoint = OffsetCentralPointHorizontaly(centralPoint);
        transform.LookAt(centralPoint, Vector3.up);
    }

    protected Vector3 OffsetCentralPointHorizontaly(Vector3 centralPoint) {
        //Vector3 currentPosition = transform.position;
        //currentPosition.y = centralPoint.y;
        Vector3 res = centralPoint - transform.position;
        float angle = ComputeHorizontalOffsetAngle();
        res = Quaternion.Euler(0, angle, 0) * res;
        return res + transform.position;
    }

    protected float ComputeHorizontalOffsetAngle() {
        float hFOV = GetHorizontalFieldOfView();
        // angle = 0 ==> Centré
        // angle = hFOV / 2 ==> A gauche
        // Jeter un coup d'oeil à mon cailler bleu et le joli dessin ! :D
        float menuPercentage = selectorManager.verticalMenuHandler.GetVerticalMenuPercentage(); // m in % (bad unit)
        float nearClipPlane = selectorManager.baseCamera.nearClipPlane; // h
        float halfHorizontalLength = nearClipPlane * Mathf.Tan((hFOV / 2) * Mathf.Deg2Rad); // l
        float menuLength = (halfHorizontalLength * 2) * menuPercentage; // m
        float angle = Mathf.Atan(menuLength / (2 * nearClipPlane)) * Mathf.Rad2Deg;
        return angle;
    }

    protected float GetHorizontalFieldOfView() {
        float radAngle = selectorManager.baseCamera.fieldOfView * Mathf.Deg2Rad;
        float radHFOV = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * selectorManager.baseCamera.aspect);
        float hFOV = radHFOV * Mathf.Rad2Deg;
        return hFOV;
    }

    public void PlaceCameraInFrontOfCurrentLevel() {
        PlaceCameraInFrontOfInterestTransform(selectorManager.GetCurrentLevel().transform);
    }

    public void PlaceCameraInFrontOfPath(SelectorPath_Password path) {
        PlaceCameraInFrontOfInterestPoint(path.cadena.transform.position);
    }

    public void PlaceCameraInFrontOfInterestTransform(Transform t) {
        Vector3 forward = IsOnCentralAxe(t.position) ? GetForwardToCamera(t.position) : t.forward;
        PlaceCameraInFrontOfInterestPoint(t.position, forward);
    }

    private Vector3 GetForwardToCamera(Vector3 pos) {
        return Vector3.ProjectOnPlane(transform.position - pos, Vector3.up).normalized;
    }

    public bool IsOnCentralAxe(Vector3 pos) {
        return Vector3.Distance(Vector3.zero, Vector3.ProjectOnPlane(pos, Vector3.up)) == 0;
    }

    public void PlaceCameraInFrontOfInterestPoint(Vector3 point) {
        Vector3 forward = GetForwardFromCenterProjection(point);
        PlaceCameraInFrontOfInterestPoint(point, forward);
    }

    protected void PlaceCameraInFrontOfInterestPoint(Vector3 interestPos, Vector3 interestForward) {
        Vector3 posToGoTo = interestPos + interestForward * GetIdealDistanceFromLevel();
        AutoMove(posToGoTo, autoMoveDuration);
        //PlaceAt(posToGoTo);
        //transform.LookAt(interestPos, Vector3.up);
    }

    protected Vector3 GetForwardFromCenterProjection(Vector3 interestPos) {
        Vector3 forward;
        if (interestPos.y < 50 && !IsOnCentralAxe(interestPos)) { // Les cadenas relous !!
            forward = Vector3.back;
        } else if (IsOnCentralAxe(interestPos)) {
            forward = GetForwardToCamera(interestPos);
        } else {
            Vector3 projection = Vector3.Project(interestPos, Vector3.up);
            Vector3 orthoToCenter = interestPos - projection;
            forward = orthoToCenter.normalized;
        }

        return forward;
    }

    protected void AutoMove(Vector3 target, float duration) {
        if(autoMoveCoroutine != null) {
            StopCoroutine(autoMoveCoroutine);
        }
        autoMoveCoroutine = StartCoroutine(CAutoMove(target, duration));
    }

    protected IEnumerator CAutoMove(Vector3 target, float duration) {
        Vector3 source = transform.position;
        Timer timer = new Timer(duration);
        while(!timer.IsOver()) {
            float avancement = autoMoveCurve.Evaluate(timer.GetAvancement());
            transform.position = Vector3.Lerp(source, target, avancement);
            yield return null;
        }
        transform.position = Vector3.Lerp(source, target, 1.0f);
        autoMoveCoroutine = null;
    }

    //protected Vector3 LerpCircular(Vector3 source, Vector3 target, float avancement) {
    //    // En plus si il faut la fonction est pas parfaite ^^'
    //    float height = MathCurves.Linear(source.y, target.y, avancement);
    //    Vector3 sourceProjected = Vector3.ProjectOnPlane(source, Vector3.up);
    //    Vector3 targetProjected = Vector3.ProjectOnPlane(target, Vector3.up);
    //    float sourceDistance = Vector3.Distance(Vector3.zero, sourceProjected);
    //    float targetDistance = Vector3.Distance(Vector3.zero, targetProjected);
    //    float distance = MathCurves.Linear(sourceDistance, targetDistance, avancement);
    //    float sourceAngle = Vector3.Angle(Vector3.right, sourceProjected);
    //    float targetAngle = Vector3.Angle(Vector3.right, targetProjected);
    //    if(Mathf.Abs(sourceAngle - targetAngle) > 180) {
    //        if(sourceAngle > targetAngle) {
    //            sourceAngle -= 360;
    //        } else {
    //            targetAngle -= 360;
    //        }
    //    }
    //    float angle = MathCurves.Linear(sourceAngle, targetAngle, avancement);
    //    Vector3 res =  Quaternion.Euler(0, angle, 0) * new Vector3(distance, height, 0);
    //    return res;
    //}

    protected void InitLuminosityVolume() {
        PostProcessManager.SetLuminosityIntensity(luminosityVolume, PrefsManager.GetFloat(PrefsManager.LUMINOSITY, MenuOptions.defaultLuminosity));
    }

}
