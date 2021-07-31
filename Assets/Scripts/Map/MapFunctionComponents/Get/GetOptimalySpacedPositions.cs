using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetOptimalySpacedPositions {

    public enum Mode { MAX_MIN_DISTANCE, MAX_AVERAGE_DISTANCE };

    public static List<Vector3> GetSpacedPositions(MapManager map, int nbPositions, List<Vector3> farFromPositions, int nbTriesByLumiere, Mode mode = Mode.MAX_MIN_DISTANCE) {
        List<Vector3> updatedFarPositions = farFromPositions ?? new List<Vector3>();
        List<Vector3> spacedPositions = new List<Vector3>();
        for(int i = 0; i < nbPositions; i++) {
            Vector3 newPosition = GetOneSpacedPosition(map, updatedFarPositions, nbTriesByLumiere, mode);
            spacedPositions.Add(newPosition);
            updatedFarPositions.Add(newPosition);
        }
        return spacedPositions;
    }

    public static Vector3 GetOneSpacedPosition(MapManager map, List<Vector3> farFromPositions, int nbTriesByLumiere, Mode mode = Mode.MAX_MIN_DISTANCE) {
        List<Vector3> positionsCandidates = Enumerable.Range(0, nbTriesByLumiere).Select(i => map.GetFreeRoundedLocation()).ToList();
        Vector3 bestPosition = GetMaxDistancePosition(farFromPositions, positionsCandidates, mode);
        return bestPosition;
    }

    protected static Vector3 GetMaxDistancePosition(List<Vector3> farFromPositions, List<Vector3> positionsCandidates, Mode mode) {
        Vector3 bestPosition = Vector3.zero;
        float maxDistance = -1;

        foreach (Vector3 positionCandidate in positionsCandidates) {
            float currentDistance = GetAdaptativeDistance(positionCandidate, farFromPositions, mode);
            if (currentDistance > maxDistance) {
                maxDistance = currentDistance;
                bestPosition = positionCandidate;
            }
        }

        return bestPosition;
    }

    protected static float GetAdaptativeDistance(Vector3 position, List<Vector3> otherPositions, Mode mode) {
        if (mode == Mode.MAX_MIN_DISTANCE) {
            return GetMinDistance(position, otherPositions);
        }
        return GetMaxAverageDistance(position, otherPositions);
    }

    protected static float GetMinDistance(Vector3 position, List<Vector3> otherPositions) {
        if (otherPositions.Count == 0) {
            return 0;
        }
        return otherPositions.Select(p => Vector3.SqrMagnitude(p - position)).Min();
    }

    protected static float GetMaxAverageDistance(Vector3 position, List<Vector3> otherPositions) {
        if (otherPositions.Count == 0) {
            return 0;
        }
        return otherPositions.Select(p => Vector3.Distance(p, position)).Average();
    }
}
