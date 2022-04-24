using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class RewardCamera : MonoBehaviour {

    [Header("Link")]
    public Volume contrasteVolume;

    protected HistoryManager hm;
    protected RewardManager rm;
    [HideInInspector] public Camera cam;

    public virtual void Initialize() {
        hm = HistoryManager.Instance;
        rm = RewardManager.Instance;
        cam = GetComponentInChildren<Camera>();
        PostProcessManager.SetContrasteIntensity(contrasteVolume, PrefsManager.GetFloat(PrefsManager.CONTRASTE_KEY, MenuOptions.defaultContraste));
    }

    public abstract void Update();
}
