using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStartInCavesComponent : PlayerStartComponent {

    public int offsetFromSides = 0;
    public bool shouldBeOverAnotherCube = true;
    [ConditionalHide("shouldBeOverAnotherCube")]
    public bool aboveSpecificCubeType = false;
    [ConditionalHide("aboveSpecificCubeType")]
    public Cube.CubeType specificCubeType = Cube.CubeType.NORMAL;

    public override Vector3 GetRawPlayerStartPosition()
    {
        List<CubeEnsemble> cubeEnsembles = map.GetCubeEnsemblesOfType(CubeEnsemble.CubeEnsembleType.CAVE);
        List<Cave> caves = cubeEnsembles.Select(c => (Cave)c).ToList();
        if (caves.Count == 0) {
            return base.GetRawPlayerStartPosition();
        }
        if(shouldBeOverAnotherCube && aboveSpecificCubeType) {
            return GetInCaveAboveCubeTypePosition(caves);
        }
        return GetInCavePosition(caves);
    }

    protected Vector3 GetInCavePosition(List<Cave> caves) {
        Cave cave = MathTools.ChoseOne(caves);
        Vector3 position = shouldBeOverAnotherCube ? cave.GetFreeLocationOverCube(offsetFromSides) : cave.GetFreeLocation(offsetFromSides);
        return position;
    }

    protected Vector3 GetInCaveAboveCubeTypePosition(List<Cave> caves) {
        MathTools.Shuffle(caves);
        foreach(Cave cave in caves) {
            List<Vector3> goodPositions = cave.GetAllFreeLocations(offsetFromSides);
            goodPositions = goodPositions.FindAll(p => map.IsOverCubeType(p, specificCubeType));
            if(goodPositions.Count > 0) {
                return MathTools.ChoseOne(goodPositions);
            }
        }
        return base.GetRawPlayerStartPosition();
    }
}
