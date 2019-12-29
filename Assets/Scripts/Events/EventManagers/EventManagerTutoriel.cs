using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManagerTutoriel : EventManager {

    public Vector3 posFinalLight;

    protected override void StartEndGame() {
        isEndGameStarted = true;
        //gm.player.FreezeLocalisation();
        gm.console.StartEndGame();

        // On va juste faire poper la final light !
        gm.map.CreateLumiere(posFinalLight, Lumiere.LumiereType.FINAL);
    }
}
