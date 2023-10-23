using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.Localization.SmartFormat.Utilities;

public abstract class OverrideOnBlock : Override {

    protected InfiniteMap infiniteMap;

    protected override void InitializeSpecific() {
        infiniteMap = gm.GetInfiniteMap();
        gm.onInitilizationFinish.AddListener(OnAllFirstBlocks);
        infiniteMap.onCreateBlock.AddListener(OnBlock);
    }

    protected void OnAllFirstBlocks() {
        foreach (Block block in infiniteMap.GetAllBlocks()) {
            OnBlock(block);
        }
    }

    protected abstract void OnBlock(Block block);
}
