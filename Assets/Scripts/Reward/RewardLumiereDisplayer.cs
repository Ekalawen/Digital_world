using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardLumiereDisplayer : RewardObjectDisplayer {

    public void Initialize(GameObject prefab, ObjectHistory history, float duration, float delay, float acceleration) {
        base.Initialize(prefab, history, duration, delay, acceleration);
    }

    public override void Update() {
        if (obj == null)
            return;
        float lastTime = history.LastTime();
        float avancement = durationTimer.GetAvancement();
        if (avancement > (lastTime / duration) / acceleration) {
            Destroy(obj);
            obj = null;
        } else {
            obj.transform.position = curve.GetAvancement(avancement);
        }
    }
}

