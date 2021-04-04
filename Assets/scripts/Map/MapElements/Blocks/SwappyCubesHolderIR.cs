﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwappyCubesHolderIR : SwappyCubesHolder {

    // We know that the cubes won't be destroyed, so we don't have to use the linky idea ! :)

    protected Vector3 impactPoint;

    public override void Initialize()
    {
        base.Initialize();
        impactPoint = ComputeImpactPoint();
    }

    protected Vector3 ComputeImpactPoint() {
        Cube mainCube = cubes[0];
        if (mainCube.IsLinky()) {
            Vector3 point = Vector3.zero;
            foreach (Cube cube in cubes) {
                point += cube.transform.position;
            }
            return point / cubes.Count;
        } else {
            return mainCube.transform.position;
        }
    }

    protected override void SetCubesVisibleState(bool visibleState) {
        cubes = cubes.FindAll(c => c != null);
        if(cubes.Count > 0) {
            foreach(Cube cube in cubes) {
                cube.StartCoroutine(cube.CSetEnableValueIn(visibleState, previsualisationDuration, impactPoint)); // Not linky
            }
        } else {
            Destroy(this);
        }
    }
}
