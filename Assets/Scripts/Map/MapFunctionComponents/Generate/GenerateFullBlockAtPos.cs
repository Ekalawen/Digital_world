using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateFullBlockAtPos : GenerateCubesMapFunction {

    public Vector3Int size;
    public Vector3Int position;
    public bool makeSpaceArround = false;

    public override void Activate() {
        FullBlock fullBlock = new FullBlock(position, size, makeSpaceArround: makeSpaceArround);
    }
}
