﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateRandomFilling : GenerateCubesMapFunction {

    public bool useRandomFilling = false;
    public float minDistanceRandomFilling = 0f;
    public float proportionRandomFilling = 0.0f;
    public int sizeCubeRandomFilling = 0; // Ca peut être intéressant d'augmenter cette taille ! :)

    public override void Activate() {
        GenerateRandomFillingCubes();
    }

    protected List<FullBlock> GenerateRandomFillingCubes() {
        List<FullBlock> fullBlocks = new List<FullBlock>();
        List<Vector3> farAwayPos = GetFarAwayPositions();
        List<Vector3> selectedPos = GaussianGenerator.SelectSomeProportionOfNaiveMethod<Vector3>(farAwayPos, proportionRandomFilling);
        foreach(Vector3 pos in selectedPos) {
            Vector3 finalPos = pos - Vector3.one * (int)Mathf.Floor(sizeCubeRandomFilling / 2.0f);
            FullBlock fb = new FullBlock(finalPos, Vector3Int.one * sizeCubeRandomFilling);
            fullBlocks.Add(fb);
        }
        return fullBlocks;
    }

    protected List<Vector3> GetFarAwayPositions() {
        List<Vector3> res = new List<Vector3>();
        foreach(Vector3 pos in map.GetAllEmptyPositions()) {
            List<Cube> nearCubes = map.GetCubesInSphere(pos, minDistanceRandomFilling);
            if (nearCubes.Count == 0)
                res.Add(pos);
        }
        return res;
    }

    public Vector3 GetFarFromEnsemble(List<Cube> farCubes, float minDistance) {
        while(true) {
            Vector3 pos = map.GetFreeRoundedLocation();
            List<float> distances = new List<float>();
            foreach (Cube cube in farCubes)
                distances.Add(Vector3.Distance(pos, cube.transform.position));
            if (distances.Min() >= minDistance)
                return pos;
        }
    }
}
