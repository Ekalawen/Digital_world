using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_WinInReprieveMode : Achievement {

    protected Timer timer;

    protected override void InitializeSpecific() {
        gm.eventManager.onWinGame.AddListener(UnlockSpecific);
    }

    protected void UnlockSpecific() {
        if(SoulRobber.playerWasRobbedAtEndOfGame) {
            Unlock();
        }
    }
}
