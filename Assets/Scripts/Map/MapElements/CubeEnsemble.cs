using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class CubeEnsemble : MapElement {

    public enum CubeEnsembleType {
        CAVE,
        ARBRE,
        FULL_BLOCK,
        MAP_CONTAINER,
        MUR,
        PONT,
        SPIKES,
        SURFACE_INTERPOLANTE,
        POLYCUBE,
    };

    protected List<Cube> cubes;
    protected GameObject cubesEnsembleFolder;
    protected CubeEnsembleType cubeEnsembleType;
    public UnityEvent<Cube> onDeleteCube = new UnityEvent<Cube>();

    protected CubeEnsemble() : base() {
        cubes = new List<Cube>();
        cubesEnsembleFolder = new GameObject(GetName());
        cubesEnsembleFolder.transform.SetParent(map.cubesFolder.transform);
        InitializeCubeEnsembleType();
    }

    public abstract string GetName();

    protected Cube CreateCube(Vector3 pos) {
        Cube cube = map.AddCube(pos, map.GetCurrentCubeType(), Quaternion.identity, cubesEnsembleFolder.transform);
        if (cube != null) {
            cubes.Add(cube);
        }
        return cube;
    }

    public List<Cube> GetCubes() { return cubes; }

    public override void OnDeleteCube(Cube cube) {
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

    public CubeEnsembleType GetCubeEnsembleType() {
        return cubeEnsembleType;
    }

    protected abstract void InitializeCubeEnsembleType();
}
