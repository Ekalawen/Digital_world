using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using static InfiniteMap;

public class OverrideFastDisconnection : Override {

    public int nbBlocksStartDisconnection = 1;
    public DifficultyMode difficultyMode = DifficultyMode.PROGRESSIVE;
    [ConditionalHide("difficultyMode", DifficultyMode.CONSTANT)]
    public float timeDifficultyCoefficient = 1.0f;
    [ConditionalHide("difficultyMode", DifficultyMode.PROGRESSIVE)]
    public float timeDifficultyOffset = 1.85f;
    [ConditionalHide("difficultyMode", DifficultyMode.PROGRESSIVE)]
    public float timeDifficultyProgression = 6f;

    protected override void InitializeSpecific() {
        InfiniteMap infiniteMap = gm.GetInfiniteMap();
        infiniteMap.nbBlocksStartCubeDestruction = nbBlocksStartDisconnection;
        infiniteMap.difficultyMode = difficultyMode;
        infiniteMap.timeDifficultyCoefficient = timeDifficultyCoefficient;
        infiniteMap.timeDifficultyOffset = timeDifficultyOffset;
        infiniteMap.timeDifficultyProgression = timeDifficultyProgression;
    }
}
