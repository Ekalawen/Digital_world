using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BlockWeight {
    public GameObject block = null;
    public float weight = 12.0f;
}

public class BlockList : MonoBehaviour{
    public List<BlockWeight> blocks;

    public float GetTotalWeight() {
        return blocks.Sum(bw => bw.weight);
    }
}
