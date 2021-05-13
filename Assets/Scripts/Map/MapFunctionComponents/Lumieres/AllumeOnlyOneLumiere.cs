using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AllumeOnlyOneLumiere : MapFunctionComponent {

    public override void Activate() {
        List<Lumiere> lumieres = map.GetLumieres();
        foreach (Lumiere lumiere in lumieres) {
            LumiereSwitchable ls = (LumiereSwitchable)lumiere;
            ls.SetState(LumiereSwitchable.LumiereSwitchableState.OFF);
            ls.startState = LumiereSwitchable.LumiereSwitchableState.OFF;
        }
        if (lumieres.Count <= 0)
            return;
        Lumiere chosenOne = lumieres[UnityEngine.Random.Range(0, lumieres.Count)];
        LumiereSwitchable chosenOneSwitchable = (LumiereSwitchable)chosenOne;
        chosenOneSwitchable.SetState(LumiereSwitchable.LumiereSwitchableState.ON, shouldCheckCollisionWithPlayer: false);
        chosenOneSwitchable.startState = LumiereSwitchable.LumiereSwitchableState.ON;
    }
}
