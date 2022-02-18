using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_NoTurnCamera : Achievement_FinishLevelIR
{

    public int nbBlocksCrossedMin = 100;
    public float frequenceCheck = 0.5f;
    public float tresholdMax = 10.0f;

    protected Quaternion initialQuaternion;
    protected bool hasTurnTheHead = false;

    protected override void InitializeSpecific() {
        base.InitializeSpecific();
        gm.eventManager.onGameOver.AddListener(UnlockSpecific);
        initialQuaternion = gm.player.camera.transform.rotation;
        StartCoroutine(CCheckTurnHead());
    }

    protected IEnumerator CCheckTurnHead() {
        while(true) {
            if(Quaternion.Angle(initialQuaternion, gm.player.camera.transform.rotation) >= tresholdMax) {
                hasTurnTheHead = true;
                break;
            }
            yield return new WaitForSecondsRealtime(frequenceCheck);
        }
    }

    public override void UnlockSpecific() {
        if (!hasTurnTheHead && map.GetNonStartNbBlocksRun() >= nbBlocksCrossedMin) {
            Unlock();
        }
    }
}
