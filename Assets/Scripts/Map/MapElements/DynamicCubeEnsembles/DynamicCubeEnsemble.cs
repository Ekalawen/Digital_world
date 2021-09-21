using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class DynamicCubeEnsemble : MonoBehaviour {

    public enum DynamicCubeEnsembleType {
        MOVING_SPIKE,
    };

    public Cube.CubeType cubeType = Cube.CubeType.NORMAL;

    protected GameManager gm;
    protected MapManager map;
    protected List<Cube> cubes;
    protected GameObject cubesEnsembleFolder;
    protected DynamicCubeEnsembleType cubeEnsembleType;
    [HideInInspector]
    public UnityEvent<Cube> onDeleteCube = new UnityEvent<Cube>();

    public virtual void Initialize() {
        gm = GameManager.Instance;
        map = gm.map;
        map.dynamicCubeEnsembles.Add(this);
        cubes = new List<Cube>();
        cubesEnsembleFolder = new GameObject(GetName());
        cubesEnsembleFolder.transform.SetParent(map.cubesFolder.transform);
        InitializeCubeEnsembleType();
    }

    public abstract string GetName();

    protected Cube CreateCube(Vector3 pos) {
        Cube cube = map.AddCube(pos, cubeType, Quaternion.identity, cubesEnsembleFolder.transform);
        if (cube != null) {
            cubes.Add(cube);
        }
        return cube;
    }

    public List<Cube> GetCubes() { return cubes; }

    public void OnDeleteCube(Cube cube) {
        if (cube != null && cubes.Contains(cube)) {
            onDeleteCube.Invoke(cube);
            cubes.Remove(cube);
        }
    }

    public void RegisterToColorSources() {
        foreach (Cube cube in cubes)
            cube.shouldRegisterToColorSources = true;
            //cube.RegisterCubeToColorSources();
    }

    public DynamicCubeEnsembleType GetCubeEnsembleType() {
        return cubeEnsembleType;
    }

    protected abstract void InitializeCubeEnsembleType();

    public void DestroyCubeEnsemble() {
        map.dynamicCubeEnsembles.Remove(this);
        foreach(Cube cube in cubes) {
            cube.Destroy();
        }
        Destroy(cubesEnsembleFolder);
        Destroy(gameObject);
    }
}
