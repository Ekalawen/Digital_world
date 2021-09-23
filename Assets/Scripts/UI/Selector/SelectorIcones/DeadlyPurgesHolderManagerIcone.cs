using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeadlyPurgesHolderManagerIcone : MonoBehaviour {

    public float previsualisationDuration = 1.0f;
    public float dissolveDuration = 0.5f;
    public float mouvementDuration = 1.0f;
    public float decomposeDuration = 0.5f;
    public List<GameObject> cubes;
    public GameObject lightningPrefab;
    public Vector3 axis = Vector3.right;
    public float distance = 2.0f;

    protected List<Vector3> cubesInitialPositions;

    public void Start() {
        cubesInitialPositions = cubes.Select(c => c.transform.localPosition - axis * distance / 2).ToList();
        StartCoroutine(CStartSpikesLoop());
    }

    protected IEnumerator CStartSpikesLoop() {
        while(true) {
            yield return StartCoroutine(CStartSpikes());
        }
    }

    protected IEnumerator CStartSpikes() {
        for (int i = 0; i < cubes.Count; i++) {
            GameObject cube = cubes[i];
            cube.transform.localPosition = cubesInitialPositions[i];
            //Lightning lightning = Instantiate(lightningPrefab, parent: transform).GetComponent<Lightning>();
            //Vector3 start = cube.transform.position;
            //Vector3 end = cube.transform.position + axis * distance;
            //lightning.InitializeWithoutGM(start, end, parent: cube.transform.parent);
        }
        yield return new WaitForSeconds(previsualisationDuration);
        for (int i = 0; i < cubes.Count; i++) {
            GameObject cube = cubes[i];
            DissolveCube(cube, dissolveDuration);
        }
        yield return new WaitForSeconds(dissolveDuration);
        Timer timer = new Timer(mouvementDuration);
        while(!timer.IsOver()) {
            for (int i = 0; i < cubes.Count; i++) {
                GameObject cube = cubes[i];
                Vector3 start = cubesInitialPositions[i];
                Vector3 end = cubesInitialPositions[i] + axis * distance;
                cube.transform.localPosition = Vector3.Lerp(start, end, timer.GetAvancement());
            }
            yield return null;
        }
        foreach(GameObject cube in cubes) {
            DecomposeCube(cube, decomposeDuration);
        }
        yield return new WaitForSeconds(decomposeDuration);
    }

    protected void DecomposeCube(GameObject cube, float decomposeDuration) {
        Material mat = cube.GetComponent<Renderer>().material;
        mat.SetFloat("_DecomposeTime", decomposeDuration);
        mat.SetFloat("_DecomposeStartingTime", Time.time);
    }

    protected void DissolveCube(GameObject cube, float dissolveDuration) {
        Material mat = cube.GetComponent<Renderer>().material;
        mat.SetFloat("_DissolveTime", dissolveDuration);
        mat.SetFloat("_DissolveStartingTime", Time.time);
        mat.SetFloat("_DecomposeStartingTime", 999999f);
    }
}
