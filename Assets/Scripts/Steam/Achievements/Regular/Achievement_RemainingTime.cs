using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_RemainingTime : Achievement {

    public enum Mode { LESS_THAN, MORE_THAN_STARTING_TIME };

    [Header("Parameters")]
    public Mode mode;
    [ConditionalHide("mode", Mode.LESS_THAN)]
    public float duration = 1.0f;

    protected override void InitializeSpecific() {
        gm.eventManager.onWinGame.AddListener(OnWin);
    }

    public void OnWin() {
        float remainingTime = gm.timerManager.GetRemainingTime();
        switch (mode) {
            case Mode.LESS_THAN:
                if (remainingTime <= duration) {
                    Unlock();
                }
                break;
            case Mode.MORE_THAN_STARTING_TIME:
                if (remainingTime >= gm.timerManager.initialTime) {
                    Unlock();
                }
                break;
        }
    }
}
