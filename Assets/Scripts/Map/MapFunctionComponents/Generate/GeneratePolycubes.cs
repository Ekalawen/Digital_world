using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class GeneratePolycubes : GenerateCubesMapFunction {

    public class NoPolycubeStartingPositionException : Exception {};

    public enum GenerationMode {
        COUNT,
        UNTIL_FULL_SPACE,
    }

    public GenerationMode generationMode;
    [ConditionalHide("generationMode", GenerationMode.COUNT)]
    public int nbPolycubes = 1;
    public Vector2Int nbInitCubesRange = new Vector2Int(5, 10);
    public int makeSpaceArroundAtEndDistance = 0;
    public bool stayInMap = true;
    public Polycube.ChosingVoisinsMethodType chosingVoisinsMethodType = Polycube.ChosingVoisinsMethodType.UNIFORM;
    [ConditionalHide("!chosingVoisinsMethodType", Polycube.ChosingVoisinsMethodType.UNIFORM)]
    public float coefChosingMethod = 2;

    public override void Activate() {
        GenerateAllPolycubes();
    }

    protected List<Polycube> GenerateAllPolycubes() {
        List<Polycube> polycubes = null;
        if(generationMode == GenerationMode.COUNT) {
            polycubes = GenerateNPolycubes(nbPolycubes);
        } else if (generationMode == GenerationMode.UNTIL_FULL_SPACE) {
            polycubes = GeneratePolycubesUntilFullSpace();
        }
        return polycubes;
    }

    protected List<Polycube> GenerateNPolycubes(int nbPolycubesToGenerate) {
        List<Polycube> polycubes = new List<Polycube>();
        for(int i = 0; i < nbPolycubesToGenerate; i++) {
            Polycube polycube = GeneratePolycube();
            if(polycube == null) { // On ne peut plus en générer !
                break;
            }
            polycubes.Add(polycube);
        }
        return polycubes;
    }

    protected List<Polycube> GeneratePolycubesUntilFullSpace() {
        List<Polycube> polycubes = new List<Polycube>();
        int kmax = 1000; // 200 pour du 13^3
        for(int k = 0; k < kmax; k++) { // On ne fait pas de boucle while !!!
            Polycube polycube = GeneratePolycube();
            if(polycube == null) { // On ne peut plus en générer !
                break;
            }
            polycubes.Add(polycube);
        }
        return polycubes;
    }

    protected Polycube GeneratePolycube() {
        try {
            Vector3 depart = GetPolycubeStartingPosition();
            int nbInitCubes = MathTools.RandBetween(nbInitCubesRange);
            Polycube polycube = new Polycube(depart, nbInitCubes, makeSpaceArroundAtEndDistance, stayInMap, chosingVoisinsMethodType, coefChosingMethod);
            return polycube;
        } catch(NoPolycubeStartingPositionException) {
            return null;
        }
    }

    protected Vector3 GetPolycubeStartingPosition() {
        List<Vector3> emptyPositions = map.GetAllEmptyPositions();
        if(stayInMap) {
            emptyPositions = emptyPositions.FindAll(v => map.IsInRegularMap(v));
        }
        if(emptyPositions.Count == 0) {
            throw new NoPolycubeStartingPositionException();
        }

        Vector3 chosenPosition = MathTools.ChoiceOne(emptyPositions);
        while(!Polycube.IsGoodStartingPosition(chosenPosition, makeSpaceArroundAtEndDistance, map) && emptyPositions.Count > 1) {
            emptyPositions.Remove(chosenPosition);
            chosenPosition = MathTools.ChoiceOne(emptyPositions);
        }
        if(emptyPositions.Count <= 1) {
            throw new NoPolycubeStartingPositionException();
        }

        return chosenPosition;
    }
}
