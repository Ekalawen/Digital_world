using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RewardCamera : MonoBehaviour {

    protected HistoryManager hm;

    public virtual void Initialize() {
        hm = HistoryManager.Instance;
    }

    public abstract void Update();
}
