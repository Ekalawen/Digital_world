using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SondeControllerSecondBoss : SondeController {

    protected SecondBoss secondBoss;

    public override void Start() {
        base.Start();
        secondBoss = GetComponent<SecondBoss>();
    }

    protected override void DetectPlayer() {
        base.DetectPlayer();
        secondBoss.OnDetectPlayer();
    }
}
