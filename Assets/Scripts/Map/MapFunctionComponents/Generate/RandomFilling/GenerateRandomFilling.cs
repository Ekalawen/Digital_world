using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Diagnostics;

public class GenerateRandomFilling : GenerateCubesMapFunction {

    public enum ZoneToFillType { MIN_DISTANCE, ALL_POSITIONS, USE_HELPER };

    [Header("Zone to Fill")]
    public ZoneToFillType typeZoneToFill = ZoneToFillType.MIN_DISTANCE;
    [ConditionalHide("typeZoneToFill", ZoneToFillType.MIN_DISTANCE)]
    public float minDistanceRandomFilling = 1f;
    [ConditionalHide("typeZoneToFill", ZoneToFillType.USE_HELPER)]
    public GetEmptyPositionsHelper getEmptyPositionsHelper;

    [Header("How to Fill")]
    public float proportionRandomFilling = 0.02f;
    public int sizeCubeRandomFilling = 1; // Ca peut être intéressant d'augmenter cette taille ! :)
    public bool registerToColorSources = false;
    public bool setDissolveTime = false;
    [ConditionalHide("setDissolveTime")]
    public float dissolveTime = 1.0f;
    public int minNbBlocks = 0;

    public override void Activate() {
        GenerateRandomFillingCubes();
    }

    protected List<FullBlock> GenerateRandomFillingCubes()
    {
        List<FullBlock> fullBlocks = new List<FullBlock>();
        List<Vector3> selectedPos = GetSelectedPos();
        foreach (Vector3 pos in selectedPos) {
            Vector3 finalPos = pos - Vector3.one * (int)Mathf.Floor(sizeCubeRandomFilling / 2.0f);
            FullBlock fb = new FullBlock(finalPos, Vector3Int.one * sizeCubeRandomFilling, cleanSpaceBeforeSpawning: false);
            if (setDissolveTime) {
                foreach (Cube cube in fb.GetCubes()) {
                    cube.SetDissolveTimeBeCareful(dissolveTime);
                }
            }
            if (registerToColorSources)
                fb.RegisterToColorSources();
            fullBlocks.Add(fb);
        }
        return fullBlocks;
    }

    protected List<Vector3> GetSelectedPos() {
        List<Vector3> posInZone = GetPositionsInZone();
        List<Vector3> selectedPos = SelectPositionsInZone(posInZone);
        return selectedPos;
    }

    protected List<Vector3> SelectPositionsInZone(List<Vector3> posInZone) {
        List<Vector3> selectedPos = GaussianGenerator.SelectSomeProportionOfNaiveMethod<Vector3>(posInZone, proportionRandomFilling);
        if (selectedPos.Count < minNbBlocks) {
            posInZone.RemoveAll(p => selectedPos.Contains(p));
            selectedPos.AddRange(GaussianGenerator.SelecteSomeNumberOf(posInZone, minNbBlocks - selectedPos.Count));
        }

        return selectedPos;
    }

    protected List<Vector3> GetPositionsInZone() {
        List<Vector3> posInZone = null;
        switch (typeZoneToFill) {
            case ZoneToFillType.MIN_DISTANCE:
                posInZone = GetFarAwayPositions();
                break;
            case ZoneToFillType.ALL_POSITIONS:
                posInZone = gm.map.GetAllEmptyPositions();
                break;
            case ZoneToFillType.USE_HELPER:
                posInZone = getEmptyPositionsHelper.Get();
                break;
        }

        return posInZone;
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
