using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleCubesZone : IZone {

    public float dissolveTime = 0.5f;

    protected List<Cube> cubes;
    protected bool haveCubesAppeared = false;

    protected override void Start() {
        base.Start();
        cubes = GatherCubes();
        MakeAllCubesDisappear();
    }

    protected List<Cube> GatherCubes() {
        List<Cube> cubes = new List<Cube>();
        foreach (Transform child in transform) {
            Cube cube = child.gameObject.GetComponent<Cube>();
            if (cube != null)
                cubes.Add(cube);
        }
        return cubes;
    }


    protected override void OnEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            MakeAllCubesAppear();
        }
    }

    public void MakeAllCubesAppear() {
        if (!haveCubesAppeared) {
            haveCubesAppeared = true;
            foreach (Cube cube in cubes) {
                cube.gameObject.SetActive(true);
                cube.StartDissolveEffect(dissolveTime, playerProximityCoef: 0.0f);
            }
        }
    }

    public void MakeAllCubesDisappear() {
        foreach(Cube cube in cubes) {
            cube.gameObject.SetActive(false);
        }
    }

    protected override void OnExit(Collider other) {
    }
}
