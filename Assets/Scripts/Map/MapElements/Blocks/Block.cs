using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class Block : MonoBehaviour {

    public static int maxTimesCountForAveraging = 10;
    protected static float defaultAverageTime = 2.5f;
    protected static float timesExtremeBounds = 0.20f;
    protected static int minTimesCountForAveraging = 3;

    public Transform startPoint;
    public Transform endPoint;
    public BlockTriggerZone triggerZone;

    public Transform cubeFolder;
    public Transform lumiereFolder;
    public List<float> timesForFinishing; // { private get; set; }  // Don't use this directly ! Use GetTimeList() !
    public bool shouldPlayerPressShift = false;

    protected GameManager gm;
    protected MapManager map;
    protected List<Cube> cubes;
    protected List<Lumiere> lumieres;
    protected int nbLumieresToChose;
    protected int nbRandomLumieresToSpawn = 0;
    protected Block originalBlockPrefab;
    protected bool shouldNotifyToPressShift = false;
    protected bool shouldDestroyLumieresOnLeave = false;
    protected BoundingBox cubeBoundingBox;
    [HideInInspector]
    public UnityEvent<Block> onEnterBlock;
    [HideInInspector]
    public UnityEvent<Block> onLeaveBlock;

    public void Initialize(Transform blocksFolder, Block originalBlockPrefab, int nbLumieresToChose) {
        gm = GameManager.Instance;
        map = gm.map;
        this.originalBlockPrefab = originalBlockPrefab;
        this.nbLumieresToChose = nbLumieresToChose;
        //Debug.Log($"BLOCK = {name} -----------------");
        //StopwatchWrapper.Mesure(GatherCubes);
        //StopwatchWrapper.Mesure(RegisterCubesToMap);
        InitializeTriggerZone();
        GatherCubes();
        RegisterCubesToMap();
        if (gm.timerManager.HasGameStarted()) {
            RegisterCubesToColorSources();
        }
        //StopwatchWrapper.Mesure(StartSwappingCubes);
        InitializeLumieres();
        StartSwappingCubes();
    }

    private void InitializeTriggerZone() {
        this.triggerZone.Initialize();
        cubeBoundingBox = new BoundingBox(MathTools.RoundToInt(transform.position), MathTools.RoundToInt(transform.position));
    }

    protected void InitializeLumieres() {
        if(!gm.IsInitializationOver()) {
            gm.onInitilizationFinish.AddListener(InitializeLumieres); // We have to delay to let OverrideChrimasTree adjust nbLumieresToChose ! :3
            return;
        }
        GatherLumieres();
        InitializeChosenLumieres();
        DestroyNonChosenLumieres();
        SpawnRandomLumieres();
    }

    protected void GatherLumieres() {
        lumieres = new List<Lumiere>();
        if(!lumiereFolder) {
            return;
        }
        List<BlockLumiere> blockLumieres = new List<BlockLumiere>();
        foreach (Transform child in lumiereFolder) {
            BlockLumiere blockLumiere = child.gameObject.GetComponent<BlockLumiere>();
            blockLumiere.Initialize();
            if (blockLumiere.CanBePicked()) {
                blockLumieres.Add(blockLumiere);
            }
        }
        blockLumieres = GaussianGenerator.SelecteSomeNumberOf(blockLumieres, nbLumieresToChose);
        blockLumieres.ForEach(bl => lumieres.AddRange(bl.GetLumieres()));
    }

    protected void DestroyNonChosenLumieres() {
        if(!lumiereFolder) {
            return;
        }
        List<Lumiere> allLumieres = new List<Lumiere>();
        foreach (Transform child in lumiereFolder) {
            BlockLumiere blockLumiere = child.gameObject.GetComponent<BlockLumiere>();
            allLumieres.AddRange(blockLumiere.GetLumieres());
        }
        allLumieres.FindAll(l => !lumieres.Contains(l)).ForEach(l => Destroy(l.gameObject));
    }

    public void InitializeInReward(int nbData) {
        GatherCubesInReward();
        foreach(Cube cube in cubes) {
            cube.InitializeInReward();
        }
        // J'ai pas trouvé de solution pour le moment pour afficher les Lumieres dans le Reward !
        // Le problème vient du fait que pour calculer BlockLumiere.CanBePicked(), j'ai besoin d'avoir l'infiniteMap pour savoir la position des cubes
        // Et comme on est dans le reward, il n'y a pas de map.
        // De plus, le destroy détruit à la fin de la frame, donc je ne peux pas savoir si les cubes ont déjà été détruits ou pas.
        // Et le destroy immediate ne fonctionne pas car il peut être appelé dans une boucle de physique :D (ce qui n'est pas autorisé)
        // Ce qu'on pourrait faire à la limite c'est passer la liste des cubes du block actuel au CanBePicked et il se baserait là-dessus !
        // En vrai ça pourrait marcher ! :o
        nbLumieresToChose = nbData;
        lumieres = new List<Lumiere>();
        DestroyNonChosenLumieres();
        StartSwappingCubesInReward();
    }

    protected void InitializeChosenLumieres() {
        lumieres.ForEach(l => map.RegisterAlreadyExistingLumiere(l));
    }

    public void AddLumieres(int nbLumieresToAdd) {
        List<Lumiere> previousLumieres = lumieres.Select(l => l).ToList();
        nbLumieresToChose += nbLumieresToAdd;
        lumieres = new List<Lumiere>();
        if(!lumiereFolder) {
            return;
        }
        List<BlockLumiere> blockLumieres = new List<BlockLumiere>();
        foreach (Transform child in lumiereFolder) {
            BlockLumiere blockLumiere = child.gameObject.GetComponent<BlockLumiere>();
            blockLumiere.Initialize();
            if (blockLumiere.CanBePicked() && !blockLumiere.GetLumieres().Any(l => previousLumieres.Contains(l))) {
                blockLumieres.Add(blockLumiere);
            }
        }
        blockLumieres = GaussianGenerator.SelecteSomeNumberOf(blockLumieres, nbLumieresToAdd);
        blockLumieres.ForEach(bl => lumieres.AddRange(bl.GetLumieres()));
        InitializeChosenLumieres();
        lumieres.AddRange(previousLumieres);
        DestroyNonChosenLumieres();
    }

    public void RegisterCubesToColorSources() {
        foreach (Cube cube in cubes)
            cube.RegisterCubeToColorSources();
        gm.colorManager.EnsureNoCubeIsBlack(cubes);
    }

    private void RegisterCubesToMap() {
        for(int i = 0; i < cubes.Count; i++) {
            Cube cube = cubes[i];
            Cube newCube = map.RegisterAlreadyExistingCube(cube, cubeFolder);
            if(cube != newCube) {
                Destroy(cube.gameObject);
                cubes.RemoveAt(i);
                i--;
            }
        }
    }

    protected void GatherCubes() {
        cubes = new List<Cube>();
        foreach (Transform child in cubeFolder) {
            Cube cube = child.gameObject.GetComponent<Cube>();
            if (cube != null) {
                AddCube(cube);
            }

            RandomCubes randomCubes = child.gameObject.GetComponent<RandomCubes>();
            if (randomCubes != null)
                AddCubes(randomCubes.GetChosenCubesAndDestroyOthers());

            SwappyCubesHolderManager swappyCubesHolderManager = child.gameObject.GetComponent<SwappyCubesHolderManager>();
            if (swappyCubesHolderManager != null) {
                swappyCubesHolderManager.Initialize(gatherCubesInChildren: true);
                AddCubes(swappyCubesHolderManager.GetCubes());
            }
        }
    }

    protected void GatherCubesInReward() {
        cubes = new List<Cube>();
        foreach (Transform child in cubeFolder) {
            Cube cube = child.gameObject.GetComponent<Cube>();
            if (cube != null)
                AddCube(cube);

            RandomCubes randomCubes = child.gameObject.GetComponent<RandomCubes>();
            if (randomCubes != null)
                AddCubes(randomCubes.GetChosenCubesAndDestroyOthers());

            SwappyCubesHolderManager swappyCubesHolderManager = child.gameObject.GetComponent<SwappyCubesHolderManager>();
            if (swappyCubesHolderManager != null) {
                swappyCubesHolderManager.InitializeInReward(gatherCubesInChildren: true);
                AddCubes(swappyCubesHolderManager.GetCubes());
            }
        }
    }

    protected void AddCube(Cube cube) {
        cubes.Add(cube);
        cubeBoundingBox.ExtendTo(cube.transform.position);
    }

    protected void AddCubes(List<Cube> cubes) {
        cubes.ForEach(c => AddCube(c));
    }

    protected void StartSwappingCubes() {
        foreach (Transform child in cubeFolder) {
            SwappyCubesHolderManager swappyCubesHolderManager = child.gameObject.GetComponent<SwappyCubesHolderManager>();
            if (swappyCubesHolderManager != null) {
                swappyCubesHolderManager.SetCubesLinky(swappyCubesHolderManager.GetCurrentInterval(), useEnableState: false);
                swappyCubesHolderManager.StartSwapping();
            }
        }
    }

    protected void StartSwappingCubesInReward() {
        foreach (Transform child in cubeFolder) {
            SwappyCubesHolderManager swappyCubesHolderManager = child.gameObject.GetComponent<SwappyCubesHolderManager>();
            if (swappyCubesHolderManager != null) {
                swappyCubesHolderManager.SetCubesLinkyInReward(swappyCubesHolderManager.GetCurrentInterval(), useEnableState: false);
                swappyCubesHolderManager.StartSwapping();
            }
        }
    }

    public List<Cube> GetCubes() {
        return cubes;
    }

    public void Destroy(float speedDestruction, float dureeDecompose) {
        cubes = cubes.FindAll(c => c != null).ToList();
        cubes.Sort(delegate (Cube A, Cube B) {
            float distAToStart = Vector3.Distance(A.transform.position, endPoint.position);
            float distBToStart = Vector3.Distance(B.transform.position, endPoint.position);
            return distBToStart.CompareTo(distAToStart);
        });
        for(int i = 0; i < cubes.Count; i++) {
            Cube cube = cubes[i];
            float coef = (float)i / cubes.Count;
            float timeBeforeDecompose = coef * speedDestruction;
            cube.DecomposeIn(dureeDecompose, timeBeforeDecompose);
        }
        StartCoroutine(DestroyWhenAllCubesAreDestroyed());
    }

    private IEnumerator DestroyWhenAllCubesAreDestroyed() {
        while(!cubes.All(c => c == null)) {
            yield return null;
        }
        yield return new WaitForSeconds(1.0f); // Sécurité car on a besoin de cet objet pendant un peu plus longtemps !
        Destroy(gameObject);
    }

    protected List<float> GetTimeList() {
        return originalBlockPrefab.timesForFinishing;
    }

    public GameObject GetOriginalBlockPrefab() {
        return originalBlockPrefab.gameObject;
    }

    public void RememberTime(float time, CounterDisplayer nbBlocksDisplayer) {
        if (ShouldKeepRememberingTimes()) {
#if UNITY_EDITOR
            EditorUtility.SetDirty(originalBlockPrefab);
#endif
            GetTimeList().Add(time);
            nbBlocksDisplayer.AddVolatileText($"{time}s", Color.red);
            RemoveExtremeTimes();
        }
    }

    protected void RemoveExtremeTimes() {
        if (HasEnoughTimesForAveraging() && !ShouldKeepRememberingTimes()) {
            List<float> times = GetTimeList();
            float mean = GetAverageTime();
            times.RemoveAll(f => Mathf.Abs(mean - f) >= mean * timesExtremeBounds);
        }
    }

    public float GetAverageTime() {
        if (HasEnoughTimesForAveraging())
            return GetTimeList().Sum() / GetTimeList().Count;
        else
            return defaultAverageTime;
    }

    public bool HasEnoughTimesForAveraging() {
        return GetTimeList().Count >= minTimesCountForAveraging;
    }

    protected bool ShouldKeepRememberingTimes() {
        return GetTimeList().Count < maxTimesCountForAveraging;
    }

    public void ShouldNotifyPlayerHowToPressShift() {
        if (shouldPlayerPressShift) {
            shouldNotifyToPressShift = true;
        }
    }

    public void NotifyPlayerToPressShiftIfNeeded() {
        if(shouldPlayerPressShift && shouldNotifyToPressShift) {
            NotifyPlayerToPressShift();
        }
    }

    public void NotifyPlayerToPressShift() {
        gm.console.NotifyPlayerToPressShift();
        shouldNotifyToPressShift = false;
    }

    public static int GetTotalBlocksCrossed() {
        return PrefsManager.GetInt(PrefsManager.TOTAL_BLOCKS_CROSSED_KEY, 0);
    }

    public static void AddToTotalBlocksCrossed(int nbBlocksToAdd) {
        int newTotal = GetTotalBlocksCrossed() + nbBlocksToAdd;
        PrefsManager.SetInt(PrefsManager.TOTAL_BLOCKS_CROSSED_KEY, newTotal);
    }

    public bool HasData() {
        return lumieres.Count > 0;
    }

    public int GetNbData() {
        return lumieres.Count;
    }

    public void SetNbDataToChose(int nbDataPerBlock) {
        nbLumieresToChose = nbDataPerBlock;
    }

    protected void SpawnLumiereAt(Vector3 pos) {
        Lumiere lumiere = map.CreateLumiere(pos, Lumiere.LumiereType.NORMAL);
        lumieres.Add(lumiere);
    }

    protected void SpawnOneRandomLumiere() {
        for (int k = 0; k < 100; k++) {
            Vector3 pos = cubeBoundingBox.GetRandomPosition() + GetHalfOffset();
            if (map.IsFree(pos)) {
                SpawnLumiereAt(pos);
                break;
            }
        }
    }

    protected Vector3 GetHalfOffset() {
        /// NOT WORKING ! x)
        //return new Vector3(0.5f, 0.0f, 0.0f);
        //Vector3 pos = transform.position;
        Vector3 pos = cubes.First().transform.position;
        return pos - new Vector3(Mathf.Floor(pos.x), Mathf.Floor(pos.y), Mathf.Floor(pos.z));
    }

    protected void SpawnRandomLumieres() {
        for(int i = 0; i < nbRandomLumieresToSpawn; ++i) {
            SpawnOneRandomLumiere();
        }
    }

    public void SetShouldDestroyLumieresOnLeave() {
        onLeaveBlock.AddListener(block => DestroyAllLumieres());
    }

    protected void DestroyAllLumieres() {
        lumieres.FindAll(l => l).ForEach(l => l.DestroySmoothly());
    }

    public void SetNbRandomToSpawn(int quantity) {
        nbRandomLumieresToSpawn = quantity;
    }
}
