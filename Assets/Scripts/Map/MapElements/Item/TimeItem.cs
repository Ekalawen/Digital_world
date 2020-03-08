using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeItem : Item {

    public float addedTime = 10.0f;

    public override void OnTrigger(Collider hit) {
        gm.timerManager.AddTime(addedTime);
    }
}
