using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiereReward_MultiplyScore : LumiereReward
{
    public float multiplier = 2.0f;

    public override void Reward() {
        gm.GetInfiniteMap().scoreManager.MultiplyAllScores(multiplier);
    }
}
