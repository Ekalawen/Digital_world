using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_DieCloseToExit : Achievement {

    [Header("Parameters")]
    public float exitDistance = 2.0f;

    protected override void InitializeSpecific() {
        gm.eventManager.onLoseGame.AddListener(OnLoseGame);
    }

    public void OnLoseGame(EventManager.DeathReason reason) {
        List<Lumiere> dataFinales = gm.map.GetLumieresFinales();
        if(dataFinales.Count == 1 && Vector3.Distance(gm.player.transform.position, dataFinales[0].transform.position) <= exitDistance) {
            Unlock();
        }
    }
}
