using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class OverrideBlackout : Override {

    public Color lightColor = Color.black;
    public float ambientIntensity;
    public float reflectionIntensity;

    protected override void InitializeSpecific() {
        RenderSettings.ambientLight = lightColor;
        RenderSettings.ambientIntensity = ambientIntensity;
        RenderSettings.reflectionIntensity = reflectionIntensity;
        gm.postProcessManager.SetSkyboxToBlackout();
    }
}
