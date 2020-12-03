using System.Collections;
using System.Collections.Generic;
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

    protected override Lumiere CreateFinalLight() {
        int kmax = 1000;
        for(int k = 0; k < kmax; k++) {
            Vector3 posLumiere = map.GetFarRoundedLocation(gm.player.transform.position);
            if(!map.IsInInsidedVerticalEdges(posLumiere)) {
                Lumiere finalLight = map.CreateLumiere(posLumiere, Lumiere.LumiereType.FINAL);
                return finalLight;
            }
        }
        Debug.LogError("Erreur dans CreateFinalLight(). Toutes les lumières sont dans les coins.");
        return null;
    }
}
