using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_AllNanoboosts : Achievement_FinishLevelIR {

    public int blocksTreshold = 100;

    protected override void InitializeSpecific() {
        base.InitializeSpecific();
        map.onBlocksCrossed.AddListener(OnBlocksCrossed);
    }

    public void OnBlocksCrossed(int nbBlocksCrossed) {
        UnlockSpecific();
    }

    public override void UnlockSpecific() {
        if (map.GetNonStartNbBlocksRun() >= blocksTreshold) {
            // Because x is the forward of the InfiniteMap ! FORWARD = Vector3.right;
            if (gm.itemManager.GetItemsOfType(Item.Type.NANOBOOST).All(n => n.transform.position.x >= gm.player.transform.position.x)) {
                Unlock();
            } else {
                map.onBlocksCrossed.RemoveListener(OnBlocksCrossed); // Check one time
            }
        }
    }
}
