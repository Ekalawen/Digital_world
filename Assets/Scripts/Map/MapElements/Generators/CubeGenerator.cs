using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CubeGenerator : IGenerator {

    [Header("CubeGenerator")]
    public Cube.CubeType cubeType = Cube.CubeType.NORMAL;
    public bool setLinky = false;
    public float dissolveEffectDuration = 0.1f;

    protected override void GenerateOneSpecific(Vector3 position) {
        Cube cube = map.GetCubeAt(position);
        if(choseType == ChoseType.GET_CUBES) {
            if(cube != null) {
                Cube newCube = map.SwapCubeType(cube, cubeType);
                SetupNewCube(newCube);
            }
        } else {
            if(cube == null) {
                Cube newCube = map.AddCube(position, cubeType);
                SetupNewCube(newCube);
            }
        }
    }

    protected void SetupNewCube(Cube newCube) {
        newCube.StartDissolveEffect(dissolveEffectDuration);
        if(newCube != null && setLinky) {
            newCube.SetLinkyValue(true);
        }
        StartCoroutine(SetOpaqueMaterialIn(newCube, dissolveEffectDuration));
    }

    protected IEnumerator SetOpaqueMaterialIn(Cube cube, float duree) {
        yield return new WaitForSeconds(duree);
        if(cube != null && !cube.IsDecomposing()) {
            cube.SetOpaqueMaterial();
        }
    }

    protected override bool IsValidPosition(Vector3 position) {
        Cube cube = map.GetCubeAt(position);
        return (choseType == ChoseType.GET_CUBES && cube != null && cube.type != cubeType)
            || (choseType == ChoseType.GET_EMPTY && cube == null);
    }
}
