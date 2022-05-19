using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class CreditsCamera : MonoBehaviour {

    [Header("Link")]
    public Volume luminosityVolume;

    [HideInInspector] public Camera cam;

    public virtual void Initialize() {
        cam = GetComponentInChildren<Camera>();
        PostProcessManager.SetLuminosityIntensity(luminosityVolume, PrefsManager.GetFloat(PrefsManager.LUMINOSITY_KEY, MenuOptions.defaultLuminosity));
    }
}
