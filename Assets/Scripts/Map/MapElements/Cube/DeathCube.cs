﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCube : NonBlackCube {

    protected override void Start() {
        base.Start();
        GameManager gm = GameManager.Instance;
        Vector3 playerPos = gm.player.transform.position;
        if(gm.player.DoubleCheckInteractWithCube(this)) {
            gm.eventManager.LoseGame(EventManager.DeathReason.TOUCHED_DEATH_CUBE);
        }
    }

    public override void InteractWithPlayer() {
        Debug.Log("Looooooooooooooooose ! :'(");
        gm.eventManager.LoseGame(EventManager.DeathReason.TOUCHED_DEATH_CUBE);
    }
}
