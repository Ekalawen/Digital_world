using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave : CubeEnsemble
{

    public Vector3 depart;
    public Vector3Int nbCubesParAxe;
    public Cube[,,] cubeMatrix;

    public Cave(Vector3 depart, Vector3Int nbCubesParAxe, bool bMakeSpaceArround = false, bool bDigInside = true) : base() {
        this.depart = depart;
        this.nbCubesParAxe = nbCubesParAxe;

        InitializeCubeMatrix();
        CleanSpaceBeforeSpawning(bMakeSpaceArround);
        GenererCubePlein();
        if (bDigInside)
            GeneratePaths();
    }

    // Génère un cube plein qui part d'un point de départ et qui va dans 3 directions avec 3 distances !
    protected void GenererCubePlein() {
		// On remplit tout
		for(int i = 0; i < nbCubesParAxe.x; i++) {
			for(int j = 0; j < nbCubesParAxe.y; j++) {
				for(int k = 0; k < nbCubesParAxe.z; k++) {
                    Vector3 pos = depart + Vector3.right * i + Vector3.up * j + Vector3.forward * k;
                    Cube cube = CreateCube(pos);
                    cubeMatrix[i, j, k] = cube;
				}
			}
		}
	}

    protected void CleanSpaceBeforeSpawning(bool bMakeSpaceArround = false) {
        // On veut détruire tous les cubes et lumières qui se trouvent dans notre cave !
        Vector3 center = GetCenter();
        Vector3 halfSize = new Vector3(Mathf.Abs(center.x - depart.x), Mathf.Abs(center.y - depart.y), Mathf.Abs(center.z - depart.z));
        center -= new Vector3(0.5f, 0.5f, 0.5f); // Petit ajustement important
        if(bMakeSpaceArround)
            halfSize += new Vector3(1f, 1f, 1f); // Petit ajustement important, pour laisser un espace autour de la cave !
        map.DeleteCubesInBox(center, halfSize);

        // Et toutes les lumières
        List<Lumiere> lumieresToDeletes = new List<Lumiere>();
        foreach(Lumiere lumiere in map.lumieres) {
            Vector3 posL = lumiere.transform.position;
            if(Mathf.Abs(center.x - posL.x) <= halfSize.x
            && Mathf.Abs(center.y - posL.y) <= halfSize.y
            && Mathf.Abs(center.z - posL.z) <= halfSize.z)
            {
                lumieresToDeletes.Add(lumiere);
            }
        }
        foreach(Lumiere lumiereToDelete in lumieresToDeletes) {
            map.lumieres.Remove(lumiereToDelete);
        }
    }

    public Vector3 GetCenter() {
        return depart + nbCubesParAxe.x * Vector3.right / 2.0f
            + nbCubesParAxe.y * Vector3.up / 2.0f
            + nbCubesParAxe.z * Vector3.forward / 2.0f;
    }

    protected void InitializeCubeMatrix() {
        cubeMatrix = new Cube[nbCubesParAxe.x, nbCubesParAxe.y, nbCubesParAxe.z];
        for (int i = 0; i < nbCubesParAxe.x; i++)
            for (int j = 0; j < nbCubesParAxe.y; j++)
                for (int k = 0; k < nbCubesParAxe.z; k++)
                    cubeMatrix[i, j, k] = null;
    }

    protected void GeneratePaths() {
        // On va choisir les entrées, une pour chaque coté !
        List<Vector3> entrees = new List<Vector3>();
        entrees.Add(new Vector3(0, Random.Range(0, nbCubesParAxe.y), Random.Range(0, nbCubesParAxe.z)));
        entrees.Add(new Vector3(nbCubesParAxe.x-1, Random.Range(0, nbCubesParAxe.y), Random.Range(0, nbCubesParAxe.z)));
        entrees.Add(new Vector3(Random.Range(0, nbCubesParAxe.x), 0, Random.Range(0, nbCubesParAxe.z)));
        entrees.Add(new Vector3(Random.Range(0, nbCubesParAxe.x), nbCubesParAxe.y-1, Random.Range(0, nbCubesParAxe.z)));
        entrees.Add(new Vector3(Random.Range(0, nbCubesParAxe.x), Random.Range(0, nbCubesParAxe.y), 0));
        entrees.Add(new Vector3(Random.Range(0, nbCubesParAxe.x), Random.Range(0, nbCubesParAxe.y), nbCubesParAxe.z-1));

        // On va choisir les points de passages internes
        int nbPointsDePassage = Random.Range(1, (int) Mathf.Ceil(nbCubesParAxe.x*nbCubesParAxe.y*nbCubesParAxe.z / 15));
        List<Vector3> pointsDePassage = new List<Vector3>();
        for (int i = 0; i < nbPointsDePassage; i++) {
            pointsDePassage.Add(new Vector3(Random.Range(1, nbCubesParAxe.x - 1), Random.Range(1, nbCubesParAxe.y - 1), Random.Range(1, nbCubesParAxe.z - 1)));
        }

        // Et maintenant on va chercher à relier tous ces points !
        List<Vector3> ptsCibles = entrees;
        ptsCibles.AddRange(pointsDePassage);

        // On part d'un point, on va creuser pour atteindre un autre point
        // et on va continuer tant qu'on a pas atteint tous les points !
        List<Vector3> ptsAtteints = new List<Vector3>();
        Vector3 depart = ptsCibles[Random.Range (0, ptsCibles.Count)]; // on rajoute le premier point
        ptsAtteints.Add(depart);
        ptsCibles.Remove(depart);
        while (ptsCibles.Count > 0) {
            Vector3 debutChemin = ptsAtteints[Random.Range(0, ptsAtteints.Count)];
            Vector3 finChemin = ptsCibles[Random.Range(0, ptsCibles.Count)];
            RelierChemin(debutChemin, finChemin);
            ptsCibles.Remove(finChemin);
            ptsAtteints.Add(finChemin);
        }
    }

	// Le but de cette fonction est de creuser un tunel allant de debutChemin a finChemin !
	protected void RelierChemin(Vector3 debutChemin, Vector3 finChemin) {
		Vector3 pointsActuel = debutChemin;
		while (pointsActuel != finChemin) {
			// On creuse
			map.DeleteCube(cubeMatrix[(int)pointsActuel.x, (int)pointsActuel.y, (int)pointsActuel.z]);
            cubeMatrix[(int)pointsActuel.x, (int)pointsActuel.y, (int)pointsActuel.z] = null;

			// On liste les bonnes directions à prendre
			List<Vector3> directions = new List<Vector3>();
			if (pointsActuel.x != finChemin.x) {
				if (pointsActuel.x < finChemin.x) {
					directions.Add (new Vector3 (pointsActuel.x + 1, pointsActuel.y, pointsActuel.z));
				} else {
					directions.Add (new Vector3 (pointsActuel.x - 1, pointsActuel.y, pointsActuel.z));
				}
			}
			if (pointsActuel.y != finChemin.y) {
				if (pointsActuel.y < finChemin.y) {
					directions.Add (new Vector3 (pointsActuel.x, pointsActuel.y + 1, pointsActuel.z));
				} else {
					directions.Add (new Vector3 (pointsActuel.x, pointsActuel.y - 1, pointsActuel.z));
				}
			}
			if (pointsActuel.z != finChemin.z) {
				if (pointsActuel.z < finChemin.z) {
					directions.Add (new Vector3 (pointsActuel.x, pointsActuel.y, pointsActuel.z + 1));
				} else {
					directions.Add (new Vector3 (pointsActuel.x, pointsActuel.y, pointsActuel.z - 1));
				}
			}

			// On se déplace dans une bonne direction aléatoirement
			pointsActuel = directions[Random.Range(0, directions.Count)];
		}
        map.DeleteCube(cubeMatrix[(int)pointsActuel.x, (int)pointsActuel.y, (int)pointsActuel.z]);
        cubeMatrix[(int)pointsActuel.x, (int)pointsActuel.y, (int)pointsActuel.z] = null;
	}

    public void AddOneLumiereInside() {
        // On cherche une case où créer un objectif !
        Vector3 posObjectif = new Vector3(
            Random.Range(1, nbCubesParAxe.x - 1),
            Random.Range(1, nbCubesParAxe.y - 1), 
            Random.Range(1, nbCubesParAxe.z - 1));
        while (cubeMatrix[(int)posObjectif.x, (int)posObjectif.y, (int)posObjectif.z] != null)
        {
            posObjectif = new Vector3(
                Random.Range(1, nbCubesParAxe.x - 1),
                Random.Range(1, nbCubesParAxe.y - 1), 
                Random.Range(1, nbCubesParAxe.z - 1));
        }
        posObjectif += depart;
        Lumiere lumiere = GameObject.Instantiate(map.lumierePrefab, posObjectif, Quaternion.identity).GetComponent<Lumiere>();
        map.lumieres.Add(lumiere);
    }

    public void AddAllLumiereInside() {
        for (int i = 0; i < nbCubesParAxe.x; i++) {
            for (int j = 0; j < nbCubesParAxe.y; j++) {
                for (int k = 0; k < nbCubesParAxe.z; k++) {
                    if(cubeMatrix[i, j, k] == null) {
                        Vector3 posObjectif = new Vector3(i, j, k);
                        posObjectif += depart;
                        Lumiere lumiere = GameObject.Instantiate(map.lumierePrefab, posObjectif, Quaternion.identity).GetComponent<Lumiere>();
                        map.lumieres.Add(lumiere);
                    }
                }
            }
        }
    }
}
