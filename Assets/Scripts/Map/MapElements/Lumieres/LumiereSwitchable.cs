using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class LumiereSwitchable : Lumiere {

    public enum LumiereSwitchableState { ON, OFF };

    [Header("Switchable")]
    public LumiereSwitchableChild lumiereOnChild;
    public LumiereSwitchableChild lumiereOffChild;
    public LumiereSwitchableState startState = LumiereSwitchableState.ON;
    public VisualEffect vfxLightExplosion;

    protected LumiereSwitchableState state;
    protected LumiereSwitchableChild currentChild;
    protected bool capturedInSwitchingToOn = false;

    protected override void Start () {
        gm = GameManager.Instance;
        SetState(startState);
        base.Start();
	}

    public void SetState(LumiereSwitchableState newState, bool shouldCheckCollisionWithPlayer = true) {
        state = newState;
        currentChild = (newState == LumiereSwitchableState.ON) ? lumiereOnChild : lumiereOffChild;
        lumiereOnChild.gameObject.SetActive(state == LumiereSwitchableState.ON);
        lumiereOffChild.gameObject.SetActive(state == LumiereSwitchableState.OFF);
        SetVue();
        if (shouldCheckCollisionWithPlayer) {
            CheckNotCollideWithPlayer();
        }
    }

    protected void SetVue() {
        lumiereHighVfx = currentChild.lumiereHighVfx;
        lumiereLow = currentChild.lumiereLow;
        lumiereTrails = currentChild.lumiereTrails;
        pointLight = currentChild.pointLight;
    }

    protected void CheckNotCollideWithPlayer() {
        if (state == LumiereSwitchableState.ON) {
            if (gm == null) {
                gm = GameManager.Instance;
            }
            Player player = gm.player;
            if (player != null) {
                float playerDistance = Vector3.Distance(player.transform.position, transform.position);
                float minDistanceOverlap = player.transform.localScale[0] + lumiereOnChild.transform.localScale[0];
                if (playerDistance <= minDistanceOverlap) {
                    capturedInSwitchingToOn = true;
                    Captured();
                    capturedInSwitchingToOn = false;
                }
            }
        }
    }

    protected override void NotifyConsoleLumiereCatpure() {
        int nbLumieres = gm.map.GetLumieres().Count + gm.ennemiManager.GetNbDataSondeTriggers();
        if (capturedInSwitchingToOn)
            nbLumieres++;
        gm.console.AttraperLumiere(nbLumieres);
        gm.console.UpdateLastLumiereAttrapee();
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

    public override void SetLumiereQuality(LumiereQuality quality) {
        base.SetLumiereQuality(quality);
        lumiereOnChild.SetLumiereQuality(quality);
        lumiereOffChild.SetLumiereQuality(quality);
    }
}
