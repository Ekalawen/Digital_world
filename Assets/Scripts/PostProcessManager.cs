using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;
//using UnityEngine.Rendering.PostProcessing;

public enum DissolveEffectType {
    REGULAR_MAP,
    INFINITE_MAP,
};

[Serializable]
public struct SkyboxParameters {
    public float proportionRectanglesL1;
    public float proportionRectanglesL2;
    public float proportionRectanglesL3;
    [ColorUsage(true, true)] public Color primariColorL1;
    [ColorUsage(true, true)] public Color secondaryColorL1;
    [ColorUsage(true, true)] public Color tertiaryColorL1;
    [ColorUsage(true, true)] public Color primariColorL2;
    [ColorUsage(true, true)] public Color secondaryColorL2;
    [ColorUsage(true, true)] public Color tertiaryColorL2;
    [ColorUsage(true, true)] public Color primariColorL3;
    [ColorUsage(true, true)] public Color secondaryColorL3;
    [ColorUsage(true, true)] public Color tertiaryColorL3;
}

public class PostProcessManager : MonoBehaviour {

    public static string LAYER_NO_BLACK_AND_WHITE_POST_PROCESS = "NoBlackAndWhitePostProcessing";

    [Header("Grip")]
    public float changeTimeGrip = 0.075f;
    public float intensityGrip = 0.315f;
    public Volume gripVolume;

    [Header("Hit")]
    public float changeTimeHit = 0.6f;
    public float intensityVignetteHit= 0.4f;
    public float intensityChromaticHit = 1.0f;
    public Volume hitVolume;

    [Header("Poussee")]
    public UnityEngine.Rendering.PostProcessing.PostProcessVolume pousseeVolume;

    [Header("Skybox")]
    public Color skyboxRectangleColor;
    public float skyboxRectangleColorIntensity;
    public float skyboxProportionRectangles = 0.4f;
    public float skyboxProportionRectanglesCriticalBound = 0.18f;
    public float skyboxScrollSpeedPower = 3.0f;
    public float skyboxVariationsAmplitude = 0.3f;
    public float skyboxRotationWhenOverride = 0.5f;
    public float skyboxBlackoutLuminosity = 0.1f;
    public float skyboxChromaticShiftCoef = 0.3f;
    public float skyboxChromaticShiftDurationOnMarker = 0.4f;
    public float skyboxChromaticShiftTotalDurationOnDisconnection = 1.5f;
    public float skyboxChromaticShiftDurationOnDisconnection = 0.5f;
    public int skyboxChromaticShiftNbOnDisconnection = 6;
    public SkyboxParameters skyboxInitialColors;
    public SkyboxParameters skyboxDisconnectionColors;

    [Header("CubeDissolve")]
    public float dissolveRegularTime = 3.0f;
    public float dissolveRegularPlayerProximityCoef = 0.037f;
    public float dissolveInfiniteTime = 0.0f;
    public float dissolveInfinitePlayerProximityCoef = -0.037f;
    public float dissolveInGameTime = 1.0f;
    public float dissolveInGamePlayerProximityCoef = 0.0f;

    [Header("CubeExplosion")]
    public GameObject cubeExplosionParticlesPrefab;
    public GameObject cubeBouncyExplosionParticlesPrefab;

    [Header("Wall VFX")]
    public float tresholdDownFront = 15;
    public float tresholdDownBack = 15;
    public float transitionToTresholdBack = 45;
    public float transitionToTresholdFront = 45;
    public float inverseAngleTreshold = 45;
    public float wallVFXOffsetPercentageOfScreenPrimary = -0.7f;
    public float wallVFXOffsetPercentageOfScreenSecondary = 0.0f;

    [Header("Shift VFX")]
    public float shiftVFXOffsetPercentageOfScreenPrimary = -0.5f;
    public float shiftVFXOffsetPercentageOfScreenSecondary = 1f;
    public float shiftLandingVFXOffsetPercentageOfScreenPrimary = 0.98f;
    public float shiftLandingVFXOffsetPercentageOfScreenSecondary = 0.97f;

    [Header("Jump VFX")]
    public float jumpVFXOffsetPercentageOfScreenPrimary = 0.9f;
    public float jumpVFXOffsetPercentageOfScreenSecondary = 0.9f;
    public Volume jumpEventVolume;

    [Header("TimeScale VFX")]
    public List<float> timeScaleVFXspawnRateByPhases = new List<float>() { 0, 40, 80 };
    public Vector2 timeScaleVFXOffsetPercentageOfScreen = new Vector2(0.75f, 0.75f);

