using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Node {
    public Vector3 pos;
    public float cout, heuristique;
    public Node parent;

    public Node(Vector3 pos, float cout, float heuristique, Node parent) {
        this.pos = pos;
        this.cout = cout;
        this.heuristique = heuristique;
        this.parent = parent;
    }
}

public class MapManager : MonoBehaviour {

	// public enum TypeMap {CUBE_MAP, PLAINE_MAP, LABYRINTHE_MAP, GROUND_MAP, EMPTY_MAP, TUTORIAL_MAP}; // Plus vraiment utile ! :D

    [Header("Cube Prefabs")]
	public GameObject cubePrefab; // On récupère ce qu'est un cube !
    public GameObject deathCubePrefab; // On récupère ce qu'est qu'un cube de la mort ! :)
	public GameObject indestructibleCubePrefab; // On récupère ce qu'est qu'un cube indestructible ! :)
	public GameObject brisableCubePrefab; // On récupère ce qu'est qu'un cube brisable ! :)
	public GameObject bouncyCubePrefab; // On récupère ce qu'est qu'un cube bouncy ! :)
    public GameObject transparentCubePrefab; // On récupère ce qu'est qu'un cube transparent ! :)
    public GameObject specialCubePrefab; // On récupère ce qu'est qu'un cube special ! :)

    [Header("Lumières Prefabs")]
	public GameObject lumierePrefab; // On récupère les lumières !
	public GameObject lumiereSpecialePrefab; // On récupère les lumières !
	public GameObject lumiereFinalePrefab; // On récupère les lumières finales !
	public GameObject lumiereAlmostFinalePrefab; // On récupère les lumières presque finales !

    [Header("Map Construction")]
    public List<MapFunctionComponent> mapFunctionComponents;

    public Vector3Int tailleMap; // La taille de la map, en largeur, hauteur et profondeur
	public int nbLumieresInitial; // Le nombre de lumières lors de la création de la map
    //public Transform alreadyExistingCubesFolder; // Le dossier contenant les cubes déjà existant !
    //public Transform alreadyExistingLumiereFolder; // Le dossier contenant les data déjà existant !


    protected Cube[,,] cubesRegular; // Toutes les positions entières dans [0, tailleMap]
    protected List<Cube> cubesNonRegular; // Toutes les autres positions (non-entières)
    [HideInInspector] public List<MapElement> mapElements;
    [HideInInspector]
    protected List<Lumiere> lumieres;
    [HideInInspector]
    public GameObject mapFolder, cubesFolder, lumieresFolder, zonesFolder;
    protected Cube.CubeType currentCubeTypeUsed = Cube.CubeType.NORMAL;
    [HideInInspector]
    public GameManager gm;
    protected PlayerStartComponent playerStartComponent = null;

	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

    public void Initialize() {
		// Initialisation
        gm = FindObjectOfType<GameManager>();
		name = "MapManager";
        mapFolder = new GameObject("Map");
        cubesFolder = new GameObject("Cubes");
        cubesFolder.transform.SetParent(mapFolder.transform);
        lumieresFolder = new GameObject("Lumieres");
        lumieresFolder.transform.SetParent(mapFolder.transform);
        zonesFolder = new GameObject("Zones");
        zonesFolder.transform.SetParent(mapFolder.transform);
        InitPlayerStartComponent();
        mapElements = new List<MapElement>();
        lumieres = new List<Lumiere>();
        cubesRegular = new Cube[tailleMap.x + 1, tailleMap.y + 1, tailleMap.z + 1];
        for (int i = 0; i <= tailleMap.x; i++)
            for (int j = 0; j <= tailleMap.y; j++)
                for (int k = 0; k <= tailleMap.z; k++)
                    cubesRegular[i, j, k] = null;
        cubesNonRegular = new List<Cube>();

        // Récupérer tous les cubes et toutes les lumières qui pourraient déjà exister avant la création de la map !
        GetAllAlreadyExistingCubesAndLumieres();

        //// Ici les classes qui hériteront de cette classe pourront faire leur génération !
        //// Mais ce n'est maintenant plus nécessaire, tout passe par les MapFunctionsComponents ! :)
        //GenerateMap();

        // Pour que les maps spéciales puissent faire leur initialisations spécifiques
        InitializeSpecific();

        // On rajoute les fonctions customs des components !
        ApplyAllMapFunctionsComponents();

        // Puis on affiche les proportions de cubes régular et non-regular pour vérifier que tout va bien :)
        PrintCubesNumbers();
    }

    public List<LumiereSwitchable> GetLumieresSwitchables() {
        return GetLumieres().FindAll(l => l.gameObject.GetComponent<LumiereSwitchable>() != null)
            .Select(l => (LumiereSwitchable)l).ToList();
    }

