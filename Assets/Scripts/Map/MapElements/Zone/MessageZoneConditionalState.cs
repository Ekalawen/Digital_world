using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageZoneConditionalState : MessageZone {

    public Player.EtatPersonnage triggerState = Player.EtatPersonnage.AU_MUR;

    public override void DisplayMessages() {
        if (gm.player.GetEtat() == triggerState) {
            base.DisplayMessages();
        }
    }
}
