using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapFunctionComponent : MonoBehaviour {

    protected GameManager gm;
    protected MapManager map;

    public virtual void Initialize() {
        gm = GameManager.Instance;
        map = gm.map;
    }

    public abstract void Activate();
}
