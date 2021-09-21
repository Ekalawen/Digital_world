using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Node {
    public Vector3 pos;
    public float cout, distanceToGoal;
    public float heuristique;
    public Node parent;

    public Node(Vector3 pos, float cout, float distanceToGoal, Node parent) {
        this.pos = pos;
        this.cout = cout;
        this.distanceToGoal = distanceToGoal;
        this.heuristique = cout + distanceToGoal;
        this.parent = parent;
    }
}

public class MapManager : MonoBehaviour {

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

    public DissolveEffectType dissolveEffectType = DissolveEffectType.REGULAR_MAP;
    public Vector3Int tailleMap; // La taille de la map, en largeur, hauteur et profondeur
	public int nbLumieresInitial; // Le nombre de lumières lors de la création de la map


    protected Cube[,,] cubesRegular; // Toutes les positions entières dans [0, tailleMap]
    //protected List<Cube> cubesNonRegular; // Toutes les autres positions (non-entières ou en-dehors de la map)
    protected Octree<Cube> nonRegularOctree; // Toutes les autres positions (non-entières ou en-dehors de la map)
    [HideInInspector] public List<MapElement> mapElements;
    [HideInInspector] public List<DynamicCubeEnsemble> dynamicCubeEnsembles;
    [HideInInspector]
    protected List<Lumiere> lumieres;
    [HideInInspector]
    public Transform mapFolder, cubesFolder, lumieresFolder, zonesFolder, cubesPoolsFolder, lightningsFolder, particlesFolder;
    protected Cube.CubeType currentCubeTypeUsed = Cube.CubeType.NORMAL;
    [HideInInspector]
    public GameManager gm;
    protected PlayerStartComponent playerStartComponent = null;
    protected List<SwappyCubesHolderManager> swappyCubesHolderManagers;
    protected Dictionary<Cube.CubeType, Stack<Cube>> cubesPools;
    protected BoundingBox boundingBox; // La taille de la map minimum comprenant tous les cubes :) [inclusif, inclusif]


