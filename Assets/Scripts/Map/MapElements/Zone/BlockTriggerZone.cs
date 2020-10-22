using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTriggerZone : IZone {

    public Block block;

    protected InfiniteMap map; 

    protected override void Start() {
        base.Start();
        map = (InfiniteMap)gm.map;
    }

    protected override void OnEnter(Collider other) {
        if (block != null && map != null)
            map.OnEnterBlock(block);
    }

    protected override void OnExit(Collider other) {
        if(block != null && map != null)
            map.OnExitBlock(block);
    }
}
