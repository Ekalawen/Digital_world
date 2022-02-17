using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_DodgeJumpWithShift : Achievement {

    protected override void InitializeSpecific() {
        gm.eventManager.onJumpSuccess.AddListener(OnJump);
    }

    public void OnJump() {
        if(InputManager.Instance.GetShift()) {
            Unlock();
        }
    }
}
