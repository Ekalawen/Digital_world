using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTimeItem : Item {

    public float settedTime = 25.0f;

    public override void OnTrigger(Collider hit) {
        float currentTime = gm.timerManager.GetRemainingTime();
        float timeToRemove = settedTime - currentTime;
        gm.timerManager.AddTime(timeToRemove);
    }
}
