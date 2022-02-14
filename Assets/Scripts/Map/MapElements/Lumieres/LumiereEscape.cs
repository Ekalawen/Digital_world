using System;
using System.Collections;
using System.Collections.Generic;
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
    public List<EscapeColors> escapeColors;

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
            nbLives--;
            if(nbLives <= 0) {
                Captured();
            } else {
                TemporaryCaptured();
            }
		}
    }

    protected void TemporaryCaptured()
    {
        if (IsCaptured())
        {
            return;
        }
        isCaptured = true;

        EscapeToAnotherLocation();

        SetCapturableIn();

        ChangeColorIn();

        NotifySoundManager();

        ScreenShakeOnLumiereCapture();
    }

    protected void ChangeColorIn() {
        StartCoroutine(CChangeColorIn());
    }

    protected IEnumerator CChangeColorIn() {
        yield return new WaitForSeconds(TimeToGoToPosition(currentEscapePosition));
        SetColorAccordingToNumberOfLives();
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

    protected Vector3 ComputeEscapeLocation() {
        return Vector3.zero;
    }

    public float TimeToGoToPosition(Vector3 pos) {
        float distance = Vector3.Distance(transform.position, pos);
        return distance / controller.vitesse;
    }
}
