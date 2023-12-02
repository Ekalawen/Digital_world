using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class RewardCamera : MonoBehaviour {

    [Header("Link")]
    public Volume luminosityVolume;

    protected HistoryManager hm;
    protected RewardManager rm;
    [HideInInspector] public Camera cam;

    public virtual void Initialize() {
        hm = HistoryManager.Instance;
        rm = RewardManager.Instance;
        cam = GetComponentInChildren<Camera>();
        PostProcessManager.SetLuminosityIntensity(luminosityVolume, PrefsManager.GetFloat(PrefsManager.LUMINOSITY, MenuOptions.defaultLuminosity));
    }

    public abstract void Update();
}
