using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Polycube : CubeEnsemble {

    public enum ChosingVoisinsMethodType { UNIFORM, LESS_CUBES_FIRST, MORE_CUBES_FIRST };

    public Vector3 depart;
    public int nbInitCubes = 5;
    public int distanceArround = 1;
    public ChosingVoisinsMethodType chosingVoisinsMethodType;
    public float coefChosingMethod;

    public Polycube(Vector3 depart, int nbInitCubes, int distanceArround, bool stayInMap,
        ChosingVoisinsMethodType chosingVoisinsMethodType = ChosingVoisinsMethodType.UNIFORM,
        float coefChosingMethod = 1) : base() {
        this.depart = depart;
        this.nbInitCubes = nbInitCubes;
        this.distanceArround = distanceArround;
        this.chosingVoisinsMethodType = chosingVoisinsMethodType;
        this.coefChosingMethod = coefChosingMethod;

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

        for(int i = 0; i < nbCubesToCreate; i++)
        {
            List<Vector3> voisinsLibres = GetPossiblesVoisins();
            if (stayInMap)
            {
                voisinsLibres = voisinsLibres.FindAll(v => map.IsInRegularMap(v));
            }
            if (voisinsLibres.Count == 0)
            {
                nbInitCubes -= nbCubesToCreate - i; // = cubes.Count ?
                break;
            }
            Vector3 voisinChosen = ChoiceVoisin(voisinsLibres);
            CreateCube(voisinChosen); // Add to cubes
        }
    }

    protected Vector3 ChoiceVoisin(List<Vector3> voisinsLibres) {
        if(chosingVoisinsMethodType == ChosingVoisinsMethodType.UNIFORM) {
            return MathTools.ChoiceOne(voisinsLibres);
        } else {
            List<float> nbPolycubeVoisins;
            if (chosingVoisinsMethodType == ChosingVoisinsMethodType.LESS_CUBES_FIRST) {
                nbPolycubeVoisins = voisinsLibres.Select(vl => (float)GetPolycubesVoisins(vl, 1).Count).ToList();
                float maxVoisins = nbPolycubeVoisins.Max();
                nbPolycubeVoisins = nbPolycubeVoisins.Select(nb => maxVoisins + 1 - nb).ToList();
            } else { // MORE_CUBES_FIRST
                nbPolycubeVoisins = voisinsLibres.Select(vl => /*1 +*/ (float)GetPolycubesVoisins(vl, 1).Count).ToList(); // Don't need +1 cause we always have at least 1 voisin ! :)
            }
            float maxWeight = nbPolycubeVoisins.Max();
            List<float> weights = nbPolycubeVoisins.Select(w => w / maxWeight).ToList();
            weights = weights.Select(w => Mathf.Pow(w, coefChosingMethod)).ToList();
            return MathTools.ChoiceOneWeighted(voisinsLibres, weights);
        }
    }

    public List<Vector3> GetPolycubesVoisins(Vector3 pos, int distance) {
        return Positions().FindAll(p => MathTools.DistanceLInfini(p, pos) <= distance);
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
