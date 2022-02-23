using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KMeans {

    public class RelatedPosition {
        public Vector3 pos;
        public Vector3 relatedCenter;
        public float distanceToCenter;

        public RelatedPosition(Vector3 pos, Vector3 relatedCenter, float distanceToCenter) {
            this.pos = pos;
            this.relatedCenter = relatedCenter;
            this.distanceToCenter = distanceToCenter;
        }

        public RelatedPosition(Vector3 pos, Vector3 relatedCenter) : this(pos, relatedCenter, Vector3.Distance(pos, relatedCenter)) {}
    }

    protected int maxNbOfIterations;
    protected int n;
    protected int k;
    protected List<RelatedPosition> positions;
    protected List<Vector3> centers;
    protected Dictionary<Vector3, List<Vector3>> clusters;

    public KMeans(int k, int maxNbOfIterations = 100) {
        this.k = k;
        this.maxNbOfIterations = maxNbOfIterations;
    }

    public void ComputeKMeans(List<Vector3> initialPositions) {
        n = initialPositions.Count;
        if(n < k) {
            CreateACenterForEachPosition(initialPositions);
            return;
        }

        centers = MathTools.ChoseSome(initialPositions, k);
        positions = initialPositions.Select(p => CreateRelatedPosWithClosestCenter(p, centers)).ToList();

        for(int i = 0; i < maxNbOfIterations; i++) {
            List<Vector3> precedentCenters = centers.Select(c => c).ToList();

            positions = UpdatePositions(positions, centers);
            clusters = GetClusters(positions);
            centers = UpdateCenters(clusters, centers);

            if (HasConverged(centers, precedentCenters)) {
                break;
            }
        }
    }

    protected List<Vector3> UpdateCenters(Dictionary<Vector3, List<Vector3>> clusters, List<Vector3> oldCenters) {
        return oldCenters.Select(oc => MathTools.VecAverage(clusters[oc])).ToList();
    }

    // The center, and every positions associated with it!
    protected Dictionary<Vector3, List<Vector3>> GetClusters(List<RelatedPosition> positions) {
        Dictionary<Vector3, List<Vector3>> clusters = new Dictionary<Vector3, List<Vector3>>();
        foreach(RelatedPosition pos in positions) {
            if(clusters.ContainsKey(pos.relatedCenter)) {
                clusters[pos.relatedCenter].Add(pos.pos);
            } else {
                clusters[pos.relatedCenter] = new List<Vector3>() { pos.pos };
            }
        }
        return clusters;
    }

    protected List<RelatedPosition> UpdatePositions(List<RelatedPosition> positions, List<Vector3> centers) {
        return positions.Select(p => CreateRelatedPosWithClosestCenter(p.pos, centers)).ToList();
    }

    protected RelatedPosition CreateRelatedPosWithClosestCenter(Vector3 p, List<Vector3> centers) {
        return new RelatedPosition(p, centers.OrderBy(c => Vector3.Distance(p, c)).First());
    }

    protected bool HasConverged(List<Vector3> currentCenters, List<Vector3> precedentCenters) {
        for(int i = 0; i < k; i++) {
            if(!MathTools.AlmostEqual(currentCenters[i], precedentCenters[i])) {
                return false;
            }
        }
        return true;
    }

    protected void CreateACenterForEachPosition(List<Vector3> initialPositions) {
        positions = initialPositions.Select(p => new RelatedPosition(p, p)).ToList();
        centers = initialPositions.Select(p => p).ToList();
        clusters = GetClusters(positions);
    }

    public List<Vector3> GetCenters() {
        return centers;
    }

    public List<RelatedPosition> GetRelatedPositions() {
        return positions;
    }

    public Dictionary<Vector3, List<Vector3>> GetClusters() {
        return clusters;
    }
}