    public void Initialize() {
		// Initialisation
        gm = FindObjectOfType<GameManager>();
		name = "MapManager";
        mapFolder = new GameObject("Map").transform;
        cubesFolder = new GameObject("Cubes").transform;
        cubesFolder.transform.SetParent(mapFolder);
        lumieresFolder = new GameObject("Lumieres").transform;
        lumieresFolder.transform.SetParent(mapFolder);
        zonesFolder = new GameObject("Zones").transform;
        zonesFolder.transform.SetParent(mapFolder);
        cubesPoolsFolder = new GameObject("CubesPools").transform;
        cubesPoolsFolder.transform.SetParent(cubesFolder);
        lightningsFolder = new GameObject("Lightnings").transform;
        lightningsFolder.transform.SetParent(mapFolder);
        particlesFolder = new GameObject("Particles").transform;
        particlesFolder.transform.SetParent(mapFolder);
        InitPlayerStartComponent();
        mapElements = new List<MapElement>();
        dynamicCubeEnsembles = new List<DynamicCubeEnsemble>();
        lumieres = new List<Lumiere>();
        boundingBox = new BoundingBox(Vector3Int.zero, new Vector3Int(tailleMap.x, tailleMap.y, tailleMap.z));
        cubesRegular = new Cube[tailleMap.x + 1, tailleMap.y + 1, tailleMap.z + 1];
        for (int i = 0; i <= tailleMap.x; i++)
            for (int j = 0; j <= tailleMap.y; j++)
                for (int k = 0; k <= tailleMap.z; k++)
                    cubesRegular[i, j, k] = null;
        nonRegularOctree = new Octree<Cube>(cellSize: 8);
        swappyCubesHolderManagers = new List<SwappyCubesHolderManager>();
        cubesPools = new Dictionary<Cube.CubeType, Stack<Cube>>();

        // Récupérer tous les cubes et toutes les lumières qui pourraient déjà exister avant la création de la map !
        GetAllAlreadyExistingCubesAndLumieres();

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

    private Cube AddCube(Cube cube) {
        Vector3 pos = cube.transform.position;
        Cube cubeAtPos = GetCubeAt(pos);
        if (cube.transform.rotation == Quaternion.identity
         && IsInRegularMap(pos)
         && MathTools.IsRounded(pos)) {
            if (cubeAtPos == null) {
                cubesRegular[(int)pos.x, (int)pos.y, (int)pos.z] = cube;
                cube.Initialize();
                return cube;
            } else {
                return cubeAtPos;
            }
        } else {
            if (cubeAtPos == null) {
                if (MathTools.IsRounded(pos)) {
                    ExtendBoundingBox(pos);
                }
                nonRegularOctree.Add(cube);
                cube.SetRegularValue(false);
                cube.Initialize();
                return cube;
            } else {
                return cubeAtPos;
            }
        }
    }

    public Cube RegisterAlreadyExistingCube(Cube cube, Transform parent = null) {
        Cube cubeAtPos = GetCubeAt(cube.transform.position);
        if (cubeAtPos != null) {// Si il y a déjà un cube à cette position on renvoie ce cube !
            cube = cubeAtPos;
        }
        if (parent != null) {
            cube.transform.SetParent(parent);
        }
        return AddCube(cube);
    }

    public Cube AddCube(Vector3 pos, Cube.CubeType cubeType, Quaternion quaternion = new Quaternion(), Transform parent = null) {
        if (IsCubeAt(pos) || IsLumiereAt(pos) || gm.itemManager.IsItemAt(pos)) // Si il y a déjà un cube ou une lumière à cette position, on ne fait rien !
            return null;
        Transform newParent = parent ?? cubesFolder.transform;
        Cube cube = TryGetCubeFromPool(cubeType);
        if (cube != null) {
            cube.transform.position = pos;
            cube.transform.rotation = quaternion;
        } else {
            cube = Instantiate(GetPrefab(cubeType), pos, quaternion, newParent).GetComponent<Cube>();
        }
        AddCube(cube);
        return cube;
    }

    protected Cube TryGetCubeFromPool(Cube.CubeType cubeType) {
        if(!cubesPools.ContainsKey(cubeType)) {
            return null;
        }
        Stack<Cube> cubesPool = cubesPools[cubeType];
        if(cubesPool.Count == 0) {
            return null;
        }
        Cube cube = cubesPool.Pop();
        cube.gameObject.SetActive(true);
        cube.transform.SetParent(cubesFolder);
        return cube;
    }

    public void PoolAddCube(Cube.CubeType cubeType) {
        Cube cube = Instantiate(GetPrefab(cubeType), Vector3.zero, Quaternion.identity, cubesFolder).GetComponent<Cube>();
        // Don't initialize it, it will be when we will get it from the pool ! :)
        StoreCubeInPool(cube);
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

    private void DestroyImmediateCube(Cube cube) {
        if (cube == null)
            return;
        foreach(ColorSource colorSource in gm.colorManager.GetAllColorSources()) {
            colorSource.RemoveCube(cube);
        }
        foreach(MapElement mapElement in mapElements) {
            mapElement.OnDeleteCube(cube);
        }
        foreach(DynamicCubeEnsemble dynamicCubeEnsemble in dynamicCubeEnsembles) {
            dynamicCubeEnsemble.OnDeleteCube(cube);
        }
        StoreCubeInPool(cube);
    }

    protected void StoreCubeInPool(Cube cube) {
        if(!cubesPools.ContainsKey(cube.type)) {
            cubesPools[cube.type] = new Stack<Cube>();
        }
        cube.ResetBeforeStoring();
        cubesPools[cube.type].Push(cube);
        transform.SetParent(cubesPoolsFolder);
        cube.gameObject.SetActive(false);
    }

    public void DeleteCubesAt(Vector3 pos) {
        if(IsInRegularMap(pos) && MathTools.IsRounded(pos)) {
            DestroyImmediateCube(cubesRegular[(int)pos.x, (int)pos.y, (int)pos.z]);
            cubesRegular[(int)pos.x, (int)pos.y, (int)pos.z] = null;
        } else {
            Cube cubeToDestroy = nonRegularOctree.PopAt(pos);
            if(cubeToDestroy != null) {
                DestroyImmediateCube(cubeToDestroy);
            }
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

        List<Cube> cubesToDestroy = nonRegularOctree.GetInSphere(center, radius);
        foreach(Cube cubeToDestroy in cubesToDestroy) {
            nonRegularOctree.Remove(cubeToDestroy);
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

        List<Cube> cubesToDestroy = nonRegularOctree.GetInBox(center, halfExtents);
        foreach(Cube cubeToDestroy in cubesToDestroy) {
            nonRegularOctree.Remove(cubeToDestroy);
            DestroyImmediateCube(cubeToDestroy);
        }
    }

    public Cube GetCubeAt(Vector3 pos) {
        if (IsInRegularMap(pos) && MathTools.IsRounded(pos)) {
            return cubesRegular[(int)pos.x, (int)pos.y, (int)pos.z];
        } else {
            return nonRegularOctree.GetAt(pos);
        }
    }

    public Cube GetCubeAt(float x, float y, float z) {
        return GetCubeAt(new Vector3(x, y, z));
    }

    public bool IsCubeAt(Vector3 pos) {
        return GetCubeAt(pos) != null;
    }

    public bool IsEnabledCubeAt(Vector3 pos) {
        Cube cube = GetCubeAt(pos);
        return cube != null && cube.IsEnabled();
    }

    public bool IsCubeAt(float x, float y, float z) {
        return GetCubeAt(new Vector3(x, y, z));
    }

    public List<Cube> GetCubesInSphere(Vector3 center, float radius) {
        List<Cube> cubes = GetRegularCubesInSphere(center, radius);
        cubes.AddRange(nonRegularOctree.GetInSphere(center, radius));
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
        cubes.AddRange(nonRegularOctree.GetInBox(center, halfExtents));
        return cubes;
    }

    public List<Cube> GetCubesAtLessThanCubeDistance(Vector3 center, int cubeDistance) {
        List<Cube> cubesInRange = GetCubesInBox(center, Vector3.one * cubeDistance);
        cubesInRange = cubesInRange.FindAll(c => MathTools.CubeDistance(c.transform.position, center) <= cubeDistance);
        return cubesInRange;
    }

    public List<Cube> GetCubesInLocation(Vector3 roundedLocation) {
        List<Cube> cubes = new List<Cube>();
        roundedLocation = MathTools.Round(roundedLocation);
        Cube cubeRegular = null;
        if(IsInRegularMap(roundedLocation))
            cubeRegular = cubesRegular[(int)roundedLocation.x, (int)roundedLocation.y, (int)roundedLocation.z];
        if (cubeRegular != null)
            cubes.Add(cubeRegular);
        cubes.AddRange(nonRegularOctree.GetInSphere(roundedLocation, 0.5f + Mathf.Sqrt(2.0f) / 2.0f)); // Sphere de rayon une demi diagonale de carré ... ^^' Ca devrait juste être une box collision mais je préfère ne pas changer ça maintenant :)
        return cubes;
    }

    public Lumiere CreateLumiere(Vector3 pos, Lumiere.LumiereType type, bool dontRoundPositions = false) {
        // On arrondie les positions pour être à une valeur entière :)
        // C'EST TRES IMPORTANT QUE CES POSITIONS SOIENT ENTIERES !!! (pour vérifier qu'elles sont accessibles)
        if(!dontRoundPositions)
            pos = MathTools.Round(pos);

        if(GetCubeAt(pos) != null) {
            PosVisualisator.DrawCross(pos, Color.black);
            Debug.Log($"Une lumière est crée dans un cube ! x)");
        }

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

    public bool IsOnMapBordure(Vector3 pos) {
        return pos.x == 0 || pos.x == tailleMap.x
            || pos.y == 0 || pos.y == tailleMap.y
            || pos.z == 0 || pos.z == tailleMap.z;
    }

    public List<Vector3> GetAllPositionsOnBordures() {
        List<Vector3> positions = new List<Vector3>();
        positions.AddRange(GetAllPositionsOnBorduresHaut());
        positions.AddRange(GetAllPositionsOnBorduresBas());
        positions.AddRange(GetAllPositionsOnBorduresGauche());
        positions.AddRange(GetAllPositionsOnBorduresDroite());
        positions.AddRange(GetAllPositionsOnBorduresAvant());
        positions.AddRange(GetAllPositionsOnBorduresArriere());
        positions = MathTools.RemoveDoublons(positions);
        return positions;
    }

    public List<Vector3> GetAllPositionsOnBorduresHaut() {
        List<Vector3> positions = new List<Vector3>();
        for(int x = 0; x <= tailleMap.x; x++) {
            for(int z = 0; z <= tailleMap.z; z++) {
                positions.Add(new Vector3(x, tailleMap.y, z));
            }
        }
        return positions;
    }
    public List<Vector3> GetAllPositionsOnBorduresBas() {
        List<Vector3> positions = new List<Vector3>();
        for(int x = 0; x <= tailleMap.x; x++) {
            for(int z = 0; z <= tailleMap.z; z++) {
                positions.Add(new Vector3(x, 0, z));
            }
        }
        return positions;
    }
    public List<Vector3> GetAllPositionsOnBorduresGauche() {
        List<Vector3> positions = new List<Vector3>();
        for(int y = 0; y <= tailleMap.y; y++) {
            for(int z = 0; z <= tailleMap.z; z++) {
                positions.Add(new Vector3(0, y, z));
            }
        }
        return positions;
    }
    public List<Vector3> GetAllPositionsOnBorduresDroite() {
        List<Vector3> positions = new List<Vector3>();
        for(int y = 0; y <= tailleMap.y; y++) {
            for(int z = 0; z <= tailleMap.z; z++) {
                positions.Add(new Vector3(tailleMap.x, y, z));
            }
        }
        return positions;
    }
    public List<Vector3> GetAllPositionsOnBorduresAvant() {
        List<Vector3> positions = new List<Vector3>();
        for(int x = 0; x <= tailleMap.x; x++) {
            for(int y = 0; y <= tailleMap.y; y++) {
                positions.Add(new Vector3(x, y, tailleMap.z));
            }
        }
        return positions;
    }
    public List<Vector3> GetAllPositionsOnBorduresArriere() {
        List<Vector3> positions = new List<Vector3>();
        for(int x = 0; x <= tailleMap.x; x++) {
            for(int y = 0; y <= tailleMap.y; y++) {
                positions.Add(new Vector3(x, y, 0));
            }
        }
        return positions;
    }

    protected void ExtendBoundingBox(Vector3 position) {
        Vector3Int pos = MathTools.RoundToInt(position);
        if(pos.x < boundingBox.xMin) {
            boundingBox.xMin = pos.x;
        } else if (pos.x > boundingBox.xMax) {
            boundingBox.xMax = pos.x;
        } else if (pos.y < boundingBox.yMin) {
            boundingBox.yMin = pos.y;
        } else if (pos.y > boundingBox.yMax) {
            boundingBox.yMax = pos.y;
        } else if (pos.z < boundingBox.zMin) {
            boundingBox.zMin = pos.z;
        } else if (pos.z > boundingBox.zMax) {
            boundingBox.zMax = pos.z;
        }
    }

    public BoundingBox GetBoundingBox() {
        return boundingBox;
    }

    public bool IsOnBoundingBox(Vector3 pos) {
        BoundingBox boundingBox = GetBoundingBox();
        return pos.x == boundingBox.xMin || pos.x == boundingBox.xMax
            || pos.y == boundingBox.yMin || pos.y == boundingBox.yMax
            || pos.z == boundingBox.zMin || pos.z == boundingBox.zMax;
    }

    public bool IsInsideBoundingBox(Vector3 pos) {
        BoundingBox boundingBox = GetBoundingBox();
        return pos.x >= boundingBox.xMin || pos.x <= boundingBox.xMax
            || pos.y >= boundingBox.yMin || pos.y <= boundingBox.yMax
            || pos.z >= boundingBox.zMin || pos.z <= boundingBox.zMax;
    }

    public List<Vector3> GetAllPositionsOnBoundingBox() {
        List<Vector3> positions = new List<Vector3>();
        positions.AddRange(GetAllPositionsOnBoundingBoxHaut());
        positions.AddRange(GetAllPositionsOnBoundingBoxBas());
        positions.AddRange(GetAllPositionsOnBoundingBoxGauche());
        positions.AddRange(GetAllPositionsOnBoundingBoxDroite());
        positions.AddRange(GetAllPositionsOnBoundingBoxAvant());
        positions.AddRange(GetAllPositionsOnBoundingBoxArriere());
        positions = MathTools.RemoveDoublons(positions);
        return positions;
    }

    public List<Vector3> GetAllPositionsOnBoundingBoxHaut() {
        BoundingBox boundingBox = GetBoundingBox();
        List<Vector3> positions = new List<Vector3>();
        for(int x = boundingBox.xMin; x <= boundingBox.xMax; x++) {
            for(int z = boundingBox.zMin; z <= boundingBox.zMax; z++) {
                positions.Add(new Vector3(x, boundingBox.yMax, z));
            }
        }
        return positions;
    }
    public List<Vector3> GetAllPositionsOnBoundingBoxBas() {
        BoundingBox boundingBox = GetBoundingBox();
        List<Vector3> positions = new List<Vector3>();
        for(int x = boundingBox.xMin; x <= boundingBox.xMax; x++) {
            for(int z = boundingBox.zMin; z <= boundingBox.zMax; z++) {
                positions.Add(new Vector3(x, boundingBox.yMin, z));
            }
        }
        return positions;
    }
    public List<Vector3> GetAllPositionsOnBoundingBoxGauche() {
        BoundingBox boundingBox = GetBoundingBox();
        List<Vector3> positions = new List<Vector3>();
        for(int y = boundingBox.yMin; y <= boundingBox.yMax; y++) {
            for(int z = boundingBox.zMin; z <= boundingBox.zMax; z++) {
                positions.Add(new Vector3(boundingBox.xMin, y, z));
            }
        }
        return positions;
    }
    public List<Vector3> GetAllPositionsOnBoundingBoxDroite() {
        BoundingBox boundingBox = GetBoundingBox();
        List<Vector3> positions = new List<Vector3>();
        for(int y = boundingBox.yMin; y <= boundingBox.yMax; y++) {
            for(int z = boundingBox.zMin; z <= boundingBox.zMax; z++) {
                positions.Add(new Vector3(boundingBox.xMax, y, z));
            }
        }
        return positions;
    }
    public List<Vector3> GetAllPositionsOnBoundingBoxAvant() {
        BoundingBox boundingBox = GetBoundingBox();
        List<Vector3> positions = new List<Vector3>();
        for(int x = boundingBox.xMin; x <= boundingBox.xMax; x++) {
            for(int y = boundingBox.yMin; y <= boundingBox.yMax; y++) {
                positions.Add(new Vector3(x, y, boundingBox.zMax));
            }
        }
        return positions;
    }
    public List<Vector3> GetAllPositionsOnBoundingBoxArriere() {
        BoundingBox boundingBox = GetBoundingBox();
        List<Vector3> positions = new List<Vector3>();
        for(int x = boundingBox.xMin; x <= boundingBox.xMax; x++) {
            for(int y = boundingBox.yMin; y <= boundingBox.yMax; y++) {
                positions.Add(new Vector3(x, y, boundingBox.zMin));
            }
        }
        return positions;
    }

    public bool MoveCubeTo(Cube cube, Vector3 newPosition) {
        Vector3 oldPosition = cube.transform.position;
        Vector3 roundedOldPosition = MathTools.Round(oldPosition);
        Vector3 roundedNewPosition = MathTools.Round(newPosition);

        // On vérifie que l'on ne sera pas à moitié dans un autre cube
        Cube otherCube = GetCubeAt(roundedNewPosition);
        if(otherCube != null && otherCube != cube) {
            return false;
        }

        // On vérifie que l'on ne touchera pas du tout le prochain cube
        Vector3 mouvement = newPosition - oldPosition;
        Vector3 mouvementNormalized = mouvement.normalized;
        if(roundedNewPosition == roundedOldPosition && mouvement.magnitude <= 1) {
            // Si on sera "après" le "cube de départ"
            if(Vector3.Dot(newPosition, mouvementNormalized) > Vector3.Dot(roundedNewPosition, mouvementNormalized)) {
                otherCube = GetCubeAt(roundedNewPosition + mouvementNormalized);
                if(otherCube != null && otherCube != cube) {
                    newPosition = roundedNewPosition; // On se met alors pile au bon endroit :)
                    // C'est un peu fourbe puisque au lieu de dire que le mouvement n'a pas fonctionné, on l'ajuste comme on "imagine" que le voulait l'appelant, ce qui pourra être génant par la suite ^^
                }
            }
        }

        // On met à jour la position ! :)
        cube.transform.position = newPosition;
        if(MathTools.IsRounded(oldPosition) && IsInRegularMap(oldPosition)) {
            cubesRegular[(int)oldPosition.x, (int)oldPosition.y, (int)oldPosition.z] = null;
        } else {
            nonRegularOctree.Remove(cube);
        }
        if(MathTools.IsRounded(newPosition) && IsInRegularMap(newPosition)) {
            cubesRegular[(int)newPosition.x, (int)newPosition.y, (int)newPosition.z] = cube;
        } else {
            nonRegularOctree.Add(cube);
        }
        //cube.UpdateColorForNewPosition(oldPosition); // ==> Beaucoup trop coûteux en performances alors qu'on ne le remarque même pas ! :D
        if(gm.player.DoubleCheckInteractWithCube(cube)) {
            cube.InteractWithPlayer();
            if(mouvement.magnitude <= 1 && cube.ShouldPushPlayerWhenMoveAndInteractingWithHim()) {
                Vector3 contactPoint = MathTools.AABBPoint_ContactPoint(cube.transform.position, cube.VectorHalfExtent(), gm.player.transform.position);
                gm.player.transform.position = contactPoint + mouvementNormalized * gm.player.GetSizeRadius();
            }
        }
        return true;
    }

    public List<Cube> GetAllCubes() {
        List<Cube> allCubes = new List<Cube>();
        for (int i = 0; i <= tailleMap.x; i++)
            for (int j = 0; j <= tailleMap.y; j++)
                for (int k = 0; k <= tailleMap.z; k++)
                    if (cubesRegular[i, j, k] != null)
                        allCubes.Add(cubesRegular[i, j, k]);
        allCubes.AddRange(nonRegularOctree.GetAll());
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
        allCubes.AddRange(nonRegularOctree.GetAll().FindAll(c => c.type == type));
        return allCubes;
    }

    public void PrintCubesNumbers() {
        int nbCubesRegular = 0;
        for (int i = 0; i <= tailleMap.x; i++) {
            for (int j = 0; j <= tailleMap.y; j++) {
                for (int k = 0; k <= tailleMap.z; k++) {
                    if (cubesRegular[i, j, k] != null) {
                        nbCubesRegular++;
                        if(nonRegularOctree.Contains(cubesRegular[i, j, k])) {
                            Debug.Log("PROBLEME !");
                        }
                    }
                }
            }
        }
        Debug.Log("Nombre cubes regular = " + nbCubesRegular);
        int nbCubesNonRegularNonNull = nonRegularOctree.GetAll().FindAll(c => c != null).Count;
        Debug.Log("Nombre cubes non-regular = " + nonRegularOctree.Count);
        Debug.Log("Nombre cubes non-regular NULL = " + (nonRegularOctree.Count - nbCubesNonRegularNonNull));
        LocaliseCubeOnLumieres();
        PrintIfSeveralLumieresOnSamePos();
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
        while(IsCubeAt(center) && k <= kmax) {
            center = MathTools.Round(new Vector3(UnityEngine.Random.Range((float)offsetFromSides, (float)tailleMap.x - offsetFromSides),
                                     UnityEngine.Random.Range((float)offsetFromSides, (float)tailleMap.y - offsetFromSides),
                                     UnityEngine.Random.Range((float)offsetFromSides, (float)tailleMap.z - offsetFromSides)));
            k++;
        }
        if (k > kmax)
            Debug.LogError("ATTENTION ON A PAS TROUVE DE LOCATION !!!");
        return center;
    }

    public Vector3 GetFreeRoundedLocationWithoutLumiere(int offsetFromSides = 1) {
        Vector3 pos = GetFreeRoundedLocation(offsetFromSides);
        for(int k = 0; GetAllLumieresPositions().Contains(pos) && k < 1000; k++) {
            pos = GetFreeRoundedLocation(offsetFromSides);
        }
        return pos;
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

    public Vector3 GetHalfExtents() {
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

    protected void PrintIfSeveralLumieresOnSamePos() {
        int nbLumieres = lumieres.Count;
        int nbLumieresDistinctes = GetAllLumieresPositions().Distinct().Count();
        if(nbLumieres != nbLumieresDistinctes) {
            Debug.LogError($"Il y a {nbLumieres - nbLumieresDistinctes + 1} lumières au même endroit !");
        } else {
            Debug.Log($"Toutes les lumières sont uniques.");
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
            RelierChemin(pos, closestPosition, dontDestroyEnd: true);
        }
    }

	// Le but de cette fonction est de creuser un tunel allant de debutChemin a finChemin !
	public void RelierChemin(Vector3 debutChemin, Vector3 finChemin, bool dontDestroyEnd = false) {
        List<Vector3> path = GetStraitPath(debutChemin, finChemin, isDeterministic: false);
        path.Insert(0, debutChemin);
        if(dontDestroyEnd) {
            path.Remove(path.Last());
        }
        PosVisualisator.DrawPath(path, Color.blue);
        foreach(Vector3 pointActuel in path) {
            DeleteCube(GetCubeAt(pointActuel));
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
            List<Vector3> voisins = GetVoisinsLibresInMap(current);
            foreach (Vector3 v in voisins) {
                if(!open.Contains(v) && !closed.Contains(v))
                    open.Push(v);
            }
            closed.Push(current);
        }

        return new List<Vector3>(closed);
    }

    protected bool HasVoinsinsLibresMLap(Vector3 pos) {
        return GetVoisinsLibresInMap(pos).Count > 0;
    }

    public List<Vector3> GetVoisinsLibresInMap(Vector3 pos, bool ignoreSwappyCubes = false) {
        List<Vector3> res = new List<Vector3>();
        int i = (int)pos.x, j = (int)pos.y, k = (int)pos.z;
        // DROITE
        Vector3 adjacentPos = new Vector3(i + 1, j, k);
        if (IsInRegularMap(adjacentPos) && IsNullOrSwappy(adjacentPos, ignoreSwappyCubes))
            res.Add(adjacentPos);
        // GAUCHE
        adjacentPos = new Vector3(i - 1, j, k);
        if (IsInRegularMap(adjacentPos) && IsNullOrSwappy(adjacentPos, ignoreSwappyCubes))
            res.Add(adjacentPos);
        // HAUT
        adjacentPos = new Vector3(i, j + 1, k);
        if (IsInRegularMap(adjacentPos) && IsNullOrSwappy(adjacentPos, ignoreSwappyCubes))
            res.Add(adjacentPos);
        // BAS
        adjacentPos = new Vector3(i, j - 1, k);
        if (IsInRegularMap(adjacentPos) && IsNullOrSwappy(adjacentPos, ignoreSwappyCubes))
            res.Add(adjacentPos);
        // DEVANT
        adjacentPos = new Vector3(i, j, k + 1);
        if (IsInRegularMap(adjacentPos) && IsNullOrSwappy(adjacentPos, ignoreSwappyCubes))
            res.Add(adjacentPos);
        // DERRIRE
        adjacentPos = new Vector3(i, j, k - 1);
        if (IsInRegularMap(adjacentPos) && IsNullOrSwappy(adjacentPos, ignoreSwappyCubes))
            res.Add(adjacentPos);
        return res;
    }

    public bool IsNullOrSwappy(Vector3 pos, bool ignoreSwappyCubes) {
        Cube cube = GetCubeAt(pos);
        return cube == null || (ignoreSwappyCubes && cube.IsSwappy());
    }

    public List<Vector3> GetVoisinsLibresAll(Vector3 pos, bool ignoreSwappyCubes = false) {
        List<Vector3> res = new List<Vector3>();
        int i = (int)pos.x, j = (int)pos.y, k = (int)pos.z;
        // DROITE
        Vector3 adjacentPos = new Vector3(i + 1, j, k);
        if (!IsInRegularMap(adjacentPos) || IsNullOrSwappy(adjacentPos, ignoreSwappyCubes))
            res.Add(adjacentPos);
        // GAUCHE
        adjacentPos = new Vector3(i - 1, j, k);
        if (!IsInRegularMap(adjacentPos) || IsNullOrSwappy(adjacentPos, ignoreSwappyCubes))
            res.Add(adjacentPos);
        // HAUT
        adjacentPos = new Vector3(i, j + 1, k);
        if (!IsInRegularMap(adjacentPos) || IsNullOrSwappy(adjacentPos, ignoreSwappyCubes))
            res.Add(adjacentPos);
        // BAS
        adjacentPos = new Vector3(i, j - 1, k);
        if (!IsInRegularMap(adjacentPos) || IsNullOrSwappy(adjacentPos, ignoreSwappyCubes))
            res.Add(adjacentPos);
        // DEVANT
        adjacentPos = new Vector3(i, j, k + 1);
        if (!IsInRegularMap(adjacentPos) || IsNullOrSwappy(adjacentPos, ignoreSwappyCubes))
            res.Add(adjacentPos);
        // DERRIRE
        adjacentPos = new Vector3(i, j, k - 1);
        if (!IsInRegularMap(adjacentPos) || IsNullOrSwappy(adjacentPos, ignoreSwappyCubes))
            res.Add(adjacentPos);
        return res;
    }

    public List<Vector3> GetVoisinsPleinsInMap(Vector3 pos) {
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

    public List<Vector3> GetVoisinsPleinsAll(Vector3 pos) {
        List<Vector3> res = new List<Vector3>();
        float i = pos.x, j = pos.y, k = pos.z;
        // DROITE
        if (GetCubeAt(new Vector3(i + 1, j, k)) != null)
            res.Add(new Vector3(i + 1, j, k));
        // GAUCHE
        if (GetCubeAt(new Vector3(i - 1, j, k)) != null)
            res.Add(new Vector3(i - 1, j, k));
        // HAUT
        if (GetCubeAt(new Vector3(i, j + 1, k)) != null)
            res.Add(new Vector3(i, j + 1, k));
        // BAS
        if (GetCubeAt(new Vector3(i, j - 1, k)) != null)
            res.Add(new Vector3(i, j - 1, k));
        // DEVANT
        if (GetCubeAt(new Vector3(i, j, k + 1)) != null)
            res.Add(new Vector3(i, j, k + 1));
        // DERRIRE
        if (GetCubeAt(new Vector3(i, j, k - 1)) != null)
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
    public List<Vector3> GetVoisinsNoObstaclesInMap(Vector3 pos) {
        List<Vector3> res = new List<Vector3>();
        int i = (int)pos.x, j = (int)pos.y, k = (int)pos.z;
        // DROITE
        if(IsInRegularMap(new Vector3(i + 1, j, k)))
            res.Add(new Vector3(i + 1, j, k));
        // GAUCHE
        if(IsInRegularMap(new Vector3(i - 1, j, k)))
            res.Add(new Vector3(i - 1, j, k));
        // HAUT
        if(IsInRegularMap(new Vector3(i, j + 1, k)))
            res.Add(new Vector3(i, j + 1, k));
        // BAS
        if(IsInRegularMap(new Vector3(i, j - 1, k)))
            res.Add(new Vector3(i, j - 1, k));
        // DEVANT
        if(IsInRegularMap(new Vector3(i, j, k + 1)))
            res.Add(new Vector3(i, j, k + 1));
        // DERRIRE
        if(IsInRegularMap(new Vector3(i, j, k - 1)))
            res.Add(new Vector3(i, j, k - 1));
        return res;
    }

    protected List<Vector3Int> GetVoisinsLibresInt(Vector3Int pos) {
        List<Vector3> v3 = GetVoisinsLibresInMap(pos);
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

    public List<Vector3> GetNoObstaclePath(Vector3 start, Vector3 end, List<Vector3> posToDodge, bool bIsRandom = false) {
        start = MathTools.Round(start);
        end = MathTools.Round(end);

        List<Vector3> path = new List<Vector3>();
        Vector3 current = start;
        path.Add(current);
        if(posToDodge == null) {
            posToDodge = new List<Vector3>();
        }

        while(current != end) {
            List<Vector3> voisins = GetVoisinsNoObstacles(current);
            voisins = voisins.FindAll(v => !posToDodge.Contains(v));
            List<Vector3> nearestVoisins = new List<Vector3>();
            float currentDist = Vector3.Distance(current, end);
            foreach(Vector3 voisin in voisins) {
                if (Vector3.Distance(voisin, end) < currentDist) {
                    nearestVoisins.Add(voisin);
                }
            }
            if (bIsRandom) {
                MathTools.Shuffle(nearestVoisins);
            }
            current = nearestVoisins[0];
            path.Add(current);
        }

        return path;
    }

    public List<Vector3> GetPath(Vector3 start, Vector3 end, List<Vector3> posToDodge,
        bool bIsRandom = false,
        bool useNotInMapVoisins = false,
        bool collideWithCubes = true,
        bool ignoreSwappyCubes = false) {

        start = MathTools.Round(start);
        end = MathTools.Round(end);

        if (posToDodge.Contains(end) || (collideWithCubes && GetCubeAt(end) != null))
            return null;

        List<Node> closed = new List<Node>();
        List<Node> opened = new List<Node>();
        opened.Add(new Node(start, 0, Vector3.Distance(start, end), null));

        while(opened.Count > 0 && opened.Count + closed.Count <= GetVolume()) {
            Node current = opened.Last();
            opened.Remove(current);

            if (current.pos == end) {
                return ComputePathBackward(start, ref current);
            }

            List<Vector3> voisins = GetVoisinsLibreNotInPosToDodge(posToDodge, current, bIsRandom, useNotInMapVoisins, collideWithCubes, ignoreSwappyCubes);

            for (int i = 0; i < voisins.Count; i++) {
                Vector3 voisin = voisins[i];
                //float distanceToGoal = Vector3.Distance(voisin, end);
                float distanceToGoal = Vector3.SqrMagnitude(voisin - end);
                Node node = new Node(voisin, current.cout + 1, distanceToGoal, current);

                if (IsNodeInCloseOrOpened(closed, opened, voisin, node))
                    continue;

                InsertNodeInOpened(opened, node);
            }

            closed.Add(current);
        }

        Debug.Log("Path failed !!!!");
        return null;
    }

    public List<Vector3> GetStraitPath(Vector3 debutChemin, Vector3 finChemin, bool isDeterministic = false) {
        List<Vector3> path = new List<Vector3>();
		Vector3 pointActuel = debutChemin;
		while (pointActuel != finChemin) {
			// On liste les bonnes directions à prendre
			List<Vector3> directions = new List<Vector3>();
			if (pointActuel.x != finChemin.x) {
				if (pointActuel.x < finChemin.x) {
					directions.Add (new Vector3 (pointActuel.x + 1, pointActuel.y, pointActuel.z));
				} else {
					directions.Add (new Vector3 (pointActuel.x - 1, pointActuel.y, pointActuel.z));
				}
			}
			if (pointActuel.y != finChemin.y) {
				if (pointActuel.y < finChemin.y) {
					directions.Add (new Vector3 (pointActuel.x, pointActuel.y + 1, pointActuel.z));
				} else {
					directions.Add (new Vector3 (pointActuel.x, pointActuel.y - 1, pointActuel.z));
				}
			}
			if (pointActuel.z != finChemin.z) {
				if (pointActuel.z < finChemin.z) {
					directions.Add (new Vector3 (pointActuel.x, pointActuel.y, pointActuel.z + 1));
				} else {
					directions.Add (new Vector3 (pointActuel.x, pointActuel.y, pointActuel.z - 1));
				}
			}

            // On se déplace dans une bonne direction aléatoirement
            if (!isDeterministic) {
                pointActuel = directions[UnityEngine.Random.Range(0, directions.Count)];
            } else {
                pointActuel = directions[0];
            }
            path.Add(pointActuel);
		}
        return path;
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

    protected List<Vector3> GetVoisinsLibreNotInPosToDodge(List<Vector3> posToDodge, Node current,
        bool bIsRandom,
        bool useNotRegularVoisins,
        bool collideWithCubes = true,
        bool ignoreSwappyCubes = false) {
        List<Vector3> voisins = null;
        if (collideWithCubes) {
            voisins = (!useNotRegularVoisins) ? GetVoisinsLibresInMap(current.pos, ignoreSwappyCubes) : GetVoisinsLibresAll(current.pos, ignoreSwappyCubes);
        } else {
            voisins = (!useNotRegularVoisins) ? GetVoisinsNoObstaclesInMap(current.pos) : GetVoisinsNoObstacles(current.pos);
        }
        voisins = voisins.FindAll(v => !posToDodge.Contains(v));
        if (bIsRandom) {
            MathTools.Shuffle(voisins);
        }

        return voisins;
    }

    protected static List<Vector3> ComputePathBackward(Vector3 start, ref Node current) {
        List<Vector3> path = new List<Vector3>();
        while (current.pos != start) {
            path.Add(current.pos);
            current = current.parent;
        }
        path.Add(current.pos);
        path.Reverse();
        return path;
    }

    public List<Vector3> GetAllLumieresPositions() {
        return lumieres.Select(l => l.transform.position).ToList();
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
        return nonRegularOctree.GetAll();
    }

    public List<Vector3> GetAllNonRegularCubePos() {
        return GetAllNonRegularCubes().Select(c => c.transform.position).ToList();
    }

    protected void CreateRandomLumiere() {
        Vector3 posLumiere = GetFreeRoundedLocation();
        CreateLumiere(posLumiere, Lumiere.LumiereType.NORMAL);
    }

    protected void GetAllAlreadyExistingCubesAndLumieres() {
        Cube[] newCubes = FindObjectsOfType<Cube>();
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

    public List<Lumiere> GetLumieresFinalesAndAlmostFinales() {
        return lumieres.FindAll(l => l.type == Lumiere.LumiereType.FINAL || l.type == Lumiere.LumiereType.ALMOST_FINAL);
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

    public List<DynamicCubeEnsemble> GetAllDynamicCubeEnsembles() {
        return dynamicCubeEnsembles;
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
        if(!GetAllDirections().Contains(direction)) {
            throw new Exception($"direction ({direction}) must be a unitary direction in GetClosestCubeInDirectionRegular()");
        }
        Vector3 current = center + direction;
        int nbCubesMax = (int)Vector3.Distance(center, GetCenter()) + Mathf.Max(tailleMap.x, tailleMap.y, tailleMap.z);
        for(int i = 0; i < nbCubesMax; i++) {
            if (IsInRegularMap(current)) {
                Cube currentCube = cubesRegular[(int)current.x, (int)current.y, (int)current.z];
                if (currentCube != null)
                    return currentCube;
            }
            current += direction;
        }
        return null;
    }

    protected Cube GetClosestCubeInDirectionNonRegular(Vector3 center, Vector3 direction) {
        if (!GetAllDirections().Contains(direction)) {
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
        newCube.RegisterCubeToColorSources();
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

    public Cube[,,] GetRegularCubes() {
        return cubesRegular;
    }

    public int GetTailleMapAlongDirection(GravityManager.Direction direction) {
        if(direction == GravityManager.Direction.DROITE || direction == GravityManager.Direction.GAUCHE) {
            return tailleMap.x;
        } else if (direction == GravityManager.Direction.BAS || direction == GravityManager.Direction.HAUT) {
            return tailleMap.y;
        } else {
            return tailleMap.z;
        }
    }

    public int GetBoundingBoxSizeAlongDirection(GravityManager.Direction direction) {
        BoundingBox boundingBox = GetBoundingBox();
        if(direction == GravityManager.Direction.DROITE || direction == GravityManager.Direction.GAUCHE) {
            return boundingBox.xMax - boundingBox.xMin;
        } else if (direction == GravityManager.Direction.BAS || direction == GravityManager.Direction.HAUT) {
            return boundingBox.yMax - boundingBox.yMin;
        } else {
            return boundingBox.zMax - boundingBox.zMin;
        }
    }

    public bool IsInTranche(Vector3 pos, GravityManager.Direction fromDirection, int offset) {
        return GetTrancheIndice(pos, fromDirection) == offset;
    }

    public float GetTrancheIndice(Vector3 pos, GravityManager.Direction fromDirection) {
        Vector3 directionVecteur = GravityManager.DirToVec(fromDirection);
        if(fromDirection == GravityManager.Direction.BAS || fromDirection == GravityManager.Direction.DROITE || fromDirection == GravityManager.Direction.AVANT) {
            float trancheIndice = Vector3.Dot(pos, -directionVecteur);
            return trancheIndice;
        } else {
            int tailleMap = GetTailleMapAlongDirection(fromDirection);
            float trancheIndice = tailleMap - Vector3.Dot(pos, directionVecteur);
            return trancheIndice;
        }
    }

    public List<Vector3> GetAllEmptyPositionsInTranche(GravityManager.Direction fromDirection, int offset) {
        return GetAllEmptyPositions().FindAll(p => IsInTranche(p, fromDirection, offset));
    }

    public void SetDataQuality(Lumiere.LumiereQuality quality) {
        foreach(Lumiere lumiere in lumieres) {
            lumiere.SetLumiereQuality(quality);
        }
    }

    public List<Cube> GetSwappyCubes() {
        List<SwappyCubesHolderManager> swappyCubesHolderManagers = FindObjectsOfType<SwappyCubesHolderManager>().ToList();
        List<Cube> swappyCubes = swappyCubesHolderManagers.SelectMany(s => s.GetCubes()).ToList();
        return swappyCubes;
    }

    public void RegisterSwappyCubesHolderManager(SwappyCubesHolderManager swappyCubesHolderManager) {
        swappyCubesHolderManagers.Add(swappyCubesHolderManager);
    }
}
