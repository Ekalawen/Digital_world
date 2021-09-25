using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RewardCamera : MonoBehaviour {

    protected HistoryManager hm;
    protected RewardManager rm;
    [HideInInspector] public Camera cam;

    public virtual void Initialize() {
        hm = HistoryManager.Instance;
        rm = RewardManager.Instance;
        cam = GetComponentInChildren<Camera>();
    }

    public abstract void Update();
}
