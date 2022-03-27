﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateNumberedRandomFilling : GenerateCubesMapFunction {

    public int nbRandomCubes = 30;
    public float minDistanceRandomFilling = 1f;
    public int sizeCubeRandomFilling = 1; // Ca peut être intéressant d'augmenter cette taille ! :)
    public bool shouldUseMinDistanceBetweenCubesOfRandomFilling = false;
    [ConditionalHide("shouldUseMinDistanceBetweenCubesOfRandomFilling")]
    public int distanceMinBetweenCubesOfRandomFilling = 0;
    public bool shouldNotVerticallyStack = false;

    public override void Activate() {
        if(shouldNotVerticallyStack) {
            GenerateNumberedRandomFillingWithoutStacking();
        } else if (shouldUseMinDistanceBetweenCubesOfRandomFilling) {
            GenerateNumberedRandomFillingCubesWithMinDistance();
        } else {
            GenerateNumberedRandomFillingCubes();
        }
    }

    // This is the default
    protected List<FullBlock> GenerateNumberedRandomFillingCubes() {
        List<FullBlock> fullBlocks = new List<FullBlock>();
        List<Vector3> farAwayPos = map.GetFarAwayFromAllCubesPositions(minDistanceRandomFilling);
        List<Vector3> selectedPos = GaussianGenerator.SelecteSomeNumberOf(farAwayPos, nbRandomCubes);
        foreach (Vector3 pos in selectedPos) {
            fullBlocks.Add(CreateFullBlockAtPos(pos));
        }
        return fullBlocks;
    }

    protected List<FullBlock> GenerateNumberedRandomFillingWithoutStacking() {
        List<FullBlock> fullBlocks = new List<FullBlock>();
        List<Vector3> farAwayPos = map.GetFarAwayFromAllCubesPositions(minDistanceRandomFilling);
        for(int i = 0; i < nbRandomCubes && farAwayPos.Count > 0;) {
            Vector3 selectedPos = MathTools.ChoseOne(farAwayPos);
            if(!map.IsCubeAt(selectedPos + Vector3.up) && !map.IsCubeAt(selectedPos + Vector3.down)) {
                fullBlocks.Add(CreateFullBlockAtPos(selectedPos));
                i++;
            }
            farAwayPos.Remove(selectedPos);
        }
        return fullBlocks;
    }

    protected List<FullBlock> GenerateNumberedRandomFillingCubesWithMinDistance() {
        List<FullBlock> fullBlocks = new List<FullBlock>();
        List<Vector3> farAwayPos = map.GetFarAwayFromAllCubesPositions(minDistanceRandomFilling);
        for(int i = 0; i < nbRandomCubes; i++) {
            Vector3 selectedPos = MathTools.ChoseOne(farAwayPos);
            fullBlocks.Add(CreateFullBlockAtPos(selectedPos));
            List<Vector3> closePos = map.GetEmptyPositionsInSphere(selectedPos, distanceMinBetweenCubesOfRandomFilling);
            farAwayPos = farAwayPos.FindAll(p => !closePos.Contains(p));
        }
        return fullBlocks;
    }

    private FullBlock CreateFullBlockAtPos(Vector3 pos) {
        Vector3 finalPos = pos - Vector3.one * Mathf.FloorToInt(sizeCubeRandomFilling / 2.0f);
        FullBlock fb = new FullBlock(finalPos, Vector3Int.one * sizeCubeRandomFilling);
        return fb;
    }
}
