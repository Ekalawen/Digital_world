using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMapContainer1sur2 : GenerateCubesMapFunction {

    public bool should1sur2Floor = true;

    public override void Activate() {
        Vector3Int tailleMap = map.tailleMap;
        MapContainer mapContainer = new MapContainer(Vector3.zero, new Vector3(tailleMap.x, tailleMap.y, tailleMap.z) + Vector3.one);

        List<Cube> cubes = mapContainer.GetCubes();
        for (int i = 0; i < cubes.Count; i++) {
            Cube cube = cubes[i];
            bool shouldDelete = (cube.transform.position.x + cube.transform.position.y + cube.transform.position.z) % 2 == 0;
            bool isFloor = cube.transform.position.y == 0;
            if (shouldDelete && (!isFloor || (isFloor && should1sur2Floor)))
                map.DeleteCube(cube);
        }
    }
}
