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
        gm.map.GetComponent<InfiniteMap>().onBlockCrossed.AddListener(UnlockIfBetterTreshold);
    }

    public void UnlockIfBetterTreshold() {
        int nbBlocksCrossed = Block.GetTotalBlocksCrossed();
        if(nbBlocksCrossed >= treshold) {
            Unlock();
        }
    }
}
