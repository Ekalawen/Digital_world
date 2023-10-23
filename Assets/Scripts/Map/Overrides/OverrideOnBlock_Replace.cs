using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class OverrideOnBlock_Replace : OverrideOnBlock {

    public ReplaceByCubeType replaceByCubeType;

    protected override void InitializeSpecific() {
        base.InitializeSpecific();
        replaceByCubeType.getCubesHelper.howToGetCubes = GetCubesHelper.HowToGetCubes.OF_BLOCK;
        replaceByCubeType.Initialize();
    }

    protected override void OnBlock(Block block) {
        replaceByCubeType.getCubesHelper.block = block;
        replaceByCubeType.Activate();
    }
}
