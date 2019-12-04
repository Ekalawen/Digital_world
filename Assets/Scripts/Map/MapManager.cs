using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapManager : MonoBehaviour {

	// public enum TypeMap {CUBE_MAP, PLAINE_MAP, LABYRINTHE_MAP, GROUND_MAP, EMPTY_MAP, TUTORIAL_MAP}; // Plus vraiment utile ! :D

	public GameObject cubePrefab; // On récupère ce qu'est un cube !
	public GameObject lumierePrefab; // On récupère les lumières !
	public GameObject ennemiPrefabs; // On récupère un ennemi !

	public int tailleMap; // La taille de la map, en largeur, hauteur et profondeur

    private Cube[,,] cubesRegular; // Toutes les positions entières dans [0, tailleMap]
    private List<Cube> cubesNonRegular; // Toutes les autres positions (non-entières)
    [HideInInspector] public List<MapElement> mapElements;
    [HideInInspector]
    public List<Lumiere> lumieres;
    [HideInInspector]
    public GameManager gm;

    // To remove !
	[HideInInspector]
	public bool lumieresAttrapees;

    // To move
	[HideInInspector]
	public int nbEnnemis;

	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

    public void Initialize() {
		// Initialisation
		name = "MapManager";
        gm = FindObjectOfType<GameManager>();
        mapElements = new List<MapElement>();
        cubesRegular = new Cube[tailleMap + 1, tailleMap + 1, tailleMap + 1];
        for (int i = 0; i <= tailleMap; i++)
            for (int j = 0; j <= tailleMap; j++)
                for (int k = 0; k <= tailleMap; k++)
                    cubesRegular[i, j, k] = null;
        cubesNonRegular = new List<Cube>();

		lumieresAttrapees = false;

        // Ici les classes qui hériteront de cette classe pourront faire leur génération !
        GenerateMap();

        // Puis on régule la map pour s'assurer que tout va bien :)
    }

    protected abstract void GenerateMap();

    private void AddCube(Cube cube) {
        Vector3 pos = cube.transform.position;
        if (cube.transform.rotation == Quaternion.identity
         && IsInRegularMap(pos)
         && MathTools.IsRounded(pos)) {
            if (GetCubeAt(pos) == null) {
                cubesRegular[(int)pos.x, (int)pos.y, (int)pos.z] = cube;
            }
        } else {
            cubesNonRegular.Add(cube);
            cube.bIsRegular = false;
        }
    }

    public Cube AddCube(Vector3 pos, Quaternion quaternion = new Quaternion()) {
        if (GetCubeAt(pos) != null) // Si il y a déjà un cube à cette position, on ne fait rien !
            return null;
        Cube cube = Instantiate(cubePrefab, pos, quaternion).GetComponent<Cube>();
        AddCube(cube);
        return cube;
    }

    private void DestroyImmediateCube(Cube cube) {
        foreach(MapElement mapElement in mapElements) {
            mapElement.OnDeleteCube(cube);
        }
        DestroyImmediate(cube.gameObject);
    }

    public void DeleteCubesAt(Vector3 pos) {
        if(IsInRegularMap(pos) && MathTools.IsRounded(pos)) {
            DestroyImmediateCube(cubesRegular[(int)pos.x, (int)pos.y, (int)pos.z]);
            cubesRegular[(int)pos.x, (int)pos.y, (int)pos.z] = null;
        } else {
            Cube cubeToDestroy = null;
            foreach(Cube cube in cubesNonRegular) {
                if(cube.transform.position == pos) {
                    cubeToDestroy = cube;
                    break;
                }
            }
            cubesNonRegular.Remove(cubeToDestroy);
            DestroyImmediateCube(cubeToDestroy);
        }
    }

    public void DeleteCube(Cube cube) {
        if(cube != null)
            DeleteCubesAt(cube.transform.position);
    }

    public void DeleteCubesInSphere(Vector3 center, float radius) {
        int xMin = (int)Mathf.Floor(center.x - radius);
        int xMax = (int)Mathf.Ceil(center.x + radius);
        int yMin = (int)Mathf.Floor(center.y - radius);
        int yMax = (int)Mathf.Ceil(center.y + radius);
        int zMin = (int)Mathf.Floor(center.z - radius);
        int zMax = (int)Mathf.Ceil(center.z + radius);
        for(int i = xMin; i <= xMax; i++) {
            for(int j = yMin; j <= yMax; j++) {
                for(int k = zMin; k <= zMax; k++) {
                    if(Vector3.Distance(new Vector3( i, j, k ), center) <= radius) {
                        if (cubesRegular[i, j, k] != null) {
                            DestroyImmediateCube(cubesRegular[i, j, k]);
                            cubesRegular[i, j, k] = null;
                        }
                    }
                }
            }
        }
        List<Cube> cubesToDestroy = new List<Cube>();
        foreach (Cube cube in cubesNonRegular) {
            if (Vector3.Distance(cube.transform.position, center) <= radius) {
                cubesToDestroy.Add(cube);
                break;
            }
        }
        foreach(Cube cubeToDestroy in cubesToDestroy) {
            cubesNonRegular.Remove(cubeToDestroy);
            DestroyImmediateCube(cubeToDestroy);
        }
    }

    public void DeleteCubesInBox(Vector3 center, Vector3 halfExtents) {
        int xMin = (int)Mathf.Ceil(center.x - halfExtents.x);
        int xMax = (int)Mathf.Floor(center.x + halfExtents.x);
        int yMin = (int)Mathf.Ceil(center.y - halfExtents.y);
        int yMax = (int)Mathf.Floor(center.y + halfExtents.y);
        int zMin = (int)Mathf.Ceil(center.z - halfExtents.z);
        int zMax = (int)Mathf.Floor(center.z + halfExtents.z);
        for (int i = xMin; i <= xMax; i++) {
            for (int j = yMin; j <= yMax; j++) {
                for (int k = zMin; k <= zMax; k++) {
                    if (cubesRegular[i, j, k] != null) {
                        DestroyImmediateCube(cubesRegular[i, j, k]);
                        cubesRegular[i, j, k] = null;
                    }
                }
            }
        }
        List<Cube> cubesToDestroy = new List<Cube>();
        foreach (Cube cube in cubesNonRegular) {
            Vector3 pos = cube.transform.position;
            if (Mathf.Abs(center.x - pos.x) <= halfExtents.x
             && Mathf.Abs(center.y - pos.y) <= halfExtents.y
             && Mathf.Abs(center.z - pos.z) <= halfExtents.z) {
                cubesToDestroy.Add(cube);
                break;
            }
        }
        foreach (Cube cubeToDestroy in cubesToDestroy)
        {
            cubesNonRegular.Remove(cubeToDestroy);
            DestroyImmediateCube(cubeToDestroy);
        }
    }

    public Cube GetCubeAt(Vector3 pos) {
        if (IsInRegularMap(pos) && MathTools.IsRounded(pos)) {
            return cubesRegular[(int)pos.x, (int)pos.y, (int)pos.z];
        } else {
            foreach (Cube cube in cubesNonRegular) {
                if (cube.transform.position == pos) {
                    return cube;
                }
            }
        }
        return null;
    }

    public List<Cube> GetCubesInSphere(Vector3 center, float radius) {
        List<Cube> cubes = new List<Cube>();
        int xMin = (int)Mathf.Floor(center.x - radius);
        int xMax = (int)Mathf.Ceil(center.x + radius);
        int yMin = (int)Mathf.Floor(center.y - radius);
        int yMax = (int)Mathf.Ceil(center.y + radius);
        int zMin = (int)Mathf.Floor(center.z - radius);
        int zMax = (int)Mathf.Ceil(center.z + radius);
        for (int i = xMin; i <= xMax; i++) {
            for (int j = yMin; j <= yMax; j++) {
                for (int k = zMin; k <= zMax; k++) {
                    Vector3 pos = new Vector3(i, j, k);
                    if (IsInRegularMap(pos) && Vector3.Distance(pos, center) <= radius) {
                        if (cubesRegular[i, j, k] != null) {
                            cubes.Add(cubesRegular[i, j, k]);
                        }
                    }
                }
            }
        }
        foreach (Cube cube in cubesNonRegular) {
            if (Vector3.Distance(cube.transform.position, center) <= radius) {
                cubes.Add(cube);
            }
        }
        return cubes;
    }

    public List<Cube> GetCubesInBox(Vector3 center, Vector3 halfExtents) {
        List<Cube> cubes = new List<Cube>();
        int xMin = (int)Mathf.Ceil(center.x - halfExtents.x);
        int xMax = (int)Mathf.Floor(center.x + halfExtents.x);
        int yMin = (int)Mathf.Ceil(center.y - halfExtents.y);
        int yMax = (int)Mathf.Floor(center.y + halfExtents.y);
        int zMin = (int)Mathf.Ceil(center.z - halfExtents.z);
        int zMax = (int)Mathf.Floor(center.z + halfExtents.z);
        for (int i = xMin; i <= xMax; i++) {
            for (int j = yMin; j <= yMax; j++) {
                for (int k = zMin; k <= zMax; k++) {
                    if (cubesRegular[i, j, k] != null) {
                        cubes.Add(cubesRegular[i, j, k]);
                    }
                }
            }
        }
        foreach (Cube cube in cubesNonRegular) {
            Vector3 pos = cube.transform.position;
            if (Mathf.Abs(center.x - pos.x) <= halfExtents.x
             && Mathf.Abs(center.y - pos.y) <= halfExtents.y
             && Mathf.Abs(center.z - pos.z) <= halfExtents.z)
            {
                cubes.Add(cube);
            }
        }
        return cubes;
    }

	// Crée un mur constitué de cubes entre les 4 coins que constitue les indices
	protected void RemplirFace(int indVertx1, int indVertx2, int indVertx3, int indVertx4, Vector3[] pos) {
		Vector3 depart = pos [indVertx1];
		//Vector3 arrivee = pos [indVertx4];
		Vector3 pas1 = (pos [indVertx2] - depart) / tailleMap;
		Vector3 pas2 = (pos [indVertx4] - depart) / tailleMap;

		for (int i = 0; i <= tailleMap; i++) {
			for (int j = 0; j <= tailleMap; j++) {
				Vector3 actuel = depart + pas1 * i + pas2 * j;
				GameObject instance = Instantiate (cubePrefab, actuel, Quaternion.identity) as GameObject;

				// On va un peu décaler les cubes pour créer du relief !
				//float decalageMax = personnage.GetComponent<CharacterController>().stepOffset / 2;
				float decalageMax = 0.1f;
				Vector3 directionDecalage = Vector3.Cross (pas1, pas2);
				directionDecalage.Normalize ();
                //instance.transform.Translate (directionDecalage * Random.Range (-decalageMax, decalageMax));

                AddCube(instance.GetComponent<Cube>());
			}
		}
	}

    public bool IsInRegularMap(Vector3 pos) {
        return 0 <= pos.x && pos.x <= tailleMap
        && 0 <= pos.y && pos.y <= tailleMap
        && 0 <= pos.z && pos.z <= tailleMap;
    }

    public List<Cube> GetAllCubes() {
        List<Cube> allCubes = cubesNonRegular;
        for (int i = 0; i <= tailleMap; i++)
            for (int j = 0; j <= tailleMap; j++)
                for (int k = 0; k <= tailleMap; k++)
                    if (cubesRegular[i, j, k] != null)
                        allCubes.Add(cubesRegular[i, j, k]);
        return allCubes;
    }
}
