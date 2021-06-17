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

public class PostProcessManager : MonoBehaviour {

    [Header("Grip")]
    public float changeTimeGrip = 0.075f;
    public float intensityGrip = 0.315f;
    public Volume gripVolume;

    [Header("Hit")]
    public float changeTimeHit = 0.6f;
    public float intensityHit= 0.4f;
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

    [Header("CubeDissolve")]
    public float dissolveRegularTime = 3.0f;
    public float dissolveRegularPlayerProximityCoef = 0.037f;
    public float dissolveInfiniteTime = 0.0f;
    public float dissolveInfinitePlayerProximityCoef = -0.037f;
    public float dissolveInGameTime = 1.0f;
    public float dissolveInGamePlayerProximityCoef = 0.0f;

    [Header("CubeExplosion")]
    public GameObject explosionParticlesPrefab;

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

    [Header("Wall Post Process")]
    public AnimationCurve wallPostProcessCurve;
    public float timeToMaxWallPostProcessCurve = 0.4f;
    public float timeToMinWallPostProcessCurve = 0.1f;
    public Volume wallPostProcessVolume;
    public float lensDistorsionMaxValue = 0.4f;
    public float chromaticAberrationMaxValue = 0.2f;

    protected Coroutine gripCoroutine = null;
    protected Coroutine hitCoroutine1 = null;
    protected Coroutine hitCoroutine2 = null;
    protected Coroutine wallPostProcessCoroutine = null;
    protected GameManager gm;
    protected new Camera camera;
    protected VisualEffect dashVfx;
    protected VisualEffect shiftVfx;
    protected VisualEffect wallVfx;
    protected InputManager inputManager;
    protected float lastFrameWallVfxAngle = 0;
    protected float lastFrameWallVfxHorizontalAngle = 0;
    protected Vector3 lastFrameWallVfxPoint = Vector3.zero;
    protected Vector2 screenSizeAtVfxDistance;

    public void Initialize() {
        gm = GameManager.Instance;
        camera = gm.player.camera;
        dashVfx = gm.player.dashVfx;
        shiftVfx = gm.player.shiftVfx;
        wallVfx = gm.player.wallVfx;
        inputManager = InputManager.Instance;
        InitScreenSizeAtVfxDistance();
        InitShiftVfx();

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
        RenderSettings.skybox.SetColor("_RectangleColor", GetSkyboxHDRColor(skyboxRectangleColor));
        RenderSettings.skybox.SetFloat("_ProportionRectangles", skyboxProportionRectangles);
        RenderSettings.skybox.SetFloat("_ScrollSpeedPower", skyboxScrollSpeedPower);
        RenderSettings.skybox.SetFloat("_VariationsAmplitude", skyboxVariationsAmplitude);
    }

    public Color GetSkyboxHDRColor(Color color) {
        return color;
        //return color * Mathf.Pow(2, skyboxRectangleColorIntensity);
    }

    public void UpdateGripEffect(Player.EtatPersonnage previousState) {
        if (!PrefsManager.GetBool(PrefsManager.GRIP_KEY, MenuOptions.defaultGripActivation)) {
            return;
        }

        Player.EtatPersonnage etat = gm.player.GetEtat();
        if(previousState != Player.EtatPersonnage.AU_MUR && etat == Player.EtatPersonnage.AU_MUR) {
            StartWallVfx();
            StartWallPostProcessCoroutine(lensDistorsionMaxValue, chromaticAberrationMaxValue, timeToMaxWallPostProcessCurve);
        } else if (previousState == Player.EtatPersonnage.AU_MUR && etat != Player.EtatPersonnage.AU_MUR) {
            StopWallVfx();
            StartWallPostProcessCoroutine(0.0f, 0.0f, timeToMinWallPostProcessCurve);
        }
        if (etat == Player.EtatPersonnage.AU_MUR) {
            OrientWallVfxEffect();
        }
    }

    protected void StartWallPostProcessCoroutine(float lensIntensity, float chromaticIntensity, float duration) {
        if(wallPostProcessCoroutine != null) {
            StopCoroutine(wallPostProcessCoroutine);
        }
        wallPostProcessCoroutine = StartCoroutine(CStartWallPostProcessCoroutine(lensIntensity, chromaticIntensity, duration));
    }

    protected IEnumerator CStartWallPostProcessCoroutine(float lensTargetIntensity, float chromaticTargetIntensity, float duration) {
        LensDistortion lensDistortion;
        ChromaticAberration chromaticAberration;
        wallPostProcessVolume.profile.TryGet<LensDistortion>(out lensDistortion);
        wallPostProcessVolume.profile.TryGet<ChromaticAberration>(out chromaticAberration);

        float lensStartIntensity = lensDistortion.intensity.GetValue<float>();
        float chromaticStartIntensity = chromaticAberration.intensity.GetValue<float>();

        Timer timer = new Timer(duration);
        while(!timer.IsOver()) {
            float avancement = wallPostProcessCurve.Evaluate(timer.GetAvancement());

            float newLensIntensity = MathCurves.Linear(lensStartIntensity, lensTargetIntensity, avancement);
            lensDistortion.intensity.Override(newLensIntensity);

            float newChromaticIntensity = MathCurves.Linear(chromaticStartIntensity, chromaticTargetIntensity, avancement);
            chromaticAberration.intensity.Override(newChromaticIntensity);

            yield return null;
        }
        lensDistortion.intensity.Override(lensTargetIntensity);
        chromaticAberration.intensity.Override(chromaticTargetIntensity);
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
        if(inputManager.GetShiftDown()) {
            StartShiftVfx();
        } else if (inputManager.GetShiftUp()) {
            StopShiftVfx();
        }
    }

    public void UpdateHitEffect() {
        hitVolume.weight = 1;
        Vignette vignette;
        if (!hitVolume.profile.TryGet<Vignette>(out vignette)) {
            Debug.LogError($"Le profil {hitVolume.profile} doit contenir une vignette !");
        }
        vignette.intensity.Override(intensityHit);
        if (hitCoroutine1 != null) {
            StopCoroutine(hitCoroutine1);
            StopCoroutine(hitCoroutine2);
        }
        hitCoroutine2 = StartCoroutine(CUpdateHitEffect());
    }

    IEnumerator CUpdateHitEffect() {
        hitCoroutine1 = StartCoroutine(SetVignette(0.0f, changeTimeHit, hitVolume));
        Timer timer = new Timer(changeTimeHit);
        while(!timer.IsOver()) {
            hitVolume.weight = MathCurves.Quadratic(0, 1, 1 - timer.GetAvancement());
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
}
