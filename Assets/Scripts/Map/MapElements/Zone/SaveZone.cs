using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveZone : IZone {

    public Transform posSaved;

    protected override void Start() {
        base.Start();
    }

    protected override void OnEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            EventManagerTutoriel emt = (EventManagerTutoriel) gm.eventManager;
            if(emt != null) {
                emt.RegisterSavedZone(this);
            } else {
                Debug.LogError("On a pas le bon event manager !");
            }
        }
    }

    protected override void OnExit(Collider other) {
    }
}
