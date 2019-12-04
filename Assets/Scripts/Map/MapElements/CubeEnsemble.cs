using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeEnsemble : MapElement {

    protected List<Cube> cubes;

    protected CubeEnsemble() : base() {
        cubes = new List<Cube>();
    }

    protected Cube CreateCube(Vector3 pos) {
        Cube cube = map.AddCube(pos);
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
