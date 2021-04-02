using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwappyCubesHolder : MonoBehaviour {

    public static float SYNCHRONIZE_PERIODE = 0.15f;

    public float frequenceSwap = 1.5f;
    public float offsetSwap = 0.0f;
    public bool startVisible = true;

    protected GameManager gm;
    protected List<Cube> cubes;

    public void Initialize()
    {
        gm = GameManager.Instance;
        cubes = GatherCubes();
    }

    public void StartSwapping() {
        SetCubesLinky();
        StartCoroutine(CStartSwapping());
    }

    protected void SetCubesLinky() {
        foreach(Cube cube in cubes) {
            cube.SetLinky();
        }
    }

    protected List<Cube> GatherCubes() {
        List<Cube> cubes = new List<Cube>();
        foreach (Transform child in transform) {
            Cube cube = child.gameObject.GetComponent<Cube>();
            if (cube != null)
                cubes.Add(cube);
        }
        return cubes;
    }

    protected IEnumerator CStartSwapping() {
        bool isVisible = startVisible;
        SetCubesVisibleState(isVisible);

        //yield return new WaitForSeconds(offsetSwap + GetSynchronizeTime());
        //yield return new WaitForSeconds(offsetSwap);

        Timer timer = new Timer(frequenceSwap);
        timer.SetElapsedTime(offsetSwap);
        while(!gm.eventManager.IsGameOver()) {
            if(timer.IsOver()) {
                isVisible = !isVisible;
                SetCubesVisibleState(isVisible);
                timer.Reset();
            }
            yield return null;
        }
    }

    protected float GetSynchronizeTime() {
        float synchronizeTime = Timer.TimeToSynchronize(SYNCHRONIZE_PERIODE);
        return synchronizeTime;
    }

    protected void SetCubesVisibleState(bool visibleState) {
        cubes = cubes.FindAll(c => c != null);
        if(cubes.Count > 0) {
            cubes[0].SetEnableValue(visibleState);
        } else {
            Destroy(this);
        }
    }

    public List<Cube> GetCubes() {
        return cubes;
    }
}
