﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KMeans {

    public class RelatedPosition {
        public Vector3 pos;
        public Vector3 relatedCenter;

        public RelatedPosition(Vector3 pos, Vector3 relatedCenter) {
            this.pos = pos;
            this.relatedCenter = relatedCenter;
        }
    }

    protected int maxNbOfIterations;
    protected int n;
    protected int k;
    protected List<RelatedPosition> positions;
    protected List<Vector3> centers;
    protected Dictionary<Vector3, List<Vector3>> clusters;
    protected bool hasComputedWSS = false;
    protected float wssScore;

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
            clusters = GetClusters(positions, centers);
            centers = UpdateCenters(clusters, centers);

            if (HasConverged(centers, precedentCenters)) {
                break;
            }
        }
    }

    protected List<Vector3> UpdateCenters(Dictionary<Vector3, List<Vector3>> clusters, List<Vector3> oldCenters) {
        return oldCenters.Select(oc => clusters[oc].Count > 0 ? MathTools.VecAverage(clusters[oc]) : oc).ToList();
    }

    // The center, and every positions associated with it!
    protected Dictionary<Vector3, List<Vector3>> GetClusters(List<RelatedPosition> positions, List<Vector3> centers) {
        Dictionary<Vector3, List<Vector3>> clusters = new Dictionary<Vector3, List<Vector3>>();
        foreach(Vector3 center in centers) {
            clusters[center] = new List<Vector3>();
        }
        foreach(RelatedPosition pos in positions) {
            clusters[pos.relatedCenter].Add(pos.pos);
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
        clusters = GetClusters(positions, centers);
    }

    public List<Vector3> GetCenters() {
        return centers;
    }

    public List<Vector3> GetRoundedCenters() {
        return centers.Select(c => MathTools.Round(c)).ToList();
    }

    public List<RelatedPosition> GetRelatedPositions() {
        return positions;
    }

    public Dictionary<Vector3, List<Vector3>> GetClusters() {
        return clusters;
    }

    // WSS = Within-Cluster-Sum of Squared Errors
    // Squared = Carré ! Donc il n'y aura pas de racine !
    public float ComputeWSS() {
        if(hasComputedWSS) {
            return wssScore;
        }
        wssScore = positions.Select(p => Vector3.SqrMagnitude(p.pos - p.relatedCenter)).Sum();
        hasComputedWSS = true;
        return wssScore;
    }

    public float ComputeSilhouette() {
        return positions.Select(p => ComputeSilhouetteValueFor(p)).Average();
    }

    protected float ComputeSilhouetteValueFor(RelatedPosition pos) {
        // s(i) = (b(i) - a(i)) / max(a(i), b(i))   if |C(i)| > 1        else s(i) = 0 if |C(i)| <= 1
        // a(i) is the measure of similarity of the point i to its own cluster
        // b(i) is the measure of dissimilarity of the point i to other clusters
        List<Vector3> cluster = clusters[pos.relatedCenter];
        if(cluster.Count <= 1) {
            return 0;
        }
        float a = cluster.Select(p => Vector3.Distance(p, pos.pos)).Sum() / (cluster.Count - 1);
        float b = 0;
        foreach(KeyValuePair<Vector3, List<Vector3>> otherCluster in clusters) {
            if(otherCluster.Key == pos.relatedCenter) {
                continue;
            }
            b += otherCluster.Value.Select(p => Vector3.Distance(p, pos.pos)).Sum() / otherCluster.Value.Count;
        }
        return (b - a) / Mathf.Max(a, b);
    }

    public int GetK() {
        return k;
    }
}