using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiereReward_IncreaseIncrement1 : LumiereReward
{
    public override void Reward() {
        gm.GetInfiniteMap().scoreManager.OnCatchData();
    }
}
