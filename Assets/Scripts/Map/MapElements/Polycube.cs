using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Polycube : CubeEnsemble {

    public Vector3 depart;
    public int nbInitCubes = 5;

    public Polycube(Vector3 depart, int nbInitCubes, int makeSpaceArroundAtEndDistance, bool preserveMapBordure = false) : base() {
        this.depart = depart;
        this.nbInitCubes = nbInitCubes;

        AssertGoodStartingPosition();
        GeneratePolycube(preserveMapBordure);
        MakeSpaceArroundAtEnd(makeSpaceArroundAtEndDistance);
    }

    public override string GetName() {
        return "Polycube";
    }

    protected override void InitializeCubeEnsembleType() {
        cubeEnsembleType = CubeEnsembleType.POLYCUBE;
    }

    protected void GeneratePolycube(bool preserveMapBordure) {
        int nbCubesToCreate = nbInitCubes;
        CreateCube(depart);
        nbCubesToCreate--;

        for(int i = 0; i < nbCubesToCreate; i++) {
            List<Vector3> voisinsLibres = GetPossiblesVoisins();
            if (preserveMapBordure) {
                voisinsLibres = voisinsLibres.FindAll(v => map.IsInInsidedRegularMap(v));
            }
            if(voisinsLibres.Count == 0) {
                nbInitCubes -= nbCubesToCreate - i;
                break;
            }
            Vector3 voisinChosen = MathTools.GetOne(voisinsLibres);
            CreateCube(voisinChosen); // Add to cubes
        }
    }

    protected List<Vector3> GetPossiblesVoisins() {
        List<Vector3> possibleVoisins = new List<Vector3>();
        foreach(Cube cube in cubes) {
            List<Vector3> voisinsLibres = map.GetVoisinsLibresAll(cube.transform.position);
            foreach(Vector3 voisinLibre in voisinsLibres) {
                List<Vector3> voisinsPleinsDesVoisinsLibres = map.GetVoisinsPleinsAll(voisinLibre);
                voisinsPleinsDesVoisinsLibres = voisinsPleinsDesVoisinsLibres.FindAll(v => !Contains(v));

                if(voisinsPleinsDesVoisinsLibres.Count == 0) {
                    possibleVoisins.Add(voisinLibre);
                }
            }
        }
        return possibleVoisins;
    }

    public bool Contains(Vector3 pos) {
        return Positions().Contains(pos);
    }

    public List<Vector3> Positions() {
        return cubes.Select(c => c.transform.position).ToList();
    }

    protected void MakeSpaceArroundAtEnd(int makeSpaceArroundAtEndDistance) {
    }

    protected void AssertGoodStartingPosition() {
        if(!IsGoodStartingPosition(depart, map)) {
            Debug.LogError($"Un Polycube n'a pas une bonne position de départ !({depart})");
        }
    }

    public static bool IsGoodStartingPosition(Vector3 depart, MapManager map) {
        return !map.IsCubeAt(depart) && map.GetVoisinsLibresAll(depart).Count == 6;
    }
}
