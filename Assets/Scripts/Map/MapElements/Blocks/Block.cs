using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Block : MonoBehaviour {

    protected static float defaultAverageTime = 2.5f;
    protected static float timesExtremeBounds = 0.20f;
    protected static int minTimesCountForAveraging = 3;
    public static int maxTimesCountForAveraging = 10;

    public Transform startPoint;
    public Transform endPoint;
    public Transform cubeFolder;
    public List<float> timesForFinishing; // { private get; set; }  // Don't use this directly ! Use GetTimeList() !
    public bool shouldPlayerPressShift = false;

    protected GameManager gm;
    protected MapManager map;
    protected List<Cube> cubes;
    protected Block originalBlockPrefab;
    protected bool shouldNotifyToPressShift = false;


    public void Initialize(Transform blocksFolder, Block originalBlockPrefab) {
        gm = GameManager.Instance;
        map = gm.map;
        this.originalBlockPrefab = originalBlockPrefab;
        GatherCubes();
        RegisterCubesToMap();
        if(gm.timerManager.HasGameStarted()) {
            RegisterCubesToColorSources();
        }
        StartSwappingCubes();
    }

    public void RegisterCubesToColorSources() {
        foreach (Cube cube in cubes)
            cube.ShouldRegisterToColorSources();
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
            if (cube != null)
                cubes.Add(cube);
            RandomCubes randomCubes = child.gameObject.GetComponent<RandomCubes>();
            if (randomCubes != null)
                cubes.AddRange(randomCubes.GetChosenCubesAndDestroyOthers());
            SwappyCubesHolderManager swappyCubesHolderManager = child.gameObject.GetComponent<SwappyCubesHolderManager>();
            if (swappyCubesHolderManager != null) {
                swappyCubesHolderManager.Initialize();
                cubes.AddRange(swappyCubesHolderManager.GetCubes());
            }
        }
    }

    protected void StartSwappingCubes() {
        foreach (Transform child in cubeFolder) {
            SwappyCubesHolderManager swappyCubesHolderManager = child.gameObject.GetComponent<SwappyCubesHolderManager>();
            if (swappyCubesHolderManager != null) {
                swappyCubesHolderManager.StartSwapping();
            }
        }
    }

    public List<Cube> GetCubes() {
        return cubes;
    }

    public void Destroy(float speedDestruction, float dureeDecompose) {
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
}
