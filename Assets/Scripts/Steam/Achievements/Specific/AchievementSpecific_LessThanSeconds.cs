using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_LessThanSeconds : Achievement_FinishLevel {

    public float duration = 10.0f;

    public override void UnlockSpecific() {
        if (gm.timerManager.GetElapsedTime() <= duration) {
            Unlock();
        }
    }
}
