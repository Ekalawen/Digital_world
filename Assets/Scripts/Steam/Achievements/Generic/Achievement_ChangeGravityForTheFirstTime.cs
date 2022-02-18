using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class Achievement_ChangeGravityForTheFirstTime : Achievement {

    protected override void InitializeSpecific() {
        gm.gravityManager.onGravityChange.AddListener(OnGravityChange);
    }

    public void OnGravityChange() {
        Unlock();
    }
}
