using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class CubeGenerator : IGenerator {

    public enum LinkyType { TRUE, FALSE, KEEP, INVERSE };

    [Header("CubeGenerator")]
    public Cube.CubeType cubeType = Cube.CubeType.NORMAL;
    public LinkyType setLinky = LinkyType.FALSE;
    public float dissolveEffectDuration = 0.1f;
    public bool decomposeCubesOnDestruction = false;

    protected List<Cube> cubesCreated = new List<Cube>();

    public override void Initialize() {
        base.Initialize();
        Assert.IsFalse(choseType == ChoseType.GET_EMPTY && setLinky == LinkyType.KEEP);
        Assert.IsFalse(choseType == ChoseType.GET_EMPTY && setLinky == LinkyType.INVERSE);
    }

    protected override void GenerateOneSpecific(Vector3 position) {
        Cube cube = map.GetCubeAt(position);
        if(choseType == ChoseType.GET_CUBES) {
            if(cube != null) {
                Cube newCube = map.SwapCubeType(cube, cubeType);
                SetupNewCube(newCube, GetNewLinkyState(cube));
            }
        } else {
            if(cube == null) {
                Cube newCube = map.AddCube(position, cubeType);
                SetupNewCube(newCube, GetNewLinkyState(cube));
            }
        }
    }

    protected bool GetNewLinkyState(Cube oldCube) {
        if(oldCube == null) {
            return setLinky == LinkyType.TRUE;
        }
        switch (setLinky) {
            case LinkyType.TRUE: return true;
            case LinkyType.FALSE: return false;
            case LinkyType.KEEP: return oldCube.IsLinky();
            case LinkyType.INVERSE: return !oldCube.IsLinky();
            default: return false;
        }
    }

    protected void SetupNewCube(Cube newCube, bool linkyState) {
        cubesCreated.Add(newCube);
        if (newCube != null) {
            newCube.StartDissolveEffect(dissolveEffectDuration);
        }
        if(newCube != null && linkyState) {
            newCube.SetLinky();
        }
        if (newCube != null) {
            newCube.SetDissolveTimeBeCareful(dissolveEffectDuration);
        }
    }

    protected override bool IsValidPosition(Vector3 position) {
        Cube cube = map.GetCubeAt(position);
        return (choseType == ChoseType.GET_CUBES && cube != null && cube.type != cubeType)
            || (choseType == ChoseType.GET_EMPTY && cube == null);
    }

    public override void DestroyIn(float duree) {
        base.DestroyIn(duree);
        if (decomposeCubesOnDestruction) {
            foreach (Cube cube in cubesCreated) {
                if (cube != null) {
                    cube.Decompose(duree);
                }
            }
        }
    }
}
