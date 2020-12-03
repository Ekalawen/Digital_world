using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCubesInVerticalEdges : GenerateCubesMapFunction {

    public int edgesOffset = 1;

    public override void Activate() {
        List<Cube> cubes = map.GetAllCubes();
        foreach(Cube cube in cubes) {
            Vector3 pos = cube.transform.position;
            if(IsInVerticalEdges(pos, edgesOffset)) {
                map.DeleteCube(cube);
                Cube newCube = map.AddCube(pos, cubeType);
                newCube.shouldRegisterToColorSources = false;
            }
        }
    }

    public bool IsInVerticalEdges(Vector3 pos, int offset = 1) {
        pos = MathTools.Round(pos);
        int nbEdges = 0;
        nbEdges += (pos.x <= offset || pos.x >= map.tailleMap.x - offset) ? 1 : 0;
        nbEdges += (pos.z <= offset || pos.z >= map.tailleMap.z - offset) ? 1 : 0;
        return pos.y != 0 && pos.y != map.tailleMap.y && nbEdges == 2;
    }
}
