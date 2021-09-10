using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public struct ArcDelimiter {
    [ColorUsageAttribute(true, true)]
    public Color rectanglesColor;
    [ColorUsageAttribute(true, true)]
    public Color backgroundColor;
    public float proportionRectangles;
    public Vector2 scrollSpeed;
    public SelectorLevel maxLevelDelimiter;
}

public class SelectorSkyboxColorUpdater : MonoBehaviour {
    public float tresholdThickness = 5.0f;
    public List<ArcDelimiter> arcDelimiters;
    public SelectorManager selectorManager;

    public void Update() {
        UpdateSkyboxColor();
    }

    public void UpdateSkyboxColor() {
        float playerHeight = selectorManager.baseCamera.transform.position.y; 
        for(int i = 0; i < arcDelimiters.Count; i++) {
            ArcDelimiter arcDelimiter = arcDelimiters[i];
            float arcDelimiterHeight = arcDelimiter.maxLevelDelimiter.transform.position.y;
            if(playerHeight < arcDelimiterHeight - tresholdThickness || i == arcDelimiters.Count - 1) {
                SetSkyboxColor(arcDelimiter);
                break;
            } else if (playerHeight < arcDelimiterHeight + tresholdThickness) {
                ArcDelimiter nextArcDelimiter = arcDelimiters[i + 1];
                float avancement = (playerHeight - (arcDelimiterHeight - tresholdThickness)) / (2 * tresholdThickness);
                SetSkyboxColorInterpolated(arcDelimiter, nextArcDelimiter, avancement);
                break;
            }
        }
    }

    protected void SetSkyboxColor(Color rectangleColor, Color backgroundColor, float proportionRectangles, Vector2 scrollSpeed) {
        RenderSettings.skybox.SetColor("_RectangleColor", rectangleColor);
        RenderSettings.skybox.SetColor("_BackgroundColor", backgroundColor);
        RenderSettings.skybox.SetFloat("_ProportionRectangles", proportionRectangles);
        RenderSettings.skybox.SetVector("_ScrollSpeed", scrollSpeed);
    }

    protected void SetSkyboxColor(ArcDelimiter arcDelimiter) {
        SetSkyboxColor(arcDelimiter.rectanglesColor, arcDelimiter.backgroundColor, arcDelimiter.proportionRectangles, arcDelimiter.scrollSpeed);
    }

    protected void SetSkyboxColorInterpolated(ArcDelimiter arcDelimiter, ArcDelimiter nextArcDelimiter, float avancement) {
        Color rectangleColor = ColorManager.InterpolateColors(arcDelimiter.rectanglesColor, nextArcDelimiter.rectanglesColor, avancement);
        Color backgroundColor = ColorManager.InterpolateColors(arcDelimiter.backgroundColor, nextArcDelimiter.backgroundColor, avancement);
        float proportionRectangles = MathCurves.Linear(arcDelimiter.proportionRectangles, nextArcDelimiter.proportionRectangles, avancement);
        Vector2 scrollSpeed = Vector2.Lerp(arcDelimiter.scrollSpeed, nextArcDelimiter.scrollSpeed, avancement);
        SetSkyboxColor(rectangleColor, backgroundColor, proportionRectangles, scrollSpeed);
    }
}
