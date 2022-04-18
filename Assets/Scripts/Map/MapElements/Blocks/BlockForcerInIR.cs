using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using EZCameraShake;
using UnityEngine.Events;

[Serializable]
public class BlockWithIndex {
    public int index;
    public GameObject blockPrefab;
}

public class BlockForcerInIR : MonoBehaviour {

    public List<BlockWithIndex> blocksWithIndexes;

    public bool ShoulForceBlockAt(int blockIndex) {
        return blocksWithIndexes.Select(b => b.index).Contains(blockIndex);
    }

    public GameObject GetForcedBlockAt(int blockIndex) {
        return blocksWithIndexes.Find(b => b.index == blockIndex)?.blockPrefab;
    }
}
