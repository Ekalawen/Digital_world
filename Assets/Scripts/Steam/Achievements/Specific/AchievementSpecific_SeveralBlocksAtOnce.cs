using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_SeveralBlocksAtOnce : Achievement_FinishLevelIR {

    public int nbBlocksAtOnce = 5;

    protected override void InitializeSpecific() {
        map = gm.GetInfiniteMap();
        map.onBlocksCrossed.AddListener(OnBlocksCrossed);
    }

    protected void OnBlocksCrossed(int nbBlocksCrossed) {
        if(nbBlocksCrossed >= nbBlocksAtOnce) {
            Unlock();
        }
    }
}
