using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateNumberedRandomFilling : GenerateCubesMapFunction {

    public int nbRandomCubes = 30;
    public float minDistanceRandomFilling = 1f;
    public int sizeCubeRandomFilling = 1; // Ca peut être intéressant d'augmenter cette taille ! :)

    public override void Activate() {
        GenerateNumberedRandomFillingCubes();
    }

    protected List<FullBlock> GenerateNumberedRandomFillingCubes() {
        List<FullBlock> fullBlocks = new List<FullBlock>();
        List<Vector3> farAwayPos = map.GetFarAwayFromAllCubesPositions(minDistanceRandomFilling);
        List<Vector3> selectedPos = GaussianGenerator.SelecteSomeNumberOf(farAwayPos, nbRandomCubes);
        foreach (Vector3 pos in selectedPos) {
            Vector3 finalPos = pos - Vector3.one * Mathf.FloorToInt(sizeCubeRandomFilling / 2.0f);
            FullBlock fb = new FullBlock(finalPos, Vector3Int.one * sizeCubeRandomFilling);
            fullBlocks.Add(fb);
        }
        return fullBlocks;
    }

}
