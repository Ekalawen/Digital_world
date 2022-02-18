using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class Achievement_FinishLevelIR : Achievement_FinishLevel {

    protected InfiniteMap map;

    protected override void InitializeSpecific() {
        map = gm.GetInfiniteMap();
        gm.eventManager.onGameOver.AddListener(UnlockSpecific);
    }
}
