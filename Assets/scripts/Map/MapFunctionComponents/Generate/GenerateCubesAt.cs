using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateCubesAt : GenerateCubesMapFunction {

    public GetEmptyPositionsHelper getPositionsHelper;

    public override void Activate() {
        Transform folder = new GameObject("GenerateCubesAt").transform;
        folder.transform.SetParent(map.cubesFolder.transform);
        foreach(Vector3 pos in getPositionsHelper.Get()) {
            map.AddCube(pos, cubeType, Quaternion.identity, folder);
        }
    }
}
