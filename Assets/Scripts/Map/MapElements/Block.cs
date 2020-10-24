using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Block : MonoBehaviour {
    public Vector3Int size;
    public Transform startPoint;
    public Transform endPoint;
    public Transform cubeFolder;
    public int minTimesForMin = 3;
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

    public void RememberTime(float time) {
        EditorUtility.SetDirty(originalBlockPrefab);
        GetTimeList().Add(time);
    }

    public float GetAverageTime() {
        if (GetTimeList().Count > 0) {
            float mean = 0;
            foreach (float time in GetTimeList())
                mean += time;
            mean /= GetTimeList().Count;
            return mean;
        }
        else
            return 2.5f;
    }

    public bool HasEnoughTimesForAveraging() {
        return GetTimeList().Count >= minTimesForMin;
    }
}
