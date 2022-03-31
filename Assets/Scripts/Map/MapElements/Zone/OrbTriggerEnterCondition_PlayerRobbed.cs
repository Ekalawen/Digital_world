using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class OrbTriggerEnterCondition_PlayerRobbed : OrbTriggerEnterCondition {

    public enum Mode { SHOULD_BE_ROBBED, SHOULD_NOT_BE_ROBBED };

    public Mode mode = Mode.SHOULD_BE_ROBBED;

    public override bool IsFullfilled() {
        switch(mode) {
            case Mode.SHOULD_BE_ROBBED:
                return SoulRobber.IsPlayerRobbed();
            case Mode.SHOULD_NOT_BE_ROBBED:
                return !SoulRobber.IsPlayerRobbed();
            default:
                return false;
        }
    }

    public override void OnTrigger() {
        if(SoulRobber.IsPlayerRobbed()) {
            SoulRobber oneSoulRobber = gm.ennemiManager.GetEnnemisOfType<SoulRobber>().First(); 
            if(oneSoulRobber == null) {
                return;
            }
            oneSoulRobber.StartUnrobb();
        }
    }
}
