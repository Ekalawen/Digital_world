using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_OutsideTheMapFor : Achievement {

    [Header("Parameters")]
    public float duration = 5.0f;
    public float frequenceCheck = 0.5f;

    protected override void InitializeSpecific() {
        StartCoroutine(CCheckPosition());
    }

    protected IEnumerator CCheckPosition() {
        Timer timeOutisdeMap = new Timer(duration);
        while(true) {
            if (!gm.map.IsInRegularMap(gm.player.transform.position)) {
                if (timeOutisdeMap.IsOver()) {
                    Unlock();
                    break;
                }
            } else {
                timeOutisdeMap.Reset();
            }
            yield return new WaitForSeconds(frequenceCheck);
        }
    }
}
