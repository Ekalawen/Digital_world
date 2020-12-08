using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FlockBehavior : MonoBehaviour {

    protected GameManager gm;
    protected FlockManager flockManager;

    public virtual void Initialize() {
        gm = GameManager.Instance;
        flockManager = gm.flockManager;
    }

    public abstract Vector3 GetMove(IController flockController);
}
