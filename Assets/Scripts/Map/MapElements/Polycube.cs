using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Polycube : CubeEnsemble {

    public Vector3 depart;
    public int nbInitCubes = 5;
    public int distanceArround = 1;

    public Polycube(Vector3 depart, int nbInitCubes, int distanceArround, bool stayInMap) : base() {
        this.depart = depart;
        this.nbInitCubes = nbInitCubes;
        this.distanceArround = distanceArround;

        AssertGoodStartingPosition();
        GeneratePolycube(stayInMap);
        //MakeSpaceArroundAtEnd(); // Pas besoin car on ne crée les cubes que si on déjà assez d'espace ! :)
    }

    public override string GetName() {
        return "Polycube";
    }

    protected override void InitializeCubeEnsembleType() {
        cubeEnsembleType = CubeEnsembleType.POLYCUBE;
    }

    protected void GeneratePolycube(bool stayInMap) {
        int nbCubesToCreate = nbInitCubes;
        CreateCube(depart);
        nbCubesToCreate--;

        for(int i = 0; i < nbCubesToCreate; i++) {
            List<Vector3> voisinsLibres = GetPossiblesVoisins();
            if (stayInMap) {
                voisinsLibres = voisinsLibres.FindAll(v => map.IsInRegularMap(v));
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
                List<Vector3> voisinsPleinsDesVoisinsLibres = map.GetCubesAtLessThanCubeDistance(voisinLibre, distanceArround).Select(c => c.transform.position).ToList();
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

    protected void MakeSpaceArroundAtEnd() {
        if (distanceArround <= 0)
            return;
        HashSet<Cube> nearCubes = new HashSet<Cube>();
        foreach(Cube cube in cubes) {
            List<Cube> cubesInRangeOfCube = map.GetCubesAtLessThanCubeDistance(cube.transform.position, distanceArround);
            nearCubes.UnionWith(cubesInRangeOfCube);
        }
        nearCubes.ExceptWith(cubes);
        foreach(Cube nearCube in nearCubes) {
            map.DeleteCube(nearCube);
        }
    }

    protected void AssertGoodStartingPosition() {
        if(!IsGoodStartingPosition(depart, distanceArround, map)) {
            Debug.LogError($"Un Polycube n'a pas une bonne position de départ !({depart})");
        }
    }

    public static bool IsGoodStartingPosition(Vector3 depart, int distance, MapManager map) {
        distance = Mathf.Max(distance, 1);
        return !map.IsCubeAt(depart) && map.GetCubesAtLessThanCubeDistance(depart, distance).Count == 0;
    }
}
