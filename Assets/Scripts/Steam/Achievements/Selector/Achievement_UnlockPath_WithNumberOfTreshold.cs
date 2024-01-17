using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class Achievement_UnlockPath_WithNumberOfTreshold : Achievement_UnlockPath {

    public enum Mode { QUANTITY, MAX };

    public Mode mode = Mode.QUANTITY;
    [ConditionalHide("mode", Mode.QUANTITY)]
    public int nbTresholdsUnlocked = 1;

    protected override void UnlockPath(SelectorPath selectorPath) {
        SelectorPath_Password passwordPath = selectorPath.gameObject.GetComponent<SelectorPath_Password>();
        if(!passwordPath) {
            return;
        }

        switch (mode) {
            case Mode.QUANTITY:
                if(passwordPath.GetNbUnlockedTresholds() == nbTresholdsUnlocked) {
                    Unlock();
                }
                break;
            case Mode.MAX:
                int nbTresholdsInLevel = passwordPath.GetTresholds().Count;
                if(nbTresholdsInLevel >= 4 && passwordPath.GetNbUnlockedTresholds() == nbTresholdsInLevel) {
                    Unlock();
                }
                break;
        }
    }
}
