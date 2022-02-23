using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class BuilderCube : NonBlackCube {

    public static Vector2Int kRange = new Vector2Int(1, 8);
    public static int kMeansNbMaxIterations = 100;

    public float range = 8.0f;
    public GetPartitioning.Method partitioningMethod = GetPartitioning.Method.ELBOW;

    protected bool hasBeenBuilt;

    public override void Initialize() {
        base.Initialize();
        hasBeenBuilt = false;
        if(gm.player.DoubleCheckInteractWithCube(this)) {
            InteractWithPlayer();
        }
    }

    public override void InteractWithPlayer() {
        Build();
    }

    protected void Build() {
        if(hasBeenBuilt) {
            return;
        }
        hasBeenBuilt = true;
        List<Cube> nearByCubes = gm.map.GetCubesInSphere(transform.position, range);
        if(nearByCubes.Count <= 0) {
            Debug.LogError($"Il ne peut pas y avoir aucun cubes dans les nearByCubes du builder ! x)");
        }
        GetPartitioning partioner = new GetPartitioning(kRange, partitioningMethod, kMeansNbMaxIterations);
        KMeans bestKMeans = partioner.GetBestKMeans(nearByCubes.Select(c => c.transform.position).ToList());
        Debug.Log($"Best k = {bestKMeans.GetK()}");
        List<Vector3> roundedCenters = bestKMeans.GetRoundedCenters();
        Vector3 myRoundedPosition = MathTools.Round(transform.position);
        if(!roundedCenters.Contains(myRoundedPosition)) {
            roundedCenters.Add(myRoundedPosition);
        }
        foreach(Vector3 center in roundedCenters) {
            Cube cube = gm.map.GetCubeAt(center);
            if (cube == null) {
                gm.map.AddCube(center, CubeType.BOUNCY);
            } else {
                gm.map.SwapCubeType(cube, Cube.CubeType.BOUNCY);
            }
        }
    }
}
