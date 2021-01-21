using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManagerTutoriel : EventManager {

    public Vector3 posFinalLight;
    public float teleportInSafeZoneTreshold = -10.0f;

    protected SaveZone savedZone = null;

    protected override void StartEndGame() {
        isEndGameStarted = true;

        // On va juste faire poper la final light !
        gm.map.CreateLumiere(posFinalLight, Lumiere.LumiereType.FINAL);
    }

    public override void Update() {
        base.Update();
        if (gm.player.transform.position.y < teleportInSafeZoneTreshold) {
            GoBackToPreviousSaveZone();
            gm.console.SavedFromFalling();
        }
    }

    public void GoBackToPreviousSaveZone() {
        gm.player.transform.position = savedZone.posSaved.position;
        gm.gravityManager.LookAt(savedZone.posSaved.forward);
    }

    public void RegisterSavedZone(SaveZone savedZone) {
        this.savedZone = savedZone;
    }
}
