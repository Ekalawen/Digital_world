using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwappyCubesHolder : MonoBehaviour {

    public float previsualisationDuration = 0.5f;
    public List<int> intervalToDisable;
    public List<int> intervalToEnable;

    protected GameManager gm;
    protected List<Cube> cubes;
    protected SwappyCubesHolderManager manager;


    public virtual void Initialize(SwappyCubesHolderManager manager) {
        gm = GameManager.Instance;
        this.manager = manager;
        cubes = GatherCubes();
    }

    public void SetCubesLinky(int initialInterval) {
        foreach(Cube cube in cubes) {
            cube.SetLinky();
        }
        SetInitialVisibleState(initialInterval);
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
