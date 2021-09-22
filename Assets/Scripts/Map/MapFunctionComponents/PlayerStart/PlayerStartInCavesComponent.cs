using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStartInCavesComponent : PlayerStartComponent {

    public int offsetFromSides = 0;
    public bool shouldBeOverAnotherCube = true;

    public override Vector3 GetRawPlayerStartPosition() {
        List<CubeEnsemble> cubeEnsembles = map.GetCubeEnsemblesOfType(CubeEnsemble.CubeEnsembleType.CAVE);
        List<Cave> caves = cubeEnsembles.Select(c => (Cave)c).ToList();
        if(caves.Count == 0) {
            return base.GetRawPlayerStartPosition();
        }
        Cave cave = MathTools.ChoseOne(caves);
        Vector3 position = shouldBeOverAnotherCube ?  cave.GetFreeLocationOverCube(offsetFromSides) : cave.GetFreeLocation(offsetFromSides);
        return position;
    }
}
