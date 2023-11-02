using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class OverrideProgressiveExplosions : Override {

    public float probability = 0.2f;
    public float range = 5.0f;

    protected Player player;
    protected List<Cube> safeCubes;

    protected override void InitializeSpecific() {
        player = gm.player;
        safeCubes = new List<Cube>();
    }

    protected void Update() {
        List<Cube> cubesInRange = map.GetCubesInSphere(player.transform.position, range);
        cubesInRange = cubesInRange.FindAll(c => !safeCubes.Contains(c));
        foreach (Cube cube in cubesInRange) {
            if (UnityEngine.Random.value < probability && CanExplode(cube)) {
                cube.Explode();
            } else {
                safeCubes.Add(cube);
            }
        }
    }

    protected bool CanExplode(Cube cube) {
        return map.GetCubesAtLessThanDistanceLInfini(cube.transform.position, 1).Count() > 1;
    }
}
