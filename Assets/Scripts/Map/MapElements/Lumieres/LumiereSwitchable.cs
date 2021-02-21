using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class LumiereSwitchable : Lumiere {

    public enum LumiereSwitchableState { ON, OFF };

    public GameObject lumiereOn;
    public GameObject lumiereOff;
    public LumiereSwitchableState startState = LumiereSwitchableState.ON;
    public VisualEffect vfxLightExplosion;

    protected LumiereSwitchableState state;
    protected GameObject currentLumiere;
    protected bool capturedInSwitchingToOn = false;

    protected override void Start () {
        base.Start();
        SetState(startState);
	}

    public void SetState(LumiereSwitchableState newState, bool shouldCheckCollisionWithPlayer = true) {
        state = newState;
        currentLumiere = (newState == LumiereSwitchableState.ON) ? lumiereOn : lumiereOff;
        lumiereOn.SetActive(state == LumiereSwitchableState.ON);
        lumiereOff.SetActive(state == LumiereSwitchableState.OFF);
        SetVfxAndLight();
        if (shouldCheckCollisionWithPlayer) {
            CheckNotCollideWithPlayer();
        }
    }

    protected void SetVfxAndLight() {
        vfx = currentLumiere.GetComponentInChildren<VisualEffect>();
        pointLight = currentLumiere.GetComponentInChildren<Light>().gameObject;
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

    public void TriggerLightExplosion() {
        vfxLightExplosion.SendEvent("Explode");
    }
}
