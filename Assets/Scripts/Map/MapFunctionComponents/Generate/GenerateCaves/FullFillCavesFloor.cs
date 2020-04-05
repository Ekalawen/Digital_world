using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullFillCavesFloor : GenerateCubesMapFunction {

    public bool exceptOne = false;

    public override void Activate() {
        List<Cave> caves = map.GetMapElementsOfType<Cave>();
        foreach(Cave cave in caves) {
            cave.FulfillFloor(exceptOne: exceptOne);
        }
    }
}
