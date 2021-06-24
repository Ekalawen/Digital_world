using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AddLumieresOptimalySpaced : GenerateLumieresMapFunction {

    public enum Mode { MAX_MIN_DISTANCE, MAX_AVERAGE_DISTANCE };

    public int nbLumieres = 1;
    public int nbTriesByLumiere = 100;
    public Mode mode = Mode.MAX_MIN_DISTANCE;

    public override void Activate() {
        CreateLumieresOptimalySpaced();
    }

    protected void CreateLumieresOptimalySpaced() {
        for(int i = 0; i < nbLumieres; i++) {
            CreateLumiereOptimalyFarFromOtherLumieres();
        }
    }

    protected void CreateLumiereOptimalyFarFromOtherLumieres() {
        Vector3 position = GetOptimalyFarPosition();
        map.CreateLumiere(position, lumiereType);
    }

    protected Vector3 GetOptimalyFarPosition() {
        List<Vector3> otherLumieresPositions = map.GetLumieres().Select(l => l.transform.position).ToList();
        List<Vector3> positionsCandidates = Enumerable.Range(0, nbTriesByLumiere).Select(i => map.GetFreeRoundedLocation()).ToList();
        Vector3 bestPosition = GetMaxDistancePosition(otherLumieresPositions, positionsCandidates);
        return bestPosition;
    }

    protected Vector3 GetMaxDistancePosition(List<Vector3> otherLumieresPositions, List<Vector3> positionsCandidates) {
        Vector3 bestPosition = Vector3.zero;
        float maxDistance = -1;

        foreach (Vector3 positionCandidate in positionsCandidates) {
            float currentDistance = GetAdaptativeDistance(positionCandidate, otherLumieresPositions);
            if (currentDistance > maxDistance) {
                maxDistance = currentDistance;
                bestPosition = positionCandidate;
            }
        }

        return bestPosition;
    }

    protected float GetAdaptativeDistance(Vector3 position, List<Vector3> otherPositions) {
        if (mode == Mode.MAX_MIN_DISTANCE) {
            return GetMinDistance(position, otherPositions);
        }
        return GetMaxAverageDistance(position, otherPositions);
    }

    protected float GetMinDistance(Vector3 position, List<Vector3> otherPositions) {
        if (otherPositions.Count == 0) {
            return 0;
        }
        return otherPositions.Select(p => Vector3.SqrMagnitude(p - position)).Min();
    }

    protected float GetMaxAverageDistance(Vector3 position, List<Vector3> otherPositions) {
        if (otherPositions.Count == 0) {
            return 0;
        }
        return otherPositions.Select(p => Vector3.Distance(p, position)).Average();
    }
}
