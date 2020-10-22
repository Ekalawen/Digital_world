using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Block : MonoBehaviour {
    public Vector3Int size;
    public Transform startPoint;
    public Transform endPoint;
    public Transform cubeFolder;

    GameManager gm;
    MapManager map;
    List<Cube> cubes;

    public void Initialize(Transform blocksFolder) {
        gm = GameManager.Instance;
        map = gm.map;
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
            Destroy(cube.gameObject);
            cubes[i] = newCube;
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
}
