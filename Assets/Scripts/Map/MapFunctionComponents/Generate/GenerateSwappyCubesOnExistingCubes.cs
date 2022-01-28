using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateSwappyCubesOnExistingCubes : MapFunctionComponent {

    public GetCubesHelper getCubesHelper;
    public float frequenceInterval = 1.5f;
    public int nbIntervals = 2;
    public Vector2Int startIntervalRange = new Vector2Int(0, 1);
    public bool useRandomStartIntervalForHolders = true;
    public bool setLinky = true;
    public float previsualisationDuration = 1f;
    public float timeBeforeStartSwapping = 3.0f;

    protected GameObject swappyManagerFolder;
    protected List<SwappyCubesHolder> holders;
    protected SwappyCubesHolderManager swappyCubesHolderManager;

    public override void Activate() {
        GenerateFolder();
        List<List<Cube>> cubesEnsembles = GetCubesEnsembles();
        holders = GenerateSwappyCubesHolders(cubesEnsembles);
        swappyCubesHolderManager = GenerateSwappyCubesHolderManager();
        StartCoroutine(CStartSwappingIn());
    }

    protected void GenerateFolder() {
        swappyManagerFolder = new GameObject("SwappyCubesHolderManager");
        swappyManagerFolder.transform.SetParent(map.cubesFolder.transform);
    }

    protected List<SwappyCubesHolder> GenerateSwappyCubesHolders(List<List<Cube>> cubesEnsembles) {
        List<SwappyCubesHolder> swappyCubesHolders = new List<SwappyCubesHolder>();

        foreach(List<Cube> cubesEnsemble in cubesEnsembles) {
            GameObject holderFolder = new GameObject("SwappyCubesHolder");
            holderFolder.transform.SetParent(swappyManagerFolder.transform);
            SwappyCubesHolder swappyCubesHolder = holderFolder.AddComponent<SwappyCubesHolder>();
            swappyCubesHolder.previsualisationDuration = previsualisationDuration;
            int startInterval = useRandomStartIntervalForHolders ? UnityEngine.Random.Range(0, nbIntervals) : 0;
            swappyCubesHolder.intervalToEnable = new List<int>() { startInterval };
            swappyCubesHolder.intervalToDisable = new List<int>() { (startInterval + nbIntervals / 2) % nbIntervals };
            swappyCubesHolders.Add(swappyCubesHolder);
            swappyCubesHolder.SetCubes(cubesEnsemble);
        }

        return swappyCubesHolders;
    }

    protected SwappyCubesHolderManager GenerateSwappyCubesHolderManager() {
        SwappyCubesHolderManager swappyCubesHolderManager = swappyManagerFolder.AddComponent<SwappyCubesHolderManager>();
        swappyCubesHolderManager.frequenceInterval = frequenceInterval;
        swappyCubesHolderManager.nbIntervals = nbIntervals;
        swappyCubesHolderManager.startIntervalRange = startIntervalRange;
        swappyCubesHolderManager.setLinky = setLinky;
        swappyCubesHolderManager.Initialize(gatherCubesInChildren: false);
        return swappyCubesHolderManager;
    }

    protected List<List<Cube>> GetCubesEnsembles() {
        List<List<Cube>> cubesEnsembles = new List<List<Cube>>();
        List<Cube> cubes = getCubesHelper.Get();
        foreach(Cube cube in cubes) {
            bool added = false;
            foreach(List<Cube> cubesEnsemble in cubesEnsembles) {
                if(cubesEnsemble.Any(c => MathTools.IsAdjacent(c, cube))) {
                    cubesEnsemble.Add(cube);
                    added = true;
                    break;
                }
            }
            if(!added) {
                cubesEnsembles.Add(new List<Cube>() { cube });
            }
        }
        return cubesEnsembles;
    }

    protected IEnumerator CStartSwappingIn() {
        swappyCubesHolderManager.SetCubesLinky(0, useEnableState: true);
        if (timeBeforeStartSwapping > 0) {
            yield return new WaitForSeconds(timeBeforeStartSwapping);
        }
        swappyCubesHolderManager.StartSwapping();
    }
}
