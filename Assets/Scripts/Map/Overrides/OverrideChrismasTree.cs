using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class OverrideChrismasTree : OverrideOnBlock {

    public int nbStandardDataPerBlock = 3;
    public int nbRandomDataPerBlock = 0;

    protected ScoreManager scoreManager;

    protected override void InitializeSpecific() {
        infiniteMap = gm.GetInfiniteMap();
        scoreManager = infiniteMap.GetScoreManager();
        scoreManager.dataProbability = 1.0f;
        scoreManager.nbDataPerBlock = nbStandardDataPerBlock;

        OnAllFirstBlocks();
        infiniteMap.onCreateBlock.AddListener(OnBlock);
    }

    protected override void OnBlock(Block block) {
        block.SetNbDataToChose(nbStandardDataPerBlock);
        block.SetNbRandomToSpawn(nbRandomDataPerBlock);
        block.SetShouldDestroyLumieresOnLeave();
    }
}
