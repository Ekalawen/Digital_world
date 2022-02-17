using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_HackFirewallOutsideOfMap : Achievement {

    protected override void InitializeSpecific() {
        gm.itemManager.onOrbTriggerHacked.AddListener(UnlockIfOutsideOfMap);
    }

    public void UnlockIfOutsideOfMap(OrbTrigger orbTrigger) {
        if (!gm.map.IsInRegularMap(gm.player.transform.position)) {
            Unlock();
        }
    }
}
