using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMapContainer : GenerateCubesMapFunction {
    public override void Activate() {
        Vector3Int tailleMap = map.tailleMap;
        MapContainer mapContainer = new MapContainer(Vector3.zero, new Vector3(tailleMap.x, tailleMap.y, tailleMap.z) + Vector3.one);
    }
}
