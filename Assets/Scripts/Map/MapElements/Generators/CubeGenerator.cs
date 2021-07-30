using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CubeGenerator : IGenerator {

    [Header("CubeGenerator")]
    public Cube.CubeType cubeType = Cube.CubeType.NORMAL;
    public bool setLinky = false;

    protected override void GenerateOneSpecific(Vector3 position) {
        Cube cube = map.GetCubeAt(position);
        if(choseType == ChoseType.GET_CUBES) {
            if(cube != null) {
                Cube newCube = map.SwapCubeType(cube, cubeType);
                if(newCube != null && setLinky) {
                    newCube.SetLinkyValue(true);
                }
            }
        } else {
            if(cube == null) {
                Cube newCube = map.AddCube(position, cubeType);
                if(newCube != null && setLinky) {
                    newCube.SetLinkyValue(true);
                }
            }
        }
    }
}
