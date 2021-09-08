using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialDeathCube : DeathCube {

    protected override void CheckPlayerCollisionOnStart() {
        // On ne le fait pas ! :)
    }

    public override void KillPlayer() {
        if (!gm.player.IsInvincible()) {
            EventManagerTutoriel eventManager = (EventManagerTutoriel)gm.eventManager;
            eventManager.GoBackToPreviousSaveZone();
            gm.console.SavedFromDeathCube();
        }
    }
}
