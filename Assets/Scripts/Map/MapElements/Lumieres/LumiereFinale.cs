using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiereFinale : Lumiere {
    protected override void OnTriggerEnter(Collider hit) {
        base.OnTriggerEnter(hit);
        if (hit.gameObject.name == "Joueur") {
            Debug.Log("WIIIIIIIIIIINNNNNNNNNNNN !!!!!!!!");
            gm.eventManager.WinGame();
        }
    }
}
