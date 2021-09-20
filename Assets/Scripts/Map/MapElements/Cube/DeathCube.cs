using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCube : NonBlackCube {

    public override void Initialize() {
        base.Initialize();
        CheckPlayerCollisionOnStart();
    }

    protected virtual void CheckPlayerCollisionOnStart() {
        GameManager gm = GameManager.Instance;
        Vector3 playerPos = gm.player.transform.position;
        if (gm.player.DoubleCheckInteractWithCube(this)) {
            KillPlayer();
        }
    }

    public override void InteractWithPlayer() {
        float time = Time.time;
        float decomposeStartingTime = transparentMaterial.GetFloat("_DecomposeStartingTime");
        if (time < decomposeStartingTime && !gm.player.IsInvincible()) {
            Debug.Log("Looooooooooooooooose ! :'(");
            KillPlayer();
        }
    }

    public virtual void KillPlayer() {
        if (!gm.eventManager.IsGameOver()) {
            gm.eventManager.LoseGame(EventManager.DeathReason.TOUCHED_DEATH_CUBE);
        }
    }
}
