using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomCubes : MonoBehaviour {

    public int nbToChose = 1;

    protected List<Cube> GatherCubes() {
        List<Cube> cubes = new List<Cube>();
        foreach (Transform child in transform) {
            Cube cube = child.gameObject.GetComponent<Cube>();
            if (cube != null)
                cubes.Add(cube);
        }
        return cubes;
    }

    protected List<RandomCubes> GatherRandomGroups() {
        List<RandomCubes> groups = new List<RandomCubes>();
        foreach (Transform child in transform) {
            RandomCubes group = child.gameObject.GetComponent<RandomCubes>();
            group.gameObject.SetActive(true);
            if (group != null)
                groups.Add(group);
        }
        return groups;
    }

    public List<Cube> GetChosenCubesAndDestroyOthers() {
        List<Cube> cubes = GatherCubes();
        if (cubes.Count > 0) {
            MathTools.Shuffle(cubes);
            List<Cube> chosen = cubes.Take(nbToChose).ToList();
            List<Cube> others = cubes.Skip(nbToChose).Take(cubes.Count - nbToChose).ToList();
            foreach (Cube cube in others)
                Destroy(cube.gameObject);
            return chosen;
        } else {
            List<RandomCubes> groups = GatherRandomGroups();
            MathTools.Shuffle(groups);
            List<RandomCubes> chosen = groups.Take(nbToChose).ToList();
            List<RandomCubes> others = groups.Skip(nbToChose).Take(groups.Count - nbToChose).ToList();
            foreach (RandomCubes otherGroup in others) {
                Destroy(otherGroup.gameObject);
            }
            return chosen.SelectMany(g => g.GetChosenCubesAndDestroyOthers()).ToList();
        }
    }
}
