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

    protected override void Start () {
        base.Start();
        SetState(startState);
	}

    public void SetState(LumiereSwitchableState newState) {
        state = newState;
        lumiereOn.SetActive(state == LumiereSwitchableState.ON);
        lumiereOff.SetActive(state == LumiereSwitchableState.OFF);
    }

    public LumiereSwitchableState GetState() {
        return state;
    }

    protected override void OnTriggerEnter(Collider hit) {
        if(GetState() == LumiereSwitchableState.ON) {
            base.OnTriggerEnter(hit);
        }
    }

    protected override void OnTriggerEnterSpecific() {
        List<Lumiere> lumieres = gm.map.GetLumieres();
        foreach(Lumiere lumiere in lumieres) {
            if(lumiere != this) {
                LumiereSwitchable ls = (LumiereSwitchable)lumiere;
                ls.SetState(LumiereSwitchableState.OFF);
            }
        }
        StartCoroutine(Reverse());
    }

    protected IEnumerator Reverse() {
        yield return new WaitForSeconds(2);
        List<Lumiere> lumieres = gm.map.GetLumieres();
        foreach(Lumiere lumiere in lumieres) {
            if(lumiere != this) {
                LumiereSwitchable ls = (LumiereSwitchable)lumiere;
                ls.SetState(LumiereSwitchableState.ON);
            }
        }
    }
}
