using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cave : CubeEnsemble {

    public Vector3 depart;
    public Vector3Int nbCubesParAxe;
    public Cube[,,] cubeMatrix;

    public Cave(Vector3 depart, Vector3Int nbCubesParAxe, bool bMakeSpaceArround = false, bool bDigInside = true, bool bPreserveMapBordures = false) : base() {
        this.depart = depart;
        this.nbCubesParAxe = nbCubesParAxe;

        InitializeCubeMatrix();
        CleanSpaceBeforeSpawning(bMakeSpaceArround, bPreserveMapBordures);
        GenererCubePlein();
        if (bDigInside)
            GeneratePaths();
    }

    public override string GetName() {
        return "Cave";
    }

    protected override void InitializeCubeEnsembleType() {
        cubeEnsembleType = CubeEnsembleType.CAVE;
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

    // Vide le cube
    public void RemoveAllCubesInside(int offset = 1) {
        for (int i = offset; i < nbCubesParAxe.x - offset; i++) {
            for (int j = offset; j < nbCubesParAxe.y - offset; j++) {
                for (int k = offset; k < nbCubesParAxe.z - offset; k++) {
                    Cube cube = cubeMatrix[i, j, k];
                    if(cube != null) {
                        map.DeleteCube(cube);
                        cubeMatrix[i, j, k] = null;
                    }
                }
            }
        }
    }

    public void AddOuverturesOnSides(int nbOuvertures = 1, bool allowOuverturesInCorners = false) {
        List<Cube>[] murs = GetAllMursPositions();
        foreach(List<Cube> mur in murs) {
            List<Cube> murToUse = mur;

            if(!allowOuverturesInCorners) {
                murToUse = new List<Cube>();
                foreach(Cube cube in mur) {
                    Vector3 cubePos = cube.transform.position - depart;
                    if(!IsInCorner(cubePos)) {
                        murToUse.Add(cube);
                    }
                }
            }

            for(int i = 0; i < (int)Mathf.Min(nbOuvertures, murToUse.Count); i++) {
                int ind = Random.Range(0, murToUse.Count);
                Cube cubeSelected = murToUse[ind];
                map.DeleteCube(cubeSelected);
                Vector3 pos = cubeSelected.transform.position - depart;
                cubeMatrix[(int)pos.x, (int)pos.y, (int)pos.z] = null;
                murToUse.RemoveAt(ind);
            }
        }
    }

    protected List<Cube>[] GetAllMursPositions() {
        List<Cube>[] murs = new List<Cube>[6];
        for(int i = 0; i < murs.Length; i++)
            murs[i] = new List<Cube>();
        for (int i = 0; i < nbCubesParAxe.x; i++) {
            for (int j = 0; j < nbCubesParAxe.y; j++) {
                for (int k = 0; k < nbCubesParAxe.z; k++) {
                    Cube cube = cubeMatrix[i, j, k];
                    if (cube != null) {
                        Vector3 pos = new Vector3(i, j, k);
                        if (i == 0) murs[0].Add(cube);
                        if (j == 0) murs[1].Add(cube);
                        if (k == 0) murs[2].Add(cube);
                        if (i == nbCubesParAxe.x - 1) murs[3].Add(cube);
                        if (j == nbCubesParAxe.y - 1) murs[4].Add(cube);
                        if (k == nbCubesParAxe.z - 1) murs[5].Add(cube);
                    }
                }
            }
        }
        return murs;
    }

    public bool IsInCorner(Vector3 pos) {
        int nbBordures = 0;
        nbBordures += pos.x == 0 ? 1 : 0;
        nbBordures += pos.y == 0 ? 1 : 0;
        nbBordures += pos.z == 0 ? 1 : 0;
        nbBordures += pos.x == nbCubesParAxe.x - 1 ? 1 : 0;
        nbBordures += pos.y == nbCubesParAxe.y - 1 ? 1 : 0;
        nbBordures += pos.z == nbCubesParAxe.z - 1 ? 1 : 0;
        return nbBordures >= 2;
    }

    protected void CleanSpaceBeforeSpawning(bool bMakeSpaceArround = false, bool preserveMapBordure = false) {
        // On veut détruire tous les cubes et lumières qui se trouvent dans notre cave !
        Vector3 center = GetCenter();
        Vector3 halfSize = new Vector3(Mathf.Abs(center.x - depart.x), Mathf.Abs(center.y - depart.y), Mathf.Abs(center.z - depart.z));
        if(bMakeSpaceArround)
            halfSize += Vector3.one; // Petit ajustement important, pour laisser un espace autour de la cave !
        List<Cube> cubesToDelete = map.GetCubesInBox(center, halfSize);
        foreach (Cube cube in cubesToDelete) {
            if (preserveMapBordure) {
                if (!map.IsInInsidedRegularMap(cube.transform.position))
                    continue;
            }
            map.DeleteCube(cube);
        }

        // Et toutes les lumières
        List<Lumiere> lumieresToDeletes = new List<Lumiere>();
        foreach(Lumiere lumiere in map.GetLumieres()) {
            Vector3 posL = lumiere.transform.position;
            if(Mathf.Abs(center.x - posL.x) <= halfSize.x
            && Mathf.Abs(center.y - posL.y) <= halfSize.y
            && Mathf.Abs(center.z - posL.z) <= halfSize.z)
            {
                lumieresToDeletes.Add(lumiere);
            }
        }
        foreach(Lumiere lumiereToDelete in lumieresToDeletes) {
            map.RemoveLumiere(lumiereToDelete);
            Object.DestroyImmediate(lumiereToDelete.gameObject);
        }
    }

    public Vector3 GetHalfExtents() {
        return (Vector3)nbCubesParAxe / 2;
    }

    public Vector3 GetCenter() {
        return depart + (nbCubesParAxe.x - 1) * Vector3.right / 2.0f
            + (nbCubesParAxe.y - 1) * Vector3.up / 2.0f
            + (nbCubesParAxe.z - 1) * Vector3.forward / 2.0f;
    }

    public Vector3 GetOnTop() {
        return depart + (nbCubesParAxe.x - 1) * Vector3.right / 2.0f
            + nbCubesParAxe.y * Vector3.up
            + (nbCubesParAxe.z - 1) * Vector3.forward / 2.0f;
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
            //Vector3 debutChemin = ptsAtteints[0];
            Vector3 finChemin = ptsCibles[Random.Range(0, ptsCibles.Count)];
            RelierChemin(cubeMatrix, map, debutChemin, finChemin);
            ptsCibles.Remove(finChemin);
            ptsAtteints.Add(finChemin);
            //ptsAtteints[0] = finChemin;
        }
    }

	// Le but de cette fonction est de creuser un tunel allant de debutChemin a finChemin !
	public static void RelierChemin(Cube[,,] cubeMatrix, MapManager map, Vector3 debutChemin, Vector3 finChemin) {
		Vector3 pointsActuel = debutChemin;
		while (pointsActuel != finChemin) {
			// On creuse
			map.DeleteCube(cubeMatrix[(int)pointsActuel.x, (int)pointsActuel.y, (int)pointsActuel.z]);
            cubeMatrix[(int)pointsActuel.x, (int)pointsActuel.y, (int)pointsActuel.z] = null; // utile car cubeMatrix n'est pas forcément la map !

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
        cubeMatrix[(int)pointsActuel.x, (int)pointsActuel.y, (int)pointsActuel.z] = null; // utile car cubeMatrix n'est pas forcément la map !
	}

    public bool AddNLumiereInside(int nbLumieresToAdd, int offsetFromCenter = 1) {
        offsetFromCenter = ComputeOffsetFromCenter(nbLumieresToAdd, offsetFromCenter);
        List<Vector3> possiblePos = ComputeAllPossiblePos(offsetFromCenter);
        for (int i = 0; i < nbLumieresToAdd; i++) {
            if(possiblePos.Count == 0) {
                Debug.LogWarning($"Nous n'avons pas trouvé de place pour {nbLumieresToAdd - i} Lumières dans AddNLumiereInside()");
                return false;
            }
            Vector3 posLumiere = possiblePos[Random.Range(0, possiblePos.Count)];
            possiblePos.Remove(posLumiere);
            Vector3 worldPosLumiere = depart + posLumiere;
            map.CreateLumiere(worldPosLumiere, Lumiere.LumiereType.NORMAL);
        }
        return true;
    }

    protected List<Vector3> ComputeAllPossiblePos(int offsetFromCenter) {
        List<Vector3> possibilities = new List<Vector3>();
        for(int i = offsetFromCenter; i < nbCubesParAxe.x - offsetFromCenter; i++) {
            for(int j = offsetFromCenter; j < nbCubesParAxe.y - offsetFromCenter; j++) {
                for(int k = offsetFromCenter; k < nbCubesParAxe.z - offsetFromCenter; k++) {
                    Vector3 pos = new Vector3(i, j, k);
                    Vector3 worldPos = depart + pos;
                    if (cubeMatrix[i, j, k] == null && map.GetCubeAt(worldPos) == null) {
                        if (!map.GetAllLumieresPositions().Contains(worldPos)) {
                            possibilities.Add(pos);
                        }
                    }
                }
            }
        }
        return possibilities;
    }

    protected int ComputeOffsetFromCenter(int nbLumieresToAdd, int offsetFromCenter) {
        while (nbLumieresToAdd > 0
            && GetNbPossibilitiesLumieresWithOffsetsFromCenter(offsetFromCenter) < nbLumieresToAdd
            && offsetFromCenter > 0) {
            offsetFromCenter--;
        }
        return offsetFromCenter;
    }

    protected int GetNbPossibilitiesLumieresWithOffsetsFromCenter(int offsetFromCenter) {
        int placesX = nbCubesParAxe.x - (2 * offsetFromCenter);
        int placesY = nbCubesParAxe.y - (2 * offsetFromCenter);
        int placesZ = nbCubesParAxe.z - (2 * offsetFromCenter);
        if (placesX <= 0 || placesY <= 0 || placesZ <= 0)
            return 0;
        return placesX * placesY * placesZ;
    }

    public void AddAllLumiereInside(Lumiere.LumiereType lumiereType = Lumiere.LumiereType.NORMAL) {
        for (int i = 0; i < nbCubesParAxe.x; i++) {
            for (int j = 0; j < nbCubesParAxe.y; j++) {
                for (int k = 0; k < nbCubesParAxe.z; k++) {
                    if(cubeMatrix[i, j, k] == null) {
                        Vector3 posLumiere = new Vector3(i, j, k);
                        posLumiere += depart;
                        map.CreateLumiere(posLumiere, lumiereType);
                    }
                }
            }
        }
    }

    public float GetVolume() {
        return (float)nbCubesParAxe[0] * nbCubesParAxe[1] * nbCubesParAxe[2];
    }

    public void FulfillFloor(bool exceptOne = false) {
        for(int x = 0; x < nbCubesParAxe.x; x++) {
            for(int z = 0; z < nbCubesParAxe.z; z++) {
                if(cubeMatrix[x, 0, z] == null) {
                    Vector3 pos = depart + new Vector3(x, 0, z);
                    Cube cube = CreateCube(pos);
                    cubeMatrix[x, 0, z] = cube;
                }
            }
        }

        if(exceptOne) {
            Vector3Int pos = new Vector3Int(Random.Range(0, nbCubesParAxe.x), 0, Random.Range(0, nbCubesParAxe.z));
            Vector3Int dest = GetFreeIndiceLocation(offsetFromSides: 1);
            RelierChemin(cubeMatrix, map, pos, dest);
        }
    }

    public Vector3 GetFreeLocation(int offsetFromSides = 0) {
        return depart + GetFreeIndiceLocation(offsetFromSides);
    }

    protected Vector3Int GetFreeIndiceLocation(int offsetFromSides = 0) {
        int k = 0, kmax = 100000;
        while(k < kmax) {
            Vector3Int pos = new Vector3Int(
                Random.Range(offsetFromSides, nbCubesParAxe.x - offsetFromSides),
                Random.Range(offsetFromSides, nbCubesParAxe.y - offsetFromSides),
                Random.Range(offsetFromSides, nbCubesParAxe.z - offsetFromSides));
            if (cubeMatrix[pos.x, pos.y, pos.z] == null)
                return pos;
            k++;
        }
        throw new System.Exception("Cette cave est pleine ! Impossible de trouver une free location !");
    }
}
