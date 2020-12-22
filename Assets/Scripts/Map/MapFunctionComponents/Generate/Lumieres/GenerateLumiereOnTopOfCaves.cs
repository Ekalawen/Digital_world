using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLumiereOnTopOfCaves : GenerateLumieresMapFunction {

    public override void Activate() {
        AddLumiereOnTopOfCaves();
    }

    protected void AddLumiereOnTopOfCaves() {
        List<Cave> caves = map.GetMapElementsOfType<Cave>();
        List<Vector3> reachableArea = map.GetReachableArea();
        foreach(Cave cave in caves) {
            Vector3 onTop = cave.GetOnTop();
            map.CreateLumiere(onTop, lumiereType);
            //map.LinkPositionToReachableArea(onTop, reachableArea);
        }
    }
}
