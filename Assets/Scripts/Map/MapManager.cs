using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapManager : MonoBehaviour {

	// public enum TypeMap {CUBE_MAP, PLAINE_MAP, LABYRINTHE_MAP, GROUND_MAP, EMPTY_MAP, TUTORIAL_MAP}; // Plus vraiment utile ! :D

	public GameObject cubePrefab; // On récupère ce qu'est un cube !
	public GameObject deathCubePrefab; // On récupère ce qu'est qu'un cube de la mort ! :)
	public GameObject indestructibleCubePrefab; // On récupère ce qu'est qu'un cube indestructible ! :)
	public GameObject lumierePrefab; // On récupère les lumières !
	public GameObject lumiereFinalePrefab; // On récupère les lumières finales !
	public GameObject ennemiPrefabs; // On récupère un ennemi !

	public int tailleMap; // La taille de la map, en largeur, hauteur et profondeur

    protected Cube[,,] cubesRegular; // Toutes les positions entières dans [0, tailleMap]
    protected List<Cube> cubesNonRegular; // Toutes les autres positions (non-entières)
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
        PrintCubesNumbers();
        //LocaliseCubeOnLumieres();
        LinkUnreachableLumiereToRest();
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

    public Cube AddCube(Vector3 pos, Cube.CubeType cubeType, Quaternion quaternion = new Quaternion()) {
        if (GetCubeAt(pos) != null) // Si il y a déjà un cube à cette position, on ne fait rien !
            return null;
        Cube cube = Instantiate(GetPrefab(cubeType), pos, quaternion).GetComponent<Cube>();
        AddCube(cube);
        return cube;
    }

    protected GameObject GetPrefab(Cube.CubeType cubeType) {
        switch(cubeType) {
            case Cube.CubeType.NORMAL:
                return cubePrefab;
            case Cube.CubeType.DEATH:
                return deathCubePrefab;
            case Cube.CubeType.INDESTRUCTIBLE:
                return indestructibleCubePrefab;
        }
        return null;
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

    public Lumiere CreateLumiere(Vector3 pos, Lumiere.LumiereType type) {
        // On arrondie les positions pour être à une valeur entière :)
        // C'EST TRES IMPORTANT QUE CES POSITIONS SOIENT ENTIERES !!! (pour vérifier qu'elles sont accessibles)
        pos = MathTools.Round(pos);

        Lumiere lumiere = GameObject.Instantiate(GetPrefab(type), pos, Quaternion.identity).GetComponent<Lumiere>();
        lumieres.Add(lumiere);
        return lumiere;
    }

    protected GameObject GetPrefab(Lumiere.LumiereType type) {
        switch(type)
        {
            case Lumiere.LumiereType.NORMAL:
                return lumierePrefab;
            case Lumiere.LumiereType.FINAL:
                return lumiereFinalePrefab;
        }
        return null;
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
        List<Cube> allCubes = new List<Cube>();
        for (int i = 0; i <= tailleMap; i++)
            for (int j = 0; j <= tailleMap; j++)
                for (int k = 0; k <= tailleMap; k++)
                    if (cubesRegular[i, j, k] != null)
                        allCubes.Add(cubesRegular[i, j, k]);
        foreach (Cube cube in cubesNonRegular)
            allCubes.Add(cube);
        return allCubes;
    }

    public void PrintCubesNumbers() {
        int nbCubesRegular = 0;
        for (int i = 0; i <= tailleMap; i++) {
            for (int j = 0; j <= tailleMap; j++) {
                for (int k = 0; k <= tailleMap; k++) {
                    if (cubesRegular[i, j, k] != null) {
                        nbCubesRegular++;
                        if(cubesNonRegular.Contains(cubesRegular[i, j, k])) {
                            Debug.Log("PROBLEME !");
                        }
                    }
                }
            }
        }
        Debug.Log("Nombre cubes regular = " + nbCubesRegular);
        int nbCubesNonRegularNonNull = 0;
        foreach (Cube cube in cubesNonRegular)
            if (cube != null)
                nbCubesNonRegularNonNull++;
        Debug.Log("Nombre cubes non-regular = " + cubesNonRegular.Count);
        Debug.Log("Nombre cubes non-regular NULL = " + (cubesNonRegular.Count - nbCubesNonRegularNonNull));
    }

    public Vector3 GetFreeSphereLocation(float radius) {
        Vector3 center = new Vector3(Random.Range(1.0f, (float)tailleMap),
                                     Random.Range(1.0f, (float)tailleMap),
                                     Random.Range(1.0f, (float)tailleMap));
        int k = 0; int kmax = 5000;
        while(GetCubesInSphere(center, radius).Count > 0 && k <= kmax) {
            center = new Vector3(Random.Range(1.0f, (float)tailleMap),
                                     Random.Range(1.0f, (float)tailleMap),
                                     Random.Range(1.0f, (float)tailleMap));
            k++;
        }
        if (k > kmax)
            Debug.LogError("ATTENTION ON A PAS TROUVE DE LOCATION !!!");
        return center;
    }

    public Vector3 GetFreeBoxLocation(Vector3 halfExtents) {
        Vector3 center = new Vector3(Random.Range(1.0f, (float)tailleMap),
                                     Random.Range(1.0f, (float)tailleMap),
                                     Random.Range(1.0f, (float)tailleMap));
        int k = 0; int kmax = 5000;
        while(GetCubesInBox(center, halfExtents).Count > 0 && k <= kmax) {
            center = new Vector3(Random.Range(1.0f, (float)tailleMap),
                                     Random.Range(1.0f, (float)tailleMap),
                                     Random.Range(1.0f, (float)tailleMap));
            k++;
        }
        if (k > kmax)
            Debug.LogError("ATTENTION ON A PAS TROUVE DE LOCATION !!!");
        return center;
    }

    public Vector3 GetCenter() {
        return Vector3.one * tailleMap / 2.0f;
    }

    protected void LocaliseCubeOnLumieres() {
        foreach(Lumiere lum in lumieres) {
            Cube cubeOn = GetCubeAt(lum.transform.position);
            if(cubeOn != null) {
                cubeOn.SetColor(Color.white);
                Debug.Log("Une lumiere dans un cube !");
            }
        }
    }

    protected void LinkUnreachableLumiereToRest() {
        // Trouver un point accessible
        Vector3 reachablePoint = MathTools.Round(GetFreeBoxLocation(Vector3.one * 1.0f));
        while(cubesRegular[(int)reachablePoint.x, (int)reachablePoint.y, (int)reachablePoint.z] != null) {
            reachablePoint = MathTools.Round(GetFreeBoxLocation(Vector3.one * 1.0f));
        }

        // Propager ce point à travers tout le niveau
        List<Vector3> reachableArea = PropagateInFreeSpace(reachablePoint);

        // Vérifier si les lumières sont dans cette zone, si elles ne le sont pas, elles sont inaccessibles
        foreach(Lumiere lumiere in lumieres) {
            if(!reachableArea.Contains(lumiere.transform.position)) {
                // Les linker au reste
                // Trouver la case de la zone la plus proche d'elle
                Debug.Log("On tente de libérer une lumière inaccessible !");
                Vector3 posLumiere = lumiere.transform.position;
                Vector3 closestPosition = reachableArea.Aggregate( // Tout est normal :)
                    System.Tuple.Create(Vector3.zero, float.PositiveInfinity),
                    delegate (System.Tuple<Vector3, float> best, Vector3 next) {
                        float dist = Vector3.Distance(posLumiere, next);
                        return dist < best.Item2 ? System.Tuple.Create(next, dist) : best;
                    },
                    (result) => result.Item1);
                // Les relier ! :)
                Cave.RelierChemin(cubesRegular, this, posLumiere, closestPosition);
            }
        }

    }

    public List<Vector3> PropagateInFreeSpace(Vector3 startPos) {
        startPos = MathTools.Round(startPos);

        Stack<Vector3> open = new Stack<Vector3>();
        Stack<Vector3> closed = new Stack<Vector3>();

        open.Push(startPos);
        while(open.Count > 0) {
            Vector3 current = open.Pop();
            List<Vector3> voisins = GetVoisinsLibres(current);
            foreach (Vector3 v in voisins) {
                if(!open.Contains(v) && !closed.Contains(v))
                    open.Push(v);
            }
            closed.Push(current);
        }

        return new List<Vector3>(closed);
    }

    protected List<Vector3> GetVoisinsLibres(Vector3 pos) {
        List<Vector3> res = new List<Vector3>();
        int i = (int)pos.x, j = (int)pos.y, k = (int)pos.z;
        // DROITE
        if (IsInRegularMap(new Vector3(i + 1, j, k)) && cubesRegular[i + 1, j, k] == null)
            res.Add(new Vector3(i + 1, j, k));
        // GAUCHE
        if (IsInRegularMap(new Vector3(i - 1, j, k)) && cubesRegular[i - 1, j, k] == null)
            res.Add(new Vector3(i - 1, j, k));
        // HAUT
        if (IsInRegularMap(new Vector3(i, j + 1, k)) && cubesRegular[i, j + 1, k] == null)
            res.Add(new Vector3(i, j + 1, k));
        // BAS
        if (IsInRegularMap(new Vector3(i, j - 1, k)) && cubesRegular[i, j - 1, k] == null)
            res.Add(new Vector3(i, j - 1, k));
        // DEVANT
        if (IsInRegularMap(new Vector3(i, j, k + 1)) && cubesRegular[i, j, k + 1] == null)
            res.Add(new Vector3(i, j, k + 1));
        // DERRIRE
        if (IsInRegularMap(new Vector3(i, j, k - 1)) && cubesRegular[i, j, k - 1] == null)
            res.Add(new Vector3(i, j, k - 1));
        return res;
    }

    public List<Vector3> GetAllEmptyPositions() {
        List<Vector3> allPos = new List<Vector3>();
        for (int i = 0; i <= tailleMap; i++)
            for (int j = 0; j <= tailleMap; j++)
                for (int k = 0; k <= tailleMap; k++)
                    if (cubesRegular[i, j, k] == null)
                        allPos.Add(new Vector3(i, j, k));
        return allPos;
    }

    public float GetVolume() {
        return (float)(tailleMap - 1) * (tailleMap - 1) * (tailleMap - 1);
    }

    public bool IsLumiereAt(Vector3 pos) {
        foreach(Lumiere lumiere in lumieres) {
            if (lumiere.transform.position == pos)
                return true;
        }
        return false;
    }
}
