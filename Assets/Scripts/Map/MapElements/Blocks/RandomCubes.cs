using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomCubes : MonoBehaviour {

    public int nbCubesToChose = 1;

    protected List<Cube> GatherCubes() {
        List<Cube> cubes = new List<Cube>();
        foreach (Transform child in transform) {
            Cube cube = child.gameObject.GetComponent<Cube>();
            if (cube != null)
                cubes.Add(cube);
        }
        return cubes;
    }

    public List<Cube> GetChosenCubesAndDestroyOthers() {
        List<Cube> cubes = GatherCubes();
        MathTools.Shuffle(cubes);
        List<Cube> chosen = cubes.Take(nbCubesToChose).ToList();
        List<Cube> others = cubes.Skip(nbCubesToChose).Take(cubes.Count - nbCubesToChose).ToList();
        foreach(Cube cube in others)
            Destroy(cube.gameObject);
        return chosen;
    }
}
