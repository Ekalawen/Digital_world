using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class OverrideChrismasTree : OverrideOnBlock {

    public int nbDataPerBlock = 3;

    protected ScoreManager scoreManager;

    protected override void InitializeSpecific() {
        base.InitializeSpecific();
        scoreManager = infiniteMap.GetScoreManager();
        scoreManager.dataProbability = 1.0f;
        scoreManager.nbDataPerBlock = nbDataPerBlock;
    }

    protected override void OnBlock(Block block) {
        int nbLumieresToChose = block.GetNbLumieresToChose();
        if(nbLumieresToChose < nbDataPerBlock) {
            block.AddLumieres(nbDataPerBlock - nbLumieresToChose);
        }
    }
}
