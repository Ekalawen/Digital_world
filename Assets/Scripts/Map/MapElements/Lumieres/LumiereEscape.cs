using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class LumiereEscape : Lumiere {

    [Serializable]
    public struct EscapeColors {
        public Color sphereMainColor;
        public Color sphereSecondaryColor;
        public Color sphereFresnelColor;
        [ColorUsage(true, true)]
        public Color heartColor;
        [Range(0, 360)]
        public float vfxColorHue;
    }

    [Header("Escape")]
    public int nbLives = 2;
    public GameObject lightningPrefab;
    public GeoData geoData;
    public List<EscapeColors> escapeColors;

    [Header("Compute Escape Position")]
    public float escapeDistance = 5.0f;
    public float minEscapeDistance = 2.5f;
    public float maxEscapeDistance = 8.0f;
    public float mapOffsetShrink = 2.0f;
    public int maxNbPositionsToConsider = 100;
    public float coefLineOfSight = 1000.0f;
    public float coefAlignWithCamera = 50.0f;
    public float coefCloseToEscapeDistance = 10.0f;
    public float coefVerticalDistance = 30.0f;
    public float coefDistanceToRegularMap = 20.0f;
    public float coefRandomness = 30.0f;

    protected GoToPositionController controller;
    protected Vector3 currentEscapePosition = Vector3.zero;
    protected int maxNbLives;

    protected override void Start() {
        base.Start();
        Assert.AreEqual(nbLives, escapeColors.Count);
        controller = GetComponent<GoToPositionController>();
        maxNbLives = nbLives;
        SetColorAccordingToNumberOfLives();
    }

    protected override void OnTriggerEnter(Collider hit) {
        if (hit.gameObject.name == "Joueur"){
            if(nbLives <= 1) {
                Captured();
            } else {
                TemporaryCaptured();
            }
		}
    }

    protected void TemporaryCaptured() {
        if (IsCaptured())
        {
            return;
        }
        isCaptured = true;

        nbLives--;

        EscapeToAnotherLocation();

        SetCapturableIn();

        SetColorAccordingToNumberOfLives();

        ThrowLightningToLocation();

        AddGeoPoint();

        NotifySoundManager();

        ScreenShakeOnLumiereCapture();
    }

    protected void ThrowLightningToLocation() {
        Lightning lightning = Instantiate(lightningPrefab).GetComponent<Lightning>();
        lightning.Initialize(transform.position, currentEscapePosition);
    }

    protected void AddGeoPoint() {
        GeoData newGeoData = new GeoData(geoData);
        newGeoData.SetTargetPosition(currentEscapePosition);
        gm.player.geoSphere.AddGeoPoint(newGeoData);
    }

    protected override void CapturedSpecific() {
        base.CapturedSpecific();
        nbLives = 0;
        CharacterController characterController = GetComponent<CharacterController>();
        characterController.enabled = false; // We don't want to collide with the controller once we captured the data ! :)
    }

    protected void SetColorAccordingToNumberOfLives() {
        EscapeColors colors = GetCurrentEscapeColors();
        lumiereLowVoronoiSphere.material.SetColor("_MainColor", colors.sphereMainColor);
        lumiereLowVoronoiSphere.material.SetColor("_SecondColor", colors.sphereSecondaryColor);
        lumiereLowVoronoiSphere.material.SetColor("_FresnelColor", colors.sphereFresnelColor);
        lumiereLowGlowingHeart.material.SetColor("_MainColor", colors.heartColor);
        SetVfxColorForProperty("GlowColor", colors.vfxColorHue / 360);
        SetVfxColorForProperty("ParticlesColor", colors.vfxColorHue / 360);
        SetVfxColorForProperty("FlareColor", colors.vfxColorHue / 360);
        SetVfxColorForProperty("CircleColor", colors.vfxColorHue / 360);
        SetVfxColorForProperty("ExplosionColor", colors.vfxColorHue / 360);
    }

    protected void SetVfxColorForProperty(string propertyName, float hue) {
        Vector3 colorVector = lumiereLowVfx.GetVector4(propertyName);
        Color color = new Color(colorVector.x, colorVector.y, colorVector.z);
        Color rotatedColor = ColorManager.RotateHueTo(color, hue);
        Vector3 rotatedColorVector = new Vector3(rotatedColor.r, rotatedColor.g, rotatedColor.b);
        lumiereLowVfx.SetVector4(propertyName, rotatedColorVector);
    }

    public EscapeColors GetCurrentEscapeColors() {
        return escapeColors[Mathf.Max(nbLives - 1, 0)];
    }

    protected void SetCapturableIn() {
        StartCoroutine(CSetCapturableIn());
    }

    protected IEnumerator CSetCapturableIn() {
        yield return new WaitForSeconds(TimeToGoToPosition(currentEscapePosition));
        isCaptured = false;
    }

    protected void EscapeToAnotherLocation() {
        currentEscapePosition = ComputeEscapeLocation();
        controller.GoTo(currentEscapePosition);
    }

    public float TimeToGoToPosition(Vector3 pos) {
        float distance = Vector3.Distance(transform.position, pos);
        return distance / controller.vitesse;
    }

    protected Vector3 ComputeEscapeLocation() {
        List<Vector3> positions = gm.map.GetEmptyPositionsInSphere(transform.position, maxEscapeDistance);
        positions = positions.FindAll(p => Vector3.Distance(p, transform.position) >= minEscapeDistance);
        positions = MathTools.ChoseSome(positions, 100);
        Vector3 chosenPosition = positions.OrderBy(p => ComputeScoreForPosition(p)).Last();
        ComputeScoreForPosition(chosenPosition, displayLog: true);
        return chosenPosition;
    }

    protected float ComputeScoreForPosition(Vector3 pos, bool displayLog = false) {
        float isInLineOfSightScore = (IsInLineOfSight(pos) ? 1 : 0) * coefLineOfSight;
        float isAlignedWithCameraScore = IsAlignedWithCamera(pos) * coefAlignWithCamera;
        float isDistanceCloseToEscapeDistanceScore = - IsDistanceCloseToEscapeDistance(pos) * coefCloseToEscapeDistance;
        float distanceToRegularMapScore = - DistanceToRegularMap(pos) * coefDistanceToRegularMap;
        float verticalDistanceScore = - VerticalDistance(pos) * coefVerticalDistance;
        float randomnessScore = UnityEngine.Random.Range(0.0f, 1.0f) * coefRandomness;
        float totalScore = isInLineOfSightScore
            + isAlignedWithCameraScore
            + isDistanceCloseToEscapeDistanceScore
            + distanceToRegularMapScore
            + verticalDistanceScore
            + randomnessScore;
        if (displayLog) {
            Debug.Log($"LineOfSight = {isInLineOfSightScore / coefLineOfSight}({isInLineOfSightScore}) AlignedCamera = {isAlignedWithCameraScore / coefAlignWithCamera}({isAlignedWithCameraScore}) DistanceToDistance = {isDistanceCloseToEscapeDistanceScore / coefCloseToEscapeDistance}({isDistanceCloseToEscapeDistanceScore}) DistanceToMap = {distanceToRegularMapScore / coefDistanceToRegularMap}({distanceToRegularMapScore}) VerticalDistance = {verticalDistanceScore / coefVerticalDistance}({verticalDistanceScore}) Randomness = {randomnessScore / coefRandomness}({randomnessScore}) TOTAL = {totalScore}");
        }
        return totalScore;
    }

    protected float VerticalDistance(Vector3 pos) {
        Vector3 direction = pos - transform.position;
        return (direction - Vector3.ProjectOnPlane(direction, gm.gravityManager.Up())).magnitude;
    }

    protected float IsDistanceCloseToEscapeDistance(Vector3 pos) {
        return Mathf.Abs(escapeDistance - Vector3.Distance(transform.position, pos));
    }

    protected bool IsInLineOfSight(Vector3 pos) {
        Vector3 direction = (pos - transform.position);
        float distance = direction.magnitude;
        return !Physics.Raycast(transform.position, direction, distance);
    }

    protected float IsAlignedWithCamera(Vector3 pos) {
        Vector3 cameraPos = gm.player.camera.transform.position;
        Vector3 cameraForward = gm.player.camera.transform.forward;
        return Vector3.Dot(pos - cameraPos, cameraForward) / Vector3.Distance(pos, cameraPos);
    }

    protected float DistanceToRegularMap(Vector3 pos) {
        return Mathf.Max(0, MathTools.AABBPointDistance(gm.map.GetCenter(), gm.map.GetHalfExtents() - Vector3.one * mapOffsetShrink, pos));
    }
}
