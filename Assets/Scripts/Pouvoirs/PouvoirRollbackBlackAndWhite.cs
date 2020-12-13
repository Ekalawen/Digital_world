using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirRollbackBlackAndWhite : PouvoirRollback {

    public float maxTimerMalus = 10.0f;

    protected override void InitPouvoir() {
        base.InitPouvoir();
        foreach(Cube cube in gm.map.GetAllCubesOfType(Cube.CubeType.SPECIAL)) {
            CorruptedCube corruptedCube = (CorruptedCube)cube;
            corruptedCube.CancelCorruption();
        }
    }

    protected override void ApplyTimerMalus() {
        float timeMalus = Mathf.Min(gm.timerManager.GetRemainingTime() / 3, maxTimerMalus);
        gm.timerManager.RemoveTime(timeMalus, EventManager.DeathReason.POUVOIR_COST);
    }
}
