using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwappyCubesHolder : MonoBehaviour {

    public float previsualisationDuration = 1f;
    public List<int> intervalToDisable;
    public List<int> intervalToEnable;

    protected GameManager gm;
    protected List<Cube> cubes;
    protected SwappyCubesHolderManager manager;


    public virtual void Initialize(SwappyCubesHolderManager manager, bool gatherCubesInChildren) {
        gm = GameManager.Instance;
        this.manager = manager;
        if (gatherCubesInChildren) {
            cubes = GatherCubes();
        }
    }

    public void SetCubesLinky(int initialInterval, Texture2D linkyTexture, bool useEnableState = false) {
        if (gm.GetMapType() == MenuLevel.LevelType.INFINITE && gm.timerManager.HasGameStarted()) {
            StartCoroutine(CSetCubesLinkyForIR(initialInterval, linkyTexture, useEnableState));
        } else {
            foreach (Cube cube in cubes) {
                cube.SetLinky(linkyTexture);
                cube.SetSwappy();
            }
            if (!useEnableState) {
                SetInitialVisibleState(initialInterval);
            }
        }
    }

    // Optimisation
    protected IEnumerator CSetCubesLinkyForIR(int initialInterval, Texture2D linkyTexture, bool useEnableState = false) {
        int nbCubesByFrame = 4;
        for(int i = 0; i < cubes.Count; i++) {
            Cube cube = cubes[i];
            cube.SetLinky(linkyTexture);
            cube.SetSwappy();
            if(i != 0 && i % nbCubesByFrame == 0) {
                yield return null;
            }
        }
        if (!useEnableState) {
            SetInitialVisibleState(initialInterval);
        }
    }

    protected List<Cube> GatherCubes() {
        List<Cube> cubes = new List<Cube>();
        foreach (Transform child in transform) {
            Cube cube = child.gameObject.GetComponent<Cube>();
            if (cube != null) {
                //cube.SetSwappy(); // Cette ligne pourrait être utile dans un autre niveau que SwapHighway !
                cubes.Add(cube);
            }
        }
        return cubes;
    }

    protected virtual void SetCubesVisibleState(bool visibleState, float prevDuration) {
        cubes = cubes.FindAll(c => c != null);
        if(cubes.Count > 0) {
            Cube mainCube = cubes[0];
            Vector3 impactPoint = mainCube.IsLinky() ? mainCube.GetLinkyCubeComponent().GetBarycentre() : mainCube.transform.position;
            mainCube.SetEnableValueIn(visibleState, prevDuration, impactPoint);
        } else {
            Destroy(this);
        }
    }

    public List<Cube> GetCubes() {
        return cubes;
    }

    public void NotifyNewInterval(int currentInterval) {
        if(intervalToDisable.Contains(currentInterval)) {
            SetCubesVisibleState(false, previsualisationDuration);
        } else if (intervalToEnable.Contains(currentInterval)) {
            SetCubesVisibleState(true, previsualisationDuration);
        }
    }

    public void SetCubes(List<Cube> cubesEnsemble) {
        cubes = cubesEnsemble;
    }

    public void SetInitialVisibleState(int initialInterval) {
        bool isVisible = GetIsVisibleStateFor(initialInterval);
        SetCubesVisibleState(isVisible, 0.0f);
    }

    public bool GetIsVisibleStateFor(int initialInterval) {
        int nbIntervals = manager.nbIntervals;
        List<Tuple<bool, int>> intervals = new List<Tuple<bool, int>>();
        intervals.AddRange(intervalToDisable.Select(i => new Tuple<bool, int>(false, (i + nbIntervals - (initialInterval + 1)) % nbIntervals)));
        intervals.AddRange(intervalToEnable.Select(i => new Tuple<bool, int>(true, (i + nbIntervals - (initialInterval + 1)) % nbIntervals)));
        return intervals.OrderByDescending(i => i.Item2).First().Item1;
    }
}
