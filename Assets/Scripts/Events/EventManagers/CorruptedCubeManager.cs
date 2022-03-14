using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class CorruptedCubeManager : MonoBehaviour {

    protected GameManager gm;
    protected List<Cube> harmlessCubesOfCorruptedCubes = new List<Cube>();
    protected List<Cube> dangerousCubesOfCorruptedCubes = new List<Cube>();
    protected List<CorruptedCube> createdCorruptedCubeOfCorruptedCubes = new List<CorruptedCube>();
    protected float decomposeDuration;
    protected float progressiveDelayByDistance;

    public void Initialize() {
        gm = GameManager.Instance;
        decomposeDuration = gm.map.corruptedCubePrefab.GetComponent<CorruptedCube>().cancelCorruptionDecomposeDuration;
        progressiveDelayByDistance = gm.map.corruptedCubePrefab.GetComponent<CorruptedCube>().cancelCorruptionProgressiveDelayByDistance;
        gm.eventManager.onCaptureLumiere.AddListener(ClearCorruptionOfCorruptedCubes);
    }

    public void RegisterDangerousCubeOfCorruptedCube(Cube cube) {
        dangerousCubesOfCorruptedCubes.Add(cube);
    }

    public void RegisterHarmlessCubeOfCorruptedCube(Cube cube) {
        harmlessCubesOfCorruptedCubes.Add(cube);
    }

    public void ClearCorruptionOfCorruptedCubes(Lumiere fromLumiere) {
        StopCorruptionOfCorruptedCubes();
        DecomposeAllDangerousCubesOfCorruptedCubes(fromLumiere.transform.position);
        ReplaceHarmlessCubesByCorruptedCubes(fromLumiere.transform.position);
    }

    protected void DecomposeAllDangerousCubesOfCorruptedCubes(Vector3 origin) {
        foreach(Cube cube in dangerousCubesOfCorruptedCubes) {
            DecomposeCubeIn(cube, origin);
        }
        dangerousCubesOfCorruptedCubes.Clear();
        foreach(CorruptedCube cube in createdCorruptedCubeOfCorruptedCubes) {
            DecomposeCubeIn(cube, origin);
        }
        createdCorruptedCubeOfCorruptedCubes.Clear();
    }

    protected void ReplaceHarmlessCubesByCorruptedCubes(Vector3 origin) {
        foreach(Cube cube in harmlessCubesOfCorruptedCubes) {
            ReplaceByCorruptedCubeIn(cube, origin);
        }
        harmlessCubesOfCorruptedCubes.Clear();
    }

    protected void ReplaceByCorruptedCubeIn(Cube cube, Vector3 origin) {
        if (cube == null || cube.IsStored()) {
            return;
        }
        float timeToWait = GetTimeToWait(cube, origin);
        gm.map.SwapCubeTypeIn(cube, Cube.CubeType.CORRUPTED, timeToWait);
    }

    protected float GetTimeToWait(Cube cube, Vector3 origin) {
        return progressiveDelayByDistance * Vector3.Distance(origin, cube.transform.position);
    }

    protected void DecomposeCubeIn(Cube cube, Vector3 origin) {
        if(cube == null || cube.IsStored()) {
            return;
        }
        float timeToWait = GetTimeToWait(cube, origin);
        cube.DecomposeIn(decomposeDuration, timeToWait);
    }

    protected void StopCorruptionOfCorruptedCubes() {
        foreach(CorruptedCube corruptedCube in gm.map.GetAllCubesOfType<CorruptedCube>(Cube.CubeType.CORRUPTED)) {
            corruptedCube.CancelCorruption();
        }
    }

    public void RegisterCreatedCorruptedCube(CorruptedCube corruptedCube) {
        createdCorruptedCubeOfCorruptedCubes.Add(corruptedCube);
    }
}