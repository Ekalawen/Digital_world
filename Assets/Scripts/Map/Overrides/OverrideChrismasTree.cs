using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class OverrideChrismasTree : OverrideOnBlock {

    public int nbStandardDataPerBlock = 3;
    public float randomDataProbability = 0.02f;

    protected ScoreManager scoreManager;

    protected override void InitializeSpecific() {
        infiniteMap = gm.GetInfiniteMap();
        scoreManager = infiniteMap.GetScoreManager();
        scoreManager.SetDataProbabilities(Enumerable.Repeat(1.0f, nbStandardDataPerBlock).ToList());

        OnAllFirstBlocks();
        infiniteMap.onBeforeInitializeBlock.AddListener(OnBlock);
    }

    protected override void OnBlock(Block block) {
        block.SetNbDataToChose(nbStandardDataPerBlock);
        block.SetRandomDataProbability(randomDataProbability);
        block.SetShouldDestroyLumieresOnLeave();
    }
}
