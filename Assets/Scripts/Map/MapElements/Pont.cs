using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pont : CubeEnsemble {

    public Vector3 depart;
    public Vector3 direction;
    public int nbCubes;

    public override string GetName() {
        return "Pont";
    }

    public Pont(Vector3 depart, Vector3 direction, int nbCubes) : base() {
        this.depart = depart;
        this.direction = direction;
        this.nbCubes= nbCubes;

        GenererPont();
    }

    protected override void InitializeCubeEnsembleType() {
        cubeEnsembleType = CubeEnsembleType.PONT;
    }

    public Pont CreatePontWithPoints(Vector3 depart, Vector3 arriveeDirection, int nbCubes) {
        return new Pont(
            depart,
            (arriveeDirection - depart) / (float)(nbCubes - 1),
            nbCubes);
    }

    // Génère un pont qui part d'un point de départ dans une direction et sur une certaine distance !
    public void GenererPont() {
		for(int i = 0; i < nbCubes; i++) {
			Vector3 pos = depart + direction * i;
            CreateCube(pos);
		}
	}

    public List<Vector3> GetAllSurroundingPositions() {
        List<Vector3> res = new List<Vector3>();
        List<Vector3> cubesPositions = new List<Vector3>();
        foreach (Cube cube in cubes) cubesPositions.Add(cube.transform.position);
        foreach(Cube cube in cubes) {
            foreach(Vector3 voisin in MathTools.GetAllVoisins(cube.transform.position)) {
                if (!res.Contains(voisin) && !cubesPositions.Contains(voisin))
                    res.Add(voisin);
            }
        }
        return res;
    }

    public List<Lumiere> SurroundWithLumieres(Lumiere.LumiereType lumiereType, bool onlyHorizontaly = false) {
        List<Lumiere> lumieres = new List<Lumiere>();
        foreach(Vector3 pos in GetAllSurroundingPositions()) {
            if(map.IsInInsidedRegularMap(pos)) {
                if (!onlyHorizontaly || (onlyHorizontaly && pos.y == depart.y)) {
                    lumieres.Add(map.CreateLumiere(pos, lumiereType));
                }
            }
        }
        return lumieres;
    }
}
