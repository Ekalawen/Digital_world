﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManagerTutoriel : EventManager {

    public Vector3 posFinalLight;
    public float teleportInSafeZoneTreshold = -10.0f;

    protected SaveZone savedZone = null;

    protected override void StartEndGame() {
        isEndGameStarted = true;
        //gm.player.FreezeLocalisation();
        gm.console.StartEndGame();

        // On va juste faire poper la final light !
        gm.map.CreateLumiere(posFinalLight, Lumiere.LumiereType.FINAL);
    }

    protected void Update() {
        if (gm.player.transform.position.y < teleportInSafeZoneTreshold) {
            gm.player.transform.position = savedZone.posSaved.position;
            gm.gravityManager.LookAt(savedZone.posSaved.forward);
            gm.console.SavedFromFalling();
        }
    }

    public void RegisterSavedZone(SaveZone savedZone) {
        Debug.Log("Zone registered !");
        this.savedZone = savedZone;
    }
}
