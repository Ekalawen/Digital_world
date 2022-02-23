using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class BuilderCube : NonBlackCube {

    public static Vector2Int kRange = new Vector2Int(1, 8);
    public static int kMeansNbMaxIterations = 100;

    public CubeType cubeGeneratedType = CubeType.NORMAL;
    public float range = 8.0f;
    public GetPartitioning.Method partitioningMethod = GetPartitioning.Method.ELBOW;
    public int nbCubesToGenerate = 45;

    protected bool hasBeenBuilt;
    protected int nbCubesToGenerateRemaining;

    public override void Initialize() {
        base.Initialize();
        hasBeenBuilt = false;
        nbCubesToGenerateRemaining = nbCubesToGenerate;
        if(gm.player.DoubleCheckInteractWithCube(this)) {
            InteractWithPlayer();
        }
    }

    public override void InteractWithPlayer() {
        Build();
    }

    protected void Build()
    {
        if (hasBeenBuilt)
        {
            return;
        }
        hasBeenBuilt = true;
        List<Cube> nearByCubes = gm.map.GetCubesInSphere(transform.position, range);
        if (nearByCubes.Count <= 0) {
            Debug.LogError($"Il ne peut pas y avoir aucun cubes dans les nearByCubes du builder ! x)");
        }
        List<Vector3> clusterCenters = GetClusterCenters(nearByCubes);
        List<Cube> createdCubes = CreatePathToClusterCenters(clusterCenters);
        DisplayClusterCentersAsBouncyCubes(clusterCenters);
    }

    protected List<Cube> CreatePathToClusterCenters(List<Vector3> clusterCenters) {
        List<Cube> createdCubes = new List<Cube>();
        foreach(Vector3 clusterCenter in clusterCenters) {
            createdCubes.AddRange(CreateCubePathTo(clusterCenter));
        }
        return createdCubes;
    }

    protected List<Cube> CreateCubePathTo(Vector3 target) {
        Vector3 start = MathTools.Round(transform.position);
        Vector3 roundedTarget = MathTools.Round(target);
        List<Vector3> straightPath = gm.map.GetStraitPathVerticalLast(start, roundedTarget);
        List<Cube> createdCubes = new List<Cube>();
        for(int i = 0; i < straightPath.Count; i++) {
            Vector3 current = straightPath[i];
            Cube newCube = gm.map.AddCube(current, cubeGeneratedType);
            if(newCube != null) {
                createdCubes.Add(newCube);
                nbCubesToGenerateRemaining--;
            } else {
                createdCubes.Add(gm.map.GetCubeAt(current));
            }
        }
        // On finit les chemins même si ça nous coûte plus de cubes que prévu !
        Debug.Log($"Il reste {nbCubesToGenerateRemaining} cubes à construire !");
        return createdCubes;
    }

    private List<Vector3> GetClusterCenters(List<Cube> nearByCubes) {
        GetPartitioning partioner = new GetPartitioning(kRange, partitioningMethod, kMeansNbMaxIterations);
        KMeans bestKMeans = partioner.GetBestKMeans(nearByCubes.Select(c => c.transform.position).ToList());
        Debug.Log($"Best k = {bestKMeans.GetK()}");
        List<Vector3> roundedCenters = bestKMeans.GetRoundedCenters();
        Vector3 myRoundedPosition = MathTools.Round(transform.position);
        if (!roundedCenters.Contains(myRoundedPosition)) {
            roundedCenters.Add(myRoundedPosition);
        }
        return roundedCenters;
    }

    protected void DisplayClusterCentersAsBouncyCubes(List<Vector3> roundedCenters) {
        foreach (Vector3 center in roundedCenters) {
            Cube cube = gm.map.GetCubeAt(center);
            if (cube == null) {
                gm.map.AddCube(center, CubeType.BOUNCY);
            } else {
                gm.map.SwapCubeType(cube, Cube.CubeType.BOUNCY);
            }
        }
    }
}
