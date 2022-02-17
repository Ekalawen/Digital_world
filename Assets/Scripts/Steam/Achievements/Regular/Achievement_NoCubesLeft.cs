using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_NoCubesLeft : Achievement {

    protected override void InitializeSpecific() {
        gm.eventManager.onWinGame.AddListener(OnWinGame);
    }

    public void OnWinGame() {
        if(gm.map.GetAllCubes().Count == 0) {
            Unlock();
        }
    }
}
