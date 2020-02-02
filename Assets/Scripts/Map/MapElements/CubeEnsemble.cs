using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CubeEnsemble : MapElement {

    protected List<Cube> cubes;
    protected GameObject cubesEnsembleFolder;

    protected CubeEnsemble() : base() {
        cubes = new List<Cube>();
        cubesEnsembleFolder = new GameObject(GetName());
        cubesEnsembleFolder.transform.SetParent(map.cubesFolder.transform);
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
        cubes.Remove(cube);
    }
}