    [Header("Wall Post Process")]
    public AnimationCurve wallPostProcessCurve;
    public float timeToMaxWallPostProcess = 0.4f;
    public float timeToMinWallPostProcess = 0.1f;
    public Volume wallPostProcessVolume;
    public float lensDistorsionMaxValue = 0.4f;
    public float chromaticAberrationMaxValue = 0.2f;

    [Header("SoulRobber Post Process")]
    public Volume soulRobberVolume;
    public float timeToBlackAndWhite = 0.5f;
    public float timeFromBlackAndWhite = 0.5f;
    public AnimationCurve soulRobberBlackCurve;
    public float soulRobberRestoreBlackValue = 0.1f;
    public AnimationCurve soulRobberRestoreBlackCurve;
    public float soulRobberRestoreBlackDuration = 3.0f;

    [Header("Luminosity")]
    public Volume luminosityVolume;

    [Header("Markers")]
    public float markerScreenShakeMagnitude = 10.0f;
    public float markerScreenShakeRoughness = 10.0f;
    public float markerScreenShakeDecreaseTime = 10.0f;

    protected Coroutine gripCoroutine = null;
    protected Coroutine hitCoroutine = null;
    protected Coroutine wallPostProcessCoroutine = null;
    protected GameManager gm;
    protected new Camera camera;
    protected Camera noBlackAndWhiteCamera;
    protected VisualEffect dashVfx;
    protected VisualEffect powerDashVfx;
    protected VisualEffect gripDashVfx;
    protected VisualEffect shiftVfx;
    protected VisualEffect shiftLandingVfx;
    protected VisualEffect jumpVfx;
    protected VisualEffect wallVfx;
    protected VisualEffect timeScaleVfx;
    protected InputManager inputManager;
    protected float lastFrameWallVfxAngle = 0;
    protected float lastFrameWallVfxHorizontalAngle = 0;
    protected Vector3 lastFrameWallVfxPoint = Vector3.zero;
    protected Vector2 screenSizeAtVfxDistance;
    protected Fluctuator wallLensDistorsionFluctuator;
    protected Fluctuator wallChromaticAberrationFluctuator;
    protected Fluctuator soulRobberWeightFluctuator;
    protected Fluctuator soulRobberBlackFluctuator;
    protected Fluctuator jumpEventFluctuator;
    protected bool timeScaleVfxIsRunning = false;
    protected bool timeScaleEffectActivation;
    protected bool alwaysHasMoveForTimeScaleVfx = false;
    protected bool isWallVfxOn = false;
    protected SingleCoroutine skyboxChromaticCoroutine;
    protected BoolTimer skyboxChromaticAmount;

    public void Initialize() {
        gm = GameManager.Instance;
        camera = gm.player.camera;
        noBlackAndWhiteCamera = gm.player.noBlackAndWhiteCamera;
        dashVfx = gm.player.dashVfx;
        powerDashVfx = gm.player.powerDashVfx;
        gripDashVfx = gm.player.gripDashVfx;
        shiftVfx = gm.player.shiftVfx;
        shiftLandingVfx = gm.player.shiftLandingVfx;
        jumpVfx = gm.player.jumpVfx;
        wallVfx = gm.player.wallVfx;
        timeScaleVfx = gm.player.timeScaleVfx;
        inputManager = InputManager.Instance;
        InitScreenSizeAtVfxDistance();
        InitShiftVfx();
        InitJumpVfx();
        InitShiftLandingVfx();
        InitTimeScaleVfx();
        InitLuminosityPostProcess();
        wallLensDistorsionFluctuator = new Fluctuator(this, GetLensDistorsionIntensity, SetLensDistorsionIntensity, wallPostProcessCurve);
        wallChromaticAberrationFluctuator = new Fluctuator(this, GetChromaticAberrationIntensity, SetChromaticAberrationIntensity, wallPostProcessCurve);
        soulRobberWeightFluctuator = new Fluctuator(this, GetSoulRobberWeight, SetSoulRobberWeight);
        soulRobberBlackFluctuator = new Fluctuator(this, GetSoulRobberBlack, SetSoulRobberBlack);
        jumpEventFluctuator = new Fluctuator(this, GetJumpEventFlashWeight, SetJumpEventFlashWeight);
        skyboxChromaticCoroutine = new SingleCoroutine(this);
        skyboxChromaticAmount = new BoolTimer(this);

        ResetSkyboxParameters();
        hitVolume.weight = 0;
        StopShiftVfx();

        gm.onInitilizationFinish.AddListener(StartCubesDissolveOnStart);
    }

