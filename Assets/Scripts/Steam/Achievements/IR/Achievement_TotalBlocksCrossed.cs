using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_TotalBlocksCrossed : Achievement {

    [Header("Parameters")]
    public int treshold = 100;

    protected override void InitializeSpecific() {
        gm.map.GetComponent<InfiniteMap>().onBlocksCrossed.AddListener(UnlockIfBetterTreshold);
    }

    public void UnlockIfBetterTreshold(int nbBlocksJustCrossed) {
        int totalNbBlocksCrossed = Block.GetTotalBlocksCrossed();
        if(totalNbBlocksCrossed >= treshold) {
            Unlock();
        }
    }
}
