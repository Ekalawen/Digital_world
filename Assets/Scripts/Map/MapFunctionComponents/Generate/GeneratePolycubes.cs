using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class GeneratePolycubes : GenerateCubesMapFunction {

    public class NoPolycubeStartingPositionException : Exception {};

    public int nbPolycubes = 1;
    public Vector2Int nbInitCubesRange = new Vector2Int(5, 10);
    public int makeSpaceArroundAtEndDistance = 0;
    public bool preserveMapBordure = true;

    public override void Activate() {
        GenerateAllPolycubes();
    }

    protected List<Polycube> GenerateAllPolycubes() {
        List<Polycube> polycubes = GenerateNPolycubes(nbPolycubes);
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

    protected Polycube GeneratePolycube() {
        try {
            Vector3 depart = GetPolycubeStartingPosition();
            int nbInitCubes = MathTools.RandBetween(nbInitCubesRange);
            Polycube polycube = new Polycube(depart, nbInitCubes, makeSpaceArroundAtEndDistance, preserveMapBordure);
            return polycube;
        } catch(NoPolycubeStartingPositionException) {
            return null;
        }
    }

    protected Vector3 GetPolycubeStartingPosition() {
        List<Vector3> emptyPositions = map.GetAllEmptyPositions();

        Vector3 chosenPosition = MathTools.GetOne(emptyPositions);
        while(!Polycube.IsGoodStartingPosition(chosenPosition, map) && emptyPositions.Count > 0) {
            emptyPositions.Remove(chosenPosition);
            chosenPosition = MathTools.GetOne(emptyPositions);
        }
        if(emptyPositions.Count == 0) {
            throw new NoPolycubeStartingPositionException();
        }

        return chosenPosition;
    }
}
