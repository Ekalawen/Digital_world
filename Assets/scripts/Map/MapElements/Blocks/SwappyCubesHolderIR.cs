using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwappyCubesHolderIR : SwappyCubesHolder {

    // We know that the cubes won't be destroyed, so we don't have to use the linky idea ! :)

    protected Vector3 impactPoint;

    public override void Initialize(SwappyCubesHolderManager manager) {
        base.Initialize(manager);
        impactPoint = ComputeImpactPoint();
    }

    protected Vector3 ComputeImpactPoint() {
        Cube mainCube = cubes[0];
        if (manager.setLinky) {
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
                StartCoroutine(cube.CSetEnableValueIn(visibleState, previsualisationDuration, impactPoint)); // Not linky
            }
        //} else {
        //    Destroy(this);
        }
    }
}
