﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventManagerWhileTrueConstantDestruction : EventManagerWhileTrue {

    protected bool isFirstStartEndGame = true;
    protected List<Vector3> originalCubePositions;

    public override void Initialize() {
        base.Initialize();
        originalCubePositions = map.GetAllCubes().Select(c => c.transform.position).ToList();
    }

    protected override void StartEndGame() {
        base.StartEndGame();
        if(isFirstStartEndGame) {
            gm.timerManager.isInfinitTime = false;
            gm.timerManager.SetTime(gm.timerManager.initialTime, showVolatileText: false);
            isFirstStartEndGame = false;
        }
    }

    protected override IEnumerator CRestoreOriginalMap() {
        int nbCubesToCreateByFrame = 15;  // Equivalent à 10ms sur mon ordi pour la destruction
        originalCubePositions = originalCubePositions.OrderBy(pos => Vector3.SqrMagnitude(pos - gm.player.transform.position)).ToList();
        int nbCubesCreated = 0;
        foreach(Vector3 pos in originalCubePositions) {
            Cube cube = map.AddCube(pos, Cube.CubeType.NORMAL);
            if (cube != null) {
                cube.ShouldRegisterToColorSources();
                nbCubesCreated++;
                if (nbCubesCreated >= nbCubesToCreateByFrame) {
                    nbCubesCreated = 0;
                    yield return new WaitForSeconds(1f / 60f);
                }
            }
        }
    }
}
