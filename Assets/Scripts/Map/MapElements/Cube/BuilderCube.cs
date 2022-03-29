using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class BuilderCube : NonBlackCube {

    public class VoisinScored {
        public Vector3 pos;
        public float score;

        public VoisinScored(Vector3 pos, float score) {
            this.pos = pos;
            this.score = score;
        }
    }

    public static Vector2Int kRange = new Vector2Int(1, 8);
    public static int kMeansNbMaxIterations = 100;

    [Header("Generation")]
    public CubeType cubeGeneratedType = CubeType.NORMAL;
    public float range = 8.0f;
    public GetPartitioning.Method partitioningMethod = GetPartitioning.Method.ELBOW;
    public int nbCubesToGenerate = 45;
    public float generatedDissolveTime = 1.0f;

    [Header("Expand Scores")]
    public float distanceToCentersCoef = 10.0f;
    public float distanceVerticalToCentersCoef = 30;

    [Header("Custom Clusters")]
    public bool useCustomClusters = false;
    public List<Transform> customClusters;

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
        if(IsLinky()) {
            linkyCube.LinkyBuild();
        } else {
            RealBuild();
        }
    }

    public void Build() {
        RealBuild();
    }

    public void RealBuild() {
        if (hasBeenBuilt) {
            return;
        }
        hasBeenBuilt = true;

        List<Cube> nearByCubes = gm.map.GetCubesInSphere(transform.position, range);
        if (nearByCubes.Count <= 0) {
            Debug.LogError($"Il ne peut pas y avoir aucun cubes dans les nearByCubes du builder ! x)");
        }
        List<Vector3> clusterCenters = GetClusterCenters(nearByCubes);
        if(clusterCenters.Count == 0) { // Usefull in IR where some Builders are just linky and have no customCenters :)
            return;
        }
        List<Cube> createdCubes = CreatePathToClusterCenters(clusterCenters);
        ExpandPathWithRemaininCubes(createdCubes, clusterCenters);
        ReplaceItselfWithNormalCube();
        EnsurePlayerIsNotTrapped();
        //DisplayClusterCentersAsBouncyCubes(clusterCenters);
    }

    protected void ReplaceItselfWithNormalCube() {
        Cube replacingCube = gm.map.SwapCubeType(this, CubeType.NORMAL, transform.parent);
        replacingCube.StartDissolveEffect(generatedDissolveTime);
        if(gm.IsIR() && replacingCube != null) {
            replacingCube.GetComponentInParent<Block>().GetCubes().Add(replacingCube);
        }
    }

    protected void ExpandPathWithRemaininCubes(List<Cube> createdCubes, List<Vector3> centers) {
        List<VoisinScored> voisins = new List<VoisinScored>();
        foreach(Cube createdCube in createdCubes) {
            if (createdCube != null) {
                voisins = AddVoisinsOffPositionInVoisinsOrdered(createdCube.transform.position, voisins, centers, new List<Vector3>());
            }
        }

        int nbCubesToCreate = nbCubesToGenerateRemaining;
        List<Vector3> openVoisins = new List<Vector3>();
        while(nbCubesToGenerateRemaining > 0) {
            VoisinScored bestVoisin = voisins.Last();
            if(!gm.map.IsCubeAt(bestVoisin.pos)) {
                Cube newCube = CreateNewCube(bestVoisin.pos);
                if (newCube != null) {
                    nbCubesToGenerateRemaining--;
                }
            }
            openVoisins.Add(bestVoisin.pos);
            voisins = AddVoisinsOffPositionInVoisinsOrdered(bestVoisin.pos, voisins, centers, openVoisins);
            voisins.Remove(bestVoisin);
        }
    }

    protected Cube CreateNewCube(Vector3 pos) {
        Cube newCube = gm.map.AddCube(pos, cubeGeneratedType, parent: transform.parent);
        if (newCube != null) {
            newCube.StartDissolveEffect(generatedDissolveTime);
            if(gm.IsIR()) {
                GetComponentInParent<Block>().GetCubes().Add(newCube);
            }
        }
        return newCube;
    }

    protected List<VoisinScored> AddVoisinsOffPositionInVoisinsOrdered(Vector3 position, List<VoisinScored> voisins, List<Vector3> centers, List<Vector3> alreadyOpenVoisins) {
        float absoluteHeight = gm.gravityManager.GetHeightAbsolute(position);
        List<Vector3> voisinsLibres = !useCustomClusters ? gm.map.GetVoisinsLibresAll(position) : gm.map.GetVoisinsLibresAllWithoutRound(position);
        //voisinsLibres = voisinsLibres.FindAll(v => gm.gravityManager.GetHeightAbsolute(v) == absoluteHeight);
        List<VoisinScored> voisinsScoredLibres = voisinsLibres.Select(v => new VoisinScored(v, GetScoreFor(v, centers))).ToList();
        voisinsScoredLibres = voisinsScoredLibres.FindAll(vs => !alreadyOpenVoisins.Contains(vs.pos));
        foreach(VoisinScored vs in voisinsScoredLibres) {
            voisins = AddVoisinInVoisinsOrdered(vs, voisins);
        }
        return voisins;
    }

    protected List<VoisinScored> AddVoisinInVoisinsOrdered(VoisinScored vs, List<VoisinScored> voisins) {
        if(voisins.Count == 0) {
            voisins.Add(vs);
            return voisins;
        }
        int m = 0;
        int M = voisins.Count;
        while(m <= M) {
            int c = (m + M) / 2; // Rounded down
            if (c == voisins.Count) {
                if (voisins[c - 1].score <= vs.score) {
                    voisins.Insert(c, vs);
                    break;
                } else {
                    M = c - 1;
                }
            } else if (c == 0) {
                if (vs.score <= voisins[0].score) {
                    voisins.Insert(0, vs);
                    break;
                } else {
                    m = c + 1;
                }
            } else if(voisins[c - 1].score <= vs.score && vs.score <= voisins[c].score) {
                voisins.Insert(c, vs);
                break;
            } else if (vs.score <= voisins[c - 1].score){
                M = c - 1;
            } else { // voisins[c].score <= vs.score
                m = c + 1;
            }
        }
        return voisins;
    }

    protected float GetScoreFor(Vector3 pos, List<Vector3> centers) {
        float score = 0;
        float posAbsoluteHeight = gm.gravityManager.GetHeightAbsolute(pos);
        float playerAbsoluteHeight = gm.gravityManager.GetHeightAbsolute(gm.player.transform.position);
        Vector3 closestCenter = centers.OrderBy(c => Vector3.Distance(c, pos)).First();
        score -= Vector3.Distance(closestCenter, pos) * distanceToCentersCoef;
        score -= Mathf.Abs(gm.gravityManager.GetHeightAbsolute(closestCenter) - posAbsoluteHeight) * distanceVerticalToCentersCoef;
        return score;
    }

    protected List<Cube> CreatePathToClusterCenters(List<Vector3> clusterCenters) {
        List<Cube> createdCubes = new List<Cube>() { this };
        foreach(Vector3 clusterCenter in clusterCenters) {
            createdCubes.AddRange(CreateCubePathTo(clusterCenter));
        }
        return createdCubes;
    }

    protected List<Cube> CreateCubePathTo(Vector3 target) {
        Vector3 start = !useCustomClusters ? MathTools.Round(transform.position) : transform.position;
        Vector3 bestTarget = !useCustomClusters ? MathTools.Round(GetBestTargetForTarget(target)) : GetBestTargetForTarget(target);
        List<Vector3> straightPath = gm.map.GetStraitPathVerticalLast(start, bestTarget, shouldRoundPositions: !useCustomClusters);
        Vector3 roundedTarget = !useCustomClusters ? MathTools.Round(target) : target;
        if(straightPath.Count > 0 && straightPath.Last() != roundedTarget) {
            straightPath.Add(roundedTarget);
        }
        List<Cube> createdCubes = new List<Cube>();
        //Color pathColor = MathTools.ChoseOne(new List<Color>() { Color.red, Color.green, Color.yellow });
        for (int i = 0; i < straightPath.Count; i++) {
            Vector3 current = straightPath[i];
            Cube newCube = CreateNewCube(current);
            if (newCube != null) {
                createdCubes.Add(newCube);
                nbCubesToGenerateRemaining--;
            } else {
                createdCubes.Add(gm.map.GetCubeAt(current));
            }
        }
        // On finit les chemins même si ça nous coûte plus de cubes que prévu !
        //Debug.Log($"Il reste {nbCubesToGenerateRemaining} cubes à construire !");
        return createdCubes;
    }

    protected Vector3 GetBestTargetForTarget(Vector3 target) {
        float absoluteHeight = gm.gravityManager.GetHeightAbsolute(transform.position);
        float targetAbsoluteHeight = gm.gravityManager.GetHeightAbsolute(target);
        Vector3 up = gm.gravityManager.Up();
        if (targetAbsoluteHeight >= absoluteHeight || (target - up * targetAbsoluteHeight == transform.position - up * absoluteHeight)) {
            return target;
        }
        List<Vector3> voisins = !useCustomClusters ? gm.map.GetVoisinsLibresAll(target) : gm.map.GetVoisinsLibresAllWithoutRound(target);
        if (voisins.Count > 0) {
            List<Vector3> horizontalVoisins = voisins.FindAll(v => gm.gravityManager.GetHeightAbsolute(v) == targetAbsoluteHeight);
            if (horizontalVoisins.Count > 0) {
                return horizontalVoisins.OrderBy(v => Vector3.SqrMagnitude(v - transform.position)).First();
            }
        }
        return target;
    }

    private List<Vector3> GetClusterCenters(List<Cube> nearByCubes) {
        if(useCustomClusters) {
            return customClusters.FindAll(c => c != null).Select(c => c.transform.position).ToList();
        }
        GetPartitioning partioner = new GetPartitioning(kRange, partitioningMethod, kMeansNbMaxIterations);
        KMeans bestKMeans = partioner.GetBestKMeans(nearByCubes.Select(c => c.transform.position).ToList());
        //Debug.Log($"Best k = {bestKMeans.GetK()}");
        List<Vector3> roundedCenters = bestKMeans.GetRoundedCenters();
        Vector3 myRoundedPosition = MathTools.Round(transform.position);
        if (!roundedCenters.Contains(myRoundedPosition)) {
            roundedCenters.Add(myRoundedPosition);
        }
        // We add all builders as centers ! ==> Might have another range for builders cubes !
        foreach(Cube cube in nearByCubes) {
            Vector3 cubeRoundedPosition = MathTools.Round(cube.transform.position);
            if(cube.type == CubeType.BUILDER && !roundedCenters.Contains(cubeRoundedPosition)) {
                roundedCenters.Add(cubeRoundedPosition);
            }
        }
        return roundedCenters;
    }

    protected void DisplayClusterCentersAsBouncyCubes(List<Vector3> roundedCenters) {
        foreach (Vector3 center in roundedCenters) {
            Cube cube = gm.map.GetCubeAt(center);
            if (cube == null) {
                gm.map.AddCube(center, CubeType.BOUNCY, parent: transform.parent);
            } else {
                gm.map.SwapCubeType(cube, Cube.CubeType.BOUNCY);
            }
        }
    }

    protected void EnsurePlayerIsNotTrapped() {
        Vector3 playerRoundedPosition = !useCustomClusters ? MathTools.Round(gm.player.transform.position) : gm.player.transform.position;
        if(gm.map.GetVoisinsPleinsAll(playerRoundedPosition).Count == 6) {
            Vector3 endPath = gm.map.ChoseOneEmptyPositionInSphere(playerRoundedPosition, 4.0f);
            List<Vector3> path = gm.map.GetStraitPath(playerRoundedPosition, endPath);
            path.Add(playerRoundedPosition);
            foreach(Vector3 pos in path) {
                Cube cube = gm.map.GetCubeAt(pos);
                if (cube != null) {
                    cube.Decompose(1.0f);
                }
            }
        }
    }
}