    protected virtual void InitializeSpecific() {
    }

    protected virtual void GenerateMap() {
    }

    protected virtual void Update() {
    }

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

    public Cube AddCube(Vector3 pos, Cube.CubeType cubeType, Quaternion quaternion = new Quaternion(), Transform parent = null) {
        if (GetCubeAt(pos) != null) // Si il y a déjà un cube à cette position, on ne fait rien !
            return null;
        Transform newParent = parent ?? cubesFolder.transform;
        Cube cube = Instantiate(GetPrefab(cubeType), pos, quaternion, newParent).GetComponent<Cube>();
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
            case Cube.CubeType.BOUNCY:
                return bouncyCubePrefab;
            case Cube.CubeType.BRISABLE:
                return brisableCubePrefab;
            case Cube.CubeType.TRANSPARENT:
                return transparentCubePrefab;
            case Cube.CubeType.SPECIAL:
                return specialCubePrefab;
        }
        return null;
    }

    private void DestroyImmediateCube(Cube cube, bool bJustInactive = false) {
        if (cube == null)
            return;
        foreach(ColorSource colorSource in gm.colorManager.GetAllColorSources()) {
            colorSource.RemoveCube(cube);
        }
        foreach(MapElement mapElement in mapElements) {
            mapElement.OnDeleteCube(cube);
        }
        if (bJustInactive)
            cube.gameObject.SetActive(false);
        else
            Destroy(cube.gameObject);
    }

    public void DeleteCubesAt(Vector3 pos, bool bJustInactive = false) {
        if(IsInRegularMap(pos) && MathTools.IsRounded(pos)) {
            DestroyImmediateCube(cubesRegular[(int)pos.x, (int)pos.y, (int)pos.z], bJustInactive);
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
            DestroyImmediateCube(cubeToDestroy, bJustInactive);
        }
    }

    public void DeleteCube(Cube cube, bool bJustInactive = false) {
        if(cube != null)
            DeleteCubesAt(cube.transform.position, bJustInactive);
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
                    Vector3 v = new Vector3(i, j, k);
                    if(Vector3.Distance(v, center) <= radius && IsInRegularMap(v)) {
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
                    if (IsInRegularMap(new Vector3(i, j, k))) {
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

    public Cube GetCubeAt(float x, float y, float z) {
        return GetCubeAt(new Vector3(x, y, z));
    }

    public List<Cube> GetCubesInSphere(Vector3 center, float radius) {
        List<Cube> cubes = GetRegularCubesInSphere(center, radius);
        foreach (Cube cube in cubesNonRegular) {
            if (Vector3.Distance(cube.transform.position, center) <= radius) {
                cubes.Add(cube);
            }
        }
        return cubes;
    }

    public List<Cube> GetRegularCubesInSphere(Vector3 center, float radius) {
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
                    if (IsInRegularMap(new Vector3(i, j, k)) && cubesRegular[i, j, k] != null) {
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

    public List<Cube> GetCubesInLocation(Vector3 roundedLocation) {
        List<Cube> cubes = new List<Cube>();
        roundedLocation = MathTools.Round(roundedLocation);
        Cube cubeRegular = null;
        if(IsInRegularMap(roundedLocation))
            cubeRegular = cubesRegular[(int)roundedLocation.x, (int)roundedLocation.y, (int)roundedLocation.z];
        if (cubeRegular != null)
            cubes.Add(cubeRegular);
        foreach (Cube cube in cubesNonRegular) {
            Vector3 pos = cube.transform.position;
            if(Vector3.Distance(pos, roundedLocation) <= 0.5 + Mathf.Sqrt(2.0f) / 2.0f) {
                cubes.Add(cube);
            }
        }
        return cubes;
    }

    public Lumiere CreateLumiere(Vector3 pos, Lumiere.LumiereType type, bool dontRoundPositions = false) {
        // On arrondie les positions pour être à une valeur entière :)
        // C'EST TRES IMPORTANT QUE CES POSITIONS SOIENT ENTIERES !!! (pour vérifier qu'elles sont accessibles)
        if(!dontRoundPositions)
            pos = MathTools.Round(pos);

        Lumiere lumiere = GameObject.Instantiate(GetPrefab(type), pos, Quaternion.identity, lumieresFolder.transform).GetComponent<Lumiere>();

        RegisterAlreadyExistingLumiere(lumiere);

        return lumiere;
    }

    public Lumiere RegisterAlreadyExistingLumiere(Lumiere lumiere) {
        lumieres.Add(lumiere);
        gm.historyManager.AddLumiereHistory(lumiere, lumiere.rewardLumierePrefab);
        return lumiere;
    }

    protected GameObject GetPrefab(Lumiere.LumiereType type) {
        switch(type)
        {
            case Lumiere.LumiereType.NORMAL:
                return lumierePrefab;
            case Lumiere.LumiereType.SPECIAL:
                return lumiereSpecialePrefab;
            case Lumiere.LumiereType.FINAL:
                return lumiereFinalePrefab;
            case Lumiere.LumiereType.ALMOST_FINAL:
                return lumiereAlmostFinalePrefab;
        }
        return null;
    }

    public bool IsInRegularMap(Vector3 pos) {
        return 0 <= pos.x && pos.x <= tailleMap.x
        && 0 <= pos.y && pos.y <= tailleMap.y
        && 0 <= pos.z && pos.z <= tailleMap.z;
    }

    public bool IsInInsidedRegularMap(Vector3 pos, int offsetFromSides = 1) {
        return offsetFromSides <= pos.x && pos.x <= tailleMap.x - offsetFromSides
        && offsetFromSides <= pos.y && pos.y <= tailleMap.y - offsetFromSides
        && offsetFromSides <= pos.z && pos.z <= tailleMap.z - offsetFromSides;
    }

    public List<Cube> GetAllCubes() {
        List<Cube> allCubes = new List<Cube>();
        for (int i = 0; i <= tailleMap.x; i++)
            for (int j = 0; j <= tailleMap.y; j++)
                for (int k = 0; k <= tailleMap.z; k++)
                    if (cubesRegular[i, j, k] != null)
                        allCubes.Add(cubesRegular[i, j, k]);
        foreach (Cube cube in cubesNonRegular)
            allCubes.Add(cube);
        return allCubes;
    }

    public List<Vector3> GetAllPositions() {
        List<Vector3> allPositions = new List<Vector3>();
        for (int i = 0; i <= tailleMap.x; i++)
            for (int j = 0; j <= tailleMap.y; j++)
                for (int k = 0; k <= tailleMap.z; k++)
                    allPositions.Add(new Vector3(i, j, k));
        return allPositions;
    }

    public List<Cube> GetAllCubesOfType(Cube.CubeType type) {
        List<Cube> allCubes = new List<Cube>();
        for (int i = 0; i <= tailleMap.x; i++)
            for (int j = 0; j <= tailleMap.y; j++)
                for (int k = 0; k <= tailleMap.z; k++)
                    if (cubesRegular[i, j, k] != null && cubesRegular[i, j, k].type == type)
                        allCubes.Add(cubesRegular[i, j, k]);
        foreach (Cube cube in cubesNonRegular) {
            if (cube.type == type)
                allCubes.Add(cube);
        }
        return allCubes;
    }

    public void PrintCubesNumbers() {
        int nbCubesRegular = 0;
        for (int i = 0; i <= tailleMap.x; i++) {
            for (int j = 0; j <= tailleMap.y; j++) {
                for (int k = 0; k <= tailleMap.z; k++) {
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
        LocaliseCubeOnLumieres();
    }

    public Vector3 GetRoundedLocation() {
        int x = UnityEngine.Random.Range(1, tailleMap.x);
        int y = UnityEngine.Random.Range(1, tailleMap.y);
        int z = UnityEngine.Random.Range(1, tailleMap.z);
        return new Vector3(x, y, z);
    }

    public Vector3 GetFreeSphereLocation(float radius) {
        Vector3 center = new Vector3(UnityEngine.Random.Range(1.0f, (float)tailleMap.x),
                                     UnityEngine.Random.Range(1.0f, (float)tailleMap.y),
                                     UnityEngine.Random.Range(1.0f, (float)tailleMap.z));
        int k = 0; int kmax = 5000;
        while(GetCubesInSphere(center, radius).Count > 0 && k <= kmax) {
            center = new Vector3(UnityEngine.Random.Range(1.0f, (float)tailleMap.x),
                                     UnityEngine.Random.Range(1.0f, (float)tailleMap.y),
                                     UnityEngine.Random.Range(1.0f, (float)tailleMap.z));
            k++;
        }
        if (k > kmax)
            Debug.LogError("ATTENTION ON A PAS TROUVE DE LOCATION !!!");
        return center;
    }

    public Vector3 GetFreeRoundedLocation(int offsetFromSides = 1) {
        Vector3 center = MathTools.Round(new Vector3(UnityEngine.Random.Range((float)offsetFromSides, (float)tailleMap.x - offsetFromSides),
                                     UnityEngine.Random.Range((float)offsetFromSides, (float)tailleMap.y - offsetFromSides),
                                     UnityEngine.Random.Range((float)offsetFromSides, (float)tailleMap.z - offsetFromSides)));
        int k = 0; int kmax = 5000;
        while(GetCubesInLocation(center).Count > 0 && k <= kmax) {
            center = MathTools.Round(new Vector3(UnityEngine.Random.Range((float)offsetFromSides, (float)tailleMap.x - offsetFromSides),
                                     UnityEngine.Random.Range((float)offsetFromSides, (float)tailleMap.y - offsetFromSides),
                                     UnityEngine.Random.Range((float)offsetFromSides, (float)tailleMap.z - offsetFromSides)));
            k++;
        }
        if (k > kmax)
            Debug.LogError("ATTENTION ON A PAS TROUVE DE LOCATION !!!");
        return center;
    }

    public Vector3 GetFreeBoxLocation(Vector3 halfExtents) {
        Vector3 center = new Vector3(UnityEngine.Random.Range(1.0f, (float)tailleMap.x),
                                     UnityEngine.Random.Range(1.0f, (float)tailleMap.y),
                                     UnityEngine.Random.Range(1.0f, (float)tailleMap.z));
        int k = 0; int kmax = 5000;
        while(GetCubesInBox(center, halfExtents).Count > 0 && k <= kmax) {
            center = new Vector3(UnityEngine.Random.Range(1.0f, (float)tailleMap.x),
                                     UnityEngine.Random.Range(1.0f, (float)tailleMap.y),
                                     UnityEngine.Random.Range(1.0f, (float)tailleMap.z));
            k++;
        }
        if (k > kmax)
            Debug.LogError("ATTENTION ON A PAS TROUVE DE LOCATION !!!");
        return center;
    }

    public virtual Vector3 GetCenter() {
        return new Vector3(tailleMap.x, tailleMap.y, tailleMap.z) / 2.0f;
    }

    protected void LocaliseCubeOnLumieres() {
        foreach(Lumiere lum in lumieres) {
            Cube cubeOn = GetCubeAt(lum.transform.position);
            if(cubeOn != null) {
                cubeOn.SetColor(Color.white);
                Debug.Log("Une lumiere dans un cube à la position " + cubeOn.transform.position + " !");
            }
        }
    }

    public List<Vector3> GetReachableArea() {
        // Trouver un point accessible
        Vector3 reachablePoint = MathTools.Round(GetFreeRoundedLocation());
        //Vector3 reachablePoint = MathTools.Round(GetFreeBoxLocation(Vector3.one * 1.0f));
        while(cubesRegular[(int)reachablePoint.x, (int)reachablePoint.y, (int)reachablePoint.z] != null) {
            reachablePoint = MathTools.Round(GetFreeBoxLocation(Vector3.one * 1.0f));
        }

        // Propager ce point à travers tout le niveau
        List<Vector3> reachableArea = PropagateInFreeSpace(reachablePoint);
        return reachableArea;
    }

    public void LinkPositionToReachableArea(Vector3 pos, List<Vector3> reachableArea) {
        if(!reachableArea.Contains(pos)) {
            // Les linker au reste
            // Trouver la case de la zone la plus proche d'elle
            Debug.Log("On tente de libérer une lumière inaccessible !");
            Vector3 closestPosition = reachableArea.Aggregate( // Tout est normal :)
                System.Tuple.Create(Vector3.zero, float.PositiveInfinity),
                delegate (System.Tuple<Vector3, float> best, Vector3 next) {
                    float dist = Vector3.Distance(pos, next);
                    return dist < best.Item2 ? System.Tuple.Create(next, dist) : best;
                },
                (result) => result.Item1);
            // Les relier ! :)
            Cave.RelierChemin(cubesRegular, this, pos, closestPosition);
        }
    }

    public void LinkUnreachableLumiereToRest() {
        List<Vector3> reachableArea = GetReachableArea();

        // Vérifier si les lumières sont dans cette zone, si elles ne le sont pas, elles sont inaccessibles
        foreach(Lumiere lumiere in GetLumieres()) {
            if (!MathTools.IsRounded(lumiere.transform.position))
                Debug.LogWarning("Attention une lumière n'est pas à une position entière ! Peut engendrer des bugs dans le Link !");
            LinkPositionToReachableArea(MathTools.Round(lumiere.transform.position), reachableArea);
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

    protected bool HasVoinsinsLibres(Vector3 pos) {
        return GetVoisinsLibres(pos).Count > 0;
    }

    public List<Vector3> GetVoisinsLibres(Vector3 pos) {
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

    public List<Vector3> GetVoisinsPleins(Vector3 pos) {
        List<Vector3> res = new List<Vector3>();
        int i = (int)pos.x, j = (int)pos.y, k = (int)pos.z;
        // DROITE
        if (IsInRegularMap(new Vector3(i + 1, j, k)) && cubesRegular[i + 1, j, k] != null)
            res.Add(new Vector3(i + 1, j, k));
        // GAUCHE
        if (IsInRegularMap(new Vector3(i - 1, j, k)) && cubesRegular[i - 1, j, k] != null)
            res.Add(new Vector3(i - 1, j, k));
        // HAUT
        if (IsInRegularMap(new Vector3(i, j + 1, k)) && cubesRegular[i, j + 1, k] != null)
            res.Add(new Vector3(i, j + 1, k));
        // BAS
        if (IsInRegularMap(new Vector3(i, j - 1, k)) && cubesRegular[i, j - 1, k] != null)
            res.Add(new Vector3(i, j - 1, k));
        // DEVANT
        if (IsInRegularMap(new Vector3(i, j, k + 1)) && cubesRegular[i, j, k + 1] != null)
            res.Add(new Vector3(i, j, k + 1));
        // DERRIRE
        if (IsInRegularMap(new Vector3(i, j, k - 1)) && cubesRegular[i, j, k - 1] != null)
            res.Add(new Vector3(i, j, k - 1));
        return res;
    }

    public List<Vector3> GetVoisinsNoObstacles(Vector3 pos) {
        List<Vector3> res = new List<Vector3>();
        int i = (int)pos.x, j = (int)pos.y, k = (int)pos.z;
        // DROITE
        res.Add(new Vector3(i + 1, j, k));
        // GAUCHE
        res.Add(new Vector3(i - 1, j, k));
        // HAUT
        res.Add(new Vector3(i, j + 1, k));
        // BAS
        res.Add(new Vector3(i, j - 1, k));
        // DEVANT
        res.Add(new Vector3(i, j, k + 1));
        // DERRIRE
        res.Add(new Vector3(i, j, k - 1));
        return res;
    }

    protected List<Vector3Int> GetVoisinsLibresInt(Vector3Int pos) {
        List<Vector3> v3 = GetVoisinsLibres(pos);
        List<Vector3Int> v3I = new List<Vector3Int>();
        foreach (Vector3 v in v3)
            v3I.Add(MathTools.RoundToInt(v));
        return v3I;
    }

    public List<Vector3> GetAllEmptyPositions() {
        List<Vector3> allPos = new List<Vector3>();
        for (int i = 0; i <= tailleMap.x; i++)
            for (int j = 0; j <= tailleMap.y; j++)
                for (int k = 0; k <= tailleMap.z; k++)
                    if (cubesRegular[i, j, k] == null)
                        allPos.Add(new Vector3(i, j, k));
        return allPos;
    }

    public float GetVolume() {
        return (float)(tailleMap.x - 1) * (tailleMap.y - 1) * (tailleMap.z - 1);
    }

    public bool IsLumiereAt(Vector3 pos) {
        foreach(Lumiere lumiere in lumieres) {
            if (lumiere.transform.position == pos)
                return true;
        }
        return false;
    }

    public List<Vector3> GetNoObstaclePath(Vector3 start, Vector3 end, bool bIsRandom = false) {
        start = MathTools.Round(start);
        end = MathTools.Round(end);

        List<Vector3> path = new List<Vector3>();
        Vector3 current = start;
        path.Add(current);

        while(current != end) {
            List<Vector3> voisins = GetVoisinsNoObstacles(current);
            List<Vector3> nearestVoisins = new List<Vector3>();
            float currentDist = Vector3.Distance(current, end);
            foreach(Vector3 voisin in voisins) {
                if (Vector3.Distance(voisin, end) < currentDist)
                    nearestVoisins.Add(voisin);
            }
            if (bIsRandom)
                MathTools.Shuffle(nearestVoisins);
            current = nearestVoisins[0];
            path.Add(current);
        }

        return path;
    }

    public List<Vector3> GetPath(Vector3 start, Vector3 end, List<Vector3> posToDodge, bool bIsRandom = false) {
        List<Vector3> path = new List<Vector3>();

        start = MathTools.Round(start);
        end = MathTools.Round(end);

        if (!IsInRegularMap(end) || posToDodge.Contains(end) || cubesRegular[(int)end.x, (int)end.y, (int)end.z] != null)
            return null;

        List<Node> closed = new List<Node>();
        List<Node> opened = new List<Node>();
        opened.Add(new Node(start, 0, 0, null));

        while(opened.Count > 0) {
            Node current = opened.Last();
            opened.Remove(current);

            if (current.pos == end) {
                return ComputePathBackward(start, path, ref current);
            }

            List<Vector3> voisins = GetVoisinsLibreNotInPosToDodge(posToDodge, current, bIsRandom);

            for (int i = 0; i < voisins.Count; i++)
            {
                Vector3 voisin = voisins[i];
                float distanceToGoal = Vector3.SqrMagnitude(voisin - end);
                Node node = new Node(voisin, current.cout + 1, current.cout + 1 + distanceToGoal, current);

                if(IsNodeInCloseOrOpened(closed, opened, voisin, node))
                    continue;

                InsertNodeInOpened(opened, node);
            }

            closed.Add(current);
        }

        Debug.Log("Path failed !!!!");
        return null;
    }

    private static bool IsNodeInCloseOrOpened(List<Node> closed, List<Node> opened, Vector3 voisin, Node node) {
        for (int j = 0; j < closed.Count; j++) {
            Node n = closed[j];
            if (n.pos == voisin)
                return true;
        }
        for (int j = 0; j < opened.Count; j++) {
            Node n = opened[j];
            if (n.pos == voisin) {
                if (n.heuristique > node.heuristique) {
                    opened.RemoveAt(j);
                    return false;
                } else {
                    return true;
                }
            }
        }
        return false;
    }

    protected static void InsertNodeInOpened(List<Node> opened, Node node) {
        for (int j = 0; j <= opened.Count; j++) {
            if (j == opened.Count) {
                opened.Add(node);
                break;
            } else if (opened[j].heuristique < node.heuristique) {
                opened.Insert(j, node);
                break;
            }
        }
    }

    protected List<Vector3> GetVoisinsLibreNotInPosToDodge(List<Vector3> posToDodge, Node current, bool bIsRandom) {
        List<Vector3> voisins = GetVoisinsLibres(current.pos);
        for (int i = 0; i < voisins.Count; i++) {
            if (posToDodge.Contains(voisins[i])) {
                voisins.RemoveAt(i);
                i--;
            }
        }
        if (bIsRandom)
            MathTools.Shuffle(voisins);

        return voisins;
    }

    protected static List<Vector3> ComputePathBackward(Vector3 start, List<Vector3> path, ref Node current) {
        while (current.pos != start) {
            path.Add(current.pos);
            current = current.parent;
        }
        path.Add(current.pos);
        path.Reverse();
        return path;
    }

    public List<Vector3> GetAllLumieresPositions() {
        List<Vector3> res = new List<Vector3>();
        foreach (Lumiere lumiere in lumieres)
            res.Add(lumiere.transform.position);
        return res;
    }

    public List<Cube> GetAllRegularCubes() {
        List<Cube> res = new List<Cube>();
        foreach(Cube cube in cubesRegular) {
            if (cube != null)
                res.Add(cube);
        }
        return res;
    }

    public List<Cube> GetAllNonRegularCubes() {
        List<Cube> res = new List<Cube>();
        foreach (Cube cube in cubesNonRegular)
            res.Add(cube);
        return res;
    }

    public List<Vector3> GetAllNonRegularCubePos() {
        List<Vector3> res = new List<Vector3>();
        foreach (Cube cube in cubesNonRegular)
            res.Add(cube.transform.position);
        return res;
    }

    protected void CreateRandomLumiere() {
        Vector3 posLumiere = GetFreeRoundedLocation();
        CreateLumiere(posLumiere, Lumiere.LumiereType.NORMAL);
    }

    protected void CreateRandomLumiereInCave() {
        List<Cave> caves = GetMapElementsOfType<Cave>();
        List<Cave> cavesGrandes = new List<Cave>();
        foreach(Cave cave in caves) {
            if (cave.nbCubesParAxe.x >= 3 && cave.nbCubesParAxe.y >= 3 && cave.nbCubesParAxe.z >= 3)
                cavesGrandes.Add(cave);
        }
        Cave chosenCave = cavesGrandes[UnityEngine.Random.Range(0, cavesGrandes.Count)];
        chosenCave.AddNLumiereInside(1);
    }

    //protected List<Cube> GetCubesRecursivelyInHierarchy(Transform folder) {
    //    List<Cube> newCubes = new List<Cube>();
    //    foreach(Transform t in folder) {
    //        Cube cube = t.GetComponent<Cube>();
    //        if(cube == null) {
    //            newCubes.AddRange(GetCubesRecursivelyInHierarchy(t));
    //        } else {
    //            newCubes.Add(cube);
    //        }
    //    }
    //    return newCubes;
    //}

    protected void GetAllAlreadyExistingCubesAndLumieres() {
        Cube[] newCubes = FindObjectsOfType<Cube>();
        //List<Cube> newCubes = GetCubesRecursivelyInHierarchy(alreadyExistingCubesFolder);
        foreach(Cube cube in newCubes) {
            if(!cube.transform.IsChildOf(cubesFolder.transform)) {
                Transform maxParent = cube.transform.parent;
                while (maxParent.parent != null)
                    maxParent = maxParent.parent;
                maxParent.SetParent(cubesFolder.transform);
            }
            AddCube(cube);
        }

        Lumiere[] newLumieres = FindObjectsOfType<Lumiere>();
        foreach(Lumiere lumiere in newLumieres) {
            if(!lumiere.transform.IsChildOf(lumieresFolder.transform)) {
                Transform maxParent = lumiere.transform.parent;
                while (maxParent.parent != null)
                    maxParent = maxParent.parent;
                maxParent.SetParent(lumieresFolder.transform);
            }
            RegisterAlreadyExistingLumiere(lumiere);
        }
    }

    public Cube.CubeType GetCurrentCubeType() {
        return currentCubeTypeUsed;
    }

    public void SetCurrentCubeType(Cube.CubeType newType) {
        currentCubeTypeUsed = newType;
    }

    public Vector3 GetFarRoundedLocation(Vector3 farFromThis) {
        Vector3 farPos = GetFreeRoundedLocation();

        // On évite que la lumière soit trop proche
        float moyenneTailleMap = (gm.map.tailleMap.x + gm.map.tailleMap.y + gm.map.tailleMap.z) / 3.0f;
        while (Vector3.Distance(farFromThis, farPos) <= moyenneTailleMap * 0.9f) {
            farPos = GetFreeRoundedLocation();
            moyenneTailleMap *= 0.95f; // Pour éviter qu'il n'y ait aucune zone atteignable x)
        }

        return farPos;
    }

    public List<Vector3> GetAllInsidedCorners() {
        List<Vector3> corners = new List<Vector3>();
        corners.Add(new Vector3(1, 1, 1));
        corners.Add(new Vector3(tailleMap.x - 1, 1, 1));
        corners.Add(new Vector3(1, tailleMap.y - 1, 1));
        corners.Add(new Vector3(1, 1, tailleMap.z - 1));
        corners.Add(new Vector3(tailleMap.x - 1, tailleMap.y - 1, 1));
        corners.Add(new Vector3(tailleMap.x - 1, 1, tailleMap.z - 1));
        corners.Add(new Vector3(1, tailleMap.y - 1, tailleMap.z - 1));
        corners.Add(new Vector3(tailleMap.x - 1, tailleMap.y - 1, tailleMap.z - 1));
        return corners;
    }

    public void InitPlayerStartComponent() {
        playerStartComponent = GetComponent<PlayerStartComponent>();
        if (playerStartComponent == null) {
            playerStartComponent = gameObject.AddComponent<PlayerStartComponent>();
        }
        playerStartComponent.Initialize();
    }

    public virtual Vector3 GetPlayerStartPosition() {
        return playerStartComponent.GetPlayerStartPosition();
    }

    public virtual Vector2 GetPlayerStartOrientationXY(Vector3 playerStartPosition) {
        return playerStartComponent.GetPlayerStartOrientationXY(playerStartPosition);
    }

    public List<Lumiere> GetLumieres() {
        return lumieres;
    }

    public void RemoveLumiere(Lumiere lumiere) {
        lumieres.Remove(lumiere);
    }

    // distanceMin valait avant minDistanceRandomFilling !
    public List<Vector3> GetFarAwayFromAllCubesPositions(float distanceMin) {
        List<Vector3> res = new List<Vector3>();
        foreach(Vector3 pos in GetAllEmptyPositions()) {
            List<Cube> nearCubes = GetCubesInSphere(pos, distanceMin);
            if (nearCubes.Count == 0)
                res.Add(pos);
        }
        return res;
    }

    public Vector3 GetFarFromEnsemble(List<Cube> farCubes, float minDistance) {
        while(true) {
            Vector3 pos = GetFreeRoundedLocation();
            List<float> distances = new List<float>();
            foreach (Cube cube in farCubes)
                distances.Add(Vector3.Distance(pos, cube.transform.position));
            if (distances.Min() >= minDistance)
                return pos;
        }
    }

    public List<Vector3> GetCouronne(int hauteur, int offsetSides) {
        List<Vector3> res = new List<Vector3>();
        for (int x = offsetSides; x <= tailleMap.x - offsetSides - 1; x++)
            res.Add(new Vector3(x, hauteur, offsetSides));
        for (int y = offsetSides; y <= tailleMap.y - offsetSides - 1; y++)
            res.Add(new Vector3(tailleMap.x - offsetSides, hauteur, y));
        for (int x = offsetSides + 1; x <= tailleMap.x - offsetSides; x++)
            res.Add(new Vector3(x, hauteur, tailleMap.y - offsetSides));
        for (int y = offsetSides + 1; y <= tailleMap.y - offsetSides; y++)
            res.Add(new Vector3(offsetSides, hauteur, y));
        return res;
    }

    public List<MapElement> GetMapElements() {
        return mapElements;
    }

    public List<T> GetMapElementsOfType<T>() where T : MapElement {
        List<T> elements = new List<T>();
        for(int i = 0; i < mapElements.Count; i++) {
            MapElement element = mapElements[i];
            if (element is T) {
                elements.Add((T)element);
            }
        }
        return elements;
    }

    public List<CubeEnsemble> GetCubeEnsembles() {
        return GetMapElementsOfType<CubeEnsemble>();
    }

    public List<CubeEnsemble> GetCubeEnsemblesOfType(CubeEnsemble.CubeEnsembleType cubeEnsembleType) {
        return GetCubeEnsembles().FindAll(ce => ce.GetCubeEnsembleType() == cubeEnsembleType);
    }

    protected void ApplyAllMapFunctionsComponents() {
        foreach (MapFunctionComponent function in mapFunctionComponents) {
            function.Initialize();
            function.Activate();
        }
    }


    public Cube GetFarestCubeFrom(Vector3 position) {
        List<Cube> cubes = GetAllCubes();
        return cubes.OrderBy(cube => Vector3.Distance(cube.transform.position, position)).Last();
    }

    public static List<Vector3> GetAllDirections() {
        List<Vector3> directions = new List<Vector3>() {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            Vector3.up,
            Vector3.down};
        return directions;
    }

    public Cube GetClosestCubeInDirection(Vector3 center, Vector3 direction) {
        center = MathTools.Round(center);
        Cube closestRegular = GetClosestCubeInDirectionRegular(center, direction);
        Cube closestNonRegular = GetClosestCubeInDirectionNonRegular(center, direction);
        if(closestRegular != null && closestNonRegular != null) {
            if (Vector3.Distance(center, closestRegular.transform.position) <= Vector3.Distance(center, closestNonRegular.transform.position))
                return closestRegular;
            return closestNonRegular;
        } else if (closestRegular != null) {
            return closestRegular;
        } else if (closestNonRegular != null) {
            return closestNonRegular;
        } else {
            return null;
        }
    }

    protected Cube GetClosestCubeInDirectionRegular(Vector3 center, Vector3 direction) {
        if (!IsInRegularMap(center))
            return null;
        if(!MapManager.GetAllDirections().Contains(direction)) {
            throw new Exception($"direction ({direction}) must be a unitary direction in GetClosestCubeInDirectionRegular()");
        }
        Vector3 current = center + direction;
        while(IsInRegularMap(current)) {
            Cube currentCube = cubesRegular[(int)current.x, (int)current.y, (int)current.z];
            if (currentCube != null)
                return currentCube;
            current += direction;
        }
        return null;
    }

    protected Cube GetClosestCubeInDirectionNonRegular(Vector3 center, Vector3 direction) {
        if (!MapManager.GetAllDirections().Contains(direction)) {
            throw new Exception($"direction ({direction}) must be a unitary direction in GetClosestCubeInDirectionNonRegular()");
        }
        List<Vector3> alignedPos = new List<Vector3>();
        foreach(Vector3 pos in GetAllNonRegularCubePos()) {
            Vector3 diff = pos - center;
            if((direction.x == 0) == (diff.x == 0)
            && (direction.y == 0) == (diff.y == 0)
            && (direction.z == 0) == (diff.z == 0)) {
                float projection = Vector3.Project(diff, direction).magnitude;
                if(Mathf.Sign(projection) > 0) {
                    if(Mathf.Round(projection) == projection) {
                        alignedPos.Add(pos);
                    }
                }
            }
        }
        if (alignedPos.Count == 0)
            return null;
        Vector3 closestAlignedPos = alignedPos.OrderBy(p => Vector3.SqrMagnitude(center - p)).First();
        return GetCubeAt(closestAlignedPos);
    }

    public Cube SwapCubeType(Cube cube, Cube.CubeType newType) {
        if (cube == null)
            return null;
        if (cube.type == newType)
            return cube;
        Vector3 position = cube.transform.position;
        Quaternion rotation = cube.transform.rotation;
        Transform parent = cube.transform.parent;
        DeleteCube(cube);
        Cube newCube = AddCube(position, newType, rotation, parent);
        newCube.ShouldRegisterToColorSources();
        return newCube;
    }

    public bool CubeFarEnoughtFromLumieres(Vector3 pos) {
        foreach(Lumiere lumiere in GetLumieres()) {
            if (MathTools.AABBSphere(pos, Vector3.one * 0.5f, lumiere.transform.position, lumiere.transform.localScale.x / 2.0f))
                return false;
        }
        return true;
    }

    public bool IsInInsidedVerticalEdges(Vector3 pos) {
        pos = MathTools.Round(pos);
        int nbEdges = 0;
        nbEdges += (pos.x == 1 || pos.x == tailleMap.x - 1) ? 1 : 0;
        nbEdges += (pos.z == 1 || pos.z == tailleMap.z - 1) ? 1 : 0;
        return pos.y != 0 && pos.y != tailleMap.y && nbEdges == 2;
    }
}
