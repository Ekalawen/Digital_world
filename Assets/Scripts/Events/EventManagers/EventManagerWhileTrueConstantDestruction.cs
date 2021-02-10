using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventManagerWhileTrueConstantDestruction : EventManagerWhileTrue {

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
            Cube precedantCube = map.GetCubeAt(pos);
            if (precedantCube == null || precedantCube.IsDecomposing()) {
                if(precedantCube != null) {
                    map.DeleteCube(precedantCube);
                }
                Cube newCube = map.AddCube(pos, Cube.CubeType.NORMAL);
                if (newCube != null) {
                    newCube.ShouldRegisterToColorSources();
                    nbCubesCreated++;
                    if (nbCubesCreated >= nbCubesToCreateByFrame) {
                        nbCubesCreated = 0;
                        yield return new WaitForSeconds(1f / 60f);
                    }
                }
            }
        }
    }
}
