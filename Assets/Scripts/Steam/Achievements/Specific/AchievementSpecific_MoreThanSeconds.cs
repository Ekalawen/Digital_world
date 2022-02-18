using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_MoreThanSeconds : Achievement_FinishLevel {

    public float durationLeft = 30.0f;

    public override void UnlockSpecific() {
        if (gm.timerManager.GetRemainingTime() >= durationLeft) {
            Unlock();
        }
    }
}
