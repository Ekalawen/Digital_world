using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateFloor : GenerateCubesMapFunction {
    public override void Activate() {
        Mur floor = new Mur(Vector3.zero, Vector3.right, map.tailleMap.x + 1, Vector3.forward, map.tailleMap.z + 1);
    }
}
