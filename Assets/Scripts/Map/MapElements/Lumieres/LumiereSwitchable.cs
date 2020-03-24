using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiereSwitchable : Lumiere {

    public enum LumiereSwitchableState { ON, OFF };

    public GameObject lumiereOn;
    public GameObject lumiereOff;
    public LumiereSwitchableState startState = LumiereSwitchableState.ON;

    protected LumiereSwitchableState state;
    protected bool capturedInSwitchingToOn = false;

    protected override void Start () {
        base.Start();
        SetState(startState);
	}

    public void SetState(LumiereSwitchableState newState) {
        state = newState;
        lumiereOn.SetActive(state == LumiereSwitchableState.ON);
        lumiereOff.SetActive(state == LumiereSwitchableState.OFF);
        CheckNotCollideWithPlayer();
    }

    protected void CheckNotCollideWithPlayer() {
        if (state == LumiereSwitchableState.ON) {
            Player player = gm.player;
            if (player != null) {
                float playerDistance = Vector3.Distance(player.transform.position, transform.position);
                float minDistanceOverlap = player.transform.localScale[0] + lumiereOn.transform.localScale[0];
                if (playerDistance <= minDistanceOverlap) {
                    capturedInSwitchingToOn = true;
                    Captured();
                    capturedInSwitchingToOn = false;
                }
            }
        }
    }

    protected override void NotifyConsoleLumiereCatpure() {
        int nbLumieres = gm.map.GetLumieres().Count;
        if (capturedInSwitchingToOn)
            nbLumieres++;
        gm.console.AttraperLumiere(nbLumieres);
    }

    public LumiereSwitchableState GetState() {
        return state;
    }

    protected override void OnTriggerEnter(Collider hit) {
        if(GetState() == LumiereSwitchableState.ON) {
            base.OnTriggerEnter(hit);
        }
    }
}
