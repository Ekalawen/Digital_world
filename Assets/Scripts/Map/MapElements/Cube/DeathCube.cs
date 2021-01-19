using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCube : NonBlackCube {

    public override void Start() {
        base.Start();
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
        float decomposeStartingTime = GetMaterial().GetFloat("_DecomposeStartingTime");
        if (time < decomposeStartingTime) {
            Debug.Log("Looooooooooooooooose ! :'(");
            KillPlayer();
        }
    }

    public virtual void KillPlayer() {
        gm.eventManager.LoseGame(EventManager.DeathReason.TOUCHED_DEATH_CUBE);
    }
}
