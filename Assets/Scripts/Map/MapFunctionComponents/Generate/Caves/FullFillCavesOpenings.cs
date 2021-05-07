using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullFillCavesOpenings : GenerateCubesMapFunction {

    public override void Activate() {
        List<Cave> caves = map.GetMapElementsOfType<Cave>();
        foreach(Cave cave in caves) {
            cave.FullfillOpenings();
        }
    }
}
