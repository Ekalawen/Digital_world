using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class EventManagerWhileTrueFullSpikes : EventManagerWhileTrue {

    protected bool isFirstStartEndGame = true;

    protected override void StartEndGame() {
        base.StartEndGame();
        if(isFirstStartEndGame) {
            gm.timerManager.isInfinitTime = false;
            gm.timerManager.SetTime(gm.timerManager.initialTime, showVolatileText: false);
            gm.console.FullSpikesAutoDestructionEnclenche();
            isFirstStartEndGame = false;
        }
    }
}
