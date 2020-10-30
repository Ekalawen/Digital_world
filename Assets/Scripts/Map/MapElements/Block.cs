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
    protected static int maxTimesCountForAveraging = 10;

    public Vector3Int size;
    public Transform startPoint;
    public Transform endPoint;
    public Transform cubeFolder;
    public List<float> timesForFinishing; // { private get; set; }  // Don't use this directly ! Use GetTimeList() !

    GameManager gm;
    MapManager map;
    List<Cube> cubes;
    Block originalBlockPrefab;


    public void Initialize(Transform blocksFolder, Block originalBlockPrefab) {
        gm = GameManager.Instance;
        map = gm.map;
        this.originalBlockPrefab = originalBlockPrefab;
        GatherCubes();
        AddCubesToMap();
        if(gm.timerManager.HasGameStarted()) {
            RegisterCubesToColorSources();
        }
    }

    public void RegisterCubesToColorSources() {
        foreach (Cube cube in cubes)
            cube.ShouldRegisterToColorSources();
        gm.colorManager.GenerateColorSourcesInCubes(cubes);
        gm.colorManager.CheckCubeSaturationInCubes(cubes);
    }

    private void AddCubesToMap() {
        for(int i = 0; i < cubes.Count; i++) {
            Cube cube = cubes[i];
            Vector3 cubePosition = cube.transform.position;
            Cube newCube = map.AddCube(cubePosition, cube.type, parent: cubeFolder);
            if(newCube == null) {
                cubes.RemoveAt(i);
                i--;
            } else {
                cubes[i] = newCube;
            }
            Destroy(cube.gameObject);
        }
    }

    protected void GatherCubes() {
        cubes = new List<Cube>();
        foreach (Transform child in cubeFolder) {
            Cube cube = child.gameObject.GetComponent<Cube>();
            if (cube != null)
                cubes.Add(cube);
        }
    }

    public List<Cube> GetCubes() {
        return cubes;
    }

    public void Destroy(float speedDestruction) {
        cubes.Sort(delegate (Cube A, Cube B) {
            float distAToStart = Vector3.Distance(A.transform.position, endPoint.position);
            float distBToStart = Vector3.Distance(B.transform.position, endPoint.position);
            return distBToStart.CompareTo(distAToStart);
        });
        for(int i = 0; i < cubes.Count; i++) {
            Cube cube = cubes[i];
            float coef = (float)i / cubes.Count;
            float timeBeforeExplosion = coef * speedDestruction;
            cube.ExplodeIn(timeBeforeExplosion);
        }
        StartCoroutine(DestroyWhenAllCubesAreDestroyed());
    }

    private IEnumerator DestroyWhenAllCubesAreDestroyed() {
        while(true) {
            yield return null;
            bool allNull = true;
            foreach(Cube cube in cubes) {
                if (cube != null) {
                    allNull = false;
                    break;
                }
            }
            if (allNull)
                break;
        }
        Destroy(gameObject);
    }

    protected List<float> GetTimeList() {
        return originalBlockPrefab.timesForFinishing;
    }

    public void RememberTime(float time, CounterDisplayer nbBlocksDisplayer) {
        if (ShouldKeepRememberingTimes()) {
            EditorUtility.SetDirty(originalBlockPrefab);
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
}
