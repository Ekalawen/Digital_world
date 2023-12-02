using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class Achievement_UseSupercheatedpassword : Achievement {

    [Header("Parameters")]
    public int treshold = 1;

    protected override void InitializeSpecific() {
        sm.unlockScreen.onUseSupercheatedpassword.AddListener(UnlockIfBetterTreshold);
    }

    protected virtual void UnlockIfBetterTreshold() {
        if (PrefsManager.GetInt(PrefsManager.SUPERCHEATEDPASSWORD_NB_USE, 0) >= treshold) {
            Unlock();
        }
    }
}
