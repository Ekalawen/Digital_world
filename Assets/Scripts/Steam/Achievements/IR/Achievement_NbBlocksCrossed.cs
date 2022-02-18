using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_NbBlocksCrossed : Achievement {

    [Header("Parameters")]
    public int treshold = 100;

    protected InfiniteMap map;

    protected override void InitializeSpecific() {
        map = gm.map.GetComponent<InfiniteMap>();
        map.onBlocksCrossed.AddListener(UnlockIfBetterTreshold);
    }

    public void UnlockIfBetterTreshold(int nbBlocksCrossed) {
        if(map.GetNonStartNbBlocksRun() >= treshold) {
            Unlock();
        }
    }
}
