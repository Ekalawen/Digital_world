using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLumieresFarFromCubeType : GenerateLumieresMapFunction {

    public Cube.CubeType cubeTypeFarFrom = Cube.CubeType.NORMAL;
    public float minDistance = 3.0f;

    public override void Activate() {
        GenerateLumieres();
    }

    protected void GenerateLumieres() {
        List<Cube> surfacePositions = map.GetAllCubesOfType(cubeTypeFarFrom);
        for (int i = 0; i < map.nbLumieresInitial; i++) {
            Vector3 pos = map.GetFarFromEnsemble(surfacePositions, minDistance);
            map.CreateLumiere(pos, lumiereType);
        }
    }
}
