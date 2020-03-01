using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RewardLumiereDisplayer : RewardObjectDisplayer {

    public override void ResetObject() {
        base.ResetObject();
        Debug.Log("On LightCreation = \n" + Environment.StackTrace);
    }

    public override void Update() {
        if (obj == null)
            return;
        float lastTime = history.LastTime();
        float avancement = durationTimer.GetAvancement();
        //Debug.Log("dureeReward = " + duration + " avancement = " + avancement + " lastTime = " + lastTime + " acceleration = " + acceleration);
        //if (avancement >= (lastTime / duration) * acceleration) {
        if (avancement > 1.0f) {
            Debug.Log("DESTORYED !!!");
            Destroy(obj.gameObject);
            obj = null;
        } else {
            obj.transform.position = curve.GetAvancement(avancement);
        }
    }
}