    protected void InitScreenSizeAtVfxDistance() {
        float vfxDistance = Vector3.Distance(camera.transform.position, wallVfx.transform.position);
        Vector3[] frustumCorners = new Vector3[4];
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), vfxDistance, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        screenSizeAtVfxDistance = new Vector2(frustumCorners.Select(c => c.x).Max() - frustumCorners.Select(c => c.x).Min(),
                                              frustumCorners.Select(c => c.y).Max() - frustumCorners.Select(c => c.y).Min());
    }

    protected void StartCubesDissolveOnStart() {
        foreach(Cube cube in gm.map.GetAllCubes()) {
            cube.SetDissolveOnInitialization();
        }
    }

    protected void ResetSkyboxParameters() {
        RenderSettings.skybox.SetFloat("_Luminosity", 1.0f);
        RenderSettings.skybox.SetColor("_RectangleColor", GetSkyboxHDRColor(skyboxRectangleColor));
        RenderSettings.skybox.SetFloat("_ProportionRectangles", skyboxProportionRectangles);
        RenderSettings.skybox.SetFloat("_ScrollSpeedPower", skyboxScrollSpeedPower);
        RenderSettings.skybox.SetFloat("_VariationsAmplitude", skyboxVariationsAmplitude);
        RenderSettings.skybox.SetFloat("_Rotation", 0.0f);
        RenderSettings.skybox.SetVector("_LayersOffset", Vector3.zero);
        ApplySkyboxParameters(skyboxInitialColors);
    }

    public Color GetSkyboxHDRColor(Color color) {
        return color;
        //return color * Mathf.Pow(2, skyboxRectangleColorIntensity);
    }

    public void ApplySkyboxOverrideRotation() {
        RenderSettings.skybox.SetFloat("_Rotation", skyboxRotationWhenOverride * MathTools.RandomSign());
    }

    public void SetSkyboxToBlackout() {
        RenderSettings.skybox.SetFloat("_Luminosity", skyboxBlackoutLuminosity);
    }

    public void UpdateWallEffect() {
        Player.EtatPersonnage etat = gm.player.GetEtat();
        if(!isWallVfxOn && etat == Player.EtatPersonnage.AU_MUR) {
            StartWallEffect();
        } else if (isWallVfxOn && etat != Player.EtatPersonnage.AU_MUR) {
            StopWallEffect();
        }
        if (etat == Player.EtatPersonnage.AU_MUR) {
            OrientWallVfxEffect();
        }
    }

    public void StopWallEffect() {
        isWallVfxOn = false;
        if (PrefsManager.GetBool(PrefsManager.WALL_WARP, MenuOptions.defaultWallWarpActivation)) {
            StopWallVfx();
        }
        if (PrefsManager.GetBool(PrefsManager.WALL_DISTORSION, MenuOptions.defaultWallDistorsionActivation)) {
            StopWallDistorsionEffect();
        }
    }

    public void StopWallDistorsionEffect() {
        wallLensDistorsionFluctuator.GoTo(0.0f, timeToMinWallPostProcess);
        wallChromaticAberrationFluctuator.GoTo(0.0f, timeToMinWallPostProcess);
    }

    public void StartWallEffect() {
        isWallVfxOn = true;
        if (PrefsManager.GetBool(PrefsManager.WALL_WARP, MenuOptions.defaultWallWarpActivation)) {
            StartWallVfx();
        }
        if (PrefsManager.GetBool(PrefsManager.WALL_DISTORSION, MenuOptions.defaultWallDistorsionActivation)) {
            StartWallDistorsionEffect();
        }
    }

    public void StartWallDistorsionEffect() {
        wallLensDistorsionFluctuator.GoTo(lensDistorsionMaxValue, timeToMaxWallPostProcess);
        wallChromaticAberrationFluctuator.GoTo(chromaticAberrationMaxValue, timeToMaxWallPostProcess);
    }

    protected float GetLensDistorsionIntensity() {
        LensDistortion lensDistortion;
        wallPostProcessVolume.profile.TryGet<LensDistortion>(out lensDistortion);
        return lensDistortion.intensity.GetValue<float>();
    }

    protected void SetLensDistorsionIntensity(float intensity) {
        LensDistortion lensDistortion;
        wallPostProcessVolume.profile.TryGet<LensDistortion>(out lensDistortion);
        lensDistortion.intensity.Override(intensity);
    }

    public void StartBlackAndWhiteEffect(float blackDuration) {
        noBlackAndWhiteCamera.gameObject.SetActive(true);
        camera.cullingMask = camera.cullingMask & ~ (1 << LayerMask.NameToLayer(LAYER_NO_BLACK_AND_WHITE_POST_PROCESS)); // remove B&W layer
        soulRobberWeightFluctuator.GoTo(1.0f, timeToBlackAndWhite);
        soulRobberBlackFluctuator.GoTo(0.03f, blackDuration, soulRobberBlackCurve);
    }

    public void StopBlackAndWhiteEffect() {
        noBlackAndWhiteCamera.gameObject.SetActive(false);
        camera.cullingMask = camera.cullingMask | (1 << LayerMask.NameToLayer(LAYER_NO_BLACK_AND_WHITE_POST_PROCESS)); // add B&W layer
        soulRobberWeightFluctuator.GoTo(0.0f, timeFromBlackAndWhite / gm.player.GetTimeHackCurrentSlowmotionFactor());
        soulRobberBlackFluctuator.GoTo(1.0f, 0.0f);
    }

    public void SetBlackAndWhiteEffectToBarelyVisible() {
        soulRobberBlackFluctuator.GoTo(soulRobberRestoreBlackValue, soulRobberRestoreBlackDuration, soulRobberRestoreBlackCurve);
    }

    protected float GetChromaticAberrationIntensity() {
        ChromaticAberration chromaticAberration;
        wallPostProcessVolume.profile.TryGet<ChromaticAberration>(out chromaticAberration);
        return chromaticAberration.intensity.GetValue<float>();
    }

    protected void SetChromaticAberrationIntensity(float intensity) {
        ChromaticAberration chromaticAberration;
        wallPostProcessVolume.profile.TryGet<ChromaticAberration>(out chromaticAberration);
        chromaticAberration.intensity.Override(intensity);
    }

    protected float GetSoulRobberWeight() {
        return soulRobberVolume.weight;
    }

    protected void SetSoulRobberWeight(float weight) {
        soulRobberVolume.weight = weight;
    }

    protected float GetSoulRobberBlack() {
        ColorAdjustments colorAdjustments;
        soulRobberVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
        return colorAdjustments.colorFilter.value.r;
    }

    protected void SetSoulRobberBlack(float grayValue) {
        ColorAdjustments colorAdjustments;
        soulRobberVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
        colorAdjustments.colorFilter.Override(new Color(grayValue, grayValue, grayValue));
    }

    protected void StopGripEffect() {
        if (gripCoroutine != null) {
            StopCoroutine(gripCoroutine);
        }
        gripCoroutine = StartCoroutine(SetVignette(0.0f, changeTimeGrip, gripVolume));
    }

    protected void StartGripEffect() {
        if (gripCoroutine != null) {
            StopCoroutine(gripCoroutine);
        }
        gripCoroutine = StartCoroutine(SetVignette(intensityGrip, changeTimeGrip, gripVolume));
    }

    protected void OrientWallVfxEffect() {
        if (!PrefsManager.GetBool(PrefsManager.WALL_WARP, MenuOptions.defaultWallWarpActivation)) {
            return;
        }
        // Horizontal Angle
        Camera camera = gm.player.camera;
        Vector3 up = gm.gravityManager.Up();
        Vector3 currentPointOnMur = gm.player.GetCurrentPosOnMur();
        Vector3 camera2MurHorizontal = Vector3.ProjectOnPlane(currentPointOnMur - gm.player.transform.position, up);
        Vector3 cameraForwardHorizontal = Vector3.ProjectOnPlane(camera.transform.forward, up);
        float horizontalAngle = Vector3.SignedAngle(cameraForwardHorizontal, camera2MurHorizontal, up);
        float angle = 0;

        // Angle
        if (Mathf.Abs(horizontalAngle) >= tresholdDownFront && Mathf.Abs(horizontalAngle) <= 180 - tresholdDownBack) {
            if(horizontalAngle >= tresholdDownFront) {
                if (horizontalAngle <= 180 - tresholdDownBack - transitionToTresholdBack) {
                    angle = Mathf.Min(horizontalAngle.Remap(tresholdDownFront, tresholdDownFront + transitionToTresholdFront, 0, 90), 90);
                } else {
                    angle = horizontalAngle.Remap(180 - tresholdDownBack - transitionToTresholdBack, 180 - tresholdDownBack, 90, 0);
                }
            } else {
                if (horizontalAngle >= -180 + tresholdDownBack + transitionToTresholdBack) {
                    angle = Mathf.Max(horizontalAngle.Remap(-tresholdDownFront, -tresholdDownFront - transitionToTresholdFront, 0, -90), -90);
                } else {
                    angle = horizontalAngle.Remap(-180 + tresholdDownBack + transitionToTresholdBack, - 180 + tresholdDownBack, -90, 0);
                }
            }
        } else {
            angle = 0;
        }

        // Inverse Angle
        float verticalAngle = Vector3.SignedAngle(camera.transform.forward, cameraForwardHorizontal, camera.transform.right);
        if (Mathf.Abs(horizontalAngle) <= 180 - tresholdDownBack - transitionToTresholdBack) {
            if (verticalAngle <= -inverseAngleTreshold) {
                angle = 180 * Mathf.Sign(angle) - angle;
            }
        } else {
            if (verticalAngle >= inverseAngleTreshold) {
                angle = 180 * Mathf.Sign(angle) - angle;
            }
        }

        // Ensure no flickering :)
        if(Vector3.Distance(lastFrameWallVfxPoint, currentPointOnMur) <= 0.4f
        && Mathf.Abs(lastFrameWallVfxHorizontalAngle - horizontalAngle) >= 15f) {
            angle = lastFrameWallVfxAngle;
        } else {
            lastFrameWallVfxAngle = angle;
        }
        lastFrameWallVfxHorizontalAngle = horizontalAngle;
        lastFrameWallVfxPoint = currentPointOnMur;

        // Assign Angle
        Transform wallVfxHolder = wallVfx.transform.parent;
        Vector3 newRotation = wallVfxHolder.localRotation.eulerAngles;
        newRotation.z = angle;
        wallVfxHolder.localRotation = Quaternion.Euler(newRotation);

        // Adjust Offset
        float horizontalRatio = Mathf.Abs(angle) <= 90 ? Mathf.Abs(angle) / 90 : 1 - (Mathf.Abs(angle) - 90) / 90;
        float screenSizeOriented = MathCurves.Linear(screenSizeAtVfxDistance.y, screenSizeAtVfxDistance.x, horizontalRatio);
        wallVfx.SetFloat("ScreenOffsetPrimary", screenSizeOriented * wallVFXOffsetPercentageOfScreenPrimary);
        wallVfx.SetFloat("ScreenOffsetSecondary", screenSizeOriented * wallVFXOffsetPercentageOfScreenSecondary);
    }

    public void UnPauseEffects() {
        if(!inputManager.GetShift()) {
            StopShiftVfx();
        }
    }

    public void UpdateShiftEffect() {
        if (!PrefsManager.GetBool(PrefsManager.SHIFT_WARP, MenuOptions.defaultShiftWarpActivation)) {
            return;
        }
        if(gm.player.GetShiftLandingMode() == Player.ShiftLandingMode.NONE) {
            return;
        }
        if(inputManager.GetShiftDown()) {
            StartShiftVfx();
        } else if (inputManager.GetShiftUp()) {
            StopShiftVfx();
        }
    }

    public void UpdateHitEffect() {
        if (hitCoroutine != null) {
            StopCoroutine(hitCoroutine);
        }
        hitCoroutine = StartCoroutine(CUpdateHitEffect());
    }

    IEnumerator CUpdateHitEffect() {
        Vignette vignette;
        hitVolume.profile.TryGet<Vignette>(out vignette);
        vignette.intensity.Override(intensityVignetteHit);

        ChromaticAberration chromaticAberration;
        hitVolume.profile.TryGet<ChromaticAberration>(out chromaticAberration);
        chromaticAberration.intensity.Override(intensityChromaticHit);

        Timer timer = new Timer(changeTimeHit);
        hitVolume.weight = 1;
        while(!timer.IsOver()) {
            float avancement = 1 - timer.GetAvancement();
            hitVolume.weight = MathCurves.CubicInverse(0, 1, avancement);
            yield return null;
        }
        hitVolume.weight = 0;
    }

    IEnumerator SetVignette(float targetValue, float changeTime, Volume volume) {
        VolumeProfile profile = volume.profile;
        Vignette vignette;
        if(!profile.TryGet<Vignette>(out vignette)) {
            Debug.LogError($"Le profil {profile} doit contenir une vignette !");
        }
        float startValue = vignette.intensity.GetValue<float>();
        float amountNeeded = targetValue - startValue;

        Timer timer = new Timer(changeTime);
        while (!timer.IsOver()) {
            float newValue = startValue + timer.GetAvancement() * amountNeeded;
            vignette.intensity.Override(newValue);
            yield return null;
        }
        vignette.intensity.Override(targetValue);
    }

    public void SetBlur(bool state) {
        pousseeVolume.gameObject.SetActive(state);
        //MotionBlur blur = pousseeVolume.profile.GetSetting<MotionBlur>();
        //blur.active = state;
    }

    public void StartDashVfx(float duree) {
        dashVfx.SetFloat("Duration", duree);
        dashVfx.SendEvent("Dash");
    }

    public void StartPowerDashVfx(float duree) {
        powerDashVfx.SetFloat("Duration", duree);
        powerDashVfx.SendEvent("Dash");
    }

    public void StartGripDashVfx(float duree) {
        gripDashVfx.SetFloat("Duration", duree);
        gripDashVfx.SendEvent("Dash");
    }

    public void StartShiftVfx() {
        shiftVfx.SendEvent("ShiftStart");
    }

    public void StopShiftVfx() {
        shiftVfx.SendEvent("ShiftStop");
    }

    public void StartWallVfx() {
        wallVfx.SendEvent("WallStart");
    }

    public void StopWallVfx() {
        wallVfx.SendEvent("WallStop");
    }

    protected void InitShiftVfx() {
        shiftVfx.SetFloat("ScreenOffsetPrimary", screenSizeAtVfxDistance.y * shiftVFXOffsetPercentageOfScreenPrimary);
        shiftVfx.SetFloat("ScreenOffsetSecondary", screenSizeAtVfxDistance.y * shiftVFXOffsetPercentageOfScreenSecondary);
    }

    protected void InitJumpVfx() {
        jumpVfx.SetFloat("ScreenOffsetPrimary", screenSizeAtVfxDistance.x * jumpVFXOffsetPercentageOfScreenPrimary);
        jumpVfx.SetFloat("ScreenOffsetSecondary", screenSizeAtVfxDistance.x * jumpVFXOffsetPercentageOfScreenSecondary);
        jumpVfx.SetFloat("ScreenOffsetSecondaryVertical", - screenSizeAtVfxDistance.y);
    }

    protected void InitShiftLandingVfx() {
        shiftLandingVfx.SetFloat("ScreenOffsetPrimary", screenSizeAtVfxDistance.x * shiftLandingVFXOffsetPercentageOfScreenPrimary);
        shiftLandingVfx.SetFloat("ScreenOffsetSecondary", screenSizeAtVfxDistance.x * shiftLandingVFXOffsetPercentageOfScreenSecondary);
        shiftLandingVfx.SetFloat("ScreenOffsetSecondaryVertical", - screenSizeAtVfxDistance.y);
    }

    protected void InitTimeScaleVfx() {
        timeScaleEffectActivation = PrefsManager.GetBool(PrefsManager.TIME_SCALE_EFFECT, MenuOptions.defaultTimeScaleEffectActivation);
        float horizontalOffset = screenSizeAtVfxDistance.x * timeScaleVFXOffsetPercentageOfScreen.x;
        float contraction = screenSizeAtVfxDistance.y * timeScaleVFXOffsetPercentageOfScreen.y / horizontalOffset;
        timeScaleVfx.SetFloat("ElipseContraction", contraction);
        timeScaleVfx.SetFloat("RadialOffset", horizontalOffset);
        SetTimeScaleVfxPhase(0);
    }

    public void SetTimeScaleVfxPhase(int phaseIndice) {
        float spawnRate = timeScaleVFXspawnRateByPhases[phaseIndice];
        if(!timeScaleEffectActivation) {
            spawnRate = 0;
        }
        timeScaleVfx.SetFloat("PrimarySpawnRate", spawnRate);
        if (spawnRate > 0 && !timeScaleVfxIsRunning) {
            timeScaleVfx.SendEvent("Start");
            timeScaleVfxIsRunning = true;
        }
        if(spawnRate == 0) {
            timeScaleVfxIsRunning = false;
        }
    }

    public void StopTimeScaleVfx() {
        timeScaleVfx.SendEvent("Stop");
        timeScaleVfxIsRunning = false;
    }

    public void StartJumpEffect() {
        if (PrefsManager.GetBool(PrefsManager.JUMP_WARP, MenuOptions.defaultJumpWarpActivation)) {
            jumpVfx.SendEvent("Start");
        }
    }

    public void StartShiftLandingEffect() {
        shiftLandingVfx.SendEvent("Start");
    }

    public void SetAlwaysHasMoveForTimeScaleVfx() {
        alwaysHasMoveForTimeScaleVfx = true;
    }

    public void UpdateTimeScaleVfx(bool hasMove) {
        if(!alwaysHasMoveForTimeScaleVfx && !hasMove) {
            SetTimeScaleVfxPhase(0);
        } else {
            SetTimeScaleVfxPhase(gm.timerManager.GetCurrentPhaseIndice());
        }
    }

    public void SetTimeScaleEffectActivation(bool state) {
        if(timeScaleEffectActivation != state) {
            UpdateTimeScaleVfx(gm.player.HasMoveSinceLastFrame());
            timeScaleEffectActivation = state;
        }
    }

    public void MakePostExposureForJumpBounce(float intensityFlash, float dureeFadeInFlash, float dureeFadeOutFlash) {
        StartCoroutine(CMakePostExposureForJumpBounce(intensityFlash, dureeFadeInFlash, dureeFadeOutFlash));
    }

    protected IEnumerator CMakePostExposureForJumpBounce(float intensityFlash, float dureeFadeInFlash, float dureeFadeOutFlash) {
        ColorAdjustments colorAdjustments;
        jumpEventVolume.profile.TryGet(out colorAdjustments);
        colorAdjustments.postExposure.Override(intensityFlash);

        jumpEventFluctuator.GoTo(1, dureeFadeInFlash);
        yield return new WaitForSeconds(dureeFadeInFlash);
        jumpEventFluctuator.GoTo(0, dureeFadeOutFlash);
    }

    protected float GetJumpEventFlashWeight() {
        return jumpEventVolume.weight;
    }

    protected void SetJumpEventFlashWeight(float value) {
        jumpEventVolume.weight = value;
    }

    protected void InitLuminosityPostProcess() {
        SetLuminosityIntensity(PrefsManager.GetFloat(PrefsManager.LUMINOSITY, MenuOptions.defaultLuminosity));
    }

    public void SetLuminosityIntensity(float value) {
        SetLuminosityIntensity(luminosityVolume, value);
    }

    public static void SetLuminosityIntensity(Volume luminosityVolume, float value) {
        luminosityVolume.weight = Mathf.Abs(value) <= 0.05f ? 0 : 1;

        /// En fait on va juste toucher au gain, comme ça on ira vers le noir et pas vers le gris ! :)
        //ColorAdjustments colorAdjustments;
        //luminosityVolume.profile.TryGet(out colorAdjustments);
        //colorAdjustments.contrast.Override(value * 100);

        LiftGammaGain liftGammaGain;
        luminosityVolume.profile.TryGet(out liftGammaGain);
        liftGammaGain.gain.Override(new Vector4(0, 0, 0, value));
    }

    public static float GetLuminosityIntensity(Volume luminosityVolume) {
        LiftGammaGain liftGammaGain;
        luminosityVolume.profile.TryGet(out liftGammaGain);
        return liftGammaGain.gain.GetValue<Vector4>().z;
    }

    public void ShakeOnceOnMarker() {
        CameraShaker cs = CameraShaker.Instance;
        cs.ShakeOnce(markerScreenShakeMagnitude, markerScreenShakeRoughness, 0.1f, markerScreenShakeDecreaseTime);
    }

    public void AddSkyboxChromaticShiftOf(float intensity) {
        if (skyboxChromaticAmount.IsOver()) {
            skyboxChromaticShiftCoef = skyboxChromaticShiftCoef * MathTools.RandomSign();
            skyboxChromaticAmount.AddTime(intensity);
            skyboxChromaticCoroutine.Start(CUpdateSkyboxChromaticShift());
        } else {
            skyboxChromaticAmount.AddTime(intensity);
        }
    }

    protected IEnumerator CUpdateSkyboxChromaticShift() {
        while(!skyboxChromaticAmount.IsOver()) {
            float value = skyboxChromaticAmount.RemainingTime() * skyboxChromaticShiftCoef;
            RenderSettings.skybox.SetVector("_LayersOffset", new Vector3(value, -Math.Abs(value), 0));
            yield return null;
        }
        RenderSettings.skybox.SetVector("_LayersOffset", Vector3.zero);
    }

    public void ApplySkyboxChromaticShiftOnStartDisconnection() {
        StartCoroutine(CApplySkyboxChromaticShiftOnStartDisconnection());
    }

    protected IEnumerator CApplySkyboxChromaticShiftOnStartDisconnection() {
        List<float> times = Enumerable.Range(0, skyboxChromaticShiftNbOnDisconnection).Select(n => (float)n / skyboxChromaticShiftNbOnDisconnection).ToList();
        Timer timer = new Timer(skyboxChromaticShiftTotalDurationOnDisconnection);
        while (!timer.IsOver() || times.Count > 0) {
            if (times.Count > 0 && timer.GetAvancement() >= times.First()) {
                times.RemoveAt(0);
                AddSkyboxChromaticShiftOf(skyboxChromaticShiftDurationOnDisconnection);
            }
            yield return null;
        }
    }

    public void ApplySkyboxParameters(SkyboxParameters skyboxParameters) {
        RenderSettings.skybox.SetFloat("_ProportionRectanglesL1", skyboxParameters.proportionRectanglesL1);
        RenderSettings.skybox.SetFloat("_ProportionRectanglesL2", skyboxParameters.proportionRectanglesL2);
        RenderSettings.skybox.SetFloat("_ProportionRectanglesL3", skyboxParameters.proportionRectanglesL3);

        RenderSettings.skybox.SetColor("_PrimaryColorL1", skyboxParameters.primariColorL1);
        RenderSettings.skybox.SetColor("_SecondaryColorL1", skyboxParameters.secondaryColorL1);
        RenderSettings.skybox.SetColor("_TertiaryColorL1", skyboxParameters.tertiaryColorL1);

        RenderSettings.skybox.SetColor("_PrimaryColorL2", skyboxParameters.primariColorL2);
        RenderSettings.skybox.SetColor("_SecondaryColorL2", skyboxParameters.secondaryColorL2);
        RenderSettings.skybox.SetColor("_TertiaryColorL2", skyboxParameters.tertiaryColorL2);

        RenderSettings.skybox.SetColor("_PrimaryColorL3", skyboxParameters.primariColorL3);
        RenderSettings.skybox.SetColor("_SecondaryColorL3", skyboxParameters.secondaryColorL3);
        RenderSettings.skybox.SetColor("_TertiaryColorL3", skyboxParameters.tertiaryColorL3);
    }

    public void ApplyInterpolatedSkyboxParameters(SkyboxParameters p1, SkyboxParameters p2, float t) {
        SkyboxParameters res = new SkyboxParameters();
        res.proportionRectanglesL1 = MathCurves.Linear(p1.proportionRectanglesL1, p2.proportionRectanglesL1, t);
        res.proportionRectanglesL2 = MathCurves.Linear(p1.proportionRectanglesL2, p2.proportionRectanglesL2, t);
        res.proportionRectanglesL3 = MathCurves.Linear(p1.proportionRectanglesL3, p2.proportionRectanglesL3, t);

        res.primariColorL1 = ColorManager.InterpolateHSV(p1.primariColorL1, p2.primariColorL1, t);
        res.secondaryColorL1 = ColorManager.InterpolateHSV(p1.secondaryColorL1, p2.secondaryColorL1, t);
        res.tertiaryColorL1 = ColorManager.InterpolateHSV(p1.tertiaryColorL1, p2.tertiaryColorL1, t);

        res.primariColorL2 = ColorManager.InterpolateHSV(p1.primariColorL2, p2.primariColorL2, t);
        res.secondaryColorL2 = ColorManager.InterpolateHSV(p1.secondaryColorL2, p2.secondaryColorL2, t);
        res.tertiaryColorL2 = ColorManager.InterpolateHSV(p1.tertiaryColorL2, p2.tertiaryColorL2, t);

        res.primariColorL3 = ColorManager.InterpolateHSV(p1.primariColorL3, p2.primariColorL3, t);
        res.secondaryColorL3 = ColorManager.InterpolateHSV(p1.secondaryColorL3, p2.secondaryColorL3, t);
        res.tertiaryColorL3 = ColorManager.InterpolateHSV(p1.tertiaryColorL3, p2.tertiaryColorL3, t);

        ApplySkyboxParameters(res);
    }
}
