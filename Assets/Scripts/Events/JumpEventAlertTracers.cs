﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpEventAlertTracers : JumpEvent {

    protected override void Stun() {
        base.Stun();

        // On alerte tous les tracers !
        foreach(Ennemi ennemi in gm.ennemiManager.ennemis) {
            Tracer tracer = ennemi.GetComponent<Tracer>();
            if(tracer != null) {
                TracerController tracerController = tracer.GetComponent<TracerController>();
                if(tracerController != null)
                    tracerController.DetectPlayer();
            }
        }

        gm.console.AlerterTracers();
    }

}
