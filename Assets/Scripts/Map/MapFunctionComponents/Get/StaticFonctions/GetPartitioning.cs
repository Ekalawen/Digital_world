using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetPartitioning {

    public enum Method { ELBOW, SILHOUETTE };

    protected Vector2Int kRange; // Inclusive
    protected Method method;
    protected int maxNbOfIterations;
    protected List<KMeans> kMeans;

    public GetPartitioning(Vector2Int kRange, Method method, int maxNbOfIterations = 100) {
        this.kRange = kRange;
        this.method = method;
        this.maxNbOfIterations = maxNbOfIterations;
    }

    public KMeans GetBestKMeans(List<Vector3> positions) {
        kMeans = ComputeKMeans(positions);
        return GetBestKMeans(kMeans);
    }

    protected KMeans GetBestKMeans(List<KMeans> kMeans) {
        switch (method) {
            case Method.ELBOW:
                return GetBestElbow(kMeans);
            case Method.SILHOUETTE:
                return kMeans.OrderBy(k => k.ComputeSilhouette()).Last();
            default:
                Debug.LogError($"Bad value for method ({method}) in GetParitioning !");
                return null;
        }
    }

    protected KMeans GetBestElbow(List<KMeans> kMeans) {
        KMeans best = kMeans[0];
        float bestDerivativeValue = float.NegativeInfinity;
        for(int i = 1; i < kMeans.Count - 1; i++) {
            float secondDerivativeDiscrete = kMeans[i - 1].ComputeWSS() + kMeans[i + 1].ComputeWSS() - kMeans[i].ComputeWSS() * 2;
            if (secondDerivativeDiscrete > bestDerivativeValue) {
                best = kMeans[i];
                bestDerivativeValue = secondDerivativeDiscrete;
            }
        }
        return best;
    }

    public List<KMeans> ComputeKMeans(List<Vector3> positions) {
        List<KMeans> kMeans = new List<KMeans>();
        for (int k = kRange.x; k <= kRange.y; k++) {
            KMeans kmean = new KMeans(k, maxNbOfIterations);
            kmean.ComputeKMeans(positions);
            kMeans.Add(kmean);
        }
        return kMeans;
    }
}