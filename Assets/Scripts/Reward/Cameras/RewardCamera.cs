using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RewardCamera : MonoBehaviour {

    protected HistoryManager hm;
    protected RewardManager rm;

    public virtual void Initialize() {
        hm = HistoryManager.Instance;
        rm = RewardManager.Instance;
    }

    public abstract void Update();
}
